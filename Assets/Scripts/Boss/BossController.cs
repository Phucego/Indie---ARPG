using System.Collections;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(EnemyHealth), typeof(Outline))]
public class BossController : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float attackRadius = 3f;
    [SerializeField] private float slamRadius = 5f;
    [SerializeField] private float fireballInterval = 15f;

    [Header("Rotation Settings")]
    [SerializeField] private float lookSpeed = 6f;

    [Header("Special Move Settings")]
    [SerializeField] private float slamDamage = 30f;
    [SerializeField] private float slamCooldown = 3f; // Cooldown between slams in seconds
    [SerializeField] private float fireballDamage = 20f;
    [SerializeField] private float fireballSpawnHeight = 10f; // Height above ground where fireball spawns
    [SerializeField] private float defensiveHealPercentage = 0.05f; // 5% of current health
    [SerializeField] private float defensiveStateDuration = 5f;
    [SerializeField] private float defensiveStateCooldown = 30f;
    [SerializeField] private GameObject fireballPrefab; // Prefab for fireball AoE
    [SerializeField] private GameObject fireballIndicatorPrefab; // Prefab for fireball impact indicator
    [SerializeField] private GameObject slamIndicatorPrefab; // Prefab for slam attack indicator
    [SerializeField] private GameObject slamVFXPrefab; // Prefab for slam VFX

    [Header("Camera Shake Settings")]
    [SerializeField] private float slamShakeDuration = 0.5f;
    [SerializeField] private float slamShakeStrength = 0.5f;
    [SerializeField] private int slamShakeVibrato = 10;
    [SerializeField] private float slamShakeRandomness = 90f;
    [SerializeField] private float fireballShakeDuration = 0.4f;
    [SerializeField] private float fireballShakeStrength = 0.3f;
    [SerializeField] private int fireballShakeVibrato = 12;
    [SerializeField] private float fireballShakeRandomness = 90f;

    private EnemyHealth health;
    private EnemyUIManager uiManager;
    private Camera mainCamera;
    private bool isStaggered = false;
    private bool isInDefensiveState = false;
    private float nextFireballTime;
    private float nextDefensiveStateTime;
    private float nextSlamTime; // Tracks when the next slam can occur
    private bool isSlamPreparing = false; // Tracks if a slam is being prepared

    public bool IsPlayerInDetectionRange =>
        PlayerMovement.Instance != null &&
        Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= detectionRadius;
    public bool IsPlayerInAttackRange =>
        PlayerMovement.Instance != null &&
        Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= attackRadius;

    private void Start()
    {
        health = GetComponent<EnemyHealth>();
        uiManager = FindObjectOfType<EnemyUIManager>();
        mainCamera = Camera.main;

        if (health == null)
        {
            Debug.LogError("[BossController] No EnemyHealth component found!");
            return;
        }

        if (mainCamera == null)
        {
            Debug.LogWarning("[BossController] No MainCamera found for camera shake!");
        }

        GetComponent<Outline>().enabled = false;

        // Initialize UI
        if (uiManager != null)
        {
            uiManager.UpdateEnemyTarget(health);
            uiManager.ShowEnemyHealthBar(health.enemyName, health.GetCurrentHealth(), health.maxHealth);
        }

        nextFireballTime = Time.time + fireballInterval;
        nextDefensiveStateTime = Time.time + defensiveStateCooldown;
        nextSlamTime = Time.time; // Initialize slam cooldown
    }

    private void Update()
    {
        if (isStaggered || isInDefensiveState || health.IsDead) return;

        if (PlayerMovement.Instance == null) return;

        // Update UI
        if (uiManager != null && IsPlayerInDetectionRange)
        {
            uiManager.ShowEnemyHealthBar(health.enemyName, health.GetCurrentHealth(), health.maxHealth);
        }

        // Fireball AoE every 15 seconds
        if (Time.time >= nextFireballTime)
        {
            StartCoroutine(FireballAttack());
            nextFireballTime = Time.time + fireballInterval;
        }

        // Defensive state when health is low or on cooldown
        if (Time.time >= nextDefensiveStateTime && health.GetCurrentHealth() <= health.maxHealth * 0.5f)
        {
            StartCoroutine(EnterDefensiveState());
            nextDefensiveStateTime = Time.time + defensiveStateCooldown;
        }

        // Attack behavior (stationary, only rotate and cast)
        if (IsPlayerInAttackRange && Time.time >= nextSlamTime && !isSlamPreparing)
        {
            StartCoroutine(SlamAttackWithIndicator());
        }
        else if (IsPlayerInDetectionRange)
        {
            SmoothLookAt(PlayerMovement.Instance.transform);
        }
    }

    private void SnapRotateToPlayer()
    {
        if (PlayerMovement.Instance == null) return;

        Vector3 direction = (PlayerMovement.Instance.transform.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation;
        }
    }

    private void SmoothLookAt(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookSpeed);
        }
    }

    private IEnumerator SlamAttackWithIndicator()
    {
        isSlamPreparing = true;

        // Show slam indicator
        GameObject indicator = null;
        if (slamIndicatorPrefab != null)
        {
            indicator = Instantiate(slamIndicatorPrefab, transform.position, Quaternion.identity);
        }

        // Rotate to face player at the start of the slam preparation
        SnapRotateToPlayer();

        // Wait 3 seconds, checking if player remains in attack range
        float warningDuration = 3f;
        float elapsed = 0f;

        while (elapsed < warningDuration)
        {
            elapsed += Time.deltaTime;
            if (!IsPlayerInAttackRange)
            {
                // Player left the attack range, cancel slam
                if (indicator != null)
                {
                    Destroy(indicator);
                }
                isSlamPreparing = false;
                yield break;
            }
            yield return null;
        }

        // Destroy indicator after warning
        if (indicator != null)
        {
            Destroy(indicator);
        }

        // Perform slam if player is still in range
        if (IsPlayerInAttackRange)
        {
            PerformSlamAttack();
            nextSlamTime = Time.time + slamCooldown; // Set cooldown
        }

        isSlamPreparing = false;
    }

    private void PerformSlamAttack()
    {
        // Deal AoE damage in slam radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, slamRadius);
        bool hitPlayer = false;
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                // Assuming Player has a PlayerHealth component
                PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(slamDamage);
                    hitPlayer = true;
                }
            }
        }

        // Play slam VFX
        if (slamVFXPrefab != null)
        {
            Instantiate(slamVFXPrefab, transform.position, Quaternion.identity);
        }

        // Camera shake if player was hit
        if (hitPlayer && mainCamera != null)
        {
            mainCamera.transform.DOShakePosition(slamShakeDuration, slamShakeStrength, slamShakeVibrato, slamShakeRandomness);
            mainCamera.transform.DOShakeRotation(slamShakeDuration, slamShakeStrength * 0.5f, slamShakeVibrato, slamShakeRandomness);
        }

        Debug.Log("Boss performed slam attack!");
        if (uiManager != null)
        {
            uiManager.OnEnemyAttacked();
        }
    }

    private IEnumerator FireballAttack()
    {
        if (PlayerMovement.Instance == null || fireballPrefab == null) yield break;

        // Get player's position for the fireball target
        Vector3 playerPos = PlayerMovement.Instance.transform.position;
        Vector3 groundPos = new Vector3(playerPos.x, 0f, playerPos.z); // Target ground level (Y=0)
        Vector3 spawnPos = groundPos + Vector3.up * fireballSpawnHeight; // Spawn above ground

        // Show fireball indicator
        GameObject indicator = null;
        if (fireballIndicatorPrefab != null)
        {
            indicator = Instantiate(fireballIndicatorPrefab, groundPos, Quaternion.identity);
        }

        // Wait for warning period (2 seconds) to give player time to react
        float warningDuration = 2f;
        yield return new WaitForSeconds(warningDuration);

        // Destroy indicator after warning
        if (indicator != null)
        {
            Destroy(indicator);
        }

        // Spawn fireball at spawn height
        GameObject fireball = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
        
        // Vertical fall animation to ground
        float fallDuration = 1.5f;
        float elapsed = 0f;
        Vector3 startPos = fireball.transform.position;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            float newY = Mathf.Lerp(startPos.y, groundPos.y, t);
            fireball.transform.position = new Vector3(startPos.x, newY, startPos.z);
            yield return null;
        }

        // Ensure fireball is exactly at ground level
        fireball.transform.position = groundPos;

        // Deal AoE damage
        Collider[] hitColliders = Physics.OverlapSphere(groundPos, 3f); // 3m radius for fireball AoE
        bool hitPlayer = false;
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(fireballDamage);
                    hitPlayer = true;
                }
            }
        }

        // Camera shake if player was hit
        if (hitPlayer && mainCamera != null)
        {
            mainCamera.transform.DOShakePosition(fireballShakeDuration, fireballShakeStrength, fireballShakeVibrato, fireballShakeRandomness);
            mainCamera.transform.DOShakeRotation(fireballShakeDuration, fireballShakeStrength * 0.5f, fireballShakeVibrato, fireballShakeRandomness);
        }

        Debug.Log("Boss launched fireball attack!");
        Destroy(fireball);
    }

    private IEnumerator EnterDefensiveState()
    {
        isInDefensiveState = true;

        // Heal 5% of current health
        float healAmount = health.GetCurrentHealth() * defensiveHealPercentage;
        health.TakeDamage(-healAmount); // Negative damage to heal
        Debug.Log("Boss entered defensive state, healing for: " + healAmount);

        // Optionally, play defensive animation or VFX here
        yield return new WaitForSeconds(defensiveStateDuration);

        isInDefensiveState = false;
        Debug.Log("Boss exited defensive state.");
    }

    public void Stagger(float duration)
    {
        if (!isStaggered && !isInDefensiveState)
        {
            StartCoroutine(StaggerCoroutine(duration));
        }
    }

    private IEnumerator StaggerCoroutine(float duration)
    {
        isStaggered = true;
        yield return new WaitForSeconds(duration);
        isStaggered = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, slamRadius);
    }
}
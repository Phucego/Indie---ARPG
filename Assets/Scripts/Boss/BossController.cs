using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(EnemyHealth), typeof(Outline))]
public class BossController : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float attackRadius = 3f;
    [SerializeField] private float slamRadius = 5f;
    [SerializeField] private float fireballInterval = 15f;

    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float lookSpeed = 6f;

    [Header("Special Move Settings")]
    [SerializeField] private float slamDamage = 30f;
    [SerializeField] private float slamCooldown = 3f; // Cooldown between slams in seconds
    [SerializeField] private float fireballDamage = 20f;
    [SerializeField] private float defensiveHealPercentage = 0.05f; // 5% of current health
    [SerializeField] private float defensiveStateDuration = 5f;
    [SerializeField] private float defensiveStateCooldown = 30f;
    [SerializeField] private GameObject fireballPrefab; // Prefab for fireball AoE
    [SerializeField] private GameObject fireballIndicatorPrefab; // Prefab for fireball impact indicator
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

    private NavMeshAgent agent;
    private EnemyHealth health;
    private EnemyUIManager uiManager;
    private Camera mainCamera;
    private bool isStaggered = false;
    private bool isInDefensiveState = false;
    private float nextFireballTime;
    private float nextDefensiveStateTime;
    private float nextSlamTime; // Tracks when the next slam can occur

    public bool IsPlayerInDetectionRange =>
        PlayerMovement.Instance != null &&
        Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= detectionRadius;
    public bool IsPlayerInAttackRange =>
        PlayerMovement.Instance != null &&
        Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= attackRadius;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<EnemyHealth>();
        uiManager = FindObjectOfType<EnemyUIManager>();
        mainCamera = Camera.main;

        if (agent == null)
        {
            Debug.LogError("[BossController] No NavMeshAgent found!");
            return;
        }

        if (health == null)
        {
            Debug.LogError("[BossController] No EnemyHealth component found!");
            return;
        }

        if (mainCamera == null)
        {
            Debug.LogWarning("[BossController] No MainCamera found for camera shake!");
        }

        agent.speed = movementSpeed;
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

        // Normal behavior
        if (IsPlayerInAttackRange && Time.time >= nextSlamTime)
        {
            SnapRotateToPlayer();
            PerformSlamAttack();
            nextSlamTime = Time.time + slamCooldown; // Set cooldown
        }
        else if (IsPlayerInDetectionRange)
        {
            SmoothLookAt(PlayerMovement.Instance.transform);
            MoveTowardsPlayer();
        }
        else
        {
            Idle();
        }
    }

    private void MoveTowardsPlayer()
    {
        if (agent != null && PlayerMovement.Instance != null)
        {
            agent.SetDestination(PlayerMovement.Instance.transform.position);
            agent.isStopped = false;
        }
    }

    private void Idle()
    {
        if (agent != null)
        {
            agent.isStopped = true;
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

    private void PerformSlamAttack()
    {
        if (agent != null)
        {
            agent.isStopped = true;
        }

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

        // Show fireball indicator
        GameObject indicator = null;
        if (fireballIndicatorPrefab != null)
        {
            indicator = Instantiate(fireballIndicatorPrefab, playerPos, Quaternion.identity);
        }

        // Wait for warning period (e.g., 2 seconds) to give player time to react
        float warningDuration = 2f;
        yield return new WaitForSeconds(warningDuration);

        // Destroy indicator after warning
        if (indicator != null)
        {
            Destroy(indicator);
        }

        // Spawn fireball at player's position
        GameObject fireball = Instantiate(fireballPrefab, playerPos + Vector3.up * 10f, Quaternion.identity);
        
        // Simple fall animation
        float fallDuration = 1.5f;
        Vector3 targetPos = playerPos;
        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            fireball.transform.position = Vector3.Lerp(fireball.transform.position, targetPos, elapsed / fallDuration);
            yield return null;
        }

        // Deal AoE damage
        Collider[] hitColliders = Physics.OverlapSphere(targetPos, 3f); // 3m radius for fireball AoE
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
        if (agent != null)
        {
            agent.isStopped = true;
        }

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
        if (agent != null) agent.isStopped = true;
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
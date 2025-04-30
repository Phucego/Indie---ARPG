using System.Collections;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(EnemyHealth), typeof(Outline))]
public class BossController : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float attackRadius = 3f;
    [SerializeField] private float blizzardConeAngle = 60f; // Angle of the blizzard cone in degrees
    [SerializeField] private float blizzardRange = 8f; // Range of the blizzard cone
    [SerializeField] private float fireballInterval = 15f;

    [Header("Rotation Settings")]
    [SerializeField] private float lookSpeed = 6f;

    [Header("Special Move Settings")]
    [SerializeField] private float blizzardDamage = 25f;
    [SerializeField] private float blizzardCooldown = 5f; // Cooldown between blizzard attacks
    [SerializeField] private float fireballDamage = 20f;
    [SerializeField] private float fireballSpawnHeight = 10f; // Height above ground where fireball spawns
    [SerializeField] private float defensiveHealPercentage = 0.05f; // 5% of current health
    [SerializeField] private float defensiveStateDuration = 5f;
    [SerializeField] private float defensiveStateCooldown = 30f;
    [SerializeField] private GameObject fireballPrefab; // Prefab for fireball AoE
    [SerializeField] private GameObject fireballIndicatorPrefab; // Prefab for fireball impact indicator
    [SerializeField, Tooltip("Image-based prefab for blizzard cone indicator (e.g., sprite or ground texture)")]
    private GameObject blizzardIndicatorPrefab; // Prefab for blizzard cone indicator (image-based)
    [SerializeField] private GameObject blizzardVFXPrefab; // Prefab for blizzard VFX
    [SerializeField] private GameObject fireballExplosionPrefab; // Prefab for fireball explosion effect
    [SerializeField, Tooltip("If true, billboard the blizzard indicator to face the camera; if false, align to ground")]
    private bool billboardIndicator = true; // Control whether indicator faces camera or stays ground-aligned

    [Header("Camera Shake Settings")]
    [SerializeField] private float blizzardShakeDuration = 0.6f;
    [SerializeField] private float blizzardShakeStrength = 0.4f;
    [SerializeField] private int blizzardShakeVibrato = 15;
    [SerializeField] private float blizzardShakeRandomness = 90f;
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
    private float nextBlizzardTime; // Tracks when the next blizzard can occur
    private bool isBlizzardPreparing = false; // Tracks if a blizzard is being prepared

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
        nextBlizzardTime = Time.time; // Initialize blizzard cooldown
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
        if (IsPlayerInAttackRange && Time.time >= nextBlizzardTime && !isBlizzardPreparing)
        {
            StartCoroutine(BlizzardAttackWithIndicator());
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

    private IEnumerator BlizzardAttackWithIndicator()
    {
        isBlizzardPreparing = true;

        // Show blizzard cone indicator (image-based)
        GameObject indicator = null;
        if (blizzardIndicatorPrefab != null)
        {
            // Position indicator slightly above ground to avoid z-fighting
            Vector3 indicatorPos = transform.position + Vector3.up * 0.1f;
            indicator = Instantiate(blizzardIndicatorPrefab, indicatorPos, transform.rotation);
            // Ensure indicator follows boss rotation
            indicator.transform.SetParent(transform, true);

            // If billboarding, make the indicator face the camera
            if (billboardIndicator && mainCamera != null)
            {
                StartCoroutine(BillboardIndicator(indicator));
            }
            else
            {
                // Ensure indicator is flat on the ground (Y rotation follows boss, X/Z rotation zeroed)
                indicator.transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            }
        }

        // Rotate to face player at the start of the blizzard preparation
        SnapRotateToPlayer();

        // Wait 3 seconds, checking if player remains in attack range
        float warningDuration = 3f;
        float elapsed = 0f;

        while (elapsed < warningDuration)
        {
            elapsed += Time.deltaTime;
            if (!IsPlayerInAttackRange)
            {
                // Player left the attack range, cancel blizzard
                if (indicator != null)
                {
                    Destroy(indicator);
                }
                isBlizzardPreparing = false;
                yield break;
            }
            yield return null;
        }

        // Destroy indicator after warning
        if (indicator != null)
        {
            Destroy(indicator);
        }

        // Perform blizzard if player is still in range
        if (IsPlayerInAttackRange)
        {
            PerformBlizzardAttack();
            nextBlizzardTime = Time.time + blizzardCooldown; // Set cooldown
        }

        isBlizzardPreparing = false;
    }

    private IEnumerator BillboardIndicator(GameObject indicator)
    {
        while (indicator != null)
        {
            // Make indicator face the camera, keeping it flat (only rotate on Y axis for camera facing)
            Vector3 directionToCamera = (mainCamera.transform.position - indicator.transform.position).normalized;
            directionToCamera.y = 0; // Keep indicator flat on ground plane
            if (directionToCamera != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(-directionToCamera);
                indicator.transform.rotation = Quaternion.Euler(90, lookRotation.eulerAngles.y, 0); // 90 on X to lie flat
            }
            yield return null;
        }
    }

    private void PerformBlizzardAttack()
    {
        // Play blizzard VFX
        if (blizzardVFXPrefab != null)
        {
            GameObject vfx = Instantiate(blizzardVFXPrefab, transform.position, transform.rotation);
            vfx.transform.SetParent(transform, true); // Make VFX follow boss
        }

        // Check for player in cone-shaped area
        bool hitPlayer = false;
        if (PlayerMovement.Instance != null)
        {
            Vector3 playerPos = PlayerMovement.Instance.transform.position;
            Vector3 toPlayer = (playerPos - transform.position).normalized;
            Vector3 forward = transform.forward;
            float distanceToPlayer = Vector3.Distance(transform.position, playerPos);
            float angleToPlayer = Vector3.Angle(forward, toPlayer);

            // Check if player is within the cone (angle and range)
            if (angleToPlayer <= blizzardConeAngle / 2f && distanceToPlayer <= blizzardRange)
            {
                PlayerHealth playerHealth = PlayerMovement.Instance.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(blizzardDamage);
                    hitPlayer = true;
                }
            }
        }

        // Camera shake if player was hit
        if (hitPlayer && mainCamera != null)
        {
            // Store initial local position and rotation
            Vector3 initialLocalPosition = mainCamera.transform.localPosition;
            Quaternion initialLocalRotation = mainCamera.transform.localRotation;

            // Apply shake to local position and rotation
            mainCamera.transform.DOShakePosition(blizzardShakeDuration, blizzardShakeStrength, blizzardShakeVibrato, blizzardShakeRandomness, false);
            mainCamera.transform.DOShakeRotation(blizzardShakeDuration, blizzardShakeStrength * 0.5f, blizzardShakeVibrato, blizzardShakeRandomness, false)
                .OnComplete(() =>
                {
                    // Restore initial local position and rotation
                    mainCamera.transform.localPosition = initialLocalPosition;
                    mainCamera.transform.localRotation = initialLocalRotation;
                });
        }

        Debug.Log("Boss performed blizzard attack!");
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

        // Instantiate explosion prefab
        if (fireballExplosionPrefab != null)
        {
            Instantiate(fireballExplosionPrefab, groundPos, Quaternion.identity);
        }

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
            // Store initial local position and rotation
            Vector3 initialLocalPosition = mainCamera.transform.localPosition;
            Quaternion initialLocalRotation = mainCamera.transform.localRotation;

            // Apply shake to local position and rotation
            mainCamera.transform.DOShakePosition(fireballShakeDuration, fireballShakeStrength, fireballShakeVibrato, fireballShakeRandomness, false);
            mainCamera.transform.DOShakeRotation(fireballShakeDuration, fireballShakeStrength * 0.5f, fireballShakeVibrato, fireballShakeRandomness, false)
                .OnComplete(() =>
                {
                    // Restore initial local position and rotation
                    mainCamera.transform.localPosition = initialLocalPosition;
                    mainCamera.transform.localRotation = initialLocalRotation;
                });
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

        // Draw blizzard cone
        Gizmos.color = Color.cyan;
        Vector3 forward = transform.forward * blizzardRange;
        Quaternion leftRot = Quaternion.Euler(0, -blizzardConeAngle / 2f, 0);
        Quaternion rightRot = Quaternion.Euler(0, blizzardConeAngle / 2f, 0);
        Vector3 leftRay = leftRot * transform.forward * blizzardRange;
        Vector3 rightRay = rightRot * transform.forward * blizzardRange;
        Gizmos.DrawRay(transform.position, forward);
        Gizmos.DrawRay(transform.position, leftRay);
        Gizmos.DrawRay(transform.position, rightRay);
    }
}
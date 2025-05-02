using System.Collections;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(EnemyHealth), typeof(Outline), typeof(Animator))]
public class BossController : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField, Tooltip("Radius for detecting the player")] private float detectionRadius = 15f;
    [SerializeField, Tooltip("Radius for attacking the player")] private float attackRadius = 3f;
    [SerializeField, Tooltip("Angle of the lightning cone in degrees")] private float lightningConeAngle = 60f;
    [SerializeField, Tooltip("Range of the lightning cone")] private float lightningRange = 8f;
    [SerializeField, Tooltip("Interval between fireball attacks")] private float fireballInterval = 15f;

    [Header("Rotation Settings")]
    [SerializeField, Tooltip("Speed of rotation towards target")] private float lookSpeed = 6f;

    [Header("Special Move Settings")]
    [SerializeField, Tooltip("Damage dealt by lightning attack")] private float lightningDamage = 25f;
    [SerializeField, Tooltip("Cooldown between lightning attacks")] private float lightningCooldown = 5f;
    [SerializeField, Tooltip("Damage dealt by fireball attack")] private float fireballDamage = 20f;
    [SerializeField, Tooltip("Height above ground where fireball spawns")] private float fireballSpawnHeight = 10f;
    [SerializeField, Tooltip("Percentage of current health to heal in defensive state")] private float defensiveHealPercentage = 0.05f;
    [SerializeField, Tooltip("Duration of defensive state")] private float defensiveStateDuration = 5f;
    [SerializeField, Tooltip("Cooldown for defensive state")] private float defensiveStateCooldown = 30f;
    [SerializeField, Tooltip("Prefab for fireball AoE")] private GameObject fireballPrefab;
    [SerializeField, Tooltip("Prefab for fireball impact indicator")] private GameObject fireballIndicatorPrefab;
    [SerializeField, Tooltip("Image-based prefab for lightning cone indicator")] private GameObject lightningIndicatorPrefab;
    [SerializeField, Tooltip("Prefab for lightning VFX")] private GameObject lightningVFXPrefab;
    [SerializeField, Tooltip("Prefab for fireball explosion effect")] private GameObject fireballExplosionPrefab;
    [SerializeField, Tooltip("If true, billboard the lightning indicator to face the camera")] private bool billboardIndicator = true;

    [Header("Animation Settings")]
    [SerializeField, Tooltip("Idle animation clip")] private AnimationClip idleClip;
    [SerializeField, Tooltip("Lightning attack animation clip")] private AnimationClip lightningClip;

    [Header("Audio Settings")]
    [SerializeField, Tooltip("Sound played during lightning preparation (looping)")] private AudioClip lightningPrepSound;
    [SerializeField, Tooltip("Sound played on lightning strike (one-shot)")] private AudioClip lightningStrikeSound;

    [Header("Camera Shake Settings")]
    [SerializeField, Tooltip("Duration of camera shake for lightning")] private float lightningShakeDuration = 0.6f;
    [SerializeField, Tooltip("Strength of camera shake for lightning")] private float lightningShakeStrength = 0.4f;
    [SerializeField, Tooltip("Vibrato of camera shake for lightning")] private int lightningShakeVibrato = 15;
    [SerializeField, Tooltip("Randomness of camera shake for lightning")] private float lightningShakeRandomness = 90f;
    [SerializeField, Tooltip("Duration of camera shake for fireball")] private float fireballShakeDuration = 0.4f;
    [SerializeField, Tooltip("Strength of camera shake for fireball")] private float fireballShakeStrength = 0.3f;
    [SerializeField, Tooltip("Vibrato of camera shake for fireball")] private int fireballShakeVibrato = 12;
    [SerializeField, Tooltip("Randomness of camera shake for fireball")] private float fireballShakeRandomness = 90f;

    private const float LIGHTNING_WARNING_DURATION = 3f;
    private const float FIREBALL_WARNING_DURATION = 2f;
    private const float FIREBALL_FALL_DURATION = 1.5f;
    private const float FIREBALL_AOE_RADIUS = 3f;
    private const float ANIMATION_CROSSFADE = 0.02f;

    private EnemyHealth health;
    private EnemyUIManager uiManager;
    private Camera mainCamera;
    private Animator animator;
    private AudioSource audioSource;
    private PlayerHealth playerHealth;
    private string currentAnimation = "";
    private bool isStaggered;
    private bool isInDefensiveState;
    private bool isLightningPreparing;
    private float nextFireballTime;
    private float nextDefensiveStateTime;
    private float nextLightningTime;

    public bool IsPlayerInDetectionRange => PlayerMovement.Instance != null &&
        Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= detectionRadius;
    public bool IsPlayerInAttackRange => PlayerMovement.Instance != null &&
        Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= attackRadius;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        health = GetComponent<EnemyHealth>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        audioSource.loop = false; // Default to non-looping, controlled per sound

        if (health == null || animator == null || audioSource == null)
        {
            Debug.LogError("[BossController] Missing required component!", this);
            enabled = false;
            return;
        }

        GetComponent<Outline>().enabled = false;
    }

    private void Start()
    {
        uiManager = FindObjectOfType<EnemyUIManager>();
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning("[BossController] No MainCamera found for camera shake!", this);
        }

        if (PlayerMovement.Instance == null)
        {
            Debug.LogError("[BossController] PlayerMovement.Instance is null!", this);
            enabled = false;
            return;
        }

        playerHealth = PlayerMovement.Instance.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("[BossController] PlayerHealth component not found on player!", this);
            enabled = false;
            return;
        }

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("[BossController] No Animator Controller assigned!", this);
            enabled = false;
            return;
        }

        if (idleClip == null || lightningClip == null)
        {
            Debug.LogWarning("[BossController] One or more animation clips not assigned!", this);
        }
        else
        {
            ChangeAnimation(idleClip);
        }

        if (uiManager != null)
        {
            uiManager.UpdateEnemyTarget(health);
            uiManager.ShowEnemyHealthBar(health.enemyName, health.GetCurrentHealth(), health.maxHealth);
        }

        nextFireballTime = Time.time + fireballInterval;
        nextDefensiveStateTime = Time.time + defensiveStateCooldown;
        nextLightningTime = Time.time;
    }

    private void Update()
    {
        if (!enabled || isStaggered || isInDefensiveState || health.IsDead || PlayerMovement.Instance == null)
        {
            ChangeAnimation(idleClip);
            StopAudio();
            return;
        }

        if (uiManager != null && IsPlayerInDetectionRange)
        {
            uiManager.ShowEnemyHealthBar(health.enemyName, health.GetCurrentHealth(), health.maxHealth);
        }

        if (Time.time >= nextFireballTime)
        {
            StartCoroutine(FireballAttack());
            nextFireballTime = Time.time + fireballInterval;
        }

        if (Time.time >= nextDefensiveStateTime && health.GetCurrentHealth() <= health.maxHealth * 0.5f)
        {
            StartCoroutine(EnterDefensiveState());
            nextDefensiveStateTime = Time.time + defensiveStateCooldown;
        }

        if (IsPlayerInAttackRange && Time.time >= nextLightningTime && !isLightningPreparing)
        {
            StartCoroutine(LightningAttackWithIndicator());
        }
        else if (IsPlayerInDetectionRange)
        {
            SmoothLookAt(PlayerMovement.Instance.transform);
            ChangeAnimation(idleClip);
        }
    }

    private void SnapRotateToPlayer()
    {
        Vector3 direction = (PlayerMovement.Instance.transform.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
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

    private IEnumerator LightningAttackWithIndicator()
    {
        isLightningPreparing = true;
        ChangeAnimation(lightningClip);
        PlayAudio(lightningPrepSound, true);

        GameObject indicator = null;
        if (lightningIndicatorPrefab != null)
        {
            Vector3 indicatorPos = transform.position + Vector3.up * 0.1f;
            indicator = Instantiate(lightningIndicatorPrefab, indicatorPos, transform.rotation);
            indicator.transform.SetParent(transform, true);

            if (billboardIndicator && mainCamera != null)
            {
                StartCoroutine(BillboardIndicator(indicator));
            }
            else
            {
                indicator.transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            }
        }

        SnapRotateToPlayer();
        float elapsed = 0f;

        while (elapsed < LIGHTNING_WARNING_DURATION)
        {
            elapsed += Time.deltaTime;
            if (!IsPlayerInAttackRange)
            {
                if (indicator != null)
                {
                    Destroy(indicator);
                }
                isLightningPreparing = false;
                ChangeAnimation(idleClip);
                StopAudio();
                yield break;
            }
            yield return null;
        }

        if (indicator != null)
        {
            Destroy(indicator);
        }

        if (IsPlayerInAttackRange)
        {
            PerformLightningAttack();
            nextLightningTime = Time.time + lightningCooldown;
        }

        isLightningPreparing = false;
        ChangeAnimation(idleClip);
        StopAudio();
    }

    private IEnumerator BillboardIndicator(GameObject indicator)
    {
        while (indicator != null)
        {
            Vector3 directionToCamera = (mainCamera.transform.position - indicator.transform.position).normalized;
            directionToCamera.y = 0;
            if (directionToCamera != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(-directionToCamera);
                indicator.transform.rotation = Quaternion.Euler(90, lookRotation.eulerAngles.y, 0);
            }
            yield return null;
        }
    }

    private void PerformLightningAttack()
    {
        if (lightningVFXPrefab != null)
        {
            GameObject vfx = Instantiate(lightningVFXPrefab, transform.position, transform.rotation);
            vfx.transform.SetParent(transform, true);
            DestroyVFXAfterPlaying(vfx);
        }

        PlayAudio(lightningStrikeSound, false);

        bool hitPlayer = false;
        if (PlayerMovement.Instance != null)
        {
            Vector3 playerPos = PlayerMovement.Instance.transform.position;
            Vector3 toPlayer = (playerPos - transform.position).normalized;
            Vector3 forward = transform.forward;
            float distanceToPlayer = Vector3.Distance(transform.position, playerPos);
            float angleToPlayer = Vector3.Angle(forward, toPlayer);

            if (angleToPlayer <= lightningConeAngle / 2f && distanceToPlayer <= lightningRange)
            {
                playerHealth.TakeDamage(lightningDamage);
                hitPlayer = true;
            }
        }

        if (hitPlayer && mainCamera != null)
        {
            Vector3 initialLocalPosition = mainCamera.transform.localPosition;
            Quaternion initialLocalRotation = mainCamera.transform.localRotation;
            mainCamera.transform.DOShakePosition(lightningShakeDuration, lightningShakeStrength, lightningShakeVibrato, lightningShakeRandomness, false);
            mainCamera.transform.DOShakeRotation(lightningShakeDuration, lightningShakeStrength * 0.5f, lightningShakeVibrato, lightningShakeRandomness, false)
                .OnComplete(() =>
                {
                    mainCamera.transform.localPosition = initialLocalPosition;
                    mainCamera.transform.localRotation = initialLocalRotation;
                });
        }

        Debug.Log("Boss performed lightning attack!");
        if (uiManager != null)
        {
            uiManager.OnEnemyAttacked();
        }
    }

    private IEnumerator FireballAttack()
    {
        if (PlayerMovement.Instance == null || fireballPrefab == null) yield break;

        Vector3 playerPos = PlayerMovement.Instance.transform.position;
        Vector3 groundPos = new Vector3(playerPos.x, 0f, playerPos.z);
        Vector3 spawnPos = groundPos + Vector3.up * fireballSpawnHeight;

        GameObject indicator = null;
        if (fireballIndicatorPrefab != null)
        {
            indicator = Instantiate(fireballIndicatorPrefab, groundPos, Quaternion.identity);
        }

        yield return new WaitForSeconds(FIREBALL_WARNING_DURATION);

        if (indicator != null)
        {
            Destroy(indicator);
        }

        GameObject fireball = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
        float elapsed = 0f;
        Vector3 startPos = fireball.transform.position;

        while (elapsed < FIREBALL_FALL_DURATION)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / FIREBALL_FALL_DURATION;
            float newY = Mathf.Lerp(startPos.y, groundPos.y, t);
            fireball.transform.position = new Vector3(startPos.x, newY, startPos.z);
            yield return null;
        }

        fireball.transform.position = groundPos;

        if (fireballExplosionPrefab != null)
        {
            GameObject explosion = Instantiate(fireballExplosionPrefab, groundPos, Quaternion.identity);
            DestroyVFXAfterPlaying(explosion);
        }

        Collider[] hitColliders = Physics.OverlapSphere(groundPos, FIREBALL_AOE_RADIUS);
        bool hitPlayer = false;
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                playerHealth.TakeDamage(fireballDamage);
                hitPlayer = true;
            }
        }

        if (hitPlayer && mainCamera != null)
        {
            Vector3 initialLocalPosition = mainCamera.transform.localPosition;
            Quaternion initialLocalRotation = mainCamera.transform.localRotation;
            mainCamera.transform.DOShakePosition(fireballShakeDuration, fireballShakeStrength, fireballShakeVibrato, fireballShakeRandomness, false);
            mainCamera.transform.DOShakeRotation(fireballShakeDuration, fireballShakeStrength * 0.5f, fireballShakeVibrato, fireballShakeRandomness, false)
                .OnComplete(() =>
                {
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
        float healAmount = health.GetCurrentHealth() * defensiveHealPercentage;
        health.TakeDamage(-healAmount);
        Debug.Log($"Boss entered defensive state, healing for: {healAmount}");
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
        ChangeAnimation(idleClip);
        StopAudio();
        yield return new WaitForSeconds(duration);
        isStaggered = false;
    }

    private void PlayAudio(AudioClip clip, bool loop)
    {
        if (clip == null)
        {
            Debug.LogWarning("[BossController] Audio clip is null!", this);
            return;
        }

        if (audioSource.clip != clip || !audioSource.isPlaying)
        {
            audioSource.clip = clip;
            audioSource.loop = loop;
            audioSource.Play();
        }
    }

    private void StopAudio()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }

    private void DestroyVFXAfterPlaying(GameObject vfxObject)
    {
        if (vfxObject == null) return;

        ParticleSystem particleSystem = vfxObject.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            Destroy(vfxObject, particleSystem.main.duration);
        }
        else
        {
            Destroy(vfxObject, 5f);
        }
    }

    private void ChangeAnimation(AnimationClip animationClip, float crossfade = ANIMATION_CROSSFADE)
    {
        if (animationClip == null)
        {
            Debug.LogWarning("[BossController] Cannot change animation: AnimationClip is null!", this);
            return;
        }

        if (currentAnimation != animationClip.name)
        {
            currentAnimation = animationClip.name;
            animator.CrossFade(animationClip.name, crossfade);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
        Gizmos.color = Color.cyan;
        Vector3 forward = transform.forward * lightningRange;
        Quaternion leftRot = Quaternion.Euler(0, -lightningConeAngle / 2f, 0);
        Quaternion rightRot = Quaternion.Euler(0, lightningConeAngle / 2f, 0);
        Vector3 leftRay = leftRot * transform.forward * lightningRange;
        Vector3 rightRay = rightRot * transform.forward * lightningRange;
        Gizmos.DrawRay(transform.position, forward);
        Gizmos.DrawRay(transform.position, leftRay);
        Gizmos.DrawRay(transform.position, rightRay);
    }
}
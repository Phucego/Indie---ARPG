using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    #region Fields and References

    [Header("Attack Properties")]
    public float rangedAttackRange = 20f;
    [Tooltip("Maximum time to attempt moving to target before fallback attack")]
    [SerializeField] private float movementTimeout = 3f;
    [Tooltip("Minimum movement distance per frame to consider progress")]
    [SerializeField] private float minMovementProgress = 0.05f;

    [Header("Combat Animations")]
    public AnimationClip shootingAnimation;

    [Header("Ranged Attack")]
    public Transform projectileSpawnPoint;

    [Header("References")]
    public Transform playerTransform;
    public WeaponManager weaponManager;
    public StaminaManager staminaManager;
    public PlayerMovement playerMovement;
    public PlayerStats playerStats;
    public Animator animator;

    public static PlayerAttack Instance;
    private Coroutine enemyUIHideCoroutine = null;

    [Header("Hover Detection")]
    public LayerMask targetLayerMask;
    private GameObject lastHoveredTarget = null;
    private EnemyUIManager lastEnemyUIManager;

    public bool isAttacking = false;
    public GameObject currentTarget = null;
    private bool isAutoAttacking = false;

    [Header("Audio")]
    public AudioClip fireSound;
    private AudioManager audioManager;

    #endregion

    #region Initialization

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        weaponManager = GetComponent<WeaponManager>();
        staminaManager = GetComponentInChildren<StaminaManager>();
        playerMovement = GetComponent<PlayerMovement>();
        playerStats = GetComponent<PlayerStats>();
        audioManager = FindObjectOfType<AudioManager>();
    }

    #endregion

    #region Update Loop

    private void Update()
    {
        if (IsInputBlocked())
        {
            StopAutoAttack();
            return;
        }

        HandleHover();
        HandleAttackInput();
    }

    private bool IsInputBlocked()
    {
        return (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) ||
               (DialogueDisplay.Instance != null && DialogueDisplay.Instance.isDialogueActive);
    }

    #endregion

    #region Hover Detection

    private void HandleHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, targetLayerMask);
        GameObject closestTarget = GetClosestValidTarget(hits);

        if (closestTarget != null)
        {
            UpdateHoveredTarget(closestTarget);
        }
        else
        {
            ClearHoverUI();
        }
    }

    private GameObject GetClosestValidTarget(RaycastHit[] hits)
    {
        GameObject closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (RaycastHit hit in hits)
        {
            GameObject target = hit.collider.gameObject;
            if (target.CompareTag("Breakable") || target.TryGetComponent(out EnemyHealth _))
            {
                float distance = hit.distance;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = target;
                }
            }
        }

        return closestTarget;
    }

    private void UpdateHoveredTarget(GameObject hoveredTarget)
    {
        if (lastHoveredTarget != hoveredTarget)
        {
            ClearHoverUI();
            lastHoveredTarget = hoveredTarget;

            if (hoveredTarget.TryGetComponent(out EnemyHealth enemyHealth))
            {
                lastEnemyUIManager = hoveredTarget.GetComponent<EnemyUIManager>();
                if (lastEnemyUIManager != null)
                {
                    lastEnemyUIManager.UpdateEnemyTarget(enemyHealth);
                    lastEnemyUIManager.SetVisibility(true);
                }
            }
            ShowOutlineOnTarget(hoveredTarget, true);
        }

        if (Input.GetMouseButtonDown(0))
            TryAttack(hoveredTarget);
    }

    private void ClearHoverUI()
    {
        if (lastEnemyUIManager != null)
        {
            lastEnemyUIManager.SetVisibility(false);
            lastEnemyUIManager = null;
        }

        if (lastHoveredTarget != null)
        {
            ShowOutlineOnTarget(lastHoveredTarget, false);
            lastHoveredTarget = null;
        }
    }

    private void ShowOutlineOnTarget(GameObject target, bool show)
    {
        if (target != null && target.TryGetComponent(out Outline outline))
        {
            outline.enabled = show;
        }
    }

    #endregion

    #region Attack Handling

    private void HandleAttackInput()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 100f, targetLayerMask);
            GameObject closestTarget = GetClosestValidTarget(hits);

            if (closestTarget != null)
            {
                TryAttack(closestTarget);
            }
        }

        if (isAutoAttacking && ShouldStopAutoAttack())
        {
            StopAutoAttack();
        }
    }

    private bool ShouldStopAutoAttack()
    {
        return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Q) ||
               Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.E) ||
               Input.GetKeyDown(KeyCode.R) || playerMovement.IsRunning;
    }

    private void TryAttack(GameObject target)
    {
        if (target == null)
        {
            Debug.LogWarning("TryAttack: Target is null.", this);
            return;
        }

        if (isAttacking || !CanAttack())
        {
            Debug.LogWarning($"Cannot attack {target.name}: Already attacking or conditions not met.", this);
            return;
        }

        if (!target.TryGetComponent(out EnemyHealth enemyHealth) && !target.CompareTag("Breakable"))
        {
            Debug.LogWarning($"Target {target.name} is neither an Enemy nor tagged as Breakable.", this);
            return;
        }

        float distance = Vector3.Distance(playerTransform.position, target.transform.position);

        if (distance <= rangedAttackRange)
        {
            playerMovement.canMove = false;
            playerMovement.StopMoving();
            currentTarget = target;
            AttemptRangedAttack(target);
            UpdateEnemyUI(target);
        }
        else
        {
            StartCoroutine(MoveToTargetAndAttack(target));
        }
    }

    private bool CanAttack()
    {
        return !playerMovement.IsRunning && 
               (DialogueDisplay.Instance == null || !DialogueDisplay.Instance.isDialogueActive) &&
               ArrowAmmoManager.Instance.CanShoot();
    }

    private float GetSlopeAngle()
    {
        if (Physics.Raycast(playerTransform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 1f, playerMovement.movableLayer))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            return angle;
        }
        return 0f;
    }

    private void UpdateEnemyUI(GameObject target)
    {
        if (target.TryGetComponent(out EnemyHealth enemy))
        {
            lastEnemyUIManager = enemy.GetComponent<EnemyUIManager>();
            lastEnemyUIManager?.UpdateEnemyTarget(enemy);
            lastEnemyUIManager?.SetVisibility(true);

            if (enemyUIHideCoroutine != null)
                StopCoroutine(enemyUIHideCoroutine);

            enemyUIHideCoroutine = StartCoroutine(HideEnemyUIAfterDelay(3f));
        }
    }

    private IEnumerator HideEnemyUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        lastEnemyUIManager?.SetVisibility(false);
        lastEnemyUIManager = null;
        lastHoveredTarget = null;
        enemyUIHideCoroutine = null;
    }

    private IEnumerator MoveToTargetAndAttack(GameObject target)
    {
        if (target == null)
        {
            Debug.LogWarning("MoveToTargetAndAttack: Target is null.", this);
            yield break;
        }

        float startTime = Time.time;
        Vector3 initialPosition = playerTransform.position;
        float slopeAngle = GetSlopeAngle();
        float adjustedMinProgress = slopeAngle > 10f ? minMovementProgress * 0.5f : minMovementProgress;

        playerMovement.MoveToTarget(target.transform.position);

        while (target != null && Vector3.Distance(playerTransform.position, target.transform.position) > rangedAttackRange)
        {
            playerMovement.MoveToTarget(target.transform.position);

            // Check for timeout or stalled movement
            if (Time.time - startTime > movementTimeout || 
                Vector3.Distance(playerTransform.position, initialPosition) < adjustedMinProgress)
            {
                Debug.LogWarning($"Movement to {target.name} timed out or stalled (slope angle: {slopeAngle}°, distance: {Vector3.Distance(playerTransform.position, target.transform.position)}).", this);
                // Fallback: Attack from current position
                float distance = Vector3.Distance(playerTransform.position, target.transform.position);
                Debug.Log($"Attempting fallback attack on {target.name} from current position (distance: {distance}).", this);
                playerMovement.canMove = false;
                playerMovement.StopMoving();
                currentTarget = target;
                AttemptRangedAttack(target);
                UpdateEnemyUI(target);
                yield break;
            }

            initialPosition = playerTransform.position;
            yield return null;
        }

        if (target != null)
        {
            currentTarget = target;
            playerMovement.canMove = false;
            playerMovement.StopMoving();
            AttemptRangedAttack(target);
        }
    }

    private void AttemptRangedAttack(GameObject target)
    {
        GameObject weapon = weaponManager.GetCurrentWeapon();
        if (weapon == null)
        {
            Debug.LogError("Cannot perform ranged attack: Current weapon is null.", this);
            playerMovement.canMove = true;
            return;
        }
        float totalDamage = weaponManager.GetCurrentWeaponDamage();
        StartCoroutine(PerformRangedAttack(target, totalDamage));
    }

    private IEnumerator PerformRangedAttack(GameObject target, float damage)
    {
        if (shootingAnimation == null || animator == null || target == null)
        {
            Debug.LogWarning("Cannot perform ranged attack: Missing shooting animation, animator, or target.", this);
            isAttacking = false;
            playerMovement.canMove = true;
            yield break;
        }

        isAttacking = true;
        isAutoAttacking = true;

        SnapRotateToTarget(target.transform);

        playerMovement.ChangeAnimation(shootingAnimation);

        yield return new WaitForSeconds(0.2f);

        if (weaponManager.boltPrefab != null && projectileSpawnPoint != null)
        {
            Vector3 targetPos = target.transform.position;
            targetPos.y = projectileSpawnPoint.position.y;
            Vector3 direction = (targetPos - projectileSpawnPoint.position).normalized;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = playerTransform.forward;
            }

            if (fireSound != null && audioManager != null)
            {
                audioManager.PlaySoundEffect(fireSound);
            }

            GameObject projectile = Instantiate(weaponManager.boltPrefab, projectileSpawnPoint.position, Quaternion.identity);
            projectile.SetActive(true);

            if (projectile.TryGetComponent(out Projectile proj))
            {
                ArrowAmmoManager.Instance.ConsumeAmmo();
                proj.Initialize(direction, damage, target, (impactPosition) => 
                {
                    ArrowAmmoManager.Instance.DropArrow(impactPosition);
                });
            }
            else
            {
                projectile.SetActive(false);
            }
        }
        yield return new WaitForSeconds(shootingAnimation.length - 0.2f);

        isAttacking = false;
        playerMovement.canMove = true;

        if (isAutoAttacking && currentTarget != null && CanContinueAutoAttack(currentTarget))
        {
            AttemptRangedAttack(currentTarget);
        }
        else
        {
            StopAutoAttack();
        }
    }

    private bool CanContinueAutoAttack(GameObject target)
    {
        if (target == null || !target.activeInHierarchy) return false;

        bool isWithinRange = Vector3.Distance(playerTransform.position, target.transform.position) <= rangedAttackRange;

        if (target.TryGetComponent(out EnemyHealth health))
        {
            return !health.IsDead && isWithinRange && ArrowAmmoManager.Instance.CanShoot();
        }
        else if (target.CompareTag("Breakable") && target.TryGetComponent(out BreakableProps breakable))
        {
            return breakable != null && isWithinRange && ArrowAmmoManager.Instance.CanShoot();
        }
        return false;
    }

    private void StopAutoAttack()
    {
        isAutoAttacking = false;
        currentTarget = null;
        if (!playerMovement.IsRunning && !playerMovement.IsDodging)
        {
            playerMovement.ChangeAnimation(playerMovement.idleAnimation);
        }
    }

    private void SnapRotateToTarget(Transform target)
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    #endregion
}
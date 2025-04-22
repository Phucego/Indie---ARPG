using System;
using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    #region Fields and References

    [Header("Attack Properties")]
    public float attackCooldown = 1.5f;
    public float attackRange = 2.0f;

    [Header("Combat Animations")]
    [SerializeField] private AnimationClip[] comboAttacks;
    public AnimationClip whirlwindAttack;
    public AnimationClip casting_Short;

    [Header("Attack Settings")]
    public bool shuffleAttacks = false;

    [Header("Stamina")]
    public float dodgeStaminaCost = 25f;

    [Header("Skill System")]
    private Skill[] hotbarSkills = new Skill[4];
    public bool isSkillActive = false;

    public Skill whirlwindSkill;
    public Skill buffSkill;
    public Skill traversalSkill;
    public Skill lightningBallSkill;

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
    public LayerMask enemyLayerMask;
    private GameObject lastHoveredEnemy = null;
    private EnemyUIManager lastEnemyUIManager;

    private float nextAttackTime = 0f;
    public bool isAttacking = false; // Made public to align with PickupSystem, PickableItem
    private GameObject currentTarget = null; // Track the current enemy being attacked
    private bool isAutoAttacking = false; // Flag for auto-attack state

    [SerializeField] private GameObject propDestroyEffect;

    #endregion

    #region Initialization

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        weaponManager = GetComponent<WeaponManager>();
        staminaManager = GetComponentInChildren<StaminaManager>();
        playerMovement = GetComponent<PlayerMovement>();
        playerStats = GetComponent<PlayerStats>();
        if (playerTransform == null)
            playerTransform = transform;

        if (animator == null) Debug.LogError("Animator component missing on PlayerAttack.");
        if (weaponManager == null) Debug.LogError("WeaponManager component missing on PlayerAttack.");
        if (staminaManager == null) Debug.LogError("StaminaManager component missing on PlayerAttack.");
        if (playerMovement == null) Debug.LogError("PlayerMovement component missing on PlayerAttack.");
        if (playerStats == null) Debug.LogError("PlayerStats component missing on PlayerAttack.");
        if (playerTransform == null) Debug.LogError("playerTransform not assigned on PlayerAttack.");

        AssignSkill(0, whirlwindSkill);
        AssignSkill(1, buffSkill);
        AssignSkill(2, traversalSkill);
        AssignSkill(3, lightningBallSkill);
    }

    public void AssignSkill(int hotbarIndex, Skill skill)
    {
        if (hotbarIndex >= 0 && hotbarIndex < 4)
        {
            hotbarSkills[hotbarIndex] = skill;
            if (skill != null)
                Debug.Log($"Assigned {skill.skillName} to Key {hotbarIndex + 1}");
            else
                Debug.LogWarning($"Null skill assigned to hotbar index: {hotbarIndex}");
        }
        else
        {
            Debug.LogWarning($"Invalid hotbar index: {hotbarIndex}");
        }
    }

    #endregion

    #region Update Loop

    private void Update()
    {
        // Skip if over UI, in dialogue, or attacking
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() ||
            (DialogueDisplay.Instance != null && DialogueDisplay.Instance.isDialogueActive))
        {
            StopAutoAttack(); // Stop auto-attacking if over UI or in dialogue
            return;
        }

        HandleEnemyHover();
        CheckSkillInputs();
        HandleAttackInput();

        // Interrupt auto-attack if player provides input
        if (isAutoAttacking && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Alpha1) ||
                               Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3) ||
                               Input.GetKeyDown(KeyCode.Alpha4) || playerMovement.IsRunning))
        {
            StopAutoAttack();
        }
    }

    #endregion

    #region Enemy Hover Detection

    private void HandleEnemyHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, enemyLayerMask))
        {
            GameObject hoveredTarget = hit.collider.gameObject;

            if (hoveredTarget.TryGetComponent(out EnemyHealth enemyHealth))
            {
                UpdateHoveredEnemy(enemyHealth, hoveredTarget);
            }
            else
            {
                ClearHoverUI();
            }
        }
        else
        {
            ClearHoverUI();
        }
    }

    private void UpdateHoveredEnemy(EnemyHealth enemyHealth, GameObject hoveredTarget)
    {
        if (lastHoveredEnemy != hoveredTarget)
        {
            ClearHoverUI();
            lastHoveredEnemy = hoveredTarget;

            lastEnemyUIManager = hoveredTarget.GetComponent<EnemyUIManager>();
            if (lastEnemyUIManager != null)
            {
                lastEnemyUIManager.UpdateEnemyTarget(enemyHealth);
                lastEnemyUIManager.SetVisibility(true);
                ShowOutlineOnEnemy(hoveredTarget, true);
            }
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

        if (lastHoveredEnemy != null)
        {
            ShowOutlineOnEnemy(lastHoveredEnemy, false);
            lastHoveredEnemy = null;
        }
    }

    private void ShowOutlineOnEnemy(GameObject enemy, bool show)
    {
        if (enemy != null && enemy.TryGetComponent(out Outline outline))
        {
            outline.enabled = show;
        }
    }

    #endregion

    #region Skill Input

    private void CheckSkillInputs()
    {
        if (isAttacking) return;

        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && hotbarSkills[i] != null)
                ActivateSkill(hotbarSkills[i]);
        }
    }

    private void ActivateSkill(Skill skill)
    {
        if (skill == null || skill.isOnCooldown || !staminaManager.HasEnoughStamina(skill.staminaCost))
        {
            Debug.LogWarning($"Cannot activate skill {skill?.skillName}: On cooldown or insufficient stamina.");
            return;
        }

        // Check if skill requires two-handed weapon (e.g., whirlwind)
        if (skill == whirlwindSkill && !weaponManager.CanUseTwoHandedSkill())
        {
            Debug.LogWarning($"Cannot use {skill.skillName}: Requires a two-handed weapon.");
            return;
        }

        staminaManager.UseStamina(skill.staminaCost);
        skill.UseSkill(this);
        StartCoroutine(SkillCooldown(skill));
    }

    public IEnumerator SkillCooldown(Skill skill)
    {
        skill.isOnCooldown = true;
        yield return new WaitForSeconds(skill.cooldown);
        skill.isOnCooldown = false;
    }

    #endregion

    #region Attack Handling

    private void HandleAttackInput()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject hitObj = hit.collider.gameObject;

                if (hitObj.TryGetComponent(out EnemyHealth enemy))
                    HandleEnemyAttack(enemy);
                else if (hitObj.TryGetComponent(out BreakableProps breakable))
                    HandleBreakableAttack(breakable);
            }
        }
    }

    private void HandleEnemyAttack(EnemyHealth enemy)
    {
        if (enemy == null) return;

        lastEnemyUIManager = enemy.GetComponent<EnemyUIManager>();
        lastEnemyUIManager?.UpdateEnemyTarget(enemy);
        lastEnemyUIManager?.SetVisibility(true);

        if (enemyUIHideCoroutine != null)
            StopCoroutine(enemyUIHideCoroutine);

        enemyUIHideCoroutine = StartCoroutine(HideEnemyUIAfterDelay(3f));

        TryAttack(enemy.gameObject);
    }

    private IEnumerator HideEnemyUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        lastEnemyUIManager?.SetVisibility(false);
        lastEnemyUIManager = null;
        lastHoveredEnemy = null;

        enemyUIHideCoroutine = null;
    }

    private void TryAttack(GameObject target)
    {
        if (target == null || isAttacking || !CanAttack())
            return;

        float distance = Vector3.Distance(playerTransform.position, target.transform.position);
        if (distance > attackRange)
            StartCoroutine(MoveToTargetAndAttack(target));
        else
        {
            if (target.TryGetComponent(out BreakableProps breakable))
                HandleBreakableAttack(breakable);
            else
            {
                currentTarget = target; // Set the current target for auto-attack
                AttemptAttack(target);
            }
        }
    }

    private bool CanAttack() => Time.time >= nextAttackTime && !playerMovement.IsRunning && !isSkillActive &&
                               (DialogueDisplay.Instance == null || !DialogueDisplay.Instance.isDialogueActive);

    private IEnumerator MoveToTargetAndAttack(GameObject target)
    {
        if (target == null) yield break;

        playerMovement.MoveToTarget(target.transform.position);
        while (Vector3.Distance(playerTransform.position, target.transform.position) > attackRange && target != null)
            yield return null;

        if (target != null)
        {
            currentTarget = target; // Set the current target for auto-attack
            AttemptAttack(target);
        }
    }

    private void AttemptAttack(GameObject target)
    {
        GameObject weapon = weaponManager.GetCurrentWeapon();
        if (weapon == null)
        {
            Debug.LogWarning("No weapon equipped!");
            return;
        }

        float totalDamage = weaponManager.GetCurrentWeaponDamage();
        StartCoroutine(PerformAttack(target.transform, totalDamage));
    }

    private IEnumerator PerformAttack(Transform target, float damage)
    {
        if (comboAttacks == null || comboAttacks.Length == 0)
        {
            Debug.LogWarning("No combo attack animations assigned!");
            isAttacking = false;
            yield break;
        }

        isAttacking = true;
        isAutoAttacking = true; // Enable auto-attack mode
        playerMovement.canMove = false;

        if (target != null)
            SnapRotateToTarget(target);

        int index = shuffleAttacks ? UnityEngine.Random.Range(0, comboAttacks.Length) : 0;
        animator.Play(comboAttacks[index].name);

        yield return new WaitForSeconds(0.2f); // Delay for hit timing

        if (target != null && target.TryGetComponent(out EnemyHealth enemyHealth))
            enemyHealth.TakeDamage(damage);

        yield return new WaitForSeconds(comboAttacks[index].length - 0.2f);

        isAttacking = false;
        playerMovement.canMove = true;
        nextAttackTime = Time.time + attackCooldown;

        // Check if auto-attack should continue
        if (isAutoAttacking && currentTarget != null && 
            currentTarget.TryGetComponent(out EnemyHealth health) && !health.IsDead &&
            Vector3.Distance(playerTransform.position, currentTarget.transform.position) <= attackRange)
        {
            // Continue attacking the same target
            AttemptAttack(currentTarget);
        }
        else
        {
            StopAutoAttack();
        }
    }

    private void StopAutoAttack()
    {
        isAutoAttacking = false;
        currentTarget = null;
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

    #region Breakable Props

    private void HandleBreakableAttack(BreakableProps breakable)
    {
        if (breakable == null) return;

        if (Vector3.Distance(playerTransform.position, breakable.transform.position) > attackRange)
            StartCoroutine(MoveToTargetAndAttackBreakable(breakable));
        else
            StartCoroutine(AttackBreakableSequence(breakable));
    }

    private IEnumerator MoveToTargetAndAttackBreakable(BreakableProps breakable)
    {
        if (breakable == null) yield break;

        playerMovement.MoveToTarget(breakable.transform.position);
        while (Vector3.Distance(playerTransform.position, breakable.transform.position) > attackRange && breakable != null)
            yield return null;

        if (breakable != null)
            StartCoroutine(AttackBreakableSequence(breakable));
    }

    private IEnumerator AttackBreakableSequence(BreakableProps breakable)
    {
        if (comboAttacks == null || comboAttacks.Length == 0)
        {
            Debug.LogWarning("No combo attack animations assigned!");
            isAttacking = false;
            yield break;
        }

        isAttacking = true;
        playerMovement.canMove = false;

        SnapRotateToTarget(breakable.transform);

        animator.Play(comboAttacks[0].name);
        yield return new WaitForSeconds(0.2f);

        if (propDestroyEffect != null)
            Instantiate(propDestroyEffect, breakable.transform.position, Quaternion.identity);

        yield return new WaitForSeconds(comboAttacks[0].length - 0.2f);

        breakable.DestroyObject();

        isAttacking = false;
        playerMovement.canMove = true;
        nextAttackTime = Time.time + attackCooldown;
    }

    #endregion
}
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
    private bool isAttacking = false;

    [SerializeField] private GameObject propDestroyEffect;

    #endregion

    #region Initialization

    private void Awake()
    {
        animator = GetComponent<Animator>();
        Instance = this;
    }

    private void Start()
    {
        staminaManager = GetComponentInChildren<StaminaManager>();
        playerMovement = GetComponent<PlayerMovement>();
        weaponManager = GetComponent<WeaponManager>();
        playerStats = GetComponent<PlayerStats>();

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
            Debug.Log($"Assigned {skill.skillName} to Key {hotbarIndex + 1}");
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
        // Skip input processing if the mouse is over a UI element
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        HandleEnemyHover();
        CheckSkillInputs();
        HandleAttackInput();
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
        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && hotbarSkills[i] != null)
                ActivateSkill(hotbarSkills[i]);
        }
    }

    private void ActivateSkill(Skill skill)
    {
        if (!staminaManager.HasEnoughStamina(skill.staminaCost)) return;

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
        if (Input.GetMouseButtonDown(0))
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
        if (target == null || isAttacking || !CanAttack()) return;

        float distance = Vector3.Distance(playerTransform.position, target.transform.position);
        if (distance > attackRange)
            StartCoroutine(MoveToTargetAndAttack(target));
        else
        {
            if (target.TryGetComponent(out BreakableProps breakable))
                HandleBreakableAttack(breakable);
            else
                AttemptAttack(target);
        }
    }

    private bool CanAttack() => Time.time >= nextAttackTime && !playerMovement.IsRunning;

    private IEnumerator MoveToTargetAndAttack(GameObject target)
    {
        playerMovement.MoveToTarget(target.transform.position);
        while (Vector3.Distance(playerTransform.position, target.transform.position) > attackRange)
            yield return null;

        AttemptAttack(target);
    }

    private void AttemptAttack(GameObject target)
    {
        Weapon weapon = GetCurrentWeapon();
        if (weapon == null)
        {
            Debug.Log("No weapon equipped!");
            return;
        }

        float totalDamage = weapon.damageBonus; // Use only weapon's damage bonus
        StartCoroutine(PerformAttack(target.transform, totalDamage));
    }


    private IEnumerator PerformAttack(Transform target, float damage)
    {
        isAttacking = true;
        playerMovement.canMove = false;

        SnapRotateToTarget(target);
        int index = UnityEngine.Random.Range(0, comboAttacks.Length);
        animator.Play(comboAttacks[index].name);

        yield return new WaitForSeconds(0.2f);

        if (target.TryGetComponent(out EnemyHealth enemyHealth))
            enemyHealth.TakeDamage(damage);

        yield return new WaitForSeconds(comboAttacks[index].length - 0.2f);

        isAttacking = false;
        playerMovement.canMove = true;
        nextAttackTime = Time.time + attackCooldown;
    }

    private void SnapRotateToTarget(Transform target)
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    #endregion

    #region Breakable Props

    private void HandleBreakableAttack(BreakableProps breakable)
    {
        if (Vector3.Distance(playerTransform.position, breakable.transform.position) > attackRange)
            StartCoroutine(MoveToTargetAndAttackBreakable(breakable));
        else
            StartCoroutine(AttackBreakableSequence(breakable));
    }

    private IEnumerator MoveToTargetAndAttackBreakable(BreakableProps breakable)
    {
        playerMovement.MoveToTarget(breakable.transform.position);
        while (Vector3.Distance(playerTransform.position, breakable.transform.position) > attackRange)
            yield return null;

        StartCoroutine(AttackBreakableSequence(breakable));
    }

    private IEnumerator AttackBreakableSequence(BreakableProps breakable)
    {
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

    #region Weapon Reference

    private Weapon GetCurrentWeapon()
    {
        return weaponManager.GetCurrentWeapon();
    }

    #endregion
}

using System;
using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Properties")]
    public float attackCooldown = 1.5f;
    public float attackRange = 2.0f;

    [Header("Combat Animations")]
    [SerializeField] private AnimationClip[] comboAttacks;
    public AnimationClip whirlwindAttack;
    public AnimationClip casting_Short;

    [Header("Attack Settings")]
    public bool shuffleAttacks = false;

    [Header("Damage Values")]
    public float[] comboDamage = new float[] { 10, 15, 20, 25 };

    [Header("Stamina")]
    public float[] comboStaminaCost = new float[] { 20f, 25f, 30f, 35f };
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
    public Animator animator;

    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    public static PlayerAttack Instance;

    [Header("Hover Detection")]
    public LayerMask enemyLayerMask;
    private GameObject lastHoveredEnemy = null;
    private EnemyUIManager lastEnemyUIManager = null;

    [Header("Buff Modifiers")]
    public float damageBonus = 1.0f;
    public float defenseBonus = 1.0f;

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

        AssignSkill(0, whirlwindSkill);
        AssignSkill(1, buffSkill);
        AssignSkill(2, traversalSkill);
        AssignSkill(3, lightningBallSkill);
    }

    private void Update()
    {
        HandleEnemyHover();
        if (Input.GetKeyDown(KeyCode.Space) && isAttacking)
            CancelAttackForDodge();

        CheckSkillInputs();
    }

    private void HandleEnemyHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, enemyLayerMask))
        {
            if (hit.collider.TryGetComponent(out EnemyHealth enemyHealth))
            {
                GameObject hoveredEnemy = enemyHealth.gameObject;

                if (lastHoveredEnemy != hoveredEnemy)
                {
                    if (lastEnemyUIManager != null)
                        lastEnemyUIManager.HideEnemyHealthBar();

                    if (hoveredEnemy.TryGetComponent(out EnemyUIManager newUI))
                    {
                        newUI.SetVisibility(true);
                        lastEnemyUIManager = newUI;
                        lastHoveredEnemy = hoveredEnemy;
                    }
                    else
                    {
                        lastEnemyUIManager = null;
                        lastHoveredEnemy = null;
                    }
                }

                lastEnemyUIManager?.UpdateEnemyTarget(enemyHealth);

                if (Input.GetMouseButtonDown(0))
                    TryAttack(hoveredEnemy);
            }
        }
        else if (lastHoveredEnemy != null)
        {
            lastEnemyUIManager?.HideEnemyHealthBar();
            lastHoveredEnemy = null;
            lastEnemyUIManager = null;
        }
    }

    private void CheckSkillInputs()
    {
        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && hotbarSkills[i] != null)
                ActivateSkill(hotbarSkills[i]);
        }
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

    bool CanAttack()
    {
        return Time.time >= nextAttackTime && !playerMovement.IsDodging && !playerMovement.IsRunning;
    }

    void TryAttack(GameObject target)
    {
        if (isAttacking || !CanAttack()) return;

        int attackIndex = shuffleAttacks ? UnityEngine.Random.Range(0, comboAttacks.Length) : 0;

        if (attackIndex >= comboStaminaCost.Length || !staminaManager.HasEnoughStamina(comboStaminaCost[attackIndex]))
        {
            Debug.Log("Not enough stamina");
            return;
        }

        PerformAttack(attackIndex, target);
    }

    void PerformAttack(int attackIndex, GameObject target)
    {
        Weapon currentWeapon = GetCurrentWeapon();
        if (currentWeapon == null)
        {
            Debug.Log("No weapon equipped!");
            return;
        }

        StartCoroutine(AttackSequence(attackIndex, currentWeapon, target));
    }

    IEnumerator AttackSequence(int attackIndex, Weapon weapon, GameObject target)
    {
        isAttacking = true;
        playerMovement.canMove = false;
        staminaManager.UseStamina(comboStaminaCost[attackIndex]);

        animator.Play(comboAttacks[attackIndex].name);
        yield return new WaitForSeconds(comboAttacks[attackIndex].length * 0.3f);

        float totalDamage = (comboDamage[attackIndex] * damageBonus) + weapon.weaponData.damageBonus;
        ApplyDamageToTarget(target, (int)totalDamage);

        yield return new WaitForSeconds(comboAttacks[attackIndex].length * 0.3f);

        isAttacking = false;
        playerMovement.canMove = true;
        nextAttackTime = Time.time + attackCooldown;
    }

    void ApplyDamageToTarget(GameObject target, int damage)
    {
        if (target.TryGetComponent(out EnemyHealth enemy))
        {
            Weapon currentWeapon = GetCurrentWeapon();
            float finalDamage = damage + (currentWeapon?.weaponData.damageBonus ?? 0);
            enemy.TakeDamage(finalDamage, (enemy.transform.position - playerTransform.position).normalized);
        }
    }

    void CancelAttackForDodge()
    {
        isAttacking = false;
        playerMovement.canMove = true;
        Debug.Log("Attack cancelled for dodge");
    }

    public Weapon GetCurrentWeapon()
    {
        if (weaponManager.IsTwoHandedWeaponEquipped())
            return weaponManager.equippedRightHandWeapon;

        return weaponManager.equippedRightHandWeapon ?? weaponManager.equippedLeftHandWeapon;
    }

    public bool CanUseWhirlwind()
    {
        return weaponManager.IsTwoHandedWeaponEquipped();
    }
}

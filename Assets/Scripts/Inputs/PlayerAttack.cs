using System;
using System.Collections;
using System.Collections.Generic;
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
    private Skill[] hotbarSkills = new Skill[4]; // Stores skills assigned to 1-4 keys
    public bool isSkillActive = false;

    public Skill whirlwindSkill; 
    public Skill buffSkill;
    public Skill traversalSkill;
    public Skill lightningBallSkill; // Changed from aoeSpellSkill to lightningBallSkill
    
    [Header("References")]
    public Transform playerTransform;
    public WeaponManager weaponManager;
    public StaminaManager staminaManager;
    public PlayerMovement playerMovement;
    public Animator animator;
  
    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    public static PlayerAttack Instance;
    [Header("Buff Modifiers")]
    public float damageBonus = 1.0f; // Default is no bonus
    public float defenseBonus = 1.0f; // Default is no bonus

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

        // Assign the predefined skills to the 1-4 keys
        AssignSkill(0, whirlwindSkill);
        AssignSkill(1, buffSkill); // Key 2 for Buff Skill
        AssignSkill(2, traversalSkill); // Key 3 for Traversal Skill
        AssignSkill(3, lightningBallSkill); // Key 4 for Lightning Ball Skill (previously AOE Spell)
    }

    private void Update()
    {
        // Check for attacks and skills
        if (Input.GetMouseButtonDown(0) && CanAttack()) 
            TryAttack();

        if (Input.GetKeyDown(KeyCode.Space) && isAttacking) 
            CancelAttackForDodge();

        // Handle skill inputs
        CheckSkillInputs();

        // Detect key release to cancel whirlwind skill (assuming it's on the 1 key)
        if (Input.GetKeyUp(KeyCode.Alpha1) && whirlwindSkill != null && isSkillActive)
        {
            CancelWhirlwind();
        }
    }

    private void CheckSkillInputs()
    {
        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && hotbarSkills[i] != null)
            {
                ActivateSkill(hotbarSkills[i]);
            }
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
        if (!staminaManager.HasEnoughStamina(skill.staminaCost))
        {
           
            return;
        }

        if (!CanUseSkill(skill))
        {
            
            return;
        }

        // Deduct stamina and use skill
        staminaManager.UseStamina(skill.staminaCost);
        skill.UseSkill(this);

        // Start cooldown only for this skill
        StartCoroutine(SkillCooldown(skill));
    }


    private void CancelWhirlwind()
    {
        // Stop the whirlwind skill (coroutine) if it's active
        if (isSkillActive)
        {
            StopAllCoroutines(); // Stop any running coroutines including the whirlwind skill
            isSkillActive = false; // Set skill to inactive
            Debug.Log("Whirlwind skill has been canceled.");
        }
    }

    private bool CanUseSkill(Skill skill)
    {
        if (skill == whirlwindSkill && !CanUseWhirlwind()) // If Whirlwind skill, ensure the player has a two-handed weapon
        {
            Debug.Log("Cannot use Whirlwind. A two-handed weapon is required.");
            return false;
        }
        return true;
    }


    public IEnumerator SkillCooldown(Skill skill)
    {
        skill.isOnCooldown = true; // Track cooldown per skill
        yield return new WaitForSeconds(skill.cooldown);
        skill.isOnCooldown = false;
    }

    // Attack Handling
    bool CanAttack()
    {
        return Time.time >= nextAttackTime && !playerMovement.IsDodging && !playerMovement.IsRunning;
    }

    void TryAttack()
    {
        if (isAttacking) return;
        int attackIndex = shuffleAttacks ? UnityEngine.Random.Range(0, comboAttacks.Length) : 0;

        if (attackIndex >= comboStaminaCost.Length || !staminaManager.HasEnoughStamina(comboStaminaCost[attackIndex]))
        {
            Debug.Log("Not enough stamina");
            return;
        }

        PerformAttack(attackIndex);
    }

    void PerformAttack(int attackIndex)
    {
        Weapon currentWeapon = GetCurrentWeapon();
        if (currentWeapon == null)
        {
            Debug.Log("No weapon equipped!");
            return;
        }

        StartCoroutine(AttackSequence(attackIndex, currentWeapon));
    }

    IEnumerator AttackSequence(int attackIndex, Weapon weapon)
    {
        isAttacking = true;
        playerMovement.canMove = false;
        staminaManager.UseStamina(comboStaminaCost[attackIndex]);

        animator.Play(comboAttacks[attackIndex].name);

        yield return new WaitForSeconds(comboAttacks[attackIndex].length * 0.3f);

        // Apply damage bonus here before attacking
        float totalDamage = (comboDamage[attackIndex] * damageBonus) + weapon.weaponData.damageBonus;
        CheckWeaponHits((int)totalDamage);

        yield return new WaitForSeconds(comboAttacks[attackIndex].length * 0.3f);

        isAttacking = false;
        playerMovement.canMove = true;
        nextAttackTime = Time.time + attackCooldown;
    }

    void CheckWeaponHits(int damage)
    {
        Vector3 hitboxPosition = playerTransform.position + playerTransform.forward * attackRange * 0.5f;
        Vector3 hitboxSize = new Vector3(1.5f, 2.0f, attackRange);

        Collider[] hits = Physics.OverlapBox(hitboxPosition, hitboxSize / 2, playerTransform.rotation, weaponManager.enemyLayer);

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<EnemyHealth>(out EnemyHealth enemy))
            {
                Weapon currentWeapon = GetCurrentWeapon();
                // Apply the final damage calculation
                float finalDamage = damage + (currentWeapon?.weaponData.damageBonus ?? 0);
                enemy.TakeDamage(finalDamage, (enemy.transform.position - playerTransform.position).normalized);
            }
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
        {
            return weaponManager.equippedRightHandWeapon;
        }

        return weaponManager.equippedRightHandWeapon ?? weaponManager.equippedLeftHandWeapon;
    }

    public bool CanUseWhirlwind()
    {
        return weaponManager.IsTwoHandedWeaponEquipped();
    }
}
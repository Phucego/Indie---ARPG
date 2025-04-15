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
    public PlayerStats playerStats;
    public Animator animator;

    public static PlayerAttack Instance;

    [Header("Hover Detection")]
    public LayerMask enemyLayerMask;
    private GameObject lastHoveredEnemy = null;
    private EnemyUIManager lastEnemyUIManager;

    private float nextAttackTime = 0f;
    private bool isAttacking = false;

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

    private void Update()
    {
        HandleEnemyHover();
        CheckSkillInputs();
        HandleAttackInput();
    }

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
            else
            {
                Debug.LogWarning("EnemyUIManager not found on hovered enemy!");
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
        if (enemy != null)
        {
            Outline outline = enemy.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = show;
            }
            else
            {
                Debug.LogWarning("Outline component not found on enemy!");
            }
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

    private void HandleAttackInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
                BreakableProps breakable = hit.collider.GetComponent<BreakableProps>();

                if (enemy != null)
                {
                    HandleEnemyAttack(enemy);
                }
                else if (breakable != null)
                {
                    HandleBreakableAttack(breakable);
                }
            }
        }
    }

    private void HandleEnemyAttack(EnemyHealth enemy)
    {
        EnemyUIManager enemyUIManager = enemy.GetComponent<EnemyUIManager>();
        if (enemyUIManager != null)
        {
            lastEnemyUIManager = enemyUIManager;
            lastEnemyUIManager.UpdateEnemyTarget(enemy);
            lastEnemyUIManager.SetVisibility(true); // Show UI

            // Start a delay to hide UI after 3 seconds of being attacked
            StartCoroutine(HideEnemyUIAfterDelay(3f));
        }

        TryAttack(enemy.gameObject);  // Ensure that the attack method is called
    }

    private IEnumerator HideEnemyUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (lastEnemyUIManager != null)
        {
            lastEnemyUIManager.SetVisibility(false);
        }
    }

    private void HandleBreakableAttack(BreakableProps breakable)
    {
        if (breakable == null) return;

        // Check the distance to the breakable object
        float distanceToBreakable = Vector3.Distance(playerTransform.position, breakable.transform.position);

        // If the breakable object is too far, move towards it
        if (distanceToBreakable > attackRange)
        {
            StartCoroutine(MoveToTargetAndAttackBreakable(breakable)); // Move towards breakable and then attack
        }
        else
        {
            // Otherwise, attack immediately
            StartCoroutine(AttackBreakableSequence(breakable)); // Attack the breakable immediately
        }
    }

    private IEnumerator AttackBreakableSequence(BreakableProps breakable)
    {
        isAttacking = true;
        playerMovement.canMove = false;
        
        SnapRotateToTarget(breakable.transform);
        
        // Play the attack animation (you can modify this depending on which combo or attack is used)
        animator.Play(comboAttacks[0].name); // You can modify this to choose a specific attack animation for props
  
        // Wait for the attack to complete
        yield return new WaitForSeconds(comboAttacks[0].length); // Adjust timing if necessary

        // Apply damage or destroy the breakable prop
        breakable.DestroyObject(); // This handles the destruction and loot/exp drop logic

        // Reset states after attack
        isAttacking = false;
        playerMovement.canMove = true;
        nextAttackTime = Time.time + attackCooldown;
    }

    private IEnumerator MoveToTargetAndAttackBreakable(BreakableProps breakable)
    {
        if (breakable == null) yield break; // Exit early if breakable object is null

        // Move towards the breakable object
        playerMovement.MoveToTarget(breakable.transform.position); 

        // Wait until the player is in range to attack
        while (Vector3.Distance(playerTransform.position, breakable.transform.position) > attackRange)
        {
            yield return null; // Wait for the next frame
        }

        // Once in range, attempt attack
        StartCoroutine(AttackBreakableSequence(breakable)); // Attack the breakable once in range
    }

    private void TryAttack(GameObject target)
    {
        if (target == null || isAttacking || !CanAttack()) return;

        float distanceToTarget = Vector3.Distance(playerTransform.position, target.transform.position);

        if (distanceToTarget > attackRange)
        {
            StartCoroutine(MoveToTargetAndAttack(target)); // Move towards target and then attack
        }
        else
        {
            if (target.GetComponent<BreakableProps>() != null)
            {
                HandleBreakableAttack(target.GetComponent<BreakableProps>());
            }
            else
            {
                AttemptAttack(target); // For enemies
            }
        }
    }
    private IEnumerator MoveToTargetAndAttack(GameObject target)
    {
        if (target == null) yield break; // Exit early if target is null

        // Move towards target
        playerMovement.MoveToTarget(target.transform.position); 

        // Wait until the player is in range to attack
        while (Vector3.Distance(playerTransform.position, target.transform.position) > attackRange)
        {
            yield return null; // Wait for the next frame
        }

        // Once in range, attempt attack
        AttemptAttack(target);
    }


    private void AttemptAttack(GameObject target)
    {
        int attackIndex = shuffleAttacks ? UnityEngine.Random.Range(0, comboAttacks.Length) : 0;

        if (attackIndex >= comboStaminaCost.Length || !staminaManager.HasEnoughStamina(comboStaminaCost[attackIndex]))
        {
            Debug.Log("Not enough stamina");
            return;
        }

        PerformAttack(attackIndex, target);
    }

    private void PerformAttack(int attackIndex, GameObject target)
    {
        Weapon currentWeapon = GetCurrentWeapon();
        if (currentWeapon == null)
        {
            Debug.Log("No weapon equipped!");
            return;
        }

        StartCoroutine(AttackSequence(attackIndex, currentWeapon, target));
    }

    private IEnumerator AttackSequence(int attackIndex, Weapon weapon, GameObject target)
    {
        isAttacking = true;
        playerMovement.canMove = false;

        // Rotate to face target before attacking
   
        SnapRotateToTarget(target.transform);
        // Consume stamina
        staminaManager.UseStamina(comboStaminaCost[attackIndex]);

        // Play attack animation
        animator.Play(comboAttacks[attackIndex].name);

        // Wait for initial attack impact moment (adjust as needed)
        yield return new WaitForSeconds(comboAttacks[attackIndex].length * 0.3f);

        // Damage calculation
        float totalDamage = (comboDamage[attackIndex] * playerStats.damageBonus) + playerStats.attackPower;
        ApplyDamageToTarget(target, new float[] { totalDamage });

        // Wait for the rest of the animation to complete
        yield return new WaitForSeconds(comboAttacks[attackIndex].length * 0.7f);

        // Reset
        isAttacking = false;
        playerMovement.canMove = true;
        nextAttackTime = Time.time + attackCooldown;
    }

    private void FaceTarget(Transform target)
    {
        Vector3 direction = (target.position - playerTransform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, lookRotation, Time.deltaTime * 10f);
    }
    
    private void ApplyDamageToTarget(GameObject target, float[] damage)
    {
        if (target == null || damage.Length == 0) return;

        // Check if the target has an EnemyHealth component (enemy)
        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            foreach (float damageValue in damage)
            {
                enemyHealth.TakeDamage(damageValue); // Apply damage to enemy
            }
        }
        else
        {
            // If not an enemy, check if it's a breakable object and destroy it
            BreakableProps breakable = target.GetComponent<BreakableProps>();
            if (breakable != null)
            {
                breakable.DestroyObject(); // Destroy the object and apply loot/exp drop
            }
        }
    }

    public Weapon GetCurrentWeapon()
    {
        if (weaponManager == null)
        {
            Debug.LogError("WeaponManager is not assigned!");
            return null;
        }

        return weaponManager.GetCurrentWeapon(); // Ensure this method in WeaponManager is properly implemented
    }
    
    private void SnapRotateToTarget(Transform target)
    {
        Vector3 direction = (target.position - playerTransform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        playerTransform.rotation = lookRotation; // Snap rotation directly
    }
}


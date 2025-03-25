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
    [SerializeField] private AnimationClip whirlwindAttack;

    [Header("Attack Settings")]
    public bool shuffleAttacks = false;

    [Header("Damage Values")]
    public int[] comboDamage = new int[] { 10, 15, 20, 25 };
    public int whirlwindDamage = 5;

    [Header("Stamina")]
    public float[] comboStaminaCost = new float[] { 20f, 25f, 30f, 35f };
    public float dodgeStaminaCost = 25f;
    public float whirlwindStaminaCostPerTick = 15f;

    [Header("References")]
    public Transform playerTransform;
    public WeaponManager weaponManager;
    public GameObject whirlwindEffectPrefab;

    [Header("Layer Settings")]
    public LayerMask enemyLayer;
    public LayerMask breakablePropsLayer;

    private LayerMask targetLayers;
    private float nextAttackTime = 0f;
    private bool isAttacking = false;
    private bool isWhirlwinding = false;
    private Coroutine whirlwindCoroutine;
    
    [Header("Hit Effects")]
    private GameObject activeWhirlwindEffect;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject breakEffectPrefab;
    [SerializeField] private GameObject wallImpactPrefab;
    
    private StaminaManager staminaManager;
    private PlayerMovement playerMovement;
    private Animator animator;
    public static PlayerAttack Instance;

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
        targetLayers = enemyLayer | breakablePropsLayer;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && CanAttack())
        {
            TryAttack();
        }

        if (Input.GetKeyDown(KeyCode.Space) && isAttacking)
        {
            CancelAttackForDodge();
        }

        if (Input.GetMouseButton(1) && CanWhirlwind())
        {
            if (!isWhirlwinding)
            {
                StartWhirlwind();
            }
        }

        if (Input.GetMouseButtonUp(1) && isWhirlwinding)
        {
            StopWhirlwind();
        }
    }

    // Checks if the player can perform a normal attack
    bool CanAttack()
    {
        return Time.time >= nextAttackTime && !isWhirlwinding && !playerMovement.IsDodging && !playerMovement.IsRunning;
    }

    // Checks if the player can perform a whirlwind attack
    bool CanWhirlwind()
    {
        bool isTwoHandedWeapon = weaponManager.IsTwoHandedWeaponEquipped();
        bool isDualWielding = weaponManager.BothHandsOccupied();
        bool isWieldingOneHand = weaponManager.isWieldingOneHand;

        // Prevent Whirlwind if only one hand is wielding a weapon
        if (isWieldingOneHand)
        {
            Debug.Log("Whirlwind requires a two-handed weapon or dual-wielding.");
            return false;
        }

        // Check if the player has enough stamina
        if (!staminaManager.HasEnoughStamina(whirlwindStaminaCostPerTick))
        {
            Debug.Log("Not enough stamina for Whirlwind.");
            return false;
        }

        return isTwoHandedWeapon || isDualWielding;
    }



    // Attempts to execute an attack if conditions are met
    void TryAttack()
    {
        if (isAttacking) return;
        int attackIndex = shuffleAttacks ? UnityEngine.Random.Range(0, comboAttacks.Length) : 0;
        if (!staminaManager.HasEnoughStamina(comboStaminaCost[attackIndex]))
        {
            Debug.Log("Not enough stamina");
            return;
        }
        PerformAttack(attackIndex);
    }

    // Executes the selected attack sequence
    void PerformAttack(int attackIndex)
    {
        if (whirlwindCoroutine != null)
        {
            StopCoroutine(whirlwindCoroutine);
        }
        StartCoroutine(AttackSequence(attackIndex));
    }

    // Handles the attack animation and hitbox activation
    IEnumerator AttackSequence(int attackIndex)
    {
        isAttacking = true;
        playerMovement.canMove = false;
        staminaManager.UseStamina(comboStaminaCost[attackIndex]);
        animator.Play(comboAttacks[attackIndex].name);

        float animationLength = comboAttacks[attackIndex].length;
        float hitboxDelay = animationLength * 0.3f;
        float hitboxDuration = animationLength * 0.3f;

        yield return new WaitForSeconds(hitboxDelay);
        CheckWeaponHits(comboDamage[attackIndex]);
        yield return new WaitForSeconds(hitboxDuration);

        isAttacking = false;
        playerMovement.canMove = true;
        nextAttackTime = Time.time + attackCooldown;
    }

    // Starts the whirlwind attack
    void StartWhirlwind()
    {
        isWhirlwinding = true;
        playerMovement.canMove = true;
        whirlwindCoroutine = StartCoroutine(WhirlwindAttackLoop());

        if (whirlwindEffectPrefab != null)
        {
            Vector3 effectPosition = playerTransform.position + Vector3.up * 1.2f;
            activeWhirlwindEffect = Instantiate(whirlwindEffectPrefab, effectPosition, Quaternion.identity, playerTransform);
        }
    }

    // Continuously executes whirlwind attack while stamina is available
    IEnumerator WhirlwindAttackLoop()
    {
        while (isWhirlwinding && staminaManager.HasEnoughStamina(whirlwindStaminaCostPerTick))
        {
            staminaManager.UseStamina(whirlwindStaminaCostPerTick);
            animator.Play(whirlwindAttack.name);
            CheckWeaponHits(whirlwindDamage);
            yield return new WaitForSeconds(0.1f);
        }

        StopWhirlwind();
    }

    // Stops the whirlwind attack and restores movement if needed
    void StopWhirlwind()
    {
        isWhirlwinding = false;

        if (whirlwindCoroutine != null)
        {
            StopCoroutine(whirlwindCoroutine);
            whirlwindCoroutine = null;
        }

        bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || 
                        Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || 
                        Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.LeftArrow) || 
                        Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.RightArrow);

        if (isMoving)
        {
            playerMovement.canMove = true;
        }
        else
        {
            animator.Play("Idle");
        }

        if (activeWhirlwindEffect != null)
        {
            Destroy(activeWhirlwindEffect);
        }
    }

    // Cancels the attack if the player dodges
    void CancelAttackForDodge()
    {
        if (whirlwindCoroutine != null)
        {
            StopCoroutine(whirlwindCoroutine);
        }
        isAttacking = false;
        playerMovement.canMove = true;
        Debug.Log("Attack cancelled for dodge");
    }

    // Checks for enemy hits within the hitbox
    void CheckWeaponHits(int damage)
    {
        Vector3 hitboxPosition = playerTransform.position + playerTransform.forward * attackRange * 0.5f;
        Vector3 hitboxSize = new Vector3(1.5f, 2.0f, attackRange);

        Collider[] hits = Physics.OverlapBox(
            hitboxPosition,
            hitboxSize / 2,
            playerTransform.rotation,
            targetLayers
        );

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<EnemyHealth>(out EnemyHealth enemy))
            {
                CreateHitEffect(hit.transform.position);
                Vector3 hitDirection = (enemy.transform.position - playerTransform.position).normalized;
                enemy.TakeDamage(damage, hitDirection);
            }
            else if (hit.TryGetComponent<BreakableProps>(out BreakableProps breakableProps))
            {
                CreateBreakEffect(hit.transform.position);
                breakableProps.DestroyObject();
            }
            else
            {
                CreateWallImpact(hit.ClosestPoint(playerTransform.position));
            }
        }
    }

    void CreateHitEffect(Vector3 position)
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 0.5f); // Destroy after 0.5 seconds
        }
    }

    void CreateBreakEffect(Vector3 position)
    {
        if (breakEffectPrefab != null)
        {
            GameObject effect = Instantiate(breakEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 0.5f); // Destroy after 0.5 seconds
        }
    }

    void CreateWallImpact(Vector3 position)
    {
        if (wallImpactPrefab != null)
        {
            GameObject effect = Instantiate(wallImpactPrefab, position, Quaternion.identity);
            Destroy(effect, 0.5f); // Destroy after 0.5 seconds
        }
    }

}

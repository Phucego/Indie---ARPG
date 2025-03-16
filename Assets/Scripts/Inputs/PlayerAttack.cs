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
    
    [Header("Attack Settings")]
    public bool shuffleAttacks = false;
    
    [Header("Damage Values")]
    public int[] comboDamage = new int[] { 10, 15, 20, 25 };
    
    [Header("Stamina")]
    public float[] comboStaminaCost = new float[] { 20f, 25f, 30f, 35f };
    public float dodgeStaminaCost = 25f;
    
    [Header("References")]
    public Transform playerTransform;
    
    [Header("Layer Settings")]
    public LayerMask enemyLayer;
    public LayerMask breakablePropsLayer;
    
    private LayerMask targetLayers;
    private float nextAttackTime = 0f;
    private bool isAttacking = false;
    private Coroutine currentAttackCoroutine;
    
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
    }

    bool CanAttack()
    {
        return Time.time >= nextAttackTime && !playerMovement.IsBlocking && !playerMovement.IsDodging && !playerMovement.IsRunning;
    }

    void TryAttack()
    {
        if (isAttacking) return; // Prevent attack spam before the previous one finishes

        int attackIndex = shuffleAttacks ? UnityEngine.Random.Range(0, comboAttacks.Length) : 0;

        // Check stamina before performing attack
        if (!staminaManager.HasEnoughStamina(comboStaminaCost[attackIndex]))
        {
            Debug.Log("Not enough stamina");
            return;
        }
        
        PerformAttack(attackIndex);
    }

    void PerformAttack(int attackIndex)
    {
        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
        }
        
        currentAttackCoroutine = StartCoroutine(AttackSequence(attackIndex));
    }

    IEnumerator AttackSequence(int attackIndex)
    {
        isAttacking = true;
        playerMovement.canMove = false;
        
        // Deduct stamina here, ensuring it's only used when the attack actually starts
        staminaManager.UseStamina(comboStaminaCost[attackIndex]);

        animator.Play(comboAttacks[attackIndex].name);
        
        float animationLength = comboAttacks[attackIndex].length;
        float hitboxDelay = animationLength * 0.3f;
        float hitboxDuration = animationLength * 0.3f;
        
        yield return new WaitForSeconds(hitboxDelay);
        
        CheckWeaponHits(attackIndex);
        
        yield return new WaitForSeconds(hitboxDuration);
        
        isAttacking = false;
        playerMovement.canMove = true;
        nextAttackTime = Time.time + attackCooldown;
    }

    void CancelAttackForDodge()
    {
        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
        }
        isAttacking = false;
        playerMovement.canMove = true;
        Debug.Log("Attack cancelled for dodge");
    }

    void CheckWeaponHits(int attackIndex)
    {
        Vector3 hitboxPosition = playerTransform.position + playerTransform.forward * attackRange * 0.5f;
        Vector3 hitboxSize = new Vector3(1.5f, 1.5f, attackRange);

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
                // Calculate knockback direction
                Vector3 hitDirection = (enemy.transform.position - playerTransform.position).normalized;
                
                // Apply damage and knockback
                enemy.TakeDamage(comboDamage[attackIndex], hitDirection);
            }
            else if (hit.TryGetComponent<BreakableProps>(out BreakableProps breakableProps))
            {
                breakableProps.DestroyObject();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.red;
            Vector3 hitboxPosition = playerTransform.position + playerTransform.forward * attackRange * 0.5f;
            Vector3 hitboxSize = new Vector3(1.5f, 1.5f, attackRange);
            Gizmos.matrix = Matrix4x4.TRS(hitboxPosition, playerTransform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, hitboxSize);
        }
    }
}

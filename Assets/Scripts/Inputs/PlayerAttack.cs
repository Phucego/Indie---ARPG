using System;
using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Properties")]
    public float attackCooldown = 1.5f;
    public float comboWindowTime = 0.8f;
    public float attackRange = 2.0f;
    
    [Header("Combat Animations")]
    [SerializeField] private AnimationClip[] comboAttacks;
    
    [Header("Dodge Animations")]
    [SerializeField] private AnimationClip forwardDodge;
    [SerializeField] private AnimationClip backwardDodge;
    [SerializeField] private AnimationClip leftDodge;
    [SerializeField] private AnimationClip rightDodge;
    
    [Header("Damage Values")]
    public int[] comboDamage = new int[] { 10, 15, 20 };
    
    [Header("Stamina")]
    public float[] comboStaminaCost = new float[] { 20f, 25f, 30f };
    public float dodgeStaminaCost = 25f;
    
    [Header("References")]
    public Transform weaponHitbox;
    public BoxCollider weaponCollider;
    
    // Layer variables
    private LayerMask targetLayers;
    private const int ENEMY_LAYER = 6;
    private const int BREAKABLE_PROPS_LAYER = 7;
    private const int WEAPON_LAYER = 8;  // Change this to match your weapon layer
    private const int PLAYER_LAYER = 3;  // Change this to match your player layer
    
    // Private variables
    private float nextAttackTime = 0f;
    private int currentCombo = 0;
    private bool canContinueCombo = false;
    private bool isAttacking = false;
    private bool isDodging = false;
    private Coroutine currentAttackCoroutine;
    
    // Components
    public static PlayerAttack Instance;
    private StaminaManager staminaManager;
    private PlayerMovement playerMovement;
    private Animator animator;

    private LockOnSystem lockOnSystem;
    private void Awake()
    {
        Instance = this;
        animator = GetComponent<Animator>();
        weaponCollider.enabled = false;
        ValidateSetup();
    }

    private void ValidateSetup()
    {
        if (comboAttacks == null || comboAttacks.Length == 0)
        {
            Debug.LogError("No combo animations assigned!");
            enabled = false;
            return;
        }

        if (comboAttacks.Length != comboDamage.Length)
        {
            Debug.LogError("Number of combo animations doesn't match number of damage values!");
            enabled = false;
            return;
        }

        if (forwardDodge == null || backwardDodge == null || leftDodge == null || rightDodge == null)
        {
            Debug.LogError("Missing dodge animations! Please assign all directional dodge animations.");
            enabled = false;
            return;
        }

        // Validate weapon setup
        if (weaponHitbox == null)
        {
            Debug.LogError("Weapon hitbox is not assigned!");
            enabled = false;
            return;
        }

        if (weaponHitbox.gameObject.layer == -1 || weaponHitbox.gameObject.layer == 0)
        {
            Debug.LogError("Weapon hitbox layer is not set! Please set it to the Weapon layer.");
            enabled = false;
            return;
        }

        if (weaponCollider == null)
        {
            Debug.LogError("Weapon collider is not assigned!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        staminaManager = GetComponentInChildren<StaminaManager>();
        playerMovement = GetComponentInChildren<PlayerMovement>();
        
        lockOnSystem = GetComponent<LockOnSystem>();
        // Set up the layer mask to include only the layers we want to hit
        targetLayers = (1 << ENEMY_LAYER) | (1 << BREAKABLE_PROPS_LAYER);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !playerMovement.isBlocking && !isDodging)
        {
            TryAttack();
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isDodging)
        {
            TryDodge();
        }
    }

    void TryDodge()
    {
        if (!staminaManager.HasEnoughStamina(dodgeStaminaCost))
        {
            Debug.Log("Not enough stamina to dodge!");
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        if (isAttacking)
        {
            CancelAttack();
        }

        if (Mathf.Abs(vertical) > Mathf.Abs(horizontal))
        {
            if (vertical > 0)
            {
                StartCoroutine(PerformDodge(forwardDodge));
            }
            else
            {
                StartCoroutine(PerformDodge(backwardDodge));
            }
        }
        else if (Mathf.Abs(horizontal) > 0)
        {
            if (horizontal > 0)
            {
                StartCoroutine(PerformDodge(rightDodge));
            }
            else
            {
                StartCoroutine(PerformDodge(leftDodge));
            }
        }
        else
        {
            StartCoroutine(PerformDodge(backwardDodge));
        }
    }

    IEnumerator PerformDodge(AnimationClip dodgeAnimation)
    {
        isDodging = true;
        staminaManager.UseStamina(dodgeStaminaCost);
        
        animator.Play(dodgeAnimation.name);
        
        yield return new WaitForSeconds(dodgeAnimation.length);
        
        isDodging = false;
    }

    void TryAttack()
    {
        if (Time.time < nextAttackTime || !staminaManager.HasEnoughStamina(comboStaminaCost[currentCombo]))
        {
            Debug.Log("Cannot attack: Cooldown or insufficient stamina");
            return;
        }

        if (!canContinueCombo)
        {
            StartCombo();
        }
        else if (currentCombo < comboAttacks.Length - 1)
        {
            ContinueCombo();
        }
    }

    void StartCombo()
    {
        currentCombo = 0;
        PerformAttack();
    }

    void ContinueCombo()
    {
        currentCombo++;
        PerformAttack();
    }

    void PerformAttack()
    {
        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
        }
        
        currentAttackCoroutine = StartCoroutine(AttackSequence());
    }

    IEnumerator AttackSequence()
    {
        isAttacking = true;
        staminaManager.UseStamina(comboStaminaCost[currentCombo]);

        animator.Play(comboAttacks[currentCombo].name);
        
        float animationLength = comboAttacks[currentCombo].length;
        float hitboxDelay = animationLength * 0.3f;
        float hitboxDuration = animationLength * 0.3f;
        
        yield return new WaitForSeconds(hitboxDelay);
        
        weaponCollider.enabled = true;
        CheckWeaponHits();
        
        yield return new WaitForSeconds(hitboxDuration);
        
        weaponCollider.enabled = false;
        
        canContinueCombo = true;
        
        yield return new WaitForSeconds(comboWindowTime);
        
        if (canContinueCombo)
        {
            EndCombo();
        }
        
        isAttacking = false;
    }

    void CheckWeaponHits()
    {
        Collider[] hits = Physics.OverlapBox(
            weaponHitbox.position,
            weaponHitbox.localScale / 2,
            weaponHitbox.rotation,
            targetLayers  // Only check for collisions with these layers
        );

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<EnemyHealth>(out EnemyHealth enemy))
            {
                enemy.TakeDamage(comboDamage[currentCombo]);
            }
            else if (hit.TryGetComponent<BreakableProps>(out BreakableProps breakableProps))
            {
                breakableProps.DestroyObject();
            }
        }
    }

    void CancelAttack()
    {
        if (isAttacking)
        {
            if (currentAttackCoroutine != null)
            {
                StopCoroutine(currentAttackCoroutine);
            }
            
            weaponCollider.enabled = false;
            EndCombo();
            isAttacking = false;
        }
    }

    void EndCombo()
    {
        currentCombo = 0;
        canContinueCombo = false;
        nextAttackTime = Time.time + attackCooldown;
    }

    private void OnDrawGizmos()
    {
        if (weaponHitbox != null && weaponCollider.enabled)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(
                weaponHitbox.position,
                weaponHitbox.rotation,
                weaponHitbox.localScale
            );
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
    
    
}
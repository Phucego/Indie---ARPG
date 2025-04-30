using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class EnemyController : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float attackRadius = 2f;
    [SerializeField] private float attackDamage = 10f; // Damage dealt to player on attack
    [SerializeField] private float attackInterval = 0.3f; // Time between attacks in seconds

    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 3.5f;
    [SerializeField] private float lookSpeed = 5f;
    [SerializeField] private float stoppingDistance = 0.5f;

    [Header("Animation Settings")]
    public AnimationClip idleClip; // Added for idle animation
    public AnimationClip runningClip;
    public AnimationClip attackClip;

    private NavMeshAgent agent;
    private EnemyHealth health;
    private Rigidbody rb;
    private Animator animator;
    private AnimatorOverrideController overrideController;
    private bool isStaggered = false;
    private bool isBlinded = false;
    private bool isPaused = false; // Track pause state
    private Coroutine attackCoroutine; // Reference to the attack coroutine
    private readonly string tempStateName = "TempState"; // Temporary state for clip playback

    public bool IsStaggered => isStaggered;
    public bool IsBlinded => isBlinded;
    public bool IsPaused => isPaused;
    public bool IsPlayerInDetectionRange =>
        !isBlinded && !isPaused && PlayerMovement.Instance != null &&
        Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= detectionRadius;
    public bool IsPlayerInAttackRange =>
        !isBlinded && !isPaused && PlayerMovement.Instance != null &&
        Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= attackRadius;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (agent == null || health == null || rb == null || animator == null)
        {
            Debug.LogError($"[EnemyController] Missing required component on {gameObject.name}!", this);
            return;
        }

        agent.speed = movementSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.angularSpeed = lookSpeed * 60f;
        agent.autoBraking = true;
        agent.acceleration = 8f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        // Configure Rigidbody to prevent knockback
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.useGravity = false;
        rb.mass = 100f;

        GetComponent<Outline>().enabled = false;
        
        // Set up AnimatorOverrideController
        if (animator.runtimeAnimatorController != null)
        {
            overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = overrideController;
        }
    }

    private void Start()
    {
        if (PlayerMovement.Instance == null)
        {
            Debug.LogWarning("[EnemyController] PlayerMovement.Instance is null!", this);
            return;
        }
        agent.updateRotation = true;
        agent.updatePosition = true;

        // Validate Animator Controller setup
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError($"[EnemyController] No Animator Controller assigned to {gameObject.name}!", this);
            return;
        }

        // Validate animation clips
        if (idleClip == null || runningClip == null || attackClip == null)
        {
            Debug.LogWarning($"[EnemyController] One or more animation clips (idleClip, runningClip, attackClip) not assigned on {gameObject.name}!", this);
        }
    }

    private void Update()
    {
        if (health.IsDead || isStaggered || isBlinded || isPaused || PlayerMovement.Instance == null) 
        {
            StopAttacking();
            PlayAnimation(idleClip);
            return;
        }

        if (IsPlayerInAttackRange)
        {
            SnapRotateToPlayer();
            AttackPlayer();
            PlayAnimation(attackClip);
        }
        else
        {
            StopAttacking();
            if (IsPlayerInDetectionRange)
            {
                SmoothLookAt(PlayerMovement.Instance.transform);
                MoveTowardsPlayer();
                PlayAnimation(runningClip);
            }
            else
            {
                Idle();
                PlayAnimation(idleClip);
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        if (agent != null && PlayerMovement.Instance != null)
        {
            Vector3 targetPos = PlayerMovement.Instance.transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPos, out hit, 5f, NavMesh.AllAreas))
            {
                targetPos = hit.position;
            }

            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(targetPos, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                agent.SetDestination(targetPos);
                agent.isStopped = false;
            }
            else
            {
                agent.isStopped = true;
            }
        }
    }

    private void AttackPlayer()
    {
        if (agent != null)
        {
            agent.isStopped = true;
        }

        if (attackCoroutine == null)
        {
            attackCoroutine = StartCoroutine(AttackCoroutine());
        }
    }

    private void StopAttacking()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    private IEnumerator AttackCoroutine()
    {
        while (true)
        {
            if (PlayerMovement.Instance != null)
            {
                PlayerHealth playerHealth = PlayerMovement.Instance.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                    Debug.Log($"[EnemyController] Dealt {attackDamage} damage to player!");
                }
            }
            yield return new WaitForSeconds(attackInterval);
        }
    }

    private void Idle()
    {
        if (agent != null)
        {
            agent.isStopped = true;
        }
    }

    public void Stagger(float duration)
    {
        if (!isStaggered)
        {
            StartCoroutine(StaggerCoroutine(duration));
        }
    }

    public void Blind(float duration)
    {
        if (!isBlinded)
        {
            StartCoroutine(BlindCoroutine(duration));
        }
    }

    public IEnumerator PauseForDuration(float duration, AnimationClip animationClip)
    {
        isPaused = true;
        Vector3 savedDestination = agent.destination;
        bool wasStopped = agent.isStopped;
        if (agent != null)
        {
            agent.isStopped = true;
        }
        PlayAnimation(animationClip);
        yield return new WaitForSeconds(duration);
        isPaused = false;
        if (agent != null && !health.IsDead)
        {
            agent.isStopped = wasStopped;
            agent.SetDestination(savedDestination);
        }
    }

    private IEnumerator StaggerCoroutine(float duration)
    {
        isStaggered = true;
        StopAttacking();
        PlayAnimation(idleClip);
        yield return new WaitForSeconds(duration);
        isStaggered = false;
    }

    private IEnumerator BlindCoroutine(float duration)
    {
        isBlinded = true;
        StopAttacking();
        PlayAnimation(idleClip);
        Debug.Log($"[EnemyController] {gameObject.name} is blinded for {duration} seconds.");
        yield return new WaitForSeconds(duration);
        isBlinded = false;
    }

    private void SnapRotateToPlayer()
    {
        if (PlayerMovement.Instance == null) return;

        Vector3 direction = (PlayerMovement.Instance.transform.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation;
        }
    }

    private void SmoothLookAt(Transform target)
    {
        Vector3 direction = (target.position - target.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookSpeed);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == PlayerMovement.Instance?.gameObject)
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"[EnemyController] Dealt {attackDamage} damage to player on collision!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

    private void PlayAnimation(AnimationClip clip)
    {
        if (animator == null || overrideController == null || clip == null)
        {
            Debug.LogWarning($"[EnemyController] Cannot play animation: Animator, OverrideController, or clip is null on {gameObject.name}!", this);
            return;
        }

        // Override the temporary state with the provided clip
        overrideController[tempStateName] = clip;

        // Play the temporary state
        animator.Play(Animator.StringToHash(tempStateName), 0);
    }
}
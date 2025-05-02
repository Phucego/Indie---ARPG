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
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackInterval = 0.3f;

    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 3.5f;
    [SerializeField] private float lookSpeed = 5f;
    [SerializeField] private float stoppingDistance = 0.5f;

    [Header("Animation Settings")]
    public AnimationClip idleClip;
    public AnimationClip runningClip;
    public AnimationClip attackClip;
    private AnimationClip currentAnimation;

    private NavMeshAgent agent;
    private EnemyHealth health;
    private Rigidbody rb;
    private Animator animator;
    private bool isStaggered = false;
    private bool isBlinded = false;
    private bool isPaused = false;
    private bool isAttacking = false;

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
            return;
        }

        agent.speed = movementSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.angularSpeed = lookSpeed * 60f;
        agent.autoBraking = true;
        agent.acceleration = 8f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.useGravity = false;
        rb.mass = 100f;

        GetComponent<Outline>().enabled = false;
    }

    private void Start()
    {
        if (PlayerMovement.Instance == null)
        {
            return;
        }
        agent.updateRotation = true;
        agent.updatePosition = true;

        if (animator.runtimeAnimatorController == null)
        {
            return;
        }

        if (idleClip == null || runningClip == null || attackClip == null)
        {
            return;
        }
        else
        {
            ChangeAnimation(idleClip);
        }
    }

    private void Update()
    {
        if (health.IsDead || isStaggered || isBlinded || isPaused || PlayerMovement.Instance == null)
        {
            StopAttacking();
            if (!isAttacking)
            {
                ChangeAnimation(idleClip);
            }
            return;
        }

        if (IsPlayerInAttackRange && !isAttacking)
        {
            SnapRotateToPlayer();
            AttackPlayer();
        }
        else if (!isAttacking)
        {
            StopAttacking();
            if (IsPlayerInDetectionRange)
            {
                SmoothLookAt(PlayerMovement.Instance.transform);
                MoveTowardsPlayer();
                ChangeAnimation(runningClip);
            }
            else
            {
                Idle();
                ChangeAnimation(idleClip);
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

        if (!isAttacking)
        {
            isAttacking = true;
            ChangeAnimation(attackClip);

            // Randomly adjust the attack position within the attack range area
            Vector3 randomAttackPosition = GetRandomAttackPosition();
            StartCoroutine(AttackSequence(randomAttackPosition));
        }
    }

    private Vector3 GetRandomAttackPosition()
    {
        // Get a random point within the attack range (circle around the player)
        Vector3 randomDirection = Random.insideUnitSphere * attackRadius;
        randomDirection.y = 0;  // Ensure attack is horizontal

        // Adjust the attack position to be around the player
        Vector3 attackPosition = PlayerMovement.Instance.transform.position + randomDirection;

        return attackPosition;
    }

    private IEnumerator AttackSequence(Vector3 attackPosition)
    {
        yield return new WaitForSeconds(attackClip.length);

        // After attack, check if the player is within the new attack area
        if (Vector3.Distance(transform.position, attackPosition) <= attackRadius)
        {
            DealDamageToPlayer();
        }

        isAttacking = false;
    }

    private void StopAttacking()
    {
        isAttacking = false;
    }

    private void DealDamageToPlayer()
    {
        if (PlayerMovement.Instance != null && IsPlayerInAttackRange)
        {
            PlayerHealth playerHealth = PlayerMovement.Instance.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player") && isAttacking && IsPlayerInAttackRange)
        {
            DealDamageToPlayer();
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
        ChangeAnimation(animationClip);
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
        ChangeAnimation(idleClip);
        yield return new WaitForSeconds(duration);
        isStaggered = false;
    }

    private IEnumerator BlindCoroutine(float duration)
    {
        isBlinded = true;
        StopAttacking();
        ChangeAnimation(idleClip);
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
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookSpeed);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

    private void ChangeAnimation(AnimationClip animationClip, float crossfade = 0.02f)
    {
        if (animationClip == null)
        {
            return;
        }

        if (currentAnimation != animationClip)
        {
            currentAnimation = animationClip;
            animator.CrossFade(animationClip.name, crossfade);
        }
    }
}

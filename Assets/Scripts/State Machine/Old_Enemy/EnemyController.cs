using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(Rigidbody))]
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

    private NavMeshAgent agent;
    private EnemyHealth health;
    private Rigidbody rb;
    private bool isStaggered = false;
    private bool isBlinded = false;
    private Coroutine attackCoroutine; // Reference to the attack coroutine

    public bool IsStaggered => isStaggered;
    public bool IsBlinded => isBlinded;
    public bool IsPlayerInDetectionRange =>
        !isBlinded && PlayerMovement.Instance != null &&
        Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= detectionRadius;
    public bool IsPlayerInAttackRange =>
        !isBlinded && PlayerMovement.Instance != null &&
        Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= attackRadius;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody>();

        if (agent == null || health == null || rb == null)
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

        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        rb.mass = 100f;
        rb.useGravity = true;

        GetComponent<Outline>().enabled = false;
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
    }

    private void Update()
    {
        if (isStaggered || isBlinded || PlayerMovement.Instance == null) 
        {
            // Stop attacking if staggered, blinded, or no player
            StopAttacking();
            return;
        }

        if (IsPlayerInDetectionRange)
        {
            SmoothLookAt(PlayerMovement.Instance.transform);
        }

        if (IsPlayerInAttackRange)
        {
            SnapRotateToPlayer();
            AttackPlayer();
        }
        else
        {
            StopAttacking();
            if (IsPlayerInDetectionRange)
            {
                MoveTowardsPlayer();
            }
            else
            {
                Idle();
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
                rb.isKinematic = true;
            }
            else
            {
                agent.isStopped = true;
                rb.isKinematic = false;
            }
        }
    }

    private void AttackPlayer()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            rb.velocity = Vector3.zero;
        }

        // Start attacking if not already attacking
        if (attackCoroutine == null)
        {
            attackCoroutine = StartCoroutine(AttackCoroutine());
        }
    }

    private void StopAttacking()
    {
        // Stop the attack coroutine if it's running
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
            // Deal damage to the player
            if (PlayerMovement.Instance != null)
            {
                PlayerHealth playerHealth = PlayerMovement.Instance.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                    Debug.Log($"[EnemyController] Dealt {attackDamage} damage to player!");
                }
            }

            // Wait for the attack interval
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

    private IEnumerator StaggerCoroutine(float duration)
    {
        isStaggered = true;
        if (agent != null) agent.isStopped = true;
        StopAttacking(); // Stop attacking when staggered
        yield return new WaitForSeconds(duration);
        isStaggered = false;
    }

    private IEnumerator BlindCoroutine(float duration)
    {
        isBlinded = true;
        if (agent != null) agent.isStopped = true;
        StopAttacking(); // Stop attacking when blinded
        Debug.Log($"[EnemyController] {gameObject.name} is blinded for {duration} seconds.");
        // TODO: Play blind VFX or disoriented animation
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == PlayerMovement.Instance?.gameObject)
        {
            rb.velocity = Vector3.zero;
            // Optional: Deal damage on collision as well
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
}
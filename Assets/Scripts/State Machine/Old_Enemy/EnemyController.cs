using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float attackRadius = 2f;
    [SerializeField] private float pathUpdateInterval = 0.5f; // Interval for updating path

    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 3.5f;
    [SerializeField] private float lookSpeed = 5f; // Speed at which the enemy rotates toward the player
    [SerializeField] private float stoppingDistance = 1.5f; // Distance to stop from player

    private NavMeshAgent agent;
    private EnemyHealth health;
    private Rigidbody rb;
    private bool isStaggered = false;
    private float lastPathUpdateTime;

    public bool IsStaggered => isStaggered;
    public bool IsPlayerInDetectionRange =>
        PlayerMovement.Instance != null &&
        Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= detectionRadius;
    public bool IsPlayerInAttackRange =>
        PlayerMovement.Instance != null &&
        Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= attackRadius;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody>();

        if (agent == null)
        {
            Debug.LogError("[EnemyController] No NavMeshAgent found!");
            return;
        }

        if (health == null)
        {
            Debug.LogError("[EnemyController] No EnemyHealth component found!");
            return;
        }

        if (rb == null)
        {
            Debug.LogError("[EnemyController] No Rigidbody found!");
            return;
        }

        // Configure NavMeshAgent
        agent.speed = movementSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.angularSpeed = lookSpeed * 60f; // Convert to degrees per second
        agent.autoBraking = true;
        agent.acceleration = 8f; // Smooth acceleration
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        // Configure Rigidbody to prevent pushback
        rb.isKinematic = false; // Allow physics for collision response
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY; // Lock Y position and rotations
        rb.mass = 100f; // High mass to resist player pushback
        rb.useGravity = true;

        // Disable outline at start
        GetComponent<Outline>().enabled = false;
    }

    private void Start()
    {
        // Ensure agent updates rotation but not position via physics
        agent.updateRotation = true;
        agent.updatePosition = true;
    }

    private void Update()
    {
        if (isStaggered || PlayerMovement.Instance == null) return;

        // Continuously look at the player if in detection range
        if (IsPlayerInDetectionRange)
        {
            SmoothLookAt(PlayerMovement.Instance.transform);
        }

        if (IsPlayerInAttackRange)
        {
            SnapRotateToPlayer(); // Snap rotation for quick targeting
            AttackPlayer();
        }
        else if (IsPlayerInDetectionRange && Time.time - lastPathUpdateTime > pathUpdateInterval)
        {
            MoveTowardsPlayer();
            lastPathUpdateTime = Time.time;
        }
        else
        {
            Idle();
        }
    }

    private void MoveTowardsPlayer()
    {
        if (agent != null && PlayerMovement.Instance != null)
        {
            // Check if path is valid
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(PlayerMovement.Instance.transform.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                agent.SetDestination(PlayerMovement.Instance.transform.position);
                agent.isStopped = false;
            }
            else
            {
                // Path is blocked, stop moving
                agent.isStopped = true;
                Debug.Log("[EnemyController] Path to player is blocked.");
            }
        }
    }

    private void AttackPlayer()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            // Lock position to prevent pushback during attack
            rb.velocity = Vector3.zero;
        }

        Debug.Log("Attacking the player!");
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

    private IEnumerator StaggerCoroutine(float duration)
    {
        isStaggered = true;
        if (agent != null) agent.isStopped = true;
        yield return new WaitForSeconds(duration);
        isStaggered = false;
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
        direction.y = 0; // Keep enemy upright

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookSpeed);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Prevent player from pushing the enemy
        if (collision.gameObject == PlayerMovement.Instance?.gameObject)
        {
            rb.velocity = Vector3.zero; // Reset velocity to prevent pushback
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
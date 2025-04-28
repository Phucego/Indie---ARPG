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

    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 3.5f;
    [SerializeField] private float lookSpeed = 5f;
    [SerializeField] private float stoppingDistance = 0.5f;

    private NavMeshAgent agent;
    private EnemyHealth health;
    private Rigidbody rb;
    private bool isStaggered = false;

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

        if (agent == null || health == null || rb == null)
        {
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
            return;
        }
        agent.updateRotation = true;
        agent.updatePosition = true;
    }

    private void Update()
    {
        if (isStaggered || PlayerMovement.Instance == null) return;

        if (IsPlayerInDetectionRange)
        {
            SmoothLookAt(PlayerMovement.Instance.transform);
        }

        if (IsPlayerInAttackRange)
        {
            SnapRotateToPlayer();
            AttackPlayer();
        }
        else if (IsPlayerInDetectionRange)
        {
            MoveTowardsPlayer();
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
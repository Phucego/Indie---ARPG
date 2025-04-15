using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Outline))]
public class EnemyController : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float attackRadius = 2f;

    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 3.5f;
    [SerializeField] private float lookSpeed = 5f; // Speed at which the enemy rotates toward the player

    private NavMeshAgent agent;
    private EnemyHealth health;
    private bool isStaggered = false;

    public bool IsStaggered => isStaggered;
    public bool IsPlayerInDetectionRange =>
        PlayerMovement.Instance != null &&
        Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= detectionRadius;
    public bool IsPlayerInAttackRange =>
        PlayerMovement.Instance != null &&
        Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= attackRadius;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<EnemyHealth>();

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

        agent.speed = movementSpeed;

        // Disable outline at start
        GetComponent<Outline>().enabled = false;
    }

    private void Update()
    {
        if (isStaggered) return;

        if (PlayerMovement.Instance == null) return;

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
            agent.SetDestination(PlayerMovement.Instance.transform.position);
            agent.isStopped = false;
        }
    }

    private void AttackPlayer()
    {
        if (agent != null)
        {
            agent.isStopped = true;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}

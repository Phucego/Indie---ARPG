using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float attackRadius = 2f;
    [SerializeField] private float fleeHealthThreshold = 20f;

    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 3.5f;
    [SerializeField] private float fleeSpeed = 5f;

    private BaseEnemyState currentState;
    private BaseEnemyState previousState;
    private NavMeshAgent agent;
    private EnemyHealth health;
    private bool isStaggered = false;
    private bool canMove = true;

    public bool IsStaggered => isStaggered;
    public bool IsPlayerInDetectionRange =>
        PlayerMovement.Instance != null &&
        Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= detectionRadius;
    public bool IsPlayerInAttackRange =>
        PlayerMovement.Instance != null &&
        Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= attackRadius;
    public bool ShouldFlee => health != null && health.GetCurrentHealth() <= fleeHealthThreshold;
    public bool IsKnockedBack => isStaggered;
    public BaseEnemyState GetCurrentState() => currentState;
    public NavMeshAgent Agent => agent;

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
        ChangeState(new EnemyIdleState(this));
    }

    private void Update()
    {
        if (canMove && !isStaggered)
        {
            currentState?.Execute();
        }
    }

    public void ChangeState(BaseEnemyState newState)
    {
        if (newState == null || currentState == newState) return;

        previousState = currentState;
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void SetKnockbackState(bool isKnockedBack)
    {
        isStaggered = isKnockedBack;
        canMove = !isKnockedBack;

        if (agent != null)
        {
            agent.isStopped = isKnockedBack;
        }

        if (!isKnockedBack)
        {
            // After recovery, choose the appropriate state
            if (ShouldFlee)
            {
                ChangeState(new EnemyFleeState(this));
            }
            else if (IsPlayerInAttackRange)
            {
                ChangeState(new EnemyAttackState(this));
            }
            else if (IsPlayerInDetectionRange)
            {
                ChangeState(new EnemyChaseState(this));
            }
            else
            {
                ChangeState(new EnemyIdleState(this));
            }
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
        SetKnockbackState(true);
        yield return new WaitForSeconds(duration);
        SetKnockbackState(false);
    }

    public void Move(Vector3 direction)
    {
        if (agent != null && agent.enabled)
        {
            agent.SetDestination(transform.position + direction * movementSpeed * Time.deltaTime);
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

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float attackRadius = 2f;
    [SerializeField] private float fleeHealthThreshold = 20f; // HP % to trigger fleeing

    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 3.5f;
    [SerializeField] private float fleeSpeed = 5f;

    private BaseEnemyState currentState;
    private NavMeshAgent agent;
    private EnemyHealth health;
    private EnemyUIManager uiManager; // UI Manager reference
    private bool isStaggered = false;

    public bool IsStaggered => isStaggered;
    public float CurrentHealth => health != null ? health.GetCurrentHealth() : 0;
    public bool ShouldFlee => CurrentHealth <= fleeHealthThreshold;
    public NavMeshAgent Agent => agent; // Allow states to access NavMeshAgent

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<EnemyHealth>();
        uiManager = FindObjectOfType<EnemyUIManager>(); // Find UI manager in scene

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
        if (!isStaggered)
        {
            currentState?.Execute();
        }
    }

    public void ChangeState(BaseEnemyState newState)
    {
        if (currentState != null) currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public bool IsPlayerInDetectionRange()
    {
        return PlayerMovement.Instance != null && 
               Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= detectionRadius;
    }

    public bool IsPlayerInAttackRange()
    {
        return PlayerMovement.Instance != null && 
               Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= attackRadius;
    }

    public void FleeFromPlayer()
    {
        if (PlayerMovement.Instance == null) return;

        Vector3 fleeDirection = (transform.position - PlayerMovement.Instance.transform.position).normalized;
        Vector3 fleePosition = transform.position + fleeDirection * fleeSpeed;

        if (agent.enabled && !isStaggered)
        {
            agent.SetDestination(fleePosition);
        }
    }

    public void ApplyKnockback(Vector3 hitDirection)
    {
        if (isStaggered) return;

        isStaggered = true;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && health != null)
        {
            rb.velocity = Vector3.zero;
            Vector3 knockbackVector = new Vector3(hitDirection.x, 0.2f, hitDirection.z).normalized * health.GetKnockbackForce();
            rb.AddForce(knockbackVector, ForceMode.VelocityChange);
        }

        StartCoroutine(ResumeAfterKnockback(health.GetStaggerDuration()));
    }


    private IEnumerator ResumeAfterKnockback(float delay)
    {
        yield return new WaitForSeconds(delay);
        isStaggered = false;
        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
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
        if (agent != null) agent.isStopped = false;
    }

    public void NotifyUIOnDeath()
    {
        if (uiManager != null)
        {
            uiManager.HideEnemyHealthBar();
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

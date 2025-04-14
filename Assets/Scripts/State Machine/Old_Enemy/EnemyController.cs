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
        if (isStaggered) return;  // If the enemy is staggered, stop all actions.

        // Check if the player is within attack range
        if (IsPlayerInAttackRange)
        {
            AttackPlayer();
        }
        else if (IsPlayerInDetectionRange)
        {
            // Move towards the player if within detection range but not attack range
            MoveTowardsPlayer();
        }
        else
        {
            // If player is out of detection range, stop the enemy
            Idle();
        }
    }

    private void MoveTowardsPlayer()
    {
        if (agent != null && PlayerMovement.Instance != null)
        {
            agent.SetDestination(PlayerMovement.Instance.transform.position);  // Move towards the player
            agent.isStopped = false;  // Make sure the agent is moving
        }
    }

    private void AttackPlayer()
    {
        if (agent != null)
        {
            agent.isStopped = true;  // Stop the movement when in attack range
        }

        // Example of attacking logic - you can modify based on your attack implementation
        // For example, trigger attack animations, apply damage, etc.
        Debug.Log("Attacking the player!");

        // Add your custom attack logic here (e.g., play attack animation, apply damage)
    }

    private void Idle()
    {
        // Stop moving if not in range of the player
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
        if (agent != null) agent.isStopped = true;  // Stop movement during stagger
        yield return new WaitForSeconds(duration);
        isStaggered = false;  // Restore staggered state after the duration
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);  // Show detection range

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);  // Show attack range
    }
}

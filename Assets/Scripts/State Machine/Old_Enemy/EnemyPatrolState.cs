using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolState : BaseEnemyState
{
    private float patrolRadius = 7f;
    private float movementSpeed = 2f;
    private NavMeshAgent agent;
    private EnemyController enemyController;
    
    public EnemyPatrolState(EnemyController controller) : base(controller)
    {
        enemyController = controller; //Fix: assign the controller
        agent = enemyController.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        if (agent == null)
        {
            Debug.LogError("[EnemyPatrolState] No NavMeshAgent found!");
            return;
        }

        agent.speed = movementSpeed;
        SetRandomPatrolPoint();
    }

    public override void Execute()
    {
        // Stop movement if staggered
        if (enemyController.IsStaggered)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;

        // If the enemy reaches its patrol destination, pick a new one
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            SetRandomPatrolPoint();
        }

        // Check for player detection
        if (enemyController.IsPlayerInDetectionRange)
        {
            enemyController.ChangeState(new EnemyChaseState(enemyController));
        }
    }

    public override void Exit()
    {
        agent.isStopped = true;
    }

    private void SetRandomPatrolPoint()
    {
        int attempts = 5;
        for (int i = 0; i < attempts; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius + enemyController.transform.position;
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, patrolRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(navHit.position);
                return;
            }
        }

        Debug.LogWarning("[EnemyPatrolState] Failed to find valid NavMesh point after multiple attempts.");
    }


    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            SetRandomPatrolPoint();
        }
    }
}
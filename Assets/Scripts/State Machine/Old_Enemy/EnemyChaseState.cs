using UnityEngine;
using UnityEngine.AI;

public class EnemyChaseState : BaseEnemyState
{
    private NavMeshAgent agent;
    private Transform playerTransform;

    public EnemyChaseState(EnemyController enemy) : base(enemy)
    {
        agent = enemy.GetComponent<NavMeshAgent>();
        playerTransform = PlayerMovement.Instance.transform; // Assuming PlayerMovement is Singleton
    }

    public override void Enter()
    {
        Debug.Log("[AI] Entering Chase State");

        if (agent == null)
        {
            Debug.LogError("[AI] No NavMeshAgent found on enemy!");
            return;
        }

        agent.isStopped = false; // Ensure movement is enabled
        agent.speed = 4f; // Set chase speed (adjust as needed)
    }

    public override void Execute()
    {
        if (enemy.IsStaggered) 
        {
            agent.isStopped = true; // Stop movement if staggered
            return;
        }

        agent.isStopped = false; 

        if (enemy.IsPlayerInAttackRange)
        {
            enemy.ChangeState(new EnemyAttackState(enemy));
        }
        else if (enemy.ShouldFlee)
        {
            enemy.ChangeState(new EnemyFleeState(enemy));
        }
        else
        {
            ChasePlayer();
        }
    }

    public override void Exit()
    {
        Debug.Log("[AI] Exiting Chase State");
        agent.isStopped = true; // Stop moving when exiting chase state
    }

    private void ChasePlayer()
    {
        if (playerTransform != null && agent.enabled)
        {
            agent.SetDestination(playerTransform.position);
        }
    }
}
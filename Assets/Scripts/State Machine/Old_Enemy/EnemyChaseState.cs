using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseState : BaseEnemyState
{
    private float chaseSpeed = 4f;
    private float maxChaseTime = 10f;
    private float chaseTimer;

    public EnemyChaseState(EnemyController controller) : base(controller) { }

    public override void Enter()
    {
        chaseTimer = 0f;
      // enemyController.Animator.SetBool("IsChasing", true);
    }

    public override void Execute()
    {
        chaseTimer += Time.deltaTime;

        // Check if player is still in detection range
        if (enemyController.IsPlayerInDetectionRange())
        {
            // Move towards player
            Vector3 directionToPlayer = (PlayerMovement.Instance.transform.position - enemyController.transform.position).normalized;
            enemyController.transform.position += directionToPlayer * chaseSpeed * Time.deltaTime;

            // Check if in attack range
            if (enemyController.IsPlayerInAttackRange())
            {
                enemyController.ChangeState(new EnemyAttackState(enemyController));
                return;
            }
        }
        else
        {
            // Player escaped, return to idle
            enemyController.ChangeState(new EnemyIdleState(enemyController));
            return;
        }

        // Prevent infinite chasing
        if (chaseTimer >= maxChaseTime)
        {
            enemyController.ChangeState(new EnemyIdleState(enemyController));
        }
    }

    public override void Exit()
    {
        //enemyController.Animator.SetBool("IsChasing", false);
    }
}

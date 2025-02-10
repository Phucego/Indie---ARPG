using UnityEngine;

public class EnemyPatrolState : BaseEnemyState
{
    private Vector3 targetPosition;
    private float patrolRadius = 15f;
    private float movementSpeed = 2f;

    public EnemyPatrolState(EnemyController controller) : base(controller) { }

    public override void Enter()
    {
        SetRandomTargetPosition();
      //  enemyController.Animator.SetBool("IsPatrolling", true);
    }

    public override void Execute()
    {
        // Move towards target
        enemyController.transform.position = Vector3.MoveTowards(
            enemyController.transform.position, 
            targetPosition, 
            movementSpeed * Time.deltaTime
        );

        // Reached target, set new position
        if (Vector3.Distance(enemyController.transform.position, targetPosition) < 1f)
        {
            SetRandomTargetPosition();
        }

        // Check for player detection
        if (enemyController.IsPlayerInDetectionRange())
        {
            enemyController.ChangeState(new EnemyChaseState(enemyController));
        }
    }

    public override void Exit()
    {
       // enemyController.Animator.SetBool("IsPatrolling", false);
    }

    private void SetRandomTargetPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection.y = 0;
        targetPosition = enemyController.transform.position + randomDirection;
    }
}


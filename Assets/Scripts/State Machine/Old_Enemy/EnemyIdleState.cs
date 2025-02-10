using UnityEngine;

public class EnemyIdleState : BaseEnemyState
{
    private float idleTime;
    private float maxIdleTime = 3f;

    public EnemyIdleState(EnemyController controller) : base(controller) { }

    public override void Enter()
    {
        idleTime = 0f;
      //  enemyController.Animator.SetBool("IsIdle", true);
    }

    public override void Execute()
    {
        idleTime += Time.deltaTime;

        // Check for player detection
        if (enemyController.IsPlayerInDetectionRange())
        {
            enemyController.ChangeState(new EnemyChaseState(enemyController));
            return;
        }

        // Return to patrol if idle for too long
        if (idleTime >= maxIdleTime)
        {
            enemyController.ChangeState(new EnemyPatrolState(enemyController));
        }
    }

    public override void Exit()
    {
        //enemyController.Animator.SetBool("IsIdle", false);
    }
}

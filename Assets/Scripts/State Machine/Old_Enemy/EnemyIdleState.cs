using UnityEngine;

public class EnemyIdleState : BaseEnemyState
{
    public EnemyIdleState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        Debug.Log("[AI] Entering Idle State");
    }

    public override void Execute()
    {
        if (enemy.IsPlayerInDetectionRange)
        {
            enemy.ChangeState(new EnemyChaseState(enemy));
        }
    }

    public override void Exit()
    {
        Debug.Log("[AI] Exiting Idle State");
    }
}


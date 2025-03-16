using UnityEngine;

public class EnemyFleeState : BaseEnemyState
{
    public EnemyFleeState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        Debug.Log("[AI] Entering Flee State");
    }

    public override void Execute()
    {
        if (enemy.IsStaggered) return;

        enemy.FleeFromPlayer();

        if (!enemy.ShouldFlee) // If HP recovers or gets a buff, stop fleeing
        {
            enemy.ChangeState(new EnemyIdleState(enemy));
        }
    }

    public override void Exit()
    {
        Debug.Log("[AI] Exiting Flee State");
    }
}
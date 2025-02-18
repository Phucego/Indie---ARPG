using UnityEngine;

public class EnemyFleeState : BaseEnemyState
{
    private float fleeDuration = 1.4f;
    private float fleeSpeed = 3f;
    private float fleeStartTime;

    public EnemyFleeState(EnemyController controller) : base(controller) { }

    public override void Enter()
    {
        fleeStartTime = Time.time;
    }

    public override void Execute()
    {
        if (Time.time - fleeStartTime < fleeDuration)
        {
            enemyController.FleeFromPlayer(fleeSpeed);
        }
        else
        {
            enemyController.ChangeState(new EnemyIdleState(enemyController)); // Return to idle after fleeing
        }
    }

    public override void Exit()
    {
       
    }
}
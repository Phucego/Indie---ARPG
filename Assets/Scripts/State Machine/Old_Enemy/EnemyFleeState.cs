using UnityEngine;

public class EnemyFleeState : BaseEnemyState
{
    private float fleeDuration = 2f;
    private float fleeStartTime;
    private Vector3 fleeDirection;
    Vector3 playerPosition = PlayerHealth.instance?.transform.position ?? Vector3.zero;
    public EnemyFleeState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        Debug.Log("[AI] Entering Flee State");
        fleeStartTime = Time.time;

        // Move in the opposite direction of the player
        if (playerPosition != Vector3.zero)
        {
            fleeDirection = (enemy.transform.position - playerPosition).normalized;
        }
        else
        {
            fleeDirection = -enemy.transform.forward; // Default flee backward
        }
        fleeDirection = (enemy.transform.position - playerPosition).normalized;
    }

    public override void Execute()
    {
        if (Time.time >= fleeStartTime + fleeDuration)
        {
            enemy.ChangeState(new EnemyIdleState(enemy)); // ✅ After fleeing, go idle
            return;
        }

        enemy.Move(fleeDirection);
    }

    public override void Exit()
    {
        Debug.Log("[AI] Exiting Flee State");
    }
}
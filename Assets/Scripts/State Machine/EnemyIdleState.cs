using UnityEngine;

public class EnemyIdleState : IEnemyState
{
    private EnemyBaseState stateMachine;
    //private Animator animator;

    public EnemyIdleState(EnemyBaseState stateMachine/*, Animator animator*/)
    {
        this.stateMachine = stateMachine;
        //this.animator = animator;
    }

    public void Enter()
    {
        //animator.SetBool("isIdle", true);
    }

    public void Execute()
    {
        // Check if the enemy detects the player or is in range to attack
        if (Vector3.Distance(stateMachine.transform.position, PlayerMovement.Instance.transform.position) < 10f)
        {
            stateMachine.SwitchState(new EnemyAttackState(stateMachine));
        }
    }
}
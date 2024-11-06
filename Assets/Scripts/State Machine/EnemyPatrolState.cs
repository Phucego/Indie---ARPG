using UnityEngine;

public class EnemyPatrolState : IEnemyState
{
    private EnemyBaseState stateMachine;
   // private Animator animator;
    private Vector3 targetPosition;
    private float patrolRadius = 15f; // Radius within which random points will be generated
    private float movementSpeed = 2f;
    
    public EnemyPatrolState(EnemyBaseState stateMachine/*, Animator animator*/)
    {
        this.stateMachine = stateMachine;
       // this.animator = animator;
    }

    public void Enter()
    {
       // animator.SetBool("isPatrolling", true);
        SetRandomTargetPosition(); // Set the initial random target position
    }

    public void Execute()
    {
        MoveToTarget();

        // If the enemy has reached the target, pick a new random point
        if (Vector3.Distance(stateMachine.transform.position, targetPosition) < 1f)
        {
            SetRandomTargetPosition();
        }

        // Check if the player is nearby and switch to attack state
        if (Vector3.Distance(stateMachine.transform.position, PlayerMovement.Instance.transform.position) < 10f)
        {
            stateMachine.SwitchState(new EnemyAttackState(stateMachine/*, animator*/));
        }
    }

    private void SetRandomTargetPosition()
    {
        // Generate a random position within the patrol radius
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection.y = 0;  // Ensure the movement stays on the XZ plane (flat ground)
        targetPosition = stateMachine.transform.position + randomDirection;
    }

    private void MoveToTarget()
    {
        // Move the enemy towards the target position
        stateMachine.transform.position = Vector3.MoveTowards(stateMachine.transform.position, targetPosition, movementSpeed * Time.deltaTime);
    }

    public void Exit()
    {
      //  animator.SetBool("isPatrolling", false);
    }
}
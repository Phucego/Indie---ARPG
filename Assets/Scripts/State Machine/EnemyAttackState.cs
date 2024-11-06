using UnityEngine;

public class EnemyAttackState : IEnemyState
{
    private EnemyBaseState stateMachine;
   // private Animator animator;
    private float attackCooldown = 2f; // Time between attacks
    private float lastAttackTime;
    private float attackRange = 3.2f;  // Maximum range to attack the player
    private float attackDuration = 1f; // Duration of the attack animation (in seconds)

    public EnemyAttackState(EnemyBaseState stateMachine/*, Animator animator*/)
    {
        this.stateMachine = stateMachine;
        //this.animator = animator;
    }

    public void Enter()
    {
       //animator.SetBool("isAttacking", true);
    }

    public void Execute()
    {
        // Check if the player is within attack range
        float distanceToPlayer = Vector3.Distance(stateMachine.transform.position, PlayerMovement.Instance.transform.position);
        
        // If within attack range and attack cooldown has passed, perform the attack
        if (distanceToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            PerformAttack();
        }
        else if (distanceToPlayer > attackRange)
        {
            // If the player is out of range, stop attacking and switch to idle or patrol state
            stateMachine.SwitchState(new EnemyPatrolState(stateMachine/*, animator)*/)); // Switch to patrol as an example
        }
    }

    private void PerformAttack()
    {
        // Trigger attack logic here (animation, damage dealing, etc.)
        Debug.Log("Enemy attacks the player!");

        // Optionally trigger an animation or damage to the player
        // Example: Player.Instance.TakeDamage(attackDamage);

        // Reset the attack cooldown
        lastAttackTime = Time.time;

        // After the attack, switch to idle or patrol (this can be customized)
        // stateMachine.SwitchState(new EnemyIdleState(stateMachine, animator)); // Optionally switch to idle after attacking
    }

    public void Exit()
    {
        //animator.SetBool("isAttacking", false);
    }
}

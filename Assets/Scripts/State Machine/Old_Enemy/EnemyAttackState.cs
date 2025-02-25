using UnityEngine;

public class EnemyAttackState : BaseEnemyState
{
    private float attackCooldown = .5f;
    private float lastAttackTime;
    private float attackDamage = 10f;

    public EnemyAttackState(EnemyController controller) : base(controller) { }

    public override void Enter()
    {
        lastAttackTime = Time.time; // Ensure this resets when entering the attack state
    }

    public override void Execute()
    {
        if (enemyController.IsPlayerInAttackRange())
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time; // Properly update after attack
            }
        }
        else
        {
            enemyController.ChangeState(new EnemyChaseState(enemyController));
        }
    }

    private void PerformAttack()
    {
        Debug.Log("Enemy attacks the player!");
        PlayerHealth player = PlayerMovement.Instance.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(attackDamage);
        }
    
        // After attacking, transition to flee state
        enemyController.ChangeState(new EnemyFleeState(enemyController));
    }


    public override void Exit()
    {
        // Optional: Reset animations
    }
}
using UnityEngine;

public class EnemyAttackState : BaseEnemyState
{
    private float attackCooldown = 2f;
    private float lastAttackTime;
    private float attackDamage = 10f;

    public EnemyAttackState(EnemyController controller) : base(controller) { }

    public override void Enter()
    {
       // enemyController.Animator.SetBool("IsAttacking", true);
    }

    public override void Execute()
    {
        // Check if still in attack range
        if (enemyController.IsPlayerInAttackRange())
        {
            // Perform attack if cooldown passed
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
        }
        else
        {
            // Player out of range, return to chase
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
    }

    public override void Exit()
    {
       // enemyController.Animator.SetBool("IsAttacking", false);
    }
}


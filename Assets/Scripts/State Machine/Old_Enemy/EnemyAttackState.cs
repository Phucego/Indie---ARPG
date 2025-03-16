using UnityEngine;

public class EnemyAttackState : BaseEnemyState
{
    private float attackCooldown = 1.5f;
    private float lastAttackTime;

    public EnemyAttackState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        Debug.Log("[AI] Entering Attack State");
        lastAttackTime = Time.time - attackCooldown;
    }

    public override void Execute()
    {
        if (enemy.IsStaggered) return;

        if (!enemy.IsPlayerInAttackRange())
        {
            enemy.ChangeState(new EnemyChaseState(enemy));
        }
        else if (Time.time >= lastAttackTime + attackCooldown)
        {
            AttackPlayer();
            lastAttackTime = Time.time;
        }
    }

    private void AttackPlayer()
    {
        Debug.Log("[AI] Enemy attacking player!");

        // Find PlayerHealth component
        PlayerHealth player = PlayerHealth.instance; 
        if (player != null)
        {
            player.TakeDamage(10);
        }
        else
        {
            Debug.LogWarning("[AI] PlayerHealth instance is NULL!");
        }
    }

    public override void Exit()
    {
        Debug.Log("[AI] Exiting Attack State");
    }
}

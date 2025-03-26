using UnityEngine;

public class EnemyAttackState : BaseEnemyState
{
    private float attackCooldown = 1.5f;
    private float lastAttackTime;
    private const int attackDamage = 10; // Easily change attack damage

    public EnemyAttackState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        Debug.Log("[AI] Entering Attack State");
        lastAttackTime = Time.time - attackCooldown;
    }

    public override void Execute()
    {
        if (enemy.IsStaggered) return; // Prevent attacking while staggered or knocked back

        if (!enemy.IsPlayerInAttackRange)
        {
            enemy.ChangeState(new EnemyChaseState(enemy));
            return;
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }

    private void PerformAttack()
    {
        Debug.Log("[AI] Enemy attacking player!");

        Animator animator = enemy.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        PlayerHealth player = PlayerHealth.instance ?? GameObject.FindObjectOfType<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(attackDamage);
        }

        // ✅ Make enemy flee after attacking
        enemy.ChangeState(new EnemyFleeState(enemy));
    }

    public override void Exit()
    {
        Debug.Log("[AI] Exiting Attack State");
    }
}
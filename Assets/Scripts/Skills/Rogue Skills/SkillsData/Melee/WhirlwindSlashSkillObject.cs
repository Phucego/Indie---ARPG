using UnityEngine;

public class WhirlwindSlashSkillObject : Skill
{
    private void OnEnable()
    {
        skillName = "Whirlwind Slash";
        description = "Spin with daggers, dealing damage to all nearby enemies over time.";
        staminaCost = 10f;
        cooldown = 8f;
        isHoldable = true;
        tickRate = 0.5f;
    }

    protected override void ExecuteSkillEffect(PlayerAttack playerAttack)
    {
        float damage = playerAttack.weaponManager.GetCurrentWeaponDamage() * 0.8f;
        Collider[] hits = Physics.OverlapSphere(playerAttack.transform.position, 3f, playerAttack.targetLayerMask);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out EnemyHealth enemy))
            {
                enemy.TakeDamage(damage);
            }
            else if (hit.TryGetComponent(out BreakableProps breakable))
            {
                breakable.OnMeleeInteraction(damage);
            }
        }
        playerAttack.animator.Play("whirlwindAttack");
    }
}
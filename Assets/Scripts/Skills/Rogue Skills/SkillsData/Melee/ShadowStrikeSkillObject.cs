using UnityEngine;

public class ShadowStrikeSkillObject : Skill
{
    private void OnEnable()
    {
        skillName = "Shadow Strike";
        description = "A swift dagger strike that deals high damage to a single target.";
        staminaCost = 20f;
        cooldown = 5f;
        isHoldable = false;
        tickRate = 0f;
    }

    protected override void ExecuteSkillEffect(PlayerAttack playerAttack)
    {
        if (playerAttack.currentTarget != null)
        {
            float damage = playerAttack.weaponManager.GetCurrentWeaponDamage() * 1.5f;
            if (playerAttack.currentTarget.TryGetComponent(out EnemyHealth enemy))
            {
                enemy.TakeDamage(damage);
            }
            else if (playerAttack.currentTarget.TryGetComponent(out BreakableProps breakable))
            {
                breakable.OnMeleeInteraction(damage);
            }
            playerAttack.animator.Play("Attack_Combo_1");
        }
        else
        {
            Debug.LogWarning($"{skillName} failed: No target selected.", this);
        }
    }
}
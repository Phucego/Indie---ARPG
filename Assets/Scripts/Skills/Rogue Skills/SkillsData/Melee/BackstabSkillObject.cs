using UnityEngine;

public class BackstabSkillObject : Skill
{
    private void OnEnable()
    {
        skillName = "Backstab";
        description = "Teleport behind the target and deal critical damage.";
        staminaCost = 30f;
        cooldown = 12f;
        isHoldable = false;
        tickRate = 0f;
    }

    protected override void ExecuteSkillEffect(PlayerAttack playerAttack)
    {
        if (playerAttack.currentTarget != null)
        {
            Vector3 behindPos = playerAttack.currentTarget.transform.position - 
                                playerAttack.currentTarget.transform.forward * 1.5f;
            playerAttack.transform.position = behindPos;
            float damage = playerAttack.weaponManager.GetCurrentWeaponDamage() * 2f;
            if (playerAttack.currentTarget.TryGetComponent(out EnemyHealth enemy))
            {
                enemy.TakeDamage(damage);
            }
            playerAttack.animator.Play("Attack_Combo_2");
        }
        else
        {
            Debug.LogWarning($"{skillName} failed: No target selected.", this);
        }
    }
}
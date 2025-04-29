using UnityEngine;

public class RapidShotSkillObject : Skill
{
    private void OnEnable()
    {
        skillName = "Rapid Shot";
        description = "Fire a quick burst of arrows at the target.";
        staminaCost = 15f;
        cooldown = 10f;
        isHoldable = true;
        tickRate = 0.3f;
    }

    protected override void ExecuteSkillEffect(PlayerAttack playerAttack)
    {
        if (playerAttack.currentTarget != null)
        {
            GameObject projectile = playerAttack.GetPooledProjectile();
            if (projectile != null)
            {
                projectile.transform.position = playerAttack.projectileSpawnPoint.position;
                projectile.transform.rotation = Quaternion.identity;
                projectile.SetActive(true);
                if (projectile.TryGetComponent(out Projectile proj))
                {
                    float damage = playerAttack.weaponManager.GetCurrentWeaponDamage() * 0.7f;
                    Vector3 targetPos = playerAttack.currentTarget.transform.position;
                    targetPos.y = playerAttack.projectileSpawnPoint.position.y;
                    Vector3 direction = (targetPos - playerAttack.projectileSpawnPoint.position).normalized;
                    proj.Initialize(direction, damage, playerAttack.currentTarget);
                }
            }
            playerAttack.animator.Play(playerAttack.shootingAnimation.name);
        }
        else
        {
            Debug.LogWarning($"{skillName} failed: No target selected.", this);
        }
    }
}
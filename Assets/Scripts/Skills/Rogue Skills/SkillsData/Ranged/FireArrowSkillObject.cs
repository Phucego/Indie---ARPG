using UnityEngine;

public class FireArrowSkillObject : Skill
{
    private void OnEnable()
    {
        skillName = "Fire Arrow";
        description = "Shoot a flaming arrow that deals bonus damage and burns the target.";
        staminaCost = 20f;
        cooldown = 6f;
        isHoldable = false;
        tickRate = 0f;
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
                    float damage = playerAttack.weaponManager.GetCurrentWeaponDamage() * 1.3f;
                    Vector3 targetPos = playerAttack.currentTarget.transform.position;
                    targetPos.y = playerAttack.projectileSpawnPoint.position.y;
                    Vector3 direction = (targetPos - playerAttack.projectileSpawnPoint.position).normalized;
                    proj.Initialize(direction, damage, playerAttack.currentTarget);
                    Debug.Log("Fire Arrow burn effect applied.");
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
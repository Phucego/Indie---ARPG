using UnityEngine;

public class ExplosiveBoltSkillObject : Skill
{
    private void OnEnable()
    {
        skillName = "Explosive Bolt";
        description = "Fire a bolt that explodes on impact, damaging enemies in an area.";
        staminaCost = 30f;
        cooldown = 12f;
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
                    float damage = playerAttack.weaponManager.GetCurrentWeaponDamage();
                    Vector3 targetPos = playerAttack.currentTarget.transform.position;
                    targetPos.y = playerAttack.projectileSpawnPoint.position.y;
                    Vector3 direction = (targetPos - playerAttack.projectileSpawnPoint.position).normalized;
                    proj.Initialize(direction, damage, playerAttack.currentTarget, (impactPoint) =>
                    {
                        Collider[] hits = Physics.OverlapSphere(impactPoint, 3f, playerAttack.targetLayerMask);
                        foreach (var hit in hits)
                        {
                            if (hit.TryGetComponent(out EnemyHealth enemy))
                            {
                                enemy.TakeDamage(damage * 0.8f);
                            }
                            else if (hit.TryGetComponent(out BreakableProps breakable))
                            {
                                breakable.OnMeleeInteraction(damage * 0.8f);
                            }
                        }
                        Debug.Log("Explosive Bolt VFX at " + impactPoint);
                    });
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
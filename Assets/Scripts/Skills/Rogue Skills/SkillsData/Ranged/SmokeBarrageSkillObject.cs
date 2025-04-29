using UnityEngine;

public class SmokeBarrageSkillObject : Skill
{
    private void OnEnable()
    {
        skillName = "Smoke Barrage";
        description = "Fire a smoke-filled bolt at the mouse position, blinding enemies and jumping backward.";
        staminaCost = 25f;
        cooldown = 15f;
        isHoldable = false;
        tickRate = 0f;
    }

    protected override void ExecuteSkillEffect(PlayerAttack playerAttack)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, playerAttack.targetLayerMask))
        {
            GameObject projectile = playerAttack.GetPooledProjectile();
            if (projectile != null)
            {
                projectile.transform.position = playerAttack.projectileSpawnPoint.position;
                projectile.transform.rotation = Quaternion.identity;
                projectile.SetActive(true);
                if (projectile.TryGetComponent(out Projectile proj))
                {
                    Vector3 direction = (hit.point - playerAttack.projectileSpawnPoint.position).normalized;
                    proj.Initialize(direction, 0f, null, (impactPoint) =>
                    {
                        Collider[] hits = Physics.OverlapSphere(impactPoint, 5f, playerAttack.targetLayerMask);
                        foreach (var hitCollider in hits)
                        {
                            if (hitCollider.TryGetComponent(out EnemyHealth enemy))
                            {
                                enemy.Blind(5f);
                            }
                        }
                        Debug.Log("Smoke Barrage VFX at " + impactPoint);
                    });
                }
            }

            Vector3 backwardDir = -playerAttack.transform.forward;
            Vector3 jumpTarget = playerAttack.transform.position + backwardDir * 3f;
            playerAttack.playerMovement.MoveToTarget(jumpTarget);
            if (playerAttack.playerMovement.forwardDodgeAnim != null)
            {
                playerAttack.animator.Play(playerAttack.playerMovement.forwardDodgeAnim.name);
            }

            playerAttack.animator.Play(playerAttack.shootingAnimation.name);
        }
        else
        {
            Debug.LogWarning($"{skillName} failed: No valid target location.", this);
        }
    }
}
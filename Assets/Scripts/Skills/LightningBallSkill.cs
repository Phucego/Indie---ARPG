using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "LightningBallSkill", menuName = "ScriptableObjects/Skills/LightningBall")]
public class LightningBallSkill : Skill
{
    public float projectileSpeed = 15f;
    public float projectileLifetime = 3f;
    public float aoeRadius = 3f;
    public float aoeDamage = 30f;
    public float effectDuration = 1f;

    public GameObject lightningBallPrefab;
    public GameObject lightningEffectPrefab;
    public int lightningBoltsCount = 3;

    public AnimationClip spellcastShort;

    public override void UseSkill(PlayerAttack playerAttack)
    {
        if (playerAttack == null)
        {
            Debug.LogError("LightningBallSkill: PlayerAttack reference is null");
            return;
        }

        if (!playerAttack.staminaManager.HasEnoughStamina(staminaCost))
        {
            Debug.Log("Not enough stamina to use Lightning Ball skill.");
            return;
        }

        playerAttack.StartCoroutine(LightningBallRoutine(playerAttack));
    }

    private IEnumerator LightningBallRoutine(PlayerAttack playerAttack)
    {
        if (lightningBallPrefab == null)
        {
            Debug.LogError("LightningBallSkill: lightningBallPrefab is not assigned!");
            yield break;
        }

        // Player is casting the skill
        playerAttack.staminaManager.UseStamina(staminaCost);

        // Determine firing direction (using mouse position for the top-down view)
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;  // Ensure we're working in the XZ plane, not on the Y axis for isometric view

        // Get direction to the mouse position while ignoring vertical movement (Y axis)
        Vector3 fireDirection = new Vector3(mouseWorldPos.x - playerAttack.transform.position.x, 0f, mouseWorldPos.z - playerAttack.transform.position.z).normalized;

        // Spawn the lightning ball
        GameObject lightningBall = Instantiate(
            lightningBallPrefab,
            playerAttack.transform.position + fireDirection * 1.5f,
            Quaternion.identity);
        LightningBallProjectile projectile = lightningBall.AddComponent<LightningBallProjectile>();

        // Set enemy layer
        LayerMask enemyLayer = playerAttack.weaponManager != null
            ? playerAttack.weaponManager.enemyLayer
            : default;

        // Initialize projectile
        projectile.Initialize(
            fireDirection,
            projectileSpeed,
            projectileLifetime,
            aoeRadius,
            aoeDamage,
            lightningEffectPrefab,
            lightningBoltsCount,
            effectDuration,
            enemyLayer
        );

        Debug.Log("Lightning Ball casted in the direction of the player's pointing.");
    }
}

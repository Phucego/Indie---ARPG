using UnityEngine;
using System.Collections;
using FarrokhGames.Inventory.Examples;

[CreateAssetMenu(fileName = "WhirlwindSkill", menuName = "ScriptableObjects/Skills/Whirlwind")]
public class WhirlwindSkill : Skill
{
    [Header("Whirlwind Settings")]
    public float damagePerTick = 5f; // Base damage per tick
    public float staminaCostPerTick = 15f; // Stamina cost per tick
    public float effectDuration = 3f; // Total duration of the skill
    public float tickInterval = 0.5f; // Time between damage ticks
    public float radius = 3f; // Radius of the whirlwind hitbox
    public GameObject whirlwindEffectPrefab; // Visual effect prefab

    public override void UseSkill(PlayerAttack playerAttack)
    {
        if (playerAttack == null || playerAttack.weaponManager == null || playerAttack.staminaManager == null ||
            playerAttack.animator == null || playerAttack.playerTransform == null)
        {
            Debug.LogWarning("Cannot use Whirlwind: Required components missing.");
            return;
        }

        // Ensure the player has a two-handed weapon equipped
        if (!playerAttack.weaponManager.CanUseTwoHandedSkill())
        {
            Debug.Log("Cannot use Whirlwind: A two-handed weapon is required.");
            return;
        }

        // Ensure the player has enough stamina to start
        if (!playerAttack.staminaManager.HasEnoughStamina(staminaCostPerTick))
        {
            Debug.Log("Not enough stamina to start Whirlwind.");
            return;
        }

        // Start the whirlwind effect
        playerAttack.StartCoroutine(WhirlwindRoutine(playerAttack));
    }

    private IEnumerator WhirlwindRoutine(PlayerAttack playerAttack)
    {
        // Initialize skill state
        float startTime = Time.time;
        bool originalCanMove = playerAttack.playerMovement.canMove;
        playerAttack.isSkillActive = true;
        playerAttack.playerMovement.canMove = true; // Allow movement during skill

        GameObject effectInstance = null;

        // Instantiate visual effect if provided
        if (whirlwindEffectPrefab != null)
        {
            Vector3 effectPosition = playerAttack.playerTransform.position + Vector3.up * 1.2f; // Spawn at body height
            effectInstance = Instantiate(whirlwindEffectPrefab, effectPosition, Quaternion.identity, playerAttack.playerTransform);
        }

        // Set animator parameter for whirlwind (use a trigger or bool for smooth looping)
        playerAttack.animator.SetBool("WhirlwindActive", true);

        // Main whirlwind loop
        while (Time.time - startTime < effectDuration)
        {
            // Check stamina
            if (!playerAttack.staminaManager.HasEnoughStamina(staminaCostPerTick))
            {
                Debug.Log("Whirlwind stopped due to insufficient stamina.");
                break;
            }

            // Deduct stamina and apply damage
            playerAttack.staminaManager.UseStamina(staminaCostPerTick);
            ApplyDamage(playerAttack);

            yield return new WaitForSeconds(tickInterval); // Wait for next tick
        }

        // Cleanup
        playerAttack.animator.SetBool("WhirlwindActive", false);
        if (effectInstance != null)
            Destroy(effectInstance);
        playerAttack.isSkillActive = false;
        playerAttack.playerMovement.canMove = originalCanMove; // Restore original movement state
    }

    private void ApplyDamage(PlayerAttack playerAttack)
    {
        // Get current weapon and damage
        ItemDefinition currentWeapon = playerAttack.weaponManager.GetCurrentWeapon();
        if (currentWeapon == null || currentWeapon.Type != ItemType.Weapons)
        {
            Debug.LogWarning("No valid weapon equipped for Whirlwind damage.");
            return;
        }

        float totalDamage = playerAttack.weaponManager.GetModifiedDamage(currentWeapon) + damagePerTick;

        // Create a spherical hitbox around the player
        Collider[] hits = Physics.OverlapSphere(
            playerAttack.playerTransform.position + Vector3.up * 1f, // Center at player’s body
            radius,
            playerAttack.weaponManager.enemyLayer
        );

        // Apply damage to all enemies in the hitbox
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out EnemyHealth enemy))
            {
                enemy.TakeDamage(totalDamage);
            }
        }

        // Optional: Visualize hitbox in editor for debugging
#if UNITY_EDITOR
        DebugDrawSphere(playerAttack.playerTransform.position + Vector3.up * 1f, radius, Color.red, tickInterval);
#endif
    }

    // Debug utility to visualize the hitbox
    private void DebugDrawSphere(Vector3 center, float radius, Color color, float duration)
    {
        float theta = 0;
        float phi = 0;
        float thetaInc = Mathf.PI / 20;
        float phiInc = 2 * Mathf.PI / 20;
        Vector3 lastP = Vector3.zero;

        for (int i = 0; i < 20; i++)
        {
            theta = i * thetaInc;
            for (int j = 0; j < 20; j++)
            {
                phi = j * phiInc;
                float x = radius * Mathf.Sin(theta) * Mathf.Cos(phi);
                float y = radius * Mathf.Sin(theta) * Mathf.Sin(phi);
                float z = radius * Mathf.Cos(theta);
                Vector3 p = center + new Vector3(x, y, z);
                if (j > 0)
                    Debug.DrawLine(lastP, p, color, duration);
                lastP = p;
            }
        }
    }
}
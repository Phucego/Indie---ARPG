using UnityEngine;
using System.Collections;
using FarrokhGames.Inventory.Examples;

[CreateAssetMenu(fileName = "WhirlwindSkill", menuName = "ScriptableObjects/Skills/Whirlwind")]
public class WhirlwindSkill : Skill
{
    [Header("Whirlwind Settings")] public float damagePerTick = 5f; // Base damage per tick
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
            effectInstance = Instantiate(whirlwindEffectPrefab, effectPosition, Quaternion.identity,
                playerAttack.playerTransform);
        }

        // Set animator parameter for whirlwind
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

    }
}
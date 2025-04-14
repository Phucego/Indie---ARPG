using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "WhirlwindSkill", menuName = "ScriptableObjects/Skills/Whirlwind")]
public class WhirlwindSkill : Skill
{
    public float damagePerTick = 5f;
    public float staminaCostPerTick = 15f;
    public float effectDuration = 3f; // Total duration of the skill
    public GameObject whirlwindEffectPrefab;

    public override void UseSkill(PlayerAttack playerAttack)
    {
        // Ensure the player has a two-handed weapon equipped using the WeaponManager
        if (!WeaponManager.Instance.CanUseTwoHandedSkill())
        {
            Debug.Log("Cannot use Whirlwind. A two-handed weapon is required.");
            return;
        }

        // Ensure the player has enough stamina before starting the skill
        if (!playerAttack.staminaManager.HasEnoughStamina(staminaCostPerTick))
        {
            Debug.Log("Not enough stamina to start Whirlwind.");
            return;
        }

        // Start the whirlwind effect if possible
        playerAttack.StartCoroutine(WhirlwindRoutine(playerAttack));
    }

    private IEnumerator WhirlwindRoutine(PlayerAttack playerAttack)
    {
        // Start the whirlwind skill
        float startTime = Time.time;
        playerAttack.isSkillActive = true;
        playerAttack.playerMovement.canMove = true; // Allow movement during the skill

        GameObject effectInstance = null;

        // If a prefab for the effect is provided, instantiate it at the player's position
        if (whirlwindEffectPrefab)
        {
            Vector3 effectPosition = playerAttack.transform.position + Vector3.up * 1.2f; // Adjusted to spawn at body height
            effectInstance = Instantiate(whirlwindEffectPrefab, effectPosition, Quaternion.identity, playerAttack.transform);
        }

        // While the effect duration hasn't passed
        while (Time.time - startTime < effectDuration)
        {
            // If not enough stamina, stop the whirlwind
            if (!playerAttack.staminaManager.HasEnoughStamina(staminaCostPerTick))
            {
                Debug.Log("Whirlwind stopped due to insufficient stamina.");
                break;
            }

            // Deduct stamina and perform the attack
            playerAttack.staminaManager.UseStamina(staminaCostPerTick);
            playerAttack.animator.Play(playerAttack.whirlwindAttack.name); // Play the whirlwind animation
            ApplyDamage(playerAttack); // Apply damage in the area
            yield return new WaitForSeconds(playerAttack.whirlwindAttack.length * 0.5f); // Wait for the attack duration before repeating
        }

        // Clean up the effect instance after the skill ends
        if (effectInstance) Destroy(effectInstance);
        playerAttack.isSkillActive = false;
        playerAttack.playerMovement.canMove = false; // Stop movement after skill ends
    }

    private void ApplyDamage(PlayerAttack playerAttack)
    {
        // Create a hitbox based on the player's position and direction
        Vector3 hitboxPosition = playerAttack.playerTransform.position + Vector3.up * 1.2f + playerAttack.playerTransform.forward * (playerAttack.attackRange * 0.5f);
        Vector3 hitboxSize = new Vector3(1.5f, 2.0f, playerAttack.attackRange); // Adjust hitbox size

        // Get all enemies within the hitbox
        Collider[] hits = Physics.OverlapBox(hitboxPosition, hitboxSize / 2, playerAttack.playerTransform.rotation, playerAttack.weaponManager.enemyLayer);

        // Loop through all colliders hit
        foreach (Collider hit in hits)
        {
            // If it's an enemy, apply damage
            if (hit.TryGetComponent<EnemyHealth>(out EnemyHealth enemy))
            {
                // Calculate damage considering the weapon bonus
                float totalDamage = damagePerTick;
                Weapon currentWeapon = playerAttack.weaponManager.equippedRightHandWeapon; // Get the currently equipped weapon (right hand)
                if (currentWeapon == null)
                {
                    currentWeapon = playerAttack.weaponManager.equippedLeftHandWeapon; // If no right-hand weapon, check left-hand
                }

                if (currentWeapon != null)
                {
                    totalDamage += currentWeapon.weaponData.damageBonus; // Add weapon's damage bonus
                }

                // Apply the damage to the enemy
                enemy.TakeDamage(totalDamage);
            }
        }
    }
}

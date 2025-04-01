using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "BuffSkill", menuName = "ScriptableObjects/Skills/Buff")]
public class BuffSkill : Skill
{
    public float damageIncrease = 1.5f; // 50% increased damage
    public float defenseIncrease = 1.2f; // 20% increased defense
    public float effectDuration = 5f;
    public GameObject buffEffectPrefab;  // Reference to the buff effect prefab

    public override void UseSkill(PlayerAttack playerAttack)
    {
        if (!playerAttack.staminaManager.HasEnoughStamina(staminaCost))
        {
            Debug.Log("Not enough stamina to use Buff skill.");
            return;
        }

        playerAttack.StartCoroutine(BuffRoutine(playerAttack));
    }

    private IEnumerator BuffRoutine(PlayerAttack playerAttack)
    {
        playerAttack.staminaManager.UseStamina(staminaCost);
        playerAttack.isSkillActive = true;

        // Apply Buffs
        playerAttack.damageBonus *= damageIncrease;  // Increase damage
        playerAttack.defenseBonus *= defenseIncrease;  // Increase defense

        Debug.Log("Buff skill activated.");

        // Spawn Buff effect at player's position
        if (buffEffectPrefab != null)
        {
            GameObject buffEffect = Instantiate(buffEffectPrefab, playerAttack.transform.position + Vector3.up * 1.5f, Quaternion.identity);
            buffEffect.transform.SetParent(playerAttack.transform);  // Set it as a child of the player to follow their movement

            // Destroy the buff effect after the effect duration ends
            Destroy(buffEffect, effectDuration);
        }

        yield return new WaitForSeconds(effectDuration);

        // Revert Buffs after duration
        playerAttack.damageBonus /= damageIncrease;
        playerAttack.defenseBonus /= defenseIncrease;

        Debug.Log("Buff skill ended.");
        playerAttack.isSkillActive = false;
    }
}

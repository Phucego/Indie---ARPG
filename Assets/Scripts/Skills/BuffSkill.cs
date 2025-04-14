using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "BuffSkill", menuName = "ScriptableObjects/Skills/Buff")]
public class BuffSkill : Skill
{
    public float damageMultiplier = 1.5f; // 50% increased damage
    public float defenseMultiplier = 1.2f; // 20% increased defense
    public float effectDuration = 5f;
    public GameObject buffEffectPrefab;

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

        PlayerStats stats = playerAttack.GetComponent<PlayerStats>();
        if (stats == null)
        {
            Debug.LogWarning("PlayerStats component not found!");
            yield break;
        }

        // Apply buffs
        float originalAttackPower = stats.attackPower;
        float originalDefense = stats.defense;

        stats.attackPower *= damageMultiplier;
        stats.defense *= defenseMultiplier;

        // Visual effect
        if (buffEffectPrefab != null)
        {
            GameObject buffEffect = Instantiate(buffEffectPrefab, playerAttack.transform.position + Vector3.up * 1.5f, Quaternion.identity);
            buffEffect.transform.SetParent(playerAttack.transform);
            Destroy(buffEffect, effectDuration);
        }

        Debug.Log("Buff skill activated.");

        yield return new WaitForSeconds(effectDuration);

        // Revert buffs
        stats.attackPower = originalAttackPower;
        stats.defense = originalDefense;

        Debug.Log("Buff skill ended.");
        playerAttack.isSkillActive = false;
    }
}
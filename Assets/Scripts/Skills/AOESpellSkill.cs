using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "AOESpellSkill", menuName = "ScriptableObjects/Skills/AOESpell")]
public class AOESpellSkill : Skill
{
    public float aoeRadius = 3f; // Radius of the AOE
    public float aoeDamage = 30f; // Damage dealt to each enemy in range
    public float effectDuration = 1f; // Time the spell lasts

    public GameObject aoeEffectPrefab;

    public override void UseSkill(PlayerAttack playerAttack)
    {
        if (!playerAttack.staminaManager.HasEnoughStamina(staminaCost))
        {
            Debug.Log("Not enough stamina to use AOE Spell skill.");
            return;
        }

        playerAttack.StartCoroutine(AOESpellRoutine(playerAttack));
    }

    private IEnumerator AOESpellRoutine(PlayerAttack playerAttack)
    {
        playerAttack.staminaManager.UseStamina(staminaCost);
        playerAttack.isSkillActive = true;

        // Display the AOE effect
        GameObject effectInstance = null;
        if (aoeEffectPrefab)
        {
            effectInstance = Instantiate(aoeEffectPrefab, playerAttack.transform.position, Quaternion.identity);
            Destroy(effectInstance, effectDuration); // Destroy effect after the duration
        }

        // Apply AOE damage
        Collider[] hits = Physics.OverlapSphere(playerAttack.transform.position, aoeRadius, playerAttack.weaponManager.enemyLayer);
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<EnemyHealth>(out EnemyHealth enemy))
            {
                enemy.TakeDamage(aoeDamage);
                Debug.Log("AOE Damage dealt to " + enemy.name);
            }
        }


        yield return new WaitForSeconds(effectDuration);
        playerAttack.isSkillActive = false;
        Debug.Log("AOE Spell completed.");
    }
}
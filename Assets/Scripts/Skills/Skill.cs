using System.Collections;
using UnityEngine;

public class Skill : ScriptableObject
{
    [Header("Skill Properties")]
    public string skillName;
    public float staminaCost;
    public float cooldown;
    
    public string description;
    [Header("Holdable Skill Settings")]
    public bool isHoldable; // Determines if the skill is a hold-down skill
    public float tickRate = 0.2f; // How often the skill applies while holding

    public virtual void UseSkill(PlayerAttack playerAttack)
    {
        if (isHoldable)
        {
            playerAttack.StartCoroutine(HoldableSkillLoop(playerAttack));
        }
        else
        {
            ActivateOnce(playerAttack);
        }
    }

    private void ActivateOnce(PlayerAttack playerAttack)
    {
        if (playerAttack.staminaManager.HasEnoughStamina(staminaCost))
        {
            playerAttack.staminaManager.UseStamina(staminaCost);
            ExecuteSkillEffect(playerAttack);
            playerAttack.StartCoroutine(playerAttack.SkillCooldown(cooldown));
        }
        else
        {
            Debug.Log($"{skillName} cannot be used: Not enough stamina!");
        }
    }

    private IEnumerator HoldableSkillLoop(PlayerAttack playerAttack)
    {
        while (playerAttack.staminaManager.HasEnoughStamina(staminaCost))
        {
            playerAttack.staminaManager.UseStamina(staminaCost);
            ExecuteSkillEffect(playerAttack);
            yield return new WaitForSeconds(tickRate);
        }

        Debug.Log($"{skillName} stopped: Not enough stamina!");
    }

    protected virtual void ExecuteSkillEffect(PlayerAttack playerAttack)
    {
        Debug.Log($"{skillName} executed!");
    }
}
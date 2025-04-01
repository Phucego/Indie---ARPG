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
    public bool isOnCooldown = false; // Tracks individual skill cooldown

    public virtual void UseSkill(PlayerAttack playerAttack)
    {
        if (isOnCooldown)
        {
            Debug.Log($"{skillName} is on cooldown!");
            return;
        }

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
        if (!playerAttack.staminaManager.HasEnoughStamina(staminaCost))
        {
            Debug.Log($"{skillName} cannot be used: Not enough stamina!");
            return;
        }
        
        playerAttack.staminaManager.UseStamina(staminaCost);
        ExecuteSkillEffect(playerAttack);
        playerAttack.StartCoroutine(StartCooldown());
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
        playerAttack.StartCoroutine(StartCooldown());
    }

    private IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }

    protected virtual void ExecuteSkillEffect(PlayerAttack playerAttack)
    {
        Debug.Log($"{skillName} executed!");
    }
}

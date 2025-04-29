using System.Collections;
using UnityEngine;

public enum SkillType
{
    ShadowStrike,
    WhirlwindSlash,
    Backstab,
    SmokeBomb,
    FireArrow,
    RapidShot,
    ExplosiveBolt,
    SmokeBarrage
}

[CreateAssetMenu(fileName = "Skills", menuName = "New Skill Data", order = 0)]
public class Skill : ScriptableObject
{
    [Header("Skill Properties")]
    public SkillType skillType;
    public string skillName;
    public float staminaCost;
    public float cooldown;
    public string description;
    
    [Header("Holdable Skill Settings")]
    public bool isHoldable;
    public float tickRate = 0.2f;
    public bool isOnCooldown = false;

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

    private void OnValidate()
    {
        SetSkillData();
    }

    protected void SetSkillData()
    {
        switch (skillType)
        {
            case SkillType.ShadowStrike:
                skillName = "Shadow Strike";
                description = "A swift dagger strike that deals high damage to a single target.";
                staminaCost = 20f;
                cooldown = 5f;
                isHoldable = false;
                tickRate = 0f;
                break;
            case SkillType.WhirlwindSlash:
                skillName = "Whirlwind Slash";
                description = "Spin with daggers, dealing damage to all nearby enemies over time.";
                staminaCost = 10f;
                cooldown = 8f;
                isHoldable = true;
                tickRate = 0.5f;
                break;
            case SkillType.Backstab:
                skillName = "Backstab";
                description = "Teleport behind the target and deal critical damage.";
                staminaCost = 30f;
                cooldown = 12f;
                isHoldable = false;
                tickRate = 0f;
                break;
            case SkillType.SmokeBomb:
                skillName = "Smoke Bomb";
                description = "Throw a smoke bomb at the mouse position, blinding enemies and turning the player invisible while in the smoke.";
                staminaCost = 25f;
                cooldown = 15f;
                isHoldable = false;
                tickRate = 0f;
                break;
            case SkillType.FireArrow:
                skillName = "Fire Arrow";
                description = "Shoot a flaming arrow that deals bonus damage and burns the target.";
                staminaCost = 20f;
                cooldown = 6f;
                isHoldable = false;
                tickRate = 0f;
                break;
            case SkillType.RapidShot:
                skillName = "Rapid Shot";
                description = "Fire a quick burst of arrows at the target.";
                staminaCost = 15f;
                cooldown = 10f;
                isHoldable = true;
                tickRate = 0.3f;
                break;
            case SkillType.ExplosiveBolt:
                skillName = "Explosive Bolt";
                description = "Fire a bolt that explodes on impact, damaging enemies in an area.";
                staminaCost = 30f;
                cooldown = 12f;
                isHoldable = false;
                tickRate = 0f;
                break;
            case SkillType.SmokeBarrage:
                skillName = "Smoke Barrage";
                description = "Fire a smoke-filled bolt at the mouse position, blinding enemies and jumping backward.";
                staminaCost = 25f;
                cooldown = 15f;
                isHoldable = false;
                tickRate = 0f;
                break;
            default:
                Debug.LogWarning($"[Skill] Unknown SkillType {skillType} on {name}.");
                break;
        }
    }
}
using System.Collections;
using UnityEngine;

public class PlayerSkills : MonoBehaviour
{
    [Header("Skill System")]
    public SkillManager skillManager; // Reference to SkillManager for skill usage and UI

    [Header("References")]
    public PlayerAttack playerAttack;

    private void Start()
    {
        if (skillManager == null)
        {
            skillManager = GetComponent<SkillManager>();
            if (skillManager == null)
            {
                Debug.LogError("SkillManager not found on PlayerSkills. Please assign it in the Inspector or ensure it's attached to the same GameObject.");
            }
        }

        if (playerAttack == null)
        {
            playerAttack = GetComponent<PlayerAttack>();
            if (playerAttack == null)
            {
                Debug.LogError("PlayerAttack not found on PlayerSkills. Please assign it in the Inspector or ensure it's attached to the same GameObject.");
            }
        }

        // Skill assignment and input handling are managed by SkillManager
    }

    private void Update()
    {
        // Skill input checking is fully handled by SkillManager's Update method
        // This script serves as a bridge between PlayerAttack and SkillManager
    }

    // Optional: Method to trigger skill usage programmatically if needed
    public void TriggerSkill(int hotbarIndex)
    {
        if (skillManager != null)
        {
            skillManager.UseSkillFromHotbar(hotbarIndex);
        }
    }
}
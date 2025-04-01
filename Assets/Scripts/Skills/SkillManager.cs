using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    [Header("Skill Settings")]
    public Button[] skillButtons; // UI buttons for assigning skills
    public Skill[] skills; // List of available skills
    private bool isSkillActive = false;
    private bool isHoldingSkill = false;
    private int activeSkillIndex = -1;
    
    public Animator animator;
    [Header("Hotbar")]
    private Skill[] assignedSkills = new Skill[4]; // Stores skills assigned to 1-4 keys

    [Header("References")]
    public PlayerAttack playerAttack;

    [Header("Skill Description Panel")]
    public GameObject skillDescriptionPanel; // Panel for showing skill description
    public TextMeshProUGUI skillDescriptionText;
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI manaCostText;
    public TextMeshProUGUI cooldownText;

    private void Start()
    {
        // Initially hide the skill description panel
        skillDescriptionPanel.SetActive(false);
        
        animator = GetComponent<Animator>();    
    }

    private void Update()
    {
        // Check for hotkey presses (1-4 keys)
        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) // Alpha1 = Key 1, Alpha2 = Key 2, etc.
            {
                UseSkillFromHotbar(i);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha1 + i) && isHoldingSkill && activeSkillIndex == i)
            {
                isHoldingSkill = false;
                isSkillActive = false;
                activeSkillIndex = -1;
            }
        }
    }

    private void AssignSkillToHotbar(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skills.Length) return;

        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKey(KeyCode.Alpha1 + i))
            {
                assignedSkills[i] = skills[skillIndex];
                Debug.Log($"Assigned {skills[skillIndex].skillName} to Key {i + 1}");
                return;
            }
        }
    }

    private void UseSkillFromHotbar(int hotbarIndex)
    {
        if (isSkillActive || hotbarIndex < 0 || hotbarIndex >= assignedSkills.Length || assignedSkills[hotbarIndex] == null) return;
        Skill selectedSkill = assignedSkills[hotbarIndex];

        if (!playerAttack.staminaManager.HasEnoughStamina(selectedSkill.staminaCost))
        {
            Debug.Log("Not enough stamina");
            return;
        }

        if (selectedSkill.isHoldable)
        {
            isHoldingSkill = true;
            activeSkillIndex = hotbarIndex;
            StartCoroutine(UseHoldableSkill(selectedSkill));
        }
        else
        {
            isSkillActive = true;
            playerAttack.staminaManager.UseStamina(selectedSkill.staminaCost);
            selectedSkill.UseSkill(playerAttack);
            StartCoroutine(SkillCooldown(selectedSkill.cooldown));
        }
    }

    private IEnumerator UseHoldableSkill(Skill skill)
    {
        while (isHoldingSkill && playerAttack.staminaManager.HasEnoughStamina(skill.staminaCost))
        {
            playerAttack.staminaManager.UseStamina(skill.staminaCost);
            skill.UseSkill(playerAttack);
            yield return new WaitForSeconds(skill.tickRate);
        }

        isHoldingSkill = false;
        isSkillActive = false;
    }

    private IEnumerator SkillCooldown(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        isSkillActive = false;
    }

    public void ShowSkillDescription(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skills.Length) return;

        Skill selectedSkill = skills[skillIndex];

        // Show the description panel
        skillDescriptionPanel.SetActive(true);

        // Update text fields with the skill's information
        skillNameText.text = selectedSkill.skillName;
        skillDescriptionText.text = selectedSkill.description;
        manaCostText.text = $"Mana Cost: {selectedSkill.staminaCost}";
        cooldownText.text = $"Cooldown: {selectedSkill.cooldown} sec";
    }

    public void HideSkillDescription()
    {
        skillDescriptionPanel.SetActive(false);
    }

}
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillManager : MonoBehaviour
{
    [Header("Skill Settings")]
    public Button[] skillButtons; // UI buttons for assigning skills
    public Skill[] skills; // List of available skills
    private bool isSkillActive = false;
    private bool isHoldingSkill = false;
    private int activeSkillIndex = -1;

    [Header("Hotbar")]
    private Skill[] assignedSkills = new Skill[4]; // Stores skills assigned to 1-4 keys

    [Header("References")]
    public PlayerAttack playerAttack;

    [Header("Skill Description Panel")]
    public GameObject skillDescriptionPanel; // Panel for showing skill description
    public TextMeshProUGUI skillDescriptionText; // Text component to display skill description

    [Header("Animator")]
    public Animator buttonAnimator; // Single Animator controlling all buttons

    private void Start()
    {
        // Assign button events dynamically for UI
        for (int i = 0; i < skillButtons.Length; i++)
        {
            int index = i;

            EventTrigger trigger = skillButtons[i].gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown.callback.AddListener((_) => AssignSkillToHotbar(index));
            trigger.triggers.Add(pointerDown);

            EventTrigger.Entry pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            pointerEnter.callback.AddListener((_) => ShowSkillDescription(index));
            pointerEnter.callback.AddListener((_) => SetHoverAnimation(true)); // On hover in, set the "onHovered" parameter to true
            trigger.triggers.Add(pointerEnter);

            EventTrigger.Entry pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            pointerExit.callback.AddListener((_) => HideSkillDescription());
            pointerExit.callback.AddListener((_) => SetHoverAnimation(false)); // On hover out, set the "onHovered" parameter to false
            trigger.triggers.Add(pointerExit);
        }

        // Initially hide the skill description panel
        skillDescriptionPanel.SetActive(false);
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

    // Show skill description when hovering over skill button
    private void ShowSkillDescription(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skills.Length) return;

        Skill selectedSkill = skills[skillIndex];

        // Show the description panel and set the skill description text
        skillDescriptionPanel.SetActive(true);
        skillDescriptionText.text = selectedSkill.description; // Assuming 'description' is a string in your Skill class
    }

    // Hide skill description when hover ends
    private void HideSkillDescription()
    {
        skillDescriptionPanel.SetActive(false);
    }

    // Set the "onHovered" animation parameter when any button is hovered or unhovered
    private void SetHoverAnimation(bool isHovered)
    {
        if (buttonAnimator != null)
        {
            buttonAnimator.SetBool("onHovered", isHovered);
        }
    }
}

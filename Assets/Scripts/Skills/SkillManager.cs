using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    [Header("Skill Settings")]
    public Button[] skillButtons; // UI buttons for assigning skills
    public Skill[] crossbowSkills; // Skills for crossbow (ranged)
    public Skill[] daggerSkills; // Skills for dagger (melee)
    public bool isSkillActive = false;
    private bool isHoldingSkill = false;
    private int activeSkillIndex = -1;

    public Animator animator;

    [Header("Hotbar")]
    private Skill[] assignedSkills = new Skill[4]; // Current active skills (QWER)

    [Header("References")]
    public PlayerAttack playerAttack;
    public WeaponManager weaponManager;

    [Header("Skill Description Panel")]
    public GameObject skillDescriptionPanel;
    public TextMeshProUGUI skillDescriptionText;
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI manaCostText;
    public TextMeshProUGUI cooldownText;

    private void Start()
    {
        if (skillDescriptionPanel == null)
        {
            Debug.LogError("SkillDescriptionPanel not assigned in SkillManager.", this);
            skillDescriptionPanel = new GameObject("SkillDescriptionPanel");
            skillDescriptionPanel.SetActive(false);
        }
        else
        {
            skillDescriptionPanel.SetActive(false);
        }

        if (weaponManager == null)
        {
            weaponManager = GetComponent<WeaponManager>();
            if (weaponManager == null)
                Debug.LogError("WeaponManager not found on SkillManager.", this);
        }

        if (playerAttack == null)
        {
            playerAttack = GetComponent<PlayerAttack>();
            if (playerAttack == null)
                Debug.LogError("PlayerAttack not found on SkillManager.", this);
        }

        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("Animator not found on SkillManager.", this);

        UpdateSkillSet();
    }

    private void Update()
    {
        UpdateSkillSet();

        if (IsInputBlocked()) return;

        // Map QWER to hotbar indices 0-3
        if (Input.GetKeyDown(KeyCode.Q)) UseSkillFromHotbar(0);
        else if (Input.GetKeyUp(KeyCode.Q) && isHoldingSkill && activeSkillIndex == 0)
        {
            isHoldingSkill = false;
            isSkillActive = false;
            activeSkillIndex = -1;
        }

        if (Input.GetKeyDown(KeyCode.W)) UseSkillFromHotbar(1);
        else if (Input.GetKeyUp(KeyCode.W) && isHoldingSkill && activeSkillIndex == 1)
        {
            isHoldingSkill = false;
            isSkillActive = false;
            activeSkillIndex = -1;
        }

        if (Input.GetKeyDown(KeyCode.E)) UseSkillFromHotbar(2);
        else if (Input.GetKeyUp(KeyCode.E) && isHoldingSkill && activeSkillIndex == 2)
        {
            isHoldingSkill = false;
            isSkillActive = false;
            activeSkillIndex = -1;
        }

        if (Input.GetKeyDown(KeyCode.R)) UseSkillFromHotbar(3);
        else if (Input.GetKeyUp(KeyCode.R) && isHoldingSkill && activeSkillIndex == 3)
        {
            isHoldingSkill = false;
            isSkillActive = false;
            activeSkillIndex = -1;
        }
    }

    private bool IsInputBlocked()
    {
        return (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) ||
               (DialogueDisplay.Instance != null && DialogueDisplay.Instance.isDialogueActive);
    }

    private void UpdateSkillSet()
    {
        if (weaponManager == null) return;

        bool isCrossbow = weaponManager.IsRangedWeaponEquipped;
        Skill[] newSkills = isCrossbow ? crossbowSkills : daggerSkills;

        if (newSkills == null || newSkills.Length < 4)
        {
            Debug.LogWarning($"Skill set for {(isCrossbow ? "Crossbow" : "Dagger")} is null or has fewer than 4 skills.", this);
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            if (assignedSkills[i] != newSkills[i])
            {
                assignedSkills[i] = newSkills[i];
                Debug.Log($"Assigned {(isCrossbow ? "Crossbow" : "Dagger")} skill {newSkills[i]?.skillName} to hotbar index {i} (Key: {(char)('Q' + i)})", this);
            }
        }
    }

    private void AssignSkillToHotbar(int skillIndex)
    {
        bool isCrossbow = weaponManager.IsRangedWeaponEquipped;
        Skill[] skills = isCrossbow ? crossbowSkills : daggerSkills;

        if (skillIndex < 0 || skillIndex >= skills.Length) return;

        if (Input.GetKey(KeyCode.Q)) assignedSkills[0] = skills[skillIndex];
        else if (Input.GetKey(KeyCode.W)) assignedSkills[1] = skills[skillIndex];
        else if (Input.GetKey(KeyCode.E)) assignedSkills[2] = skills[skillIndex];
        else if (Input.GetKey(KeyCode.R)) assignedSkills[3] = skills[skillIndex];

        Debug.Log($"Assigned {skills[skillIndex].skillName} to Key {(char)('Q' + Array.IndexOf(assignedSkills, skills[skillIndex]))}");
    }

    public void UseSkillFromHotbar(int hotbarIndex)
    {
        if (isSkillActive || hotbarIndex < 0 || hotbarIndex >= assignedSkills.Length || assignedSkills[hotbarIndex] == null)
        {
            Debug.LogWarning($"Cannot use skill at hotbar index {hotbarIndex}: Skill is null, active, or invalid index.", this);
            return;
        }

        Skill selectedSkill = assignedSkills[hotbarIndex];

        if (!playerAttack.staminaManager.HasEnoughStamina(selectedSkill.staminaCost))
        {
            Debug.Log("Not enough stamina for skill: " + selectedSkill.skillName, this);
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
        StartCoroutine(SkillCooldown(skill.cooldown));
    }

    private IEnumerator SkillCooldown(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        isSkillActive = false;
    }

    public void ShowSkillDescription(int skillIndex)
    {
        bool isCrossbow = weaponManager.IsRangedWeaponEquipped;
        Skill[] skills = isCrossbow ? crossbowSkills : daggerSkills;

        if (skillIndex < 0 || skillIndex >= skills.Length) return;

        Skill selectedSkill = skills[skillIndex];

        skillDescriptionPanel.SetActive(true);

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
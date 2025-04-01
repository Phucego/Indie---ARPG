using System;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class SkillButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int skillIndex;
    public SkillManager skillManager; // Reference to the skill manager

    private void Start()
    {
        skillManager = gameObject.GetComponentInParent<SkillManager>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (skillManager != null)
        {
            skillManager.ShowSkillDescription(skillIndex);
            skillManager.animator.SetBool("onHovered", true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (skillManager != null)
        {
            skillManager.HideSkillDescription();
            skillManager.animator.SetBool("onHovered", false);
        }
    }
}
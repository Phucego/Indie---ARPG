using UnityEngine;

public class NPCProjectileUpgrade : MonoBehaviour
{
    [SerializeField] private DialogueTrigger dialogueTrigger; // The dialogue trigger for NPC
    [SerializeField] private RangedAttackUpgrade rangedAttackUpgrade; // The upgrade system
    [SerializeField] private GameObject upgradePromptUI; // Optional UI for prompting the player to interact

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Show the upgrade prompt and trigger the dialogue
            if (upgradePromptUI != null)
            {
                upgradePromptUI.SetActive(true); // Display the prompt
            }

            // Trigger dialogue
            dialogueTrigger.TriggerDialogue();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Hide the upgrade prompt when the player exits the trigger area
            if (upgradePromptUI != null)
            {
                upgradePromptUI.SetActive(false);
            }
        }
    }

    public void OnChooseFireProjectile()
    {
        // Player chooses fire projectile
        rangedAttackUpgrade.EnableFireProjectile();
        Debug.Log("Player selected Fire Projectile");
    }

    public void OnChooseExplosiveProjectile()
    {
        // Player chooses explosive projectile
        rangedAttackUpgrade.EnableExplosiveProjectile();
        Debug.Log("Player selected Explosive Projectile");
    }
}
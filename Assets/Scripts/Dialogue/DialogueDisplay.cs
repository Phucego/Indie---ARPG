using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class DialogueDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI characterNameText; // UI for the character's name
    public TextMeshProUGUI dialogueText; // UI for the dialogue text
    public Button nextButton; // Button to progress the dialogue

    [Header("Dialogue Data")]
    private Dialogue currentDialogue; // The currently active Dialogue ScriptableObject
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;

    [Header("Player Interaction")]
    public GameObject dialogueUI; // Reference to the dialogue UI container
    public LayerMask interactableLayer; // Layer mask for interactable characters
    public float interactionRange = 2f; // Range within which the player can interact

    void Start()
    {
        dialogueUI.SetActive(false); // Initially hide the dialogue UI
        nextButton.onClick.AddListener(OnNextButtonClicked); // Add button listener
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) // Press 'E' to interact
        {
            TryStartDialogue();
        }
    }

    void TryStartDialogue()
    {
        //TODO: Check for NPC to interact with 
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer);
        foreach (var hit in hits)
        {
            DialogueTrigger trigger = hit.GetComponent<DialogueTrigger>();
            if (trigger != null && !isDialogueActive)
            {
                StartDialogue(trigger.dialogue);
                break;
            }
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if (dialogue == null || dialogue.dialogueLines.Count == 0)
        {
            Debug.LogWarning("Dialogue data is missing or empty.");
            return;
        }

        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;
        dialogueUI.SetActive(true); // Show dialogue UI

        PlayerMovement.Instance.ChangeAnimation("Idle");
        PlayerMovement.Instance.enabled = false;
        PlayerMovement.Instance.StopMovementSound();
        PlayerAttack.Instance.enabled = false;
        
       
        DisplayLine(currentDialogue.dialogueLines[currentLineIndex]);
    }

    void DisplayLine(Dialogue.DialogueLine line)
    {
        characterNameText.text = line.characterName; // Set character name
        dialogueText.text = line.dialogueText; // Set dialogue text
    }

    public void OnNextButtonClicked()
    {
        if (currentLineIndex < currentDialogue.dialogueLines.Count - 1)
        {
            currentLineIndex++;
            DisplayLine(currentDialogue.dialogueLines[currentLineIndex]);
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        Debug.Log("Dialogue finished.");
        characterNameText.text = "";
        dialogueText.text = "";
        dialogueUI.SetActive(false); // Hide the dialogue UI
        isDialogueActive = false;
        PlayerMovement.Instance.enabled = true;
        PlayerAttack.Instance.enabled = true;
    }

    void OnDrawGizmosSelected()
    {
        // Visualize interaction range in the scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

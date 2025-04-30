using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogueDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI characterNameText; // UI for the character's name
    public TextMeshProUGUI dialogueText; // UI for the dialogue text
    public Button nextButton; // Button to progress the dialogue

    [Header("Dialogue Data")]
    private Dialogue currentDialogue; // The currently active Dialogue ScriptableObject
    private int currentLineIndex = 0;
    public bool isDialogueActive = false;

    [Header("Player Interaction")]
    public GameObject dialogueUI; // Reference to the dialogue UI container
    public LayerMask interactableLayer; // Layer mask for interactable characters
    public float interactionRange = 2f; // Range within which the player can interact
    private GameObject lastHoveredNPC; // Track the last NPC hovered
    private Outline lastNPCOutline; // Track the outline component of the hovered NPC
    private DialogueTrigger lastNPCTrigger; // Track the DialogueTrigger component of the hovered NPC
    private Tween hoverTween; // Track the DOTween animation for the NPC
    
    public event System.Action<Dialogue> OnDialogueEnded;

    [Header("Hover Visual Cue")]
    [SerializeField] private float hoverScale = 1.05f; // Scale factor for NPC hover animation
    [SerializeField] private float hoverDuration = 0.3f; // Duration of the NPC hover animation
    [SerializeField] private int hoverLoops = -1; // -1 for infinite loops
    [SerializeField] private Ease hoverEase = Ease.InOutSine; // Animation ease type

    public static DialogueDisplay Instance { get; private set; }
    public bool IsDialogueActive => isDialogueActive;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple DialogueDisplay instances detected. Destroying this one.", this);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Validate UI elements
        if (dialogueUI == null)
            Debug.LogError("DialogueUI not assigned in DialogueDisplay.");
        else
            dialogueUI.SetActive(false); // Initially hide the dialogue UI

        if (characterNameText == null)
            Debug.LogError("CharacterNameText not assigned in DialogueDisplay.");

        if (dialogueText == null)
            Debug.LogError("DialogueText not assigned in DialogueDisplay.");

        if (nextButton == null)
            Debug.LogError("NextButton not assigned in DialogueDisplay.");
        else
            nextButton.onClick.AddListener(OnNextButtonClicked); // Add button listener
    }

    private void Update()
    {
        // Skip if mouse is over UI or dialogue is active
        if (EventSystem.current.IsPointerOverGameObject() || isDialogueActive)
        {
            ClearHoverOutline();
            return;
        }

        HandleNPCHover();
    }

    private void HandleNPCHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, interactableLayer))
        {
            GameObject hitObject = hit.collider.gameObject;
            DialogueTrigger trigger = hitObject.GetComponent<DialogueTrigger>();

            if (trigger != null)
            {
                // Check if the NPC is within interaction range
                if (Vector3.Distance(transform.position, hitObject.transform.position) <= interactionRange)
                {
                    UpdateHoveredNPC(hitObject, trigger);

                    // Start dialogue on left-click
                    if (Input.GetMouseButtonDown(0))
                    {
                        StartDialogue(trigger.dialogue);
                    }
                }
                else
                {
                    ClearHoverOutline();
                }
            }
            else
            {
                ClearHoverOutline();
            }
        }
        else
        {
            ClearHoverOutline();
        }
    }

    private void UpdateHoveredNPC(GameObject npc, DialogueTrigger trigger)
    {
        if (lastHoveredNPC != npc)
        {
            ClearHoverOutline();
            lastHoveredNPC = npc;
            lastNPCTrigger = trigger;

            // Enable outline
            if (npc.TryGetComponent(out Outline outline))
            {
                outline.enabled = true;
                lastNPCOutline = outline;
            }

            // Trigger visual cue animation
            if (lastNPCTrigger != null)
            {
                lastNPCTrigger.OnHoverEnter();
            }

            // Start NPC scale animation
            Transform npcTransform = npc.transform;
            Vector3 originalScale = npcTransform.localScale;
            hoverTween = npcTransform.DOScale(originalScale * hoverScale, hoverDuration)
                .SetLoops(hoverLoops, LoopType.Yoyo)
                .SetEase(hoverEase)
                .SetUpdate(true); // Ensure animation runs even when game is paused
        }
    }

    private void ClearHoverOutline()
    {
        // Stop NPC scale animation
        if (hoverTween != null)
        {
            hoverTween.Kill();
            hoverTween = null;
            if (lastHoveredNPC != null)
            {
                lastHoveredNPC.transform.DOScale(Vector3.one, 0.2f); // Smoothly return to original scale
            }
        }

        // Disable outline
        if (lastNPCOutline != null)
        {
            lastNPCOutline.enabled = false;
            lastNPCOutline = null;
        }

        // Disable visual cue animation
        if (lastNPCTrigger != null)
        {
            lastNPCTrigger.OnHoverExit();
            lastNPCTrigger = null;
        }

        lastHoveredNPC = null;
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

        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.ChangeAnimation(PlayerMovement.Instance.idleAnimation);
            PlayerMovement.Instance.enabled = false;
            // PlayerMovement.Instance.StopMovementSound();
        }

        if (PlayerAttack.Instance != null)
        {
            PlayerAttack.Instance.enabled = false;
        }

        ClearHoverOutline(); // Disable outline, NPC animation, and visual cue when dialogue starts
        DisplayLine(currentDialogue.dialogueLines[currentLineIndex]);
    }

    private void DisplayLine(Dialogue.DialogueLine line)
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

    private void EndDialogue()
    {
        Debug.Log("Dialogue finished.");
        characterNameText.text = "";
        dialogueText.text = "";
        dialogueUI.SetActive(false); // Hide the dialogue UI
        isDialogueActive = false;

        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.enabled = true;

        if (PlayerAttack.Instance != null)
            PlayerAttack.Instance.enabled = true;
        OnDialogueEnded?.Invoke(currentDialogue); // Trigger the event for dialogue end
        ClearHoverOutline(); // Ensure outline, NPC animation, and visual cue are cleared when dialogue ends
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize interaction range in the scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
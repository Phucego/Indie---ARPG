using TMPro;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private TextMeshProUGUI promptText; // UI text for prompts
    [SerializeField] private GameObject movementTarget; // Glowing quad for movement
    [SerializeField] private GameObject crossbowPickup; // Crossbow pickup
    [SerializeField] private GameObject attackTarget; // Target for attacking
    [SerializeField] private float crossbowActivationDistance = 10f; // Distance to activate crossbow
    [SerializeField] private GameObject portal; // Portal to activate after dialogue
    [SerializeField] private DialogueTrigger dialogueTrigger; // Dialogue trigger for other dialogues (e.g., choices)
    [SerializeField] private DialogueTrigger thankYouDialogueTrigger; // Dialogue trigger for "thank you" dialogue
    [SerializeField] private float portalScaleDuration = 2f; // Duration for portal scale animation

    [Header("Events")]
    public UnityEvent OnFirstEnemyKilled; // Event triggered when the first enemy is killed

    private enum TutorialStep { Movement, Pickup, Attack }
    private TutorialStep currentStep;
    private bool hasShownPickupPrompt = false;

    public static TutorialManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        movementTarget.SetActive(true);
        crossbowPickup.SetActive(false); // Inactive until player is close
        attackTarget.SetActive(false);
        if (portal != null)
        {
            portal.SetActive(false); // Ensure portal is inactive initially
        }
        if (dialogueTrigger != null && dialogueTrigger.visualCue != null)
        {
            dialogueTrigger.visualCue.enabled = false; // Hide dialogue visual cue initially
        }
        if (thankYouDialogueTrigger != null && thankYouDialogueTrigger.visualCue != null)
        {
            thankYouDialogueTrigger.visualCue.enabled = false; // Hide thank you dialogue visual cue initially
        }
        currentStep = TutorialStep.Movement;
        UpdatePrompt("Click the ground to move to the glowing circle.");

        // Subscribe to the OnFirstEnemyKilled event
        OnFirstEnemyKilled.AddListener(OnFirstEnemyKilledHandler);

        // Subscribe to PortalDoorInteractable's OnTeleport event
        PortalDoorInteractable portalInteractable = FindObjectOfType<PortalDoorInteractable>();
        if (portalInteractable != null)
        {
            portalInteractable.OnTeleport.AddListener(OnPlayerTeleported);
        }
    }

    private void OnEnable()
    {
        // Subscribe to DialogueDisplay's OnDialogueEnded
        if (DialogueDisplay.Instance != null)
        {
            DialogueDisplay.Instance.OnDialogueEnded += HandleDialogueEnded;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        if (DialogueDisplay.Instance != null)
        {
            DialogueDisplay.Instance.OnDialogueEnded -= HandleDialogueEnded;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        OnFirstEnemyKilled.RemoveListener(OnFirstEnemyKilledHandler);
    }

    private void Update()
    {
        switch (currentStep)
        {
            case TutorialStep.Movement:
                if (Vector3.Distance(player.position, movementTarget.transform.position) < 1f)
                {
                    movementTarget.SetActive(false);
                    currentStep = TutorialStep.Pickup;
                    UpdatePrompt("Move into the crossbow and click to pick it up.");
                }
                break;

            case TutorialStep.Pickup:
                // Activate crossbow when player is close and it exists
                if (crossbowPickup != null && !crossbowPickup.activeSelf && 
                    Vector3.Distance(player.position, crossbowPickup.transform.position) < crossbowActivationDistance)
                {
                    crossbowPickup.SetActive(true);
                    if (!hasShownPickupPrompt)
                    {
                        UpdatePrompt("Move into the crossbow and click to pick it up.");
                        hasShownPickupPrompt = true;
                    }
                }
                // Check for pickup completion
                if (WeaponManager.Instance != null && WeaponManager.Instance.IsRangedWeaponEquipped)
                {
                    if (crossbowPickup != null)
                    {
                        crossbowPickup.SetActive(false);
                    }
                    attackTarget.SetActive(true);
                    currentStep = TutorialStep.Attack;
                    UpdatePrompt("Click on the skeleton to shoot the crossbow.");
                }
                break;

            case TutorialStep.Attack:
                // Progression now handled by OnFirstEnemyKilled event
                break;
        }
    }

    private void OnFirstEnemyKilledHandler()
    {
        if (currentStep == TutorialStep.Attack)
        {
            attackTarget.SetActive(false); // Ensure target is deactivated
            // Trigger "thank you" dialogue; portal activation is deferred until dialogue ends
            if (thankYouDialogueTrigger != null)
            {
                thankYouDialogueTrigger.TriggerDialogue(); // Start the "thank you" dialogue
            }
        }
    }

    private void HandleDialogueEnded(Dialogue dialogue)
    {
        if (currentStep != TutorialStep.Attack || thankYouDialogueTrigger == null)
            return;

        // Verify the dialogue is the one triggered by thankYouDialogueTrigger
        Dialogue expectedDialogue = thankYouDialogueTrigger.GetCurrentDialogue();
        if (dialogue != expectedDialogue)
            return;

        // Activate and animate portal
        if (portal != null)
        {
            portal.SetActive(true);
            portal.transform.localScale = Vector3.zero; // Start at scale 0
            portal.transform.DOScale(new Vector3(10f, 10f, 10f), portalScaleDuration).SetEase(Ease.OutBounce);
        }

        // Update prompt to guide player to portal
        UpdatePrompt("Enter the portal!");
        if (promptText != null)
        {
            promptText.gameObject.SetActive(true); // Ensure prompt is visible
        }
        if (thankYouDialogueTrigger != null && thankYouDialogueTrigger.visualCue != null)
        {
            thankYouDialogueTrigger.OnHoverExit(); // Hide thank you dialogue visual cue
        }
    }

    private void OnPlayerTeleported()
    {
        // Disable the tutorial manager and prompt when leaving the tutorial area
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false); // Hide the prompt
        }
        enabled = false; // Disable the TutorialManager
    }

    private void UpdatePrompt(string message)
    {
        if (promptText != null)
        {
            promptText.text = message;
        }
        Debug.Log(message); // Fallback for debugging
    }

    // Method to check if an enemy is the tutorial's attack target
    public bool IsTutorialAttackTarget(GameObject enemy)
    {
        return enemy == attackTarget;
    }
}
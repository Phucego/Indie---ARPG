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
    [SerializeField] private DialogueTrigger dialogueTrigger; // Dialogue trigger for other dialogues
    [SerializeField] private DialogueTrigger thankYouDialogueTrigger; // Dialogue trigger for "thank you" dialogue
    [SerializeField] private float portalScaleDuration = 2f; // Duration for portal scale animation

    [Header("Events")]
    public UnityEvent OnFirstEnemyKilled; // Event triggered when the first enemy is killed

    private enum TutorialStep { Movement, Pickup, Attack }
    private TutorialStep currentStep;
    private bool hasShownPickupPrompt = false;
    private bool isPortalPromptEnabled = true;

    public static TutorialManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        movementTarget.SetActive(true);
        crossbowPickup.SetActive(false);
        attackTarget.SetActive(false);
        if (portal != null)
        {
            portal.SetActive(false);
        }
        if (dialogueTrigger != null && dialogueTrigger.visualCue != null)
        {
            dialogueTrigger.visualCue.enabled = false;
        }
        if (thankYouDialogueTrigger != null && thankYouDialogueTrigger.visualCue != null)
        {
            thankYouDialogueTrigger.visualCue.enabled = false;
        }
        currentStep = TutorialStep.Movement;
        UpdatePrompt("Click the ground to move to the glowing circle.");

        OnFirstEnemyKilled.AddListener(OnFirstEnemyKilledHandler);

        PortalDoorInteractable portalInteractable = FindObjectOfType<PortalDoorInteractable>();
        if (portalInteractable != null)
        {
            portalInteractable.OnTeleport.AddListener(OnPlayerTeleported);
        }
    }

    private void OnEnable()
    {
        if (DialogueDisplay.Instance != null)
        {
            DialogueDisplay.Instance.OnDialogueEnded += HandleDialogueEnded;
        }
    }

    private void OnDisable()
    {
        if (DialogueDisplay.Instance != null)
        {
            DialogueDisplay.Instance.OnDialogueEnded -= HandleDialogueEnded;
        }
    }

    private void OnDestroy()
    {
        OnFirstEnemyKilled.RemoveListener(OnFirstEnemyKilledHandler);
        PortalDoorInteractable portalInteractable = FindObjectOfType<PortalDoorInteractable>();
        if (portalInteractable != null)
        {
            portalInteractable.OnTeleport.RemoveListener(OnPlayerTeleported);
        }
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
                break;
        }
    }

    private void OnFirstEnemyKilledHandler()
    {
        if (currentStep == TutorialStep.Attack)
        {
            attackTarget.SetActive(false);
            if (thankYouDialogueTrigger != null)
            {
                thankYouDialogueTrigger.TriggerDialogue();
            }
        }
    }

    private void HandleDialogueEnded(Dialogue dialogue)
    {
        if (currentStep != TutorialStep.Attack || thankYouDialogueTrigger == null)
            return;

        Dialogue expectedDialogue = thankYouDialogueTrigger.GetCurrentDialogue();
        if (dialogue != expectedDialogue)
            return;

        if (portal != null)
        {
            portal.SetActive(true);
            portal.transform.localScale = Vector3.zero;
            portal.transform.DOScale(new Vector3(10f, 10f, 10f), portalScaleDuration).SetEase(Ease.OutBounce);
        }

        if (isPortalPromptEnabled)
        {
            UpdatePrompt("Enter the portal!");
        }
        if (promptText != null)
        {
            promptText.gameObject.SetActive(true);
        }
        if (thankYouDialogueTrigger != null && thankYouDialogueTrigger.visualCue != null)
        {
            thankYouDialogueTrigger.OnHoverExit();
        }
    }

    private void OnPlayerTeleported()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
            UpdatePrompt("");
        }
        
        enabled = false;
    }

    public void UpdatePrompt(string message)
    {
        if (promptText != null)
        {
            promptText.text = message;
        }
        Debug.Log($"[TutorialManager] Prompt: {message}");
    }

    public void SetPortalPromptEnabled(bool enabled)
    {
        isPortalPromptEnabled = enabled;
    }

    public bool IsTutorialAttackTarget(GameObject enemy)
    {
        return enemy == attackTarget;
    }
}
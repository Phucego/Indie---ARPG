using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogueDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI dialogueText;
    public Button nextButton;

    [Header("Dialogue Data")]
    private Dialogue currentDialogue;
    private int currentLineIndex = 0;
    public bool isDialogueActive = false;

    [Header("Player Interaction")]
    public GameObject dialogueUI;
    public LayerMask interactableLayer;
    public float interactionRange = 2f;
    private GameObject lastHoveredNPC;
    private Outline lastNPCOutline;
    private DialogueTrigger lastNPCTrigger;
    private Tween hoverTween;

    public event System.Action<Dialogue> OnDialogueEnded;

    [Header("Hover Visual Cue")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float hoverDuration = 0.3f;
    [SerializeField] private int hoverLoops = -1;
    [SerializeField] private Ease hoverEase = Ease.InOutSine;

    [Header("Choice Options")]
    public Button choiceFireButton;
    public Button choiceExplosiveButton;
    public GameObject choicePanel;
    public Dialogue fireOrExplosiveChoiceDialogue;
    public UnityEvent onFireChosen;
    public UnityEvent onExplosiveChosen;

    public static DialogueDisplay Instance { get; private set; }
    public bool IsDialogueActive => isDialogueActive;

    private void Awake()
    {
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
        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        nextButton?.onClick.AddListener(OnNextButtonClicked);

        if (choicePanel != null)
            choicePanel.SetActive(false);
    }

    private void Update()
    {
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
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactableLayer))
        {
            var trigger = hit.collider.GetComponent<DialogueTrigger>();
            if (trigger != null && Vector3.Distance(transform.position, hit.collider.transform.position) <= interactionRange)
            {
                UpdateHoveredNPC(hit.collider.gameObject, trigger);

                if (Input.GetMouseButtonDown(0))
                    StartDialogue(trigger.GetCurrentDialogue());
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

            if (npc.TryGetComponent(out Outline outline))
            {
                outline.enabled = true;
                lastNPCOutline = outline;
            }

            lastNPCTrigger?.OnHoverEnter();

            var npcTransform = npc.transform;
            var originalScale = npcTransform.localScale;
            hoverTween = npcTransform.DOScale(originalScale * hoverScale, hoverDuration)
                .SetLoops(hoverLoops, LoopType.Yoyo)
                .SetEase(hoverEase)
                .SetUpdate(true);
        }
    }

    private void ClearHoverOutline()
    {
        hoverTween?.Kill();
        hoverTween = null;

        if (lastHoveredNPC != null)
            lastHoveredNPC.transform.DOScale(Vector3.one, 0.2f);

        if (lastNPCOutline != null)
        {
            lastNPCOutline.enabled = false;
            lastNPCOutline = null;
        }

        lastNPCTrigger?.OnHoverExit();
        lastNPCTrigger = null;
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
        dialogueUI.SetActive(true);

        PlayerMovement.Instance?.ChangeAnimation(PlayerMovement.Instance.idleAnimation);
        if (PlayerMovement.Instance != null) PlayerMovement.Instance.enabled = false;
        if (PlayerAttack.Instance != null) PlayerAttack.Instance.enabled = false;

        ClearHoverOutline();
        DisplayLine(currentDialogue.dialogueLines[currentLineIndex]);
    }

    private void DisplayLine(Dialogue.DialogueLine line)
    {
        characterNameText.text = line.characterName;
        dialogueText.text = line.dialogueText;
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
            if (currentDialogue == fireOrExplosiveChoiceDialogue)
                ShowChoiceOptions();
            else
                EndDialogue();
        }
    }

    private void ShowChoiceOptions()
    {
        choicePanel?.SetActive(true);

        characterNameText?.gameObject.SetActive(false);
        dialogueText?.gameObject.SetActive(false);
        nextButton?.gameObject.SetActive(false);

        choiceFireButton.onClick.AddListener(() =>
        {
            onFireChosen?.Invoke();
            EndDialogue();
        });

        choiceExplosiveButton.onClick.AddListener(() =>
        {
            onExplosiveChosen?.Invoke();
            EndDialogue();
        });
    }

    private void EndDialogue()
    {
        Debug.Log("Dialogue finished.");
        isDialogueActive = false;

        dialogueUI?.SetActive(false);

        PlayerMovement.Instance.enabled = true;
        PlayerAttack.Instance.enabled = true;

        characterNameText.text = "";
        dialogueText.text = "";

        OnDialogueEnded?.Invoke(currentDialogue);
        ClearHoverOutline();

        // Hide and reset choice UI
        choicePanel?.SetActive(false);
        choiceFireButton.onClick.RemoveAllListeners();
        choiceExplosiveButton.onClick.RemoveAllListeners();

        // Restore dialogue UI elements
        characterNameText?.gameObject.SetActive(true);
        dialogueText?.gameObject.SetActive(true);
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);
            nextButton.interactable = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Options")]
    public Dialogue defaultDialogue;
    public Dialogue fireDialogue;
    public Dialogue explosiveDialogue;
    public Dialogue thankYouDialogue;
    public Dialogue tutorialDialogue; // New dialogue for tutorial

    private Animator anim;
    [Tooltip("Set by event when the player chooses fire.")]
    public bool fireChosen;

    [Tooltip("Set by event when the player chooses explosive.")]
    public bool explosiveChosen;

    [Tooltip("Set when the thank you dialogue is triggered.")]
    private bool thankYouTriggered;

    [Tooltip("Tracks if tutorial dialogue has been shown.")]
    private bool tutorialShown;

    [Header("Visual Cue UI")]
    public Image visualCue;

    [Header("Visual Cue Animation")]
    [SerializeField] private float popScale = 1.2f;
    [SerializeField] private float popDuration = 0.3f;
    [SerializeField] private Ease popEase = Ease.OutBack;
    [SerializeField] private float hoverPulseScale = 1.05f;
    [SerializeField] private float hoverPulseDuration = 0.5f;
    [SerializeField] private Ease hoverPulseEase = Ease.InOutSine;
    private Tween popTween;
    private Vector3 originalScale;
    private RangedAttackUpgrade rangedAttackUpgrade;

    private void Awake()
    {
        if (visualCue == null)
        {
            Debug.LogError("VisualCue Image not assigned in DialogueTrigger.", this);
        }
        else
        {
            visualCue.enabled = false;
            originalScale = visualCue.transform.localScale;
        }

        rangedAttackUpgrade = GetComponent<RangedAttackUpgrade>();
        if (rangedAttackUpgrade == null)
        {
            Debug.LogWarning("RangedAttackUpgrade not found on this GameObject. Fire/Explosive choices won't affect upgrades.", this);
        }

        anim = GetComponent<Animator>();
        
        TutorialManager.Instance.UpdatePrompt("");
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

    private void Start()
    {
        // Trigger tutorial dialogue on first interaction
        if (!tutorialShown && tutorialDialogue != null)
        {
            TriggerDialogue(false);
        }
    }

    public Dialogue GetCurrentDialogue()
    {
        if (!tutorialShown && tutorialDialogue != null)
            return tutorialDialogue;
        if (thankYouTriggered)
            return thankYouDialogue;
        // Return null to prevent any dialogue until skeleton is killed
        return null;
    }

    public void ChooseFire()
    {
        fireChosen = true;
        explosiveChosen = false;
        Debug.Log("Player chose FIRE arrows.");
        if (rangedAttackUpgrade != null)
        {
            rangedAttackUpgrade.EnableFireProjectile();
        }
    }

    public void ChooseExplosive()
    {
        explosiveChosen = true;
        fireChosen = false;
        Debug.Log("Player chose EXPLOSIVE arrows.");
        if (rangedAttackUpgrade != null)
        {
            rangedAttackUpgrade.EnableExplosiveProjectile();
        }
    }

    public void TriggerDialogue(bool isThankYou = false)
    {
        if (visualCue == null) return;

        thankYouTriggered = isThankYou;

        visualCue.enabled = true;

        if (popTween != null)
        {
            popTween.Kill();
            popTween = null;
        }

        popTween = visualCue.transform.DOScale(originalScale * popScale, popDuration)
            .SetEase(popEase)
            .OnComplete(() =>
            {
                popTween = visualCue.transform.DOScale(originalScale * hoverPulseScale, hoverPulseDuration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(hoverPulseEase);
            });

        Dialogue currentDialogue = GetCurrentDialogue();
        if (currentDialogue != null)
        {
            Debug.Log($"[DialogueTrigger] Triggering dialogue: {currentDialogue.name}");
            DialogueDisplay.Instance.StartDialogue(currentDialogue);
        }
        else
        {
            Debug.LogWarning("[DialogueTrigger] No dialogue assigned.");
        }
    }

    public void OnHoverEnter()
    {
        if (visualCue == null) return;

        visualCue.enabled = true;

        if (popTween != null)
        {
            popTween.Kill();
            popTween = null;
        }

        popTween = visualCue.transform.DOScale(originalScale * popScale, popDuration)
            .SetEase(popEase)
            .OnComplete(() =>
            {
                popTween = visualCue.transform.DOScale(originalScale * hoverPulseScale, hoverPulseDuration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(hoverPulseEase);
            });
    }

    public void OnHoverExit()
    {
        if (visualCue == null) return;

        if (popTween != null)
        {
            popTween.Kill();
            popTween = null;
        }

        visualCue.transform.DOScale(originalScale, 0.2f)
            .OnComplete(() =>
            {
                if (visualCue != null)
                    visualCue.enabled = false;
            });
    }

    private void HandleDialogueEnded(Dialogue dialogue)
    {
        if (dialogue == tutorialDialogue)
        {
            tutorialShown = true;
            // Update movement prompt for mouse movement using TutorialManager
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.UpdatePrompt("Use mouse to move.");
            }
        }
        else if (dialogue == thankYouDialogue)
        {
            thankYouTriggered = false;
            if (anim != null)
            {
                anim.Play("Lie_Down");
            }
        }
    }

    private void OnDestroy()
    {
        if (popTween != null)
        {
            popTween.Kill();
            popTween = null;
        }
    }
}
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Options")]
    public Dialogue defaultDialogue;
    public Dialogue fireDialogue;
    public Dialogue explosiveDialogue;
    public Dialogue thankYouDialogue; // New dialogue for "thank you"

    [Tooltip("Set by event when the player chooses fire.")]
    public bool fireChosen;

    [Tooltip("Set by event when the player chooses explosive.")]
    public bool explosiveChosen;

    [Tooltip("Set when the thank you dialogue is triggered.")]
    private bool thankYouTriggered;

    [Header("Visual Cue UI")]
    public Image visualCue;

    [Header("Visual Cue Animation")]
    [SerializeField] private float popScale = 1.2f;
    [SerializeField] private float popDuration = 0.3f;
    [SerializeField] private Ease popEase = Ease.OutBack;
    [SerializeField] private float hoverPulseScale = 1.05f;
    [SerializeField] private float hoverPulseDuration = 0.5f;
    [SerializeField] private Ease hoverPulseEase = Ease.InOutSine;

    public Dialogue dialogue;
    private Tween popTween;
    private Vector3 originalScale;
    private RangedAttackUpgrade rangedAttackUpgrade; // Reference to RangedAttackUpgrade

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

        // Get the RangedAttackUpgrade component
        rangedAttackUpgrade = GetComponent<RangedAttackUpgrade>();
        if (rangedAttackUpgrade == null)
        {
            Debug.LogWarning("RangedAttackUpgrade not found on this GameObject. Fire/Explosive choices won't affect upgrades.", this);
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

    private void Start()
    {
        // Find the first enemy and subscribe to its OnEnemyKilled event
        EnemyHealth[] enemies = FindObjectsOfType<EnemyHealth>();
        foreach (var enemy in enemies)
        {
            if (enemy.GetComponent<EnemyHealth>().isFirstEnemy)
            {
                enemy.OnEnemyKilled.AddListener(() => TriggerDialogue(true));
                break;
            }
        }
    }

    /// <summary>
    /// Returns the appropriate dialogue based on state.
    /// </summary>
    public Dialogue GetCurrentDialogue()
    {
        if (thankYouTriggered) return thankYouDialogue;
        if (fireChosen) return fireDialogue;
        if (explosiveChosen) return explosiveDialogue;
        return defaultDialogue;
    }

    /// <summary>
    /// Can be hooked to a UI button or event to choose fire.
    /// </summary>
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

    /// <summary>
    /// Can be hooked to a UI button or event to choose explosive.
    /// </summary>
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

    /// <summary>
    /// Triggers the dialogue with visual cue animations.
    /// </summary>
    /// <param name="isThankYou">If true, triggers the thank you dialogue.</param>
    public void TriggerDialogue(bool isThankYou = false)
    {
        if (visualCue == null) return;

        thankYouTriggered = isThankYou; // Set flag for thank you dialogue

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

        // Trigger the dialogue
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
        // Reset thankYouTriggered if the completed dialogue was thankYouDialogue
        if (dialogue == thankYouDialogue)
        {
            thankYouTriggered = false;
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
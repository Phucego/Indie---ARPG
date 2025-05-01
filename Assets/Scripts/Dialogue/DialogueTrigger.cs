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
    public Dialogue tutstartDialogue;

    private Animator anim;
    [Tooltip("Set by event when the player chooses fire.")]
    public bool fireChosen;
    [Tooltip("Set by event when the player chooses explosive.")]
    public bool explosiveChosen;
    [Tooltip("Set when the first enemy is killed to trigger thank you dialogue.")]
    private bool firstEnemyKilled;

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

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.UpdatePrompt("");
        }
    }

    private void OnEnable()
    {
        if (DialogueDisplay.Instance != null)
        {
            DialogueDisplay.Instance.OnDialogueEnded += HandleDialogueEnded;
        }
        // Subscribe to first enemy killed event
        GameObject firstEnemy = GameObject.FindGameObjectWithTag("FirstEnemy");
        if (firstEnemy != null)
        {
            EnemyHealth enemyHealth = firstEnemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.OnEnemyKilled.AddListener(OnFirstEnemyKilled);
            }
        }
    }

    private void OnDisable()
    {
        if (DialogueDisplay.Instance != null)
        {
            DialogueDisplay.Instance.OnDialogueEnded -= HandleDialogueEnded;
        }
        // Unsubscribe from enemy event
        GameObject firstEnemy = GameObject.FindGameObjectWithTag("FirstEnemy");
        if (firstEnemy != null)
        {
            EnemyHealth enemyHealth = firstEnemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.OnEnemyKilled.RemoveListener(OnFirstEnemyKilled);
            }
        }
    }

    private void Start()
    {
        // Trigger tutstart dialogue when the game starts
        if (tutstartDialogue != null)
        {
            TriggerDialogue();
        }
    }

    private void OnFirstEnemyKilled()
    {
        firstEnemyKilled = true;
        TriggerDialogue(true); // Force thank you dialogue
        
    }

    public Dialogue GetCurrentDialogue()
    {
        if (firstEnemyKilled)
            return thankYouDialogue;
        return tutstartDialogue;
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

        Dialogue currentDialogue = isThankYou ? thankYouDialogue : GetCurrentDialogue();
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
        if (dialogue == tutstartDialogue)
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.UpdatePrompt("Use mouse to move.");
            }
        }
        else if (dialogue == thankYouDialogue)
        {
           
            firstEnemyKilled = false;
            if (anim != null)
            {
                anim.Play("Lie_Down");
            }
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.SetPortalPromptEnabled(false);
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
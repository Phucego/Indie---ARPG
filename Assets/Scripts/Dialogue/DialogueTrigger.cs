using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Options")]
    public Dialogue defaultDialogue;
    public Dialogue fireDialogue;
    public Dialogue explosiveDialogue;

    [Tooltip("Set by event when the player chooses fire.")]
    public bool fireChosen;

    [Tooltip("Set by event when the player chooses explosive.")]
    public bool explosiveChosen;

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
    }

    /// <summary>
    /// Returns the appropriate dialogue based on player choice.
    /// </summary>
    public Dialogue GetCurrentDialogue()
    {
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
    }

    /// <summary>
    /// Can be hooked to a UI button or event to choose explosive.
    /// </summary>
    public void ChooseExplosive()
    {
        explosiveChosen = true;
        fireChosen = false;
        Debug.Log("Player chose EXPLOSIVE arrows.");
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

    private void OnDestroy()
    {
        if (popTween != null)
        {
            popTween.Kill();
            popTween = null;
        }
    }
}

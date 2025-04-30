using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue; // The Dialogue ScriptableObject for this character
    public Image visualCue; // UI Image to show as a visual cue

    [Header("Visual Cue Animation")]
    [SerializeField] private float popScale = 1.2f; // Scale factor for pop-out animation
    [SerializeField] private float popDuration = 0.3f; // Duration of the pop animation
    [SerializeField] private Ease popEase = Ease.OutBack; // Animation ease type
    [SerializeField] private float hoverPulseScale = 1.05f; // Scale for subtle pulse during hover
    [SerializeField] private float hoverPulseDuration = 0.5f; // Duration of pulse cycle
    [SerializeField] private Ease hoverPulseEase = Ease.InOutSine; // Pulse ease type

    private Tween popTween; // Track the DOTween animation
    private Vector3 originalScale; // Store the original scale of the visual cue

    private void Awake()
    {
        // Validate visualCue
        if (visualCue == null)
        {
            Debug.LogError("VisualCue Image not assigned in DialogueTrigger.", this);
        }
        else
        {
            // Disable visualCue by default
            visualCue.enabled = false;
            originalScale = visualCue.transform.localScale;
        }
    }

    public void OnHoverEnter()
    {
        if (visualCue == null) return;

        // Enable the visual cue
        visualCue.enabled = true;

        // Stop any existing animation
        if (popTween != null)
        {
            popTween.Kill();
            popTween = null;
        }

        // Pop-out animation followed by subtle pulse
        popTween = visualCue.transform.DOScale(originalScale * popScale, popDuration)
            .SetEase(popEase)
            .OnComplete(() =>
            {
                // Start pulsing animation
                popTween = visualCue.transform.DOScale(originalScale * hoverPulseScale, hoverPulseDuration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(hoverPulseEase);
            });
    }

    public void OnHoverExit()
    {
        if (visualCue == null) return;

        // Stop animation and reset scale
        if (popTween != null)
        {
            popTween.Kill();
            popTween = null;
        }

        visualCue.transform.DOScale(originalScale, 0.2f)
            .OnComplete(() =>
            {
                // Disable visual cue after animation
                if (visualCue != null)
                    visualCue.enabled = false;
            });
    }

    private void OnDestroy()
    {
        // Clean up DOTween animations
        if (popTween != null)
        {
            popTween.Kill();
            popTween = null;
        }
    }
}
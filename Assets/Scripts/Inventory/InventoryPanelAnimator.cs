using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class InventoryPanelAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform inventoryPanel; // The panel to animate
    [SerializeField] private CanvasGroup canvasGroup; // For fading the panel
    [SerializeField] private GameObject panelGameObject; // The GameObject to toggle active state

    [Header("Animation Settings")]
    [SerializeField] private float slideDuration = 0.5f; // Duration of the slide animation (same for open/close)
    [SerializeField] private float offScreenOffset = 500f; // How far off-screen to the right
    [SerializeField] private float fadeDuration = 0.3f; // Duration of the fade animation
    [SerializeField] private float punchScaleAmount = 0.1f; // Scale amount for punch effect
    [SerializeField] private float punchScaleDuration = 0.2f; // Duration of punch scale effect
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab; // Key to toggle panel
    [SerializeField] private bool enableAnimations = true; // Toggle animations on/off

    private Vector2 onScreenPosition; // Position when panel is visible
    private Vector2 offScreenPosition; // Position when panel is hidden
    private bool isPanelOpen = false;
    private bool isInitialized = false; // Tracks initialization
    private bool isOpening = false; // Tracks if in opening animation
    private Sequence animationSequence; // DOTween sequence for coordinated animations

    private void Start()
    {
        // Validate required components
        if (inventoryPanel == null || canvasGroup == null || panelGameObject == null)
        {
            Debug.LogError("InventoryPanel, CanvasGroup, or PanelGameObject not assigned! Disabling script.");
            enabled = false;
            return;
        }

        // Store the panel's initial (on-screen) position
        onScreenPosition = inventoryPanel.anchoredPosition;
        offScreenPosition = onScreenPosition + new Vector2(offScreenOffset, 0f);

        // Initialize panel state
        InitializePanel();
        isInitialized = true;
    }

    private void InitializePanel()
    {
        // Start with panel off-screen and transparent
        inventoryPanel.anchoredPosition = offScreenPosition;
        canvasGroup.alpha = 0f;
        panelGameObject.SetActive(false);
        isPanelOpen = false;
        isOpening = false;
    }

    private void Update()
    {
        // Skip input if not initialized or over UI
        if (!isInitialized || IsPointerOverUI()) return;

        if (Input.GetKeyDown(toggleKey))
        {
            TogglePanel();
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private void TogglePanel()
    {
        // Toggle panel state
        isPanelOpen = !isPanelOpen;

        // Update player input state
        UpdatePlayerInputState();

        // Log panel state for debugging
        Debug.Log($"Toggling panel: isPanelOpen={isPanelOpen}, isOpening={isOpening}, active={panelGameObject.activeSelf}");

        if (enableAnimations)
        {
            // Kill any existing animation sequence
            if (animationSequence != null)
            {
                animationSequence.Kill();
                animationSequence = null;
            }

            // Handle immediate close during opening animation
            if (!isPanelOpen && isOpening)
            {
                // Immediately deactivate and reset to off-screen
                panelGameObject.SetActive(false);
                inventoryPanel.anchoredPosition = offScreenPosition;
                canvasGroup.alpha = 0f;
                isOpening = false;
                Debug.Log("Immediate close during opening, panel deactivated.");
                return;
            }

            // Set target position and alpha based on panel state
            Vector2 targetPosition = isPanelOpen ? onScreenPosition : offScreenPosition;
            float targetAlpha = isPanelOpen ? 1f : 0f;

            // Activate panel immediately for opening
            if (isPanelOpen)
            {
                panelGameObject.SetActive(true);
                isOpening = true;
            }

            // Create a new sequence
            animationSequence = DOTween.Sequence();

            // Add slide animation (same duration and easing for open/close)
            animationSequence.Append(inventoryPanel.DOAnchorPos(targetPosition, slideDuration)
                .SetEase(Ease.InOutQuad));

            // Add fade animation (run concurrently)
            animationSequence.Join(canvasGroup.DOFade(targetAlpha, fadeDuration));

            // Add punch scale effect when opening
            if (isPanelOpen)
            {
                animationSequence.Join(inventoryPanel.DOPunchScale(Vector3.one * punchScaleAmount, punchScaleDuration, 1, 0.5f));
            }

            // Handle completion (deactivate after closing, clear isOpening)
            animationSequence.OnComplete(() =>
            {
                if (!isPanelOpen)
                {
                    panelGameObject.SetActive(false);
                    Debug.Log("Closing animation complete, panel deactivated.");
                }
                isOpening = false;
                animationSequence = null;
            });
        }
        else
        {
            // No animations, snap to position
            inventoryPanel.anchoredPosition = isPanelOpen ? onScreenPosition : offScreenPosition;
            canvasGroup.alpha = isPanelOpen ? 1f : 0f;
            panelGameObject.SetActive(isPanelOpen);
            isOpening = false;
        }
    }

    private void UpdatePlayerInputState()
    {
        // Disable/enable player movement and attacks
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.canMove = !isPanelOpen;
        }
        if (PlayerAttack.Instance != null)
        {
            // Assuming PlayerAttack has a canAttack flag; add if needed
            // PlayerAttack.Instance.canAttack = !isPanelOpen;
        }
    }

    private void OnDestroy()
    {
        // Cleanup DOTween animations to prevent memory leaks
        if (animationSequence != null) animationSequence.Kill();
        if (inventoryPanel != null) inventoryPanel.DOKill();
        if (canvasGroup != null) canvasGroup.DOKill();
    }

    // Public method to programmatically toggle the panel
    public void SetPanelState(bool open)
    {
        if (isPanelOpen != open)
        {
            TogglePanel();
        }
    }
}
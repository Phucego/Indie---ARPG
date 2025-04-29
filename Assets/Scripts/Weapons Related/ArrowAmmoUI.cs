using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; 
public class ArrowAmmoUI : MonoBehaviour
{
    public static ArrowAmmoUI Instance;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI ammoText; // Text to display ammo count
    [SerializeField] private Image lowAmmoWarning; // Warning image for low ammo
    [SerializeField] private float lowAmmoThreshold = 1; // Ammo count at which warning appears
    [SerializeField] private float flashDuration = 0.5f; // Duration of flash animation
    private Tween warningTween;
    
    [SerializeField] private Image arrowIcon;
    [SerializeField] private float popScale = 1.2f;
    [SerializeField] private float popDuration = 0.15f;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple ArrowAmmoUI instances detected. Destroying this one.", this);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (ammoText == null)
            Debug.LogError("AmmoText is not assigned in ArrowAmmoUI.", this);
        if (lowAmmoWarning == null)
            Debug.LogWarning("LowAmmoWarning is not assigned in ArrowAmmoUI.", this);

        // Animate arrow icon on startup
        if (arrowIcon != null)
        {
            arrowIcon.transform.localScale = Vector3.zero;
            arrowIcon.DOFade(0f, 0f); // Ensure hidden at start
            arrowIcon.DOFade(1f, 0.3f).SetEase(Ease.InOutSine);
            arrowIcon.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetDelay(0.1f);
        }

        UpdateAmmoUI(ArrowAmmoManager.Instance.GetCurrentAmmo(), ArrowAmmoManager.Instance.GetMaxAmmo());
    }


    public void UpdateAmmoUI(int currentAmmo, int maxAmmo)
    {
        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo}/{maxAmmo}";

            // Pop animation
            ammoText.transform.DOKill(); // Kill any existing tweens
            ammoText.transform.localScale = Vector3.one;
            ammoText.transform.DOScale(popScale, popDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    ammoText.transform.DOScale(1f, popDuration).SetEase(Ease.InQuad);
                });
        }


        if (lowAmmoWarning != null)
        {
            bool showWarning = currentAmmo <= lowAmmoThreshold;

            if (showWarning)
            {
                if (warningTween == null || !warningTween.IsActive())
                {
                    lowAmmoWarning.enabled = true;
                    lowAmmoWarning.color = Color.red;
                    warningTween = lowAmmoWarning.DOFade(0f, flashDuration)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);
                }
            }
            else
            {
                if (warningTween != null && warningTween.IsActive())
                {
                    warningTween.Kill(); // Stop the flashing
                }

                lowAmmoWarning.DOFade(0f, 0.1f).OnComplete(() =>
                {
                    lowAmmoWarning.enabled = false;
                    lowAmmoWarning.color = new Color(lowAmmoWarning.color.r, lowAmmoWarning.color.g, lowAmmoWarning.color.b, 1f); // Reset alpha
                });
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArrowAmmoUI : MonoBehaviour
{
    public static ArrowAmmoUI Instance;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI ammoText; // Text to display ammo count
    [SerializeField] private Image lowAmmoWarning; // Warning image for low ammo
    [SerializeField] private float lowAmmoThreshold = 1; // Ammo count at which warning appears
    [SerializeField] private float flashDuration = 0.5f; // Duration of flash animation

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

        UpdateAmmoUI(ArrowAmmoManager.Instance.GetCurrentAmmo(), ArrowAmmoManager.Instance.GetMaxAmmo());
    }

    public void UpdateAmmoUI(int currentAmmo, int maxAmmo)
    {
        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo}/{maxAmmo}";
        }

        if (lowAmmoWarning != null)
        {
            bool showWarning = currentAmmo <= lowAmmoThreshold;
            lowAmmoWarning.enabled = showWarning;

            if (showWarning)
            {
                lowAmmoWarning.color = Color.red;
                lowAmmoWarning.CrossFadeAlpha(0f, flashDuration, false);
                lowAmmoWarning.CrossFadeAlpha(1f, flashDuration, false);
            }
        }
    }
}
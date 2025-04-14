using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject enemyHealthUI;
    [SerializeField] private TextMeshProUGUI enemyNameText;
    [SerializeField] private Slider enemyHealthSlider;
    [SerializeField] private CanvasGroup uiCanvasGroup;

    private Coroutine fadeCoroutine;
    private Coroutine hideDelayCoroutine;

    private float fadeDuration = 0.5f;
    private float hideDelay = 2f; // Default hide delay for hover behavior
    private float attackHideDelay = 3f; // Hide delay after attack

    private EnemyHealth currentEnemyHealth;

    private void Awake()
    {
        if (uiCanvasGroup == null && enemyHealthUI != null)
        {
            uiCanvasGroup = enemyHealthUI.GetComponent<CanvasGroup>();
            if (uiCanvasGroup == null)
            {
                Debug.LogWarning("CanvasGroup not found on enemyHealthUI. Please add one.");
            }
        }
    }

    private void Start()
    {
        HideEnemyHealthBar();
    }

    private void OnDisable()
    {
        StopActiveCoroutines();
    }

    private void Update()
    {
        if (currentEnemyHealth != null && enemyHealthUI.activeSelf)
        {
            // Update the health slider only if there's a valid enemy health to display
            if (enemyHealthSlider != null)
                enemyHealthSlider.value = currentEnemyHealth.GetCurrentHealth();
        }
    }

    public void UpdateEnemyTarget(EnemyHealth enemy)
    {
        if (enemy == null || enemy == currentEnemyHealth) return;

        currentEnemyHealth = enemy;

        // Use enemy name from the GameObject
        string displayName = enemy.gameObject.name;

        ShowEnemyHealthBar(displayName, enemy.GetCurrentHealth(), enemy.maxHealth);
    }

    public void SetVisibility(bool show)
    {
        if (show)
        {
            ShowInstant();
            RestartHideDelay(hideDelay); // Regular hide delay (hover-based)
        }
        else
        {
            HideEnemyHealthBar();
        }
    }

    // Show the health UI instantly
    public void ShowInstant()
    {
        StopActiveCoroutines();

        if (!enemyHealthUI.activeSelf)
            enemyHealthUI.SetActive(true);

        if (uiCanvasGroup != null)
            uiCanvasGroup.alpha = 1f;
    }

    // Show health bar with the enemy's name and health stats
    public void ShowEnemyHealthBar(string enemyName, float currentHealth, float maxHealth)
    {
        if (enemyHealthUI == null || uiCanvasGroup == null || enemyNameText == null || enemyHealthSlider == null)
        {
            Debug.LogWarning("EnemyUIManager: Missing UI references.");
            return;
        }

        StopActiveCoroutines();

        if (!enemyHealthUI.activeSelf)
            enemyHealthUI.SetActive(true);

        enemyNameText.text = enemyName;
        enemyHealthSlider.maxValue = maxHealth;
        enemyHealthSlider.value = currentHealth;

        uiCanvasGroup.alpha = 1f;
        RestartHideDelay(hideDelay); // Regular hide delay (hover-based)
    }

    // Hide the enemy health UI
    public void HideEnemyHealthBar()
    {
        StopActiveCoroutines();

        if (enemyHealthUI != null)
            enemyHealthUI.SetActive(false);

        if (uiCanvasGroup != null)
            uiCanvasGroup.alpha = 0f;

        currentEnemyHealth = null;
    }

    // Restart the hide delay coroutine
    private void RestartHideDelay(float delay)
    {
        if (hideDelayCoroutine != null)
        {
            StopCoroutine(hideDelayCoroutine);
        }
        hideDelayCoroutine = StartCoroutine(HideAfterDelay(delay));
    }

    // Handle the hiding of the UI after the specified delay
    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        fadeCoroutine = StartCoroutine(FadeOutUI());
    }

    // Handle the fade-out animation for the UI
    private IEnumerator FadeOutUI()
    {
        if (uiCanvasGroup == null) yield break;

        float elapsedTime = 0f;
        float startAlpha = uiCanvasGroup.alpha;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            uiCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);

            yield return null;
        }

        if (enemyHealthUI != null)
            enemyHealthUI.SetActive(false);

        currentEnemyHealth = null;
        fadeCoroutine = null;
        hideDelayCoroutine = null;
    }

    // Stop all active coroutines related to hiding and fading
    private void StopActiveCoroutines()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        if (hideDelayCoroutine != null)
        {
            StopCoroutine(hideDelayCoroutine);
            hideDelayCoroutine = null;
        }
    }

    // Call this method when the enemy is attacked, to trigger the hide after 3 seconds
    public void OnEnemyAttacked()
    {
        StopActiveCoroutines(); // Stop any ongoing hide delay coroutines
        RestartHideDelay(attackHideDelay); // Set the hide delay to 3 seconds after attack
    }
}

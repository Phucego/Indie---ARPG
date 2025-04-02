using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject enemyHealthUI; // Panel for enemy health UI
    [SerializeField] private TextMeshProUGUI enemyNameText;
    [SerializeField] private Slider enemyHealthSlider;
    [SerializeField] private CanvasGroup uiCanvasGroup; // CanvasGroup for fade effect

    private Coroutine fadeCoroutine;
    private float fadeDuration = 0.5f; // Time it takes to fade out
    private float hideDelay = 2f; // Delay before starting fade-out

    private void Start()
    {
        if (uiCanvasGroup == null)
            uiCanvasGroup = enemyHealthUI.GetComponent<CanvasGroup>();

        HideEnemyHealthBar();
    }

    public void ShowEnemyHealthBar(string enemyName, float currentHealth, float maxHealth)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine); // Stop any fade-out in progress

        enemyHealthUI.SetActive(true);
        uiCanvasGroup.alpha = 1f; // Ensure UI is fully visible
        enemyNameText.text = enemyName;
        enemyHealthSlider.maxValue = maxHealth;
        enemyHealthSlider.value = currentHealth;

        // Restart the fade-out timer
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);

        // Start fading out UI
        fadeCoroutine = StartCoroutine(FadeOutUI());
    }

    private IEnumerator FadeOutUI()
    {
        float elapsedTime = 0f;
        float startAlpha = uiCanvasGroup.alpha;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            uiCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        enemyHealthUI.SetActive(false);
    }

    public void HideEnemyHealthBar()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        enemyHealthUI.SetActive(false);
        uiCanvasGroup.alpha = 0f;
    }
}

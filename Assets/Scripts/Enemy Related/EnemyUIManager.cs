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
    private float hideDelay = 2f;

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
            enemyHealthSlider.value = currentEnemyHealth.GetCurrentHealth();
        }
    }

    public void UpdateEnemyTarget(EnemyHealth enemy)
    {
        if (enemy == null || enemy == currentEnemyHealth) return;

        currentEnemyHealth = enemy;
        ShowEnemyHealthBar(enemy.enemyName, enemy.GetCurrentHealth(), enemy.maxHealth);
    }

    public void SetVisibility(bool show)
    {
        if (show)
        {
            ShowInstant();
            RestartHideDelay();
        }
        else
        {
            HideEnemyHealthBar();
        }
    }

    public void ShowInstant()
    {
        StopActiveCoroutines();

        if (!enemyHealthUI.activeSelf)
            enemyHealthUI.SetActive(true);

        if (uiCanvasGroup != null)
            uiCanvasGroup.alpha = 1f;
    }

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
        RestartHideDelay();
    }

    public void HideEnemyHealthBar()
    {
        StopActiveCoroutines();

        if (enemyHealthUI != null)
            enemyHealthUI.SetActive(false);

        if (uiCanvasGroup != null)
            uiCanvasGroup.alpha = 0f;

        currentEnemyHealth = null;
    }

    private void RestartHideDelay()
    {
        if (hideDelayCoroutine != null)
        {
            StopCoroutine(hideDelayCoroutine);
        }
        hideDelayCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        fadeCoroutine = StartCoroutine(FadeOutUI());
    }

    private IEnumerator FadeOutUI()
    {
        float elapsedTime = 0f;
        float startAlpha = uiCanvasGroup != null ? uiCanvasGroup.alpha : 1f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            if (uiCanvasGroup != null)
                uiCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);

            yield return null;
        }

        if (enemyHealthUI != null)
            enemyHealthUI.SetActive(false);

        currentEnemyHealth = null;
        fadeCoroutine = null;
        hideDelayCoroutine = null;
    }

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
}

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
    [SerializeField] private RectTransform debuffContainer; // HorizontalLayoutGroup for debuff icons

    private Coroutine fadeCoroutine;
    private Coroutine hideDelayCoroutine;

    private float fadeDuration = 0.5f;
    private float hideDelay = 2f;
    private float attackHideDelay = 3f;

    private EnemyHealth currentEnemyHealth;
    private readonly Vector2 debuffIconSize = new Vector2(24f, 24f); // Size of debuff icons

    private void Awake()
    {
        if (uiCanvasGroup == null && enemyHealthUI != null)
        {
            uiCanvasGroup = enemyHealthUI.GetComponent<CanvasGroup>();
            if (uiCanvasGroup == null)
            {
                Debug.LogWarning("CanvasGroup not found on enemyHealthUI. Please add one.", this);
            }
        }

        if (debuffContainer == null)
        {
            Debug.LogWarning("DebuffContainer not assigned. Creating default container.", this);
            CreateDebuffContainer();
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
            if (enemyHealthSlider != null)
                enemyHealthSlider.value = currentEnemyHealth.GetCurrentHealth();
        }
    }

    public void UpdateEnemyTarget(EnemyHealth enemy)
    {
        if (enemy == null || enemy == currentEnemyHealth) return;

        currentEnemyHealth = enemy;
        string displayName = enemy.gameObject.name;
        ShowEnemyHealthBar(displayName, enemy.GetCurrentHealth(), enemy.maxHealth);
    }

    public void SetVisibility(bool show)
    {
        if (show)
        {
            ShowInstant();
            RestartHideDelay(hideDelay);
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
            Debug.LogWarning("EnemyUIManager: Missing UI references.", this);
            return;
        }

        StopActiveCoroutines();

        if (!enemyHealthUI.activeSelf)
            enemyHealthUI.SetActive(true);

        enemyNameText.text = enemyName;
        enemyHealthSlider.maxValue = maxHealth;
        enemyHealthSlider.value = currentHealth;

        uiCanvasGroup.alpha = 1f;
        RestartHideDelay(hideDelay);
    }

    public void HideEnemyHealthBar()
    {
        StopActiveCoroutines();

        if (enemyHealthUI != null)
            enemyHealthUI.SetActive(false);

        if (uiCanvasGroup != null)
            uiCanvasGroup.alpha = 0f;

        currentEnemyHealth = null;

        // Clear debuff icons
        foreach (Transform child in debuffContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void OnEnemyAttacked()
    {
        StopActiveCoroutines();
        RestartHideDelay(attackHideDelay);
    }

    public void AddDebuffIcon(Debuff debuff)
    {
        if (debuffContainer == null || debuff.icon == null)
        {
            Debug.LogWarning("Cannot add debuff icon: DebuffContainer or icon is null.", this);
            return;
        }

        GameObject iconObj = new GameObject($"DebuffIcon_{debuff.instanceId}");
        iconObj.transform.SetParent(debuffContainer, false);
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.sprite = debuff.icon;
        iconImage.rectTransform.sizeDelta = debuffIconSize;
        iconImage.preserveAspect = true;

        // Add a tag or component to identify the icon by instanceId
        DebuffIconTag tag = iconObj.AddComponent<DebuffIconTag>();
        tag.instanceId = debuff.instanceId;
    }

    public void RemoveDebuffIcon(int instanceId)
    {
        if (debuffContainer == null) return;

        foreach (Transform child in debuffContainer)
        {
            DebuffIconTag tag = child.GetComponent<DebuffIconTag>();
            if (tag != null && tag.instanceId == instanceId)
            {
                Destroy(child.gameObject);
                break;
            }
        }
    }

    private void CreateDebuffContainer()
    {
        GameObject containerObj = new GameObject("DebuffContainer");
        containerObj.transform.SetParent(enemyHealthUI.transform, false);
        debuffContainer = containerObj.AddComponent<RectTransform>();
        debuffContainer.anchorMin = new Vector2(0.5f, 0f);
        debuffContainer.anchorMax = new Vector2(0.5f, 0f);
        debuffContainer.pivot = new Vector2(0.5f, 1f);
        debuffContainer.anchoredPosition = new Vector2(0f, -enemyHealthSlider.GetComponent<RectTransform>().sizeDelta.y - 5f);

        HorizontalLayoutGroup layout = containerObj.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 4f;
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    private void RestartHideDelay(float delay)
    {
        if (hideDelayCoroutine != null)
        {
            StopCoroutine(hideDelayCoroutine);
        }
        hideDelayCoroutine = StartCoroutine(HideAfterDelay(delay));
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        fadeCoroutine = StartCoroutine(FadeOutUI());
    }

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

        // Clear debuff icons
        foreach (Transform child in debuffContainer)
        {
            Destroy(child.gameObject);
        }
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

// Helper component to tag debuff icons with instanceId
public class DebuffIconTag : MonoBehaviour
{
    public int instanceId;
}
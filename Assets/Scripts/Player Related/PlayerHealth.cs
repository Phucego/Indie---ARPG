using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] public float currentHealth = 100f;
    [SerializeField] public float maxHealth = 100f;
    [SerializeField] private float lerpSpeed = 5f;
    [SerializeField] private float baseHealthBarWidth = 100f;

    [SerializeField] private AnimationClip damageAnimationClip;

    public Slider healthBar;
    public GameObject staminaBar;
    public GameObject damageTextPrefab;
    private Vector3 originalStaminaBarScale;

    public static PlayerHealth instance;
    private PlayerMovement _playerMovement;

    public event Action OnPlayerDeath; // Event for player death

    void Awake()
    {
        instance = this;
        _playerMovement = GetComponent<PlayerMovement>();  

        if (_playerMovement == null)
        {
            Debug.LogError("[PlayerHealth] PlayerMovement component missing on Player!", this);
            enabled = false;
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        originalStaminaBarScale = staminaBar != null ? staminaBar.transform.localScale : Vector3.one;
        UpdateHealthBar();
    }

    void Update()
    {
        DestroyObject();
        UpdateHealthBarSize();
    }

    #region Damage Taking and Death

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        UpdateHealthBar();
        StartCoroutine(ShowDamageNumber(damage));
    }

    public void DestroyObject()
    {
        if (currentHealth <= 0)
        {
            // Disable player movement and interaction
            _playerMovement.enabled = false;
            GetComponent<PlayerInteraction>().enabled = false;

            // Trigger the OnPlayerDeath event to show the lose screen
            OnPlayerDeath?.Invoke();

            // Call the UIManager to show the lose screen
            UIManager.Instance.ShowLoseScreen();
        }
    }

    #endregion

    public void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
    }

    private void UpdateHealthBarSize()
    {
        if (healthBar != null)
        {
            RectTransform healthBarRect = healthBar.GetComponent<RectTransform>();
            healthBarRect.sizeDelta = new Vector2(baseHealthBarWidth * (maxHealth * 0.626f), healthBarRect.sizeDelta.y);
        }
    }

    private IEnumerator ShowDamageNumber(float damage)
    {
        if (damageTextPrefab == null) yield break;

        Vector3 spawnPosition = transform.position + Vector3.up * 1.5f;
        GameObject damageText = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity);
        Text damageTextComponent = damageText.GetComponent<Text>();
        
        if (damageTextComponent == null)
        {
            Destroy(damageText);
            yield break;
        }

        damageTextComponent.text = damage.ToString();
        Color textColor = damageTextComponent.color;

        float duration = 1f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            textColor.a = Mathf.Lerp(1, 0, elapsedTime / duration);
            damageTextComponent.color = textColor;
            damageText.transform.position += Vector3.up * Time.deltaTime * 0.5f;
            yield return null;
        }

        Destroy(damageText);
    }
}

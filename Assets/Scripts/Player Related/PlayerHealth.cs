using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] public float currentHealth;
    [SerializeField] public float maxHealth;
    [SerializeField] private float collisionDamageTaken;
    [SerializeField] private float lerpSpeed;
    [SerializeField] private float baseHealthBarWidth;

    public Slider healthBar;
    public GameObject staminaBar;
    public GameObject damageTextPrefab;
    private Vector3 originalStaminaBarScale;

    private Animator animator;
    public string currentAnimation = "";
    
    public static PlayerHealth instance;
    PlayerMovement _playerMovement;
    
    void Start()
    {
        currentHealth = maxHealth;
        instance = this;
        
        animator = GetComponent<Animator>();
        _playerMovement = GetComponent<PlayerMovement>();  

        originalStaminaBarScale = staminaBar.transform.localScale;

        
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
       
        currentHealth -= damage;
        
        
        UpdateHealthBar();
      //  StartCoroutine(ShowDamageNumber(damage));
    }

    public void DestroyObject()
    {
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!other.collider.name.Contains("Enemy_")) return;
    }

    #endregion

    private void UpdateHealthBar()
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
        GameObject damageText = Instantiate(damageTextPrefab, transform.position, Quaternion.identity);
        Text damageTextComponent = damageText.GetComponent<Text>();
        damageTextComponent.text = damage.ToString();
        Color textColor = damageTextComponent.color;

        float duration = 1f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            textColor.a = Mathf.Lerp(1, 0, elapsedTime / duration);
            damageTextComponent.color = textColor;
            damageText.transform.position += Vector3.up * Time.deltaTime;
            yield return null;
        }

        Destroy(damageText);
    }
}

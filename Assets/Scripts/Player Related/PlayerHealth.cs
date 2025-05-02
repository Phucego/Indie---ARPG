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

    [SerializeField] private AnimationClip damageAnimationClip;

    public Slider healthBar;
    public GameObject staminaBar;
    public GameObject damageTextPrefab;
    private Vector3 originalStaminaBarScale;

    private Animator animator;
    public string currentAnimation = "";
    
    public static PlayerHealth instance;
    private PlayerMovement _playerMovement;
    
    void Awake()
    {
        instance = this;
        animator = GetComponent<Animator>();
        _playerMovement = GetComponent<PlayerMovement>();  

        if (animator == null)
        {
            Debug.LogError("[PlayerHealth] Animator component missing on Player!", this);
        }

        if (_playerMovement == null)
        {
            Debug.LogError("[PlayerHealth] PlayerMovement component missing on Player!", this);
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
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
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        UpdateHealthBar();
        StartCoroutine(ShowDamageNumber(damage));

        if (damageAnimationClip != null && animator != null)
        {
            ChangeAnimation(damageAnimationClip);
        }
    }

    public void DestroyObject()
    {
        if (currentHealth <= 0)
        {
            if (animator != null)
            {
                animator.SetTrigger("Death");
            }
            Destroy(gameObject, 1f);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(collisionDamageTaken);
        }
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

    private void ChangeAnimation(AnimationClip animationClip, float crossfade = 0.02f)
    {
        if (animationClip == null || animator == null)
        {
            Debug.LogWarning($"[PlayerHealth] Cannot change animation: AnimationClip or Animator is null on {gameObject.name}!", this);
            return;
        }

        if (currentAnimation != animationClip.name)
        {
            currentAnimation = animationClip.name;
            animator.CrossFade(animationClip.name, crossfade);
        }
    }
}
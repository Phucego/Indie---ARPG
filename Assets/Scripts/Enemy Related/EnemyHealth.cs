using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Configuration")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Health Bar Settings")]
    [SerializeField] private Slider mainHealthSlider;
    [SerializeField] private Slider backHealthSlider;
    [SerializeField] private float barFadeSpeed = 2f;
    [SerializeField] private float barDecayDelay = 1f;

    [Header("Hit Effect & Knockback")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float staggerDuration = 0.4f; // Stagger time before enemy moves again

    private float lastDamageTime;
    private bool isHealthBarVisible;
    private bool isKnockedBack = false;
    private Rigidbody rb;
    private EnemyController enemyController; // Reference to the enemy AI controller

    private void Start()
    {
        rb = GetComponent<Rigidbody>(); 
        enemyController = GetComponent<EnemyController>(); // Get reference to AI controller
        InitializeHealthBar();
    }

    private void Update()
    {
        UpdateHealthBarVisibility();
        UpdateHealthBarDisplay();
    }

    private void InitializeHealthBar()
    {
        currentHealth = maxHealth;
        mainHealthSlider.maxValue = maxHealth;
        backHealthSlider.maxValue = maxHealth;
        mainHealthSlider.value = maxHealth;
        backHealthSlider.value = maxHealth;
        
        mainHealthSlider.gameObject.SetActive(false);
        backHealthSlider.gameObject.SetActive(false);
    }

    private void UpdateHealthBarVisibility()
    {
        if (Time.time - lastDamageTime > barDecayDelay)
        {
            FadeOutHealthBar();
        }
    }

    private void UpdateHealthBarDisplay()
    {
        mainHealthSlider.value = Mathf.Lerp(
            mainHealthSlider.value, 
            currentHealth, 
            Time.deltaTime * barFadeSpeed
        );

        backHealthSlider.value = Mathf.Lerp(
            backHealthSlider.value, 
            mainHealthSlider.value, 
            Time.deltaTime * barFadeSpeed * 0.5f
        );
    }

    private void FadeOutHealthBar()
    {
        if (isHealthBarVisible)
        {
            mainHealthSlider.gameObject.SetActive(false);
            backHealthSlider.gameObject.SetActive(false);
            isHealthBarVisible = false;
        }
    }

    public void TakeDamage(float damage, Vector3 hitDirection)
    {
        if (currentHealth <= 0) return; // Prevent damage after death

        lastDamageTime = Time.time;
        mainHealthSlider.gameObject.SetActive(true);
        backHealthSlider.gameObject.SetActive(true);
        isHealthBarVisible = true;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"Enemy took {damage} damage. Remaining health: {currentHealth}");

        SpawnHitEffect();

        // Apply knockback and stagger enemy
        if (enemyController != null)
        {
            enemyController.ApplyKnockback(hitDirection);
            enemyController.Stagger(staggerDuration);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    private void Die()
    {
        if (enemyController != null)
        {
            enemyController.enabled = false; // Stop AI behavior
        }

        GetComponent<LootBag>()?.InstantiateLoot(transform.position);
        Debug.Log("Enemy died!");

        Destroy(gameObject);
    }

    // Public Getters for EnemyController
    public float GetCurrentHealth() => currentHealth;
    public float GetKnockbackForce() => knockbackForce;
    public float GetStaggerDuration() => staggerDuration;
}

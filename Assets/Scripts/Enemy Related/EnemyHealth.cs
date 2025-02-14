using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Configuration")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Health Bar Settings")]
    [SerializeField] private Slider mainHealthSlider;
    [SerializeField] private Slider backHealthSlider;
    [SerializeField] private float barFadeSpeed = 2f;
    [SerializeField] private float barDecayDelay = 1f;

    private float lastDamageTime;
    private bool isHealthBarVisible;

    private void Start()
    {
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

    public void TakeDamage(float damage)
    {
        lastDamageTime = Time.time;

        mainHealthSlider.gameObject.SetActive(true);
        backHealthSlider.gameObject.SetActive(true);
        isHealthBarVisible = true;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"Enemy took {damage} damage. Remaining health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GetComponent<LootBag>()?.InstantiateLoot(transform.position);
        Debug.Log("Enemy died!");
        Destroy(gameObject);
    }
}
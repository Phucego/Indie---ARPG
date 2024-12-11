using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public int health;
    public Slider healthSlider;
    public Slider easeHealthSlider;
    [SerializeField] private float lerpSpeed = 0.03f;

    [SerializeField] private float baseHealthBarWidth; // Default width for a base max health
    [SerializeField]
    private float maxHealth = 50;


   
    private void Start()
    {
        maxHealth = health;
        
        UpdateHealthBarSize();
        healthSlider.maxValue = maxHealth;
        easeHealthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
        easeHealthSlider.value = maxHealth;

       
    }

    private void Update()
    { 

        if (healthSlider.value != health)
        {
            healthSlider.value = health;
        }

        if (Mathf.Abs(easeHealthSlider.value - health) > 0.01f)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, health, lerpSpeed * Time.deltaTime);
        }
        DestroyObject();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Enemy took " + damage + " damage.");

        if (health <= 0)
        {
            DestroyObject();
        }
    }

    private void DestroyObject()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
            GetComponent<LootBag>().InstantiateLoot(transform.position);
            Debug.Log("Enemy died!");
        }
    }
//TODO: Update the health bar width size depends on the current max health
    private void UpdateHealthBarSize()
    {
        float healthBarWidth = baseHealthBarWidth * maxHealth; // Scale width based on maxHealth
        RectTransform healthRectTransform = healthSlider.GetComponent<RectTransform>();
        RectTransform easeHealthRectTransform = easeHealthSlider.GetComponent<RectTransform>();
        
        healthRectTransform.sizeDelta = new Vector2(healthBarWidth, healthRectTransform.sizeDelta.y);
        easeHealthRectTransform.sizeDelta = new Vector2(healthBarWidth, easeHealthRectTransform.sizeDelta.y);
    }
}
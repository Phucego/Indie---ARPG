using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class PlayerHealth : MonoBehaviour, IDamageable
{

    [SerializeField] public float currentHealth;
    [SerializeField] public float maxHealth;
    [SerializeField] private float collisionDamageTaken;
    [SerializeField] private float lerpSpeed;
    [SerializeField] private float baseHealthBarWidth; // Default width for a base max health
    
    /*
    private float damageTimer = 0f;           // Track time since last damage
    private float damageCooldown = 1f;        // Cooldown duration in seconds
    */
    
    
    public float healthBarWidth;

    [SerializeField] private float xPivot;
    [SerializeField] private float yPivot;
     
    public Slider healthSlider;
    public Slider easeHealthSlider;

    private Animator animator;
    public string currentAnimation = "";

    public PlayerHealth instance;
    PlayerMovement _playerMovement;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        instance = this;
        healthSlider.value = maxHealth;
        
        animator = GetComponent<Animator>();

        _playerMovement = GetComponent<PlayerMovement>();   
        UpdateHealthBarSize();
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: Setting the max value of the slider equal to the max health
        healthSlider.maxValue = maxHealth;
        easeHealthSlider.maxValue = maxHealth;
        
        if (healthSlider.value != currentHealth)
        {
            healthSlider.value = currentHealth;
        }

        if (healthSlider.value != easeHealthSlider.value)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, currentHealth, lerpSpeed);
        }

        DestroyObject();
    }

    #region Damage Taking and Death

    public void TakeDamage(float damage)
    {
        if (PlayerMovement.Instance.IsBlocking)
        {
            currentHealth -= damage / 2;
        }
        else
        {
            currentHealth -= damage;
        }
    }

    public void DestroyObject()
    {
        if (currentHealth <= 0)
        {
            //currentHealth = 0;
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!other.collider.name.Contains("Enemy_")) return;
    
        /*// Check if enough time has passed since last damage
        if (Time.time >= damageTimer)
        {
            TakeDamage(collisionDamageTaken);
            _playerMovement.ChangeAnimation("Player_GotHit");
            damageTimer = Time.time + damageCooldown;    // Set next allowed damage time
        }*/
    }

    #endregion


    private void UpdateHealthBarSize()
    {
        float healthBarWidth = baseHealthBarWidth * maxHealth; // Scale width based on maxHealth

        RectTransform healthRectTransform = healthSlider.GetComponent<RectTransform>();
        RectTransform easeHealthRectTransform = easeHealthSlider.GetComponent<RectTransform>();

        // Set anchor points and pivot to (0, 0.5) to expand from the left
        healthRectTransform.anchorMin = new Vector2(0, 0.5f);
        healthRectTransform.anchorMax = new Vector2(0, 0.5f);
        healthRectTransform.pivot = new Vector2(0, 0.5f);

        easeHealthRectTransform.anchorMin = new Vector2(0, 0.5f);
        easeHealthRectTransform.anchorMax = new Vector2(0, 0.5f);
        easeHealthRectTransform.pivot = new Vector2(0, 0.5f);

        // Update size to expand from the left side
        healthRectTransform.sizeDelta = new Vector2(healthBarWidth, healthRectTransform.sizeDelta.y);
        easeHealthRectTransform.sizeDelta = new Vector2(healthBarWidth, easeHealthRectTransform.sizeDelta.y);

    }

}
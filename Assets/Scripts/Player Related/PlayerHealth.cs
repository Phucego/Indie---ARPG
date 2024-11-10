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
    public Slider healthSlider;
    public Slider easeHealthSlider;

    private Animator animator;
    public string currentAnimation = "";
    
    public PlayerHealth instance;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        instance = this;
        
        animator = GetComponent<Animator>();
    }
    // Update is called once per frame
    void Update()
    {
        if (healthSlider.value != currentHealth)
        {
            healthSlider.value = currentHealth;
        }

        if (healthSlider.value != easeHealthSlider.value)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, currentHealth, lerpSpeed);
        }
        Die();
    }
    
    #region Damage Taking and Death

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
    }

    public void Die()
    {
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!other.collider.name.Contains("Enemy_")) return;
        TakeDamage(collisionDamageTaken);
        ChangeAnimation("Hit_A");
    }

    #endregion
    
    void ChangeAnimation(string animation, float _crossfade = 0.02f)
    {
        if (currentAnimation != animation)
        {
            currentAnimation = animation;
            animator.CrossFade(animation, _crossfade);
        }
    }
    
   
}

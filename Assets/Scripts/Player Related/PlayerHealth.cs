using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{

    [SerializeField] public float currentHealth;
    [SerializeField] public float maxHealth;
    [SerializeField] private float collisionDamageTaken;

  
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
            
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    
    #region Damage Taking and Death

    private void TakeDamage(float damage)
    {
        currentHealth -= damage;
    }

    public void Die()
    {
        if (currentHealth >= 0)
        {
            currentHealth = 0;
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.collider.name.Contains("Enemy_"))
        {
            TakeDamage(collisionDamageTaken);
        }
    }

    #endregion
}

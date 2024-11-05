using System;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackCooldown;  
    public float attackRange = 2.0f;    
    public int attackDamage = 10;       

    [SerializeField] private float nextAttackTime = 0f; 

    public StaminaManager staminaManager;  
    public float staminaCost = 20f;        

    public LayerMask enemyLayer;

    private void Start()
    {
        staminaManager = GetComponentInChildren<StaminaManager>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))  // Left mouse button for attack
        {
            PerformAttack();
        }
    }

    void PerformAttack()
    {
        if (Time.time >= nextAttackTime && staminaManager.HasEnoughStamina(staminaCost))
        {
            
            staminaManager.UseStamina(staminaCost);  // Consume stamina
            nextAttackTime = Time.time + attackCooldown;

            // Detect enemies in range
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange, enemyLayer))
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(attackDamage);  // Apply damage
                }
            }

            Debug.Log("Attack performed!");
        }
        else
        {
            Debug.Log("Not enough stamina or still in cooldown.");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, attackRange);
    }
}
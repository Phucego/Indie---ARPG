using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Configuration")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Enemy Data")]
    [SerializeField] private string enemyName = "Enemy"; // Enemy name to display

    [Header("Hit Effect & Knockback")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float staggerDuration = 0.4f;

    private EnemyUIManager uiManager;
    private EnemyController enemyController;

    private void Start()
    {
        currentHealth = maxHealth;
        enemyController = GetComponent<EnemyController>(); 

        // Find the Enemy UI Manager in the scene
        uiManager = FindObjectOfType<EnemyUIManager>();
    }

    public void TakeDamage(float damage, Vector3 hitDirection)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"Enemy took {damage} damage. Remaining health: {currentHealth}");

        SpawnHitEffect();

        // Update UI health bar
        if (uiManager != null)
        {
            uiManager.ShowEnemyHealthBar(enemyName, currentHealth, maxHealth);
        }

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
            enemyController.enabled = false;
        }

        // Hide the enemy health UI on death
        if (uiManager != null)
        {
            uiManager.HideEnemyHealthBar();
        }

        GetComponent<LootBag>()?.InstantiateLoot(transform.position);
        Debug.Log("Enemy died!");

        Destroy(gameObject);
    }

    // ✅ Fix: Ensure knockback and stagger values are accessible
    public float GetCurrentHealth() => currentHealth;
    public float GetKnockbackForce() => knockbackForce;
    public float GetStaggerDuration() => staggerDuration;
}

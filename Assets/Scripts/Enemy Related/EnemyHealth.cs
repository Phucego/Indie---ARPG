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
    private Rigidbody rb;

    private void Start()
    {
        currentHealth = maxHealth;
        enemyController = GetComponent<EnemyController>(); 
        rb = GetComponent<Rigidbody>();
        uiManager = FindObjectOfType<EnemyUIManager>();
    }

    public void TakeDamage(float damage, Vector3 hitDirection)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"Enemy took {damage} damage. Remaining health: {currentHealth}");

        SpawnHitEffect();
        uiManager?.ShowEnemyHealthBar(enemyName, currentHealth, maxHealth);

        ApplyKnockback(hitDirection);

        if (currentHealth <= 0) Die();
    }

    private BaseEnemyState previousState;

    private void ApplyKnockback(Vector3 hitDirection)
    {
        if (rb == null || enemyController == null) return;

        rb.velocity = Vector3.zero;
        rb.AddForce(hitDirection.normalized * knockbackForce, ForceMode.Impulse);

        // ✅ Only store the state if the enemy is NOT already knocked back
        if (!enemyController.IsKnockedBack)
        {
            previousState = enemyController.GetCurrentState();
        }

        enemyController.SetKnockbackState(true);
        StartCoroutine(RecoverFromKnockback());
    }

    private IEnumerator RecoverFromKnockback()
    {
        yield return new WaitForSeconds(staggerDuration);

        enemyController.SetKnockbackState(false);

        // ✅ Ensure we return to the previous state (not always chase)
        if (previousState != null)
        {
            enemyController.ChangeState(previousState);
            previousState = null; // Clear after restoring
        }
    }

    private void SpawnHitEffect()
    {
        if (hitEffectPrefab)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
    }

    private void Die()
    {
        enemyController?.SetKnockbackState(false);
        if (enemyController != null) enemyController.enabled = false;
        uiManager?.HideEnemyHealthBar();

        GetComponent<LootBag>()?.InstantiateLoot(transform.position);
        Debug.Log("Enemy died!");

        Destroy(gameObject);
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetKnockbackForce() => knockbackForce;
    public float GetStaggerDuration() => staggerDuration;
}

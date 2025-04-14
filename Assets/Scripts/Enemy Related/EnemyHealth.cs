using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    private bool isDead = false;

    private EnemyController controller;
    private NavMeshAgent agent;
    public string enemyName = "Enemy";
    public bool IsDead => isDead;
    public float GetCurrentHealth() => currentHealth;

    [Header("Experience Settings")]
    public GameObject expDropPrefab;  // Reference to the experience prefab

    void Awake()
    {
        currentHealth = maxHealth;
        controller = GetComponent<EnemyController>();
        agent = GetComponent<NavMeshAgent>();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Disable movement and AI logic
        if (controller != null)
        {
            controller.enabled = false;
        }

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Play death animation, effect, or trigger loot drop here

        // Drop experience
        DropExp();

        // Destroy the enemy after a delay
        Destroy(gameObject, 2f);
    }

    private void DropExp()
    {
        if (expDropPrefab != null)
        {
            GameObject exp = Instantiate(expDropPrefab, transform.position, Quaternion.identity);
            // Add pop or rotation animation for exp drop if necessary
            exp.transform.DOMove(exp.transform.position + Vector3.up * 1.5f, 0.5f).SetEase(Ease.OutQuad);
        }
    }
}

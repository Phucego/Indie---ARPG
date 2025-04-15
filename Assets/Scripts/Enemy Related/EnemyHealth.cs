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

    [Header("Death VFX Settings")]
    public GameObject deathVFXPrefab;  // Reference to death VFX prefab

    [Header("Death Animation Settings")]
    public AnimationClip deathAnimationClip;  // Reference to the death animation clip

    private Animator animator;  // Animator for playing death animation

    void Awake()
    {
        currentHealth = maxHealth;
        controller = GetComponent<EnemyController>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();  // Assuming there's an Animator component
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

        // Play death animation
        if (animator != null && deathAnimationClip != null)
        {
            Debug.Log("Playing death animation: " + deathAnimationClip.name);  // Add a debug log to verify
            animator.Play(deathAnimationClip.name);  // Play the death animation clip directly
        }
        else
        {
            Debug.LogWarning("Animator or Death Animation Clip is not set up properly");
        }

        // Play death VFX
        PlayDeathVFX();

        // Drop experience
        DropExp();

        // Destroy the enemy after a delay (to let animation or VFX finish)
        Destroy(gameObject, 2f);
    }


    private void PlayDeathVFX()
    {
        if (deathVFXPrefab != null)
        {
            // Instantiate VFX prefab at the enemy's position
            GameObject vfx = Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            // Optionally, you can add some animation to the VFX (e.g., fade out or grow in size)
            vfx.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InQuad).OnKill(() => Destroy(vfx));
        }
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

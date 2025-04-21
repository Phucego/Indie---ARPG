using System.Collections;
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

        if (animator == null)
        {
            Debug.LogError($"[EnemyHealth] No Animator component found on {gameObject.name}!");
        }
        if (deathAnimationClip == null)
        {
            Debug.LogWarning($"[EnemyHealth] No deathAnimationClip assigned on {gameObject.name}!");
        }
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

        // Play death animation and wait for it to finish
        if (animator != null && deathAnimationClip != null)
        {
            string stateName = deathAnimationClip.name;
            int stateID = Animator.StringToHash(stateName);

            // Check if the state exists in the Animator Controller
            if (animator.HasState(0, stateID))
            {
                Debug.Log($"[EnemyHealth] Playing death animation: {stateName} on {gameObject.name}");
                animator.Play(stateID);
                StartCoroutine(WaitForDeathAnimation(stateName));
            }
            else
            {
                Debug.LogWarning($"[EnemyHealth] Animation state '{stateName}' not found in Animator Controller on {gameObject.name}!");
                Destroy(gameObject, 2f); // Fallback delay if animation state is missing
            }
        }
        else
        {
            Debug.LogWarning($"[EnemyHealth] Animator or deathAnimationClip is not set on {gameObject.name}!");
            Destroy(gameObject, 2f); // Fallback delay if animator or clip is missing
        }

        // Play death VFX
        PlayDeathVFX();

        // Drop experience
        DropExp();
    }

    private IEnumerator WaitForDeathAnimation(string stateName)
    {
        // Wait for the animation to start playing
        float waitTime = 0f;
        const float maxWaitTime = 1f; // Max time to wait for animation to start
        int stateID = Animator.StringToHash(stateName);

        while (waitTime < maxWaitTime)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.fullPathHash == stateID)
            {
                break;
            }
            waitTime += Time.deltaTime;
            yield return null;
        }
        // Check if the animation started
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        if (currentState.fullPathHash != stateID)
        {
            Destroy(gameObject, 2f);
            yield break;
        }
        // Wait for the animation to complete
        float animationLength = deathAnimationClip.length;
        

        while (currentState.normalizedTime < 1f)
        {
            currentState = animator.GetCurrentAnimatorStateInfo(0);
            if (currentState.fullPathHash != stateID)
            {
                break;
            }
            yield return null;
        }
        Destroy(gameObject);
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
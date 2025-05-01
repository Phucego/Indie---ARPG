using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using UnityEngine.Events;

public enum DebuffType
{
    Blind
}

[System.Serializable]
public struct Debuff
{
    public DebuffType type;
    public float duration;
    public Sprite icon;
    public int instanceId;
}

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    public bool isFirstEnemy; // Mark as first enemy
    private bool isDead = false;

    private EnemyController controller;
    private NavMeshAgent agent;
    public string enemyName = "Enemy";
    public bool IsDead => isDead;
    public float GetCurrentHealth() => currentHealth;

    [Header("Experience Settings")]
    public GameObject expDropPrefab;

    [Header("Death VFX Settings")]
    public GameObject deathVFXPrefab;

    [Header("Death Animation Settings")]
    public AnimationClip deathAnimationClip;

    [Header("Debuff Settings")]
    public Sprite blindIcon;
    private List<Debuff> activeDebuffs = new List<Debuff>();
    private int debuffInstanceCounter = 0;

    [Header("Events")]
    public UnityEvent OnEnemyKilled; // Event for when this enemy dies

    private Animator animator;
    private EnemyUIManager uiManager;
    private Coroutine pauseCoroutine;

    void Awake()
    {
        currentHealth = maxHealth;
        controller = GetComponent<EnemyController>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        uiManager = GetComponent<EnemyUIManager>();

        // Tag first enemy for DialogueTrigger
        if (isFirstEnemy)
        {
            gameObject.tag = "FirstEnemy";
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (controller != null && !controller.IsPaused)
        {
            AnimationClip animationClip = controller.GetComponent<EnemyController>().idleClip;
            if (controller.IsPlayerInAttackRange)
            {
                animationClip = controller.GetComponent<EnemyController>().attackClip;
            }
            else if (controller.IsPlayerInDetectionRange)
            {
                animationClip = controller.GetComponent<EnemyController>().runningClip;
            }

            if (pauseCoroutine != null)
            {
                StopCoroutine(pauseCoroutine);
            }
            pauseCoroutine = StartCoroutine(controller.PauseForDuration(1.5f, animationClip));
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Blind(float duration)
    {
        if (isDead) return;
        ApplyDebuff(new Debuff
        {
            type = DebuffType.Blind,
            duration = duration,
            icon = blindIcon,
            instanceId = debuffInstanceCounter++
        });
    }

    private void ApplyDebuff(Debuff debuff)
    {
        activeDebuffs.Add(debuff);
        if (debuff.type == DebuffType.Blind && controller != null)
        {
            controller.Blind(debuff.duration);
        }
        if (uiManager != null)
        {
            uiManager.AddDebuffIcon(debuff);
        }
        StartCoroutine(ManageDebuff(debuff));
    }

    private IEnumerator ManageDebuff(Debuff debuff)
    {
        yield return new WaitForSeconds(debuff.duration);
        activeDebuffs.Remove(debuff);
        if (uiManager != null)
        {
            uiManager.RemoveDebuffIcon(debuff.instanceId);
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (isFirstEnemy && TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialAttackTarget(gameObject))
        {
            TutorialManager.Instance.OnFirstEnemyKilled.Invoke();
            OnEnemyKilled.Invoke();
        }

        foreach (var debuff in activeDebuffs.ToArray())
        {
            activeDebuffs.Remove(debuff);
            if (uiManager != null)
            {
                uiManager.RemoveDebuffIcon(debuff.instanceId);
            }
        }

        if (controller != null)
        {
            controller.enabled = false;
        }

        if (animator != null && deathAnimationClip != null)
        {
            string stateName = deathAnimationClip.name;
            int stateID = Animator.StringToHash(stateName);

            if (animator.HasState(0, stateID))
            {
                Debug.Log($"[EnemyHealth] Playing death animation: {stateName} on {gameObject.name}");
                animator.Play(stateName, 0);
                StartCoroutine(WaitForDeathAnimation(stateName));
            }
            else
            {
                Debug.LogWarning($"[EnemyHealth] Animation state '{stateName}' not found in Animator Controller on {gameObject.name}!");
                Destroy(gameObject, 2f);
            }
        }
        else
        {
            Debug.LogWarning($"[EnemyHealth] Animator or deathAnimationClip is not set on {gameObject.name}!");
            Destroy(gameObject, 2f);
        }

        PlayDeathVFX();
        DropExp();
    }

    private IEnumerator WaitForDeathAnimation(string stateName)
    {
        float waitTime = 0f;
        const float maxWaitTime = 1f;
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

        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        if (currentState.fullPathHash != stateID)
        {
            Destroy(gameObject, 2f);
            yield break;
        }

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
            GameObject vfx = Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            vfx.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InQuad).OnKill(() => Destroy(vfx));
        }
    }

    private void DropExp()
    {
        if (expDropPrefab != null)
        {
            GameObject exp = Instantiate(expDropPrefab, transform.position, Quaternion.identity);
            exp.transform.DOMove(exp.transform.position + Vector3.up * 1.5f, 0.5f).SetEase(Ease.OutQuad);
        }
    }
}
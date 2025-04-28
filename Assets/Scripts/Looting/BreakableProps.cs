using UnityEngine;
using DG.Tweening;

public class BreakableProps : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject drops;
    [SerializeField] private GameObject expDropPrefab;
    [SerializeField] private GameObject destroyEffect;
    [SerializeField] private float popForce = 1.5f;
    [SerializeField] private float popDuration = 0.5f;
    [SerializeField] private float rotateAmount = 180f;
    [SerializeField] private bool destroyOnMelee = false; // Whether melee attacks destroy the object
    [SerializeField] private bool destroyOnRanged = true; // Whether ranged attacks destroy the object
    [SerializeField] private GameObject interactionEffect; // Visual effect for interactions
    [SerializeField] private AudioClip hitSound; // Sound effect for ranged hits

    private Outline currentOutline;
    private AudioManager audioManager;

    private void Awake()
    {
        currentOutline = GetComponent<Outline>();
        if (currentOutline != null)
        {
            currentOutline.enabled = false;
        }

        audioManager = FindObjectOfType<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogWarning("AudioManager not found in scene!");
        }
    }

    public void OnMeleeInteraction(float damage)
    {
        Debug.Log($"Melee interaction on {gameObject.name} with damage: {damage}");

        if (interactionEffect != null)
        {
            Instantiate(interactionEffect, transform.position, Quaternion.identity);
        }

        if (destroyOnMelee)
        {
            DestroyObject();
        }
    }

    public void OnRangedInteraction(float damage)
    {
        Debug.Log($"Ranged interaction on {gameObject.name} with damage: {damage}");

        if (interactionEffect != null)
        {
            Instantiate(interactionEffect, transform.position, Quaternion.identity);
        }

        if (hitSound != null && audioManager != null)
        {
            audioManager.PlaySoundEffect(hitSound);
        }

        if (destroyOnRanged)
        {
            DestroyObject();
        }
    }

    public void DestroyObject()
    {
        Vector3 spawnPos = transform.position;

        Destroy(gameObject);

        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, spawnPos, Quaternion.identity);
        }

        if (drops != null)
        {
            GameObject loot = Instantiate(drops, spawnPos, Quaternion.identity);
            Vector3 jumpTarget = loot.transform.position + Vector3.up * popForce;
            loot.transform.DOMove(jumpTarget, popDuration).SetEase(Ease.OutQuad);
            loot.transform.DORotate(new Vector3(0, rotateAmount, 0), popDuration, RotateMode.LocalAxisAdd).SetEase(Ease.OutCubic);
        }

        DropExp(spawnPos);
    }

    private void DropExp(Vector3 position)
    {
        if (expDropPrefab != null)
        {
            GameObject exp = Instantiate(expDropPrefab, position, Quaternion.identity);
            exp.transform.DOMove(exp.transform.position + Vector3.up * popForce, popDuration).SetEase(Ease.OutQuad);
        }
    }
}
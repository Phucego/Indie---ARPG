using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BreakableProps : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject drops;
    [SerializeField] private GameObject expDropPrefab;  // Reference to the experience prefab
    [SerializeField] private GameObject destroyEffect;  // Reference to the visual effect prefab
    [SerializeField] private float popForce = 1.5f;
    [SerializeField] private float popDuration = 0.5f;
    [SerializeField] private float rotateAmount = 180f;

    private Outline currentOutline;

    private void Awake()
    {
        currentOutline = GetComponent<Outline>();

        if (currentOutline != null)
        {
            currentOutline.enabled = false;
        }
    }

    public void DestroyObject()
    {
        Vector3 spawnPos = transform.position;
        
        // Immediately destroy this object
        Destroy(gameObject);
        // Spawn the visual effect first
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, spawnPos, Quaternion.identity);
        }

       

        // Spawn loot and experience (after object is gone)
        if (drops != null)
        {
            GameObject loot = Instantiate(drops, spawnPos, Quaternion.identity);

            // Pop-up animation for loot
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

            // Pop animation for exp drop
            exp.transform.DOMove(exp.transform.position + Vector3.up * popForce, popDuration).SetEase(Ease.OutQuad);
        }
    }
}

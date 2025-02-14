using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    private int damage;
    private Collider hitboxCollider;
    private bool isActive = false;
    private HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();

    [Header("Hitbox Settings")]
    public LayerMask targetLayers;

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        if (hitboxCollider == null)
        {
            Debug.LogError("[WeaponHitbox] No Collider found! Add a BoxCollider, SphereCollider, or CapsuleCollider.");
        }
        hitboxCollider.isTrigger = true;
        hitboxCollider.enabled = false;
    }

    public void ActivateHitbox(float duration, int attackDamage)
    {
        damage = attackDamage;
        isActive = true;
        hitEnemies.Clear();
        hitboxCollider.enabled = true;

        Debug.Log($"[WeaponHitbox] Activated for {duration}s with damage {damage}");
        StartCoroutine(DeactivateAfterTime(duration));
    }

    public void DeactivateHitbox()
    {
        isActive = false;
        hitboxCollider.enabled = false;
        Debug.Log("[WeaponHitbox] Deactivated.");
    }

    private IEnumerator DeactivateAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        DeactivateHitbox();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        Debug.Log($"[WeaponHitbox] Hit detected: {other.name} (Layer: {other.gameObject.layer})");

        if (((1 << other.gameObject.layer) & targetLayers) != 0)
        {
            if (other.TryGetComponent(out EnemyHealth enemy))
            {
                if (!hitEnemies.Contains(enemy))
                {
                    Debug.Log($"[WeaponHitbox] Applying {damage} damage to {enemy.name}");
                    enemy.TakeDamage(damage);
                    hitEnemies.Add(enemy);
                }
            }
            else
            {
                Debug.Log($"[WeaponHitbox] Object '{other.name}' is in target layers but has no EnemyHealth component.");
            }
        }
        else
        {
            Debug.Log($"[WeaponHitbox] Object '{other.name}' is NOT in target layers.");
        }
    }
}
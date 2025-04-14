using UnityEngine;
using System.Collections;

public class LightningBallProjectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float lifetime;
    private float aoeRadius;
    private float damage;
    public GameObject lightningEffectPrefab;
    private int lightningBoltsCount;
    private float effectDuration;
    private LayerMask enemyLayer;

    private float aliveTime = 0f;
    private bool isInitialized = false;

    public void Initialize(Vector3 dir, float spd, float life, float radius, float dmg, 
                          GameObject effectPrefab, int boltsCount, float effectDur, LayerMask enemyLyr)
    {
        // Adjust spawn height to match player's body level
        transform.position += Vector3.up * 1.5f; // Assuming this aligns with the player's height

        direction = dir.normalized;
        speed = spd;
        lifetime = life;
        aoeRadius = radius;
        damage = dmg;
        lightningEffectPrefab = effectPrefab;
        lightningBoltsCount = boltsCount;
        effectDuration = effectDur;
        enemyLayer = enemyLyr;
        isInitialized = true;

        // Add components only if they don't already exist
        if (GetComponent<SphereCollider>() == null)
        {
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = 0.5f;
            collider.isTrigger = true;
        }

        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // Log successful initialization
        Debug.Log("Lightning Ball projectile initialized successfully at correct height");
    }

    private void Update()
    {
        if (!isInitialized) return;

        // Move the projectile along the XZ plane (in isometric view)
        transform.position += direction * speed * Time.deltaTime;

        // Check lifetime
        aliveTime += Time.deltaTime;
        if (aliveTime >= lifetime)
        {
            // Reached max lifetime, explode
            ExplodeAndDamage();
        }
    }

    // Handle collisions with trigger colliders
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collision is with a Wall or Enemy tag
        if (other != null && (other.CompareTag("Wall") || other.CompareTag("Enemy")))
        {
            Debug.Log($"Lightning Ball collided with {other.tag}");
            ExplodeAndDamage();
        }
    }

    private void ExplodeAndDamage()
    {
        // Apply AOE damage if we have a valid enemy layer
        if (enemyLayer != default)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, aoeRadius, enemyLayer);
            foreach (Collider hit in hits)
            {
                if (hit != null && hit.TryGetComponent<EnemyHealth>(out EnemyHealth enemy))
                {
                    enemy.TakeDamage(damage);

                    // Spawn lightning bolt effects on the enemy
                    SpawnLightningBolts(hit.transform.position);

                    Debug.Log("Lightning Ball damage dealt to " + enemy.name);
                }
            }
        }

        // Spawn an explosion effect at the ball's position
        if (lightningEffectPrefab != null)
        {
            GameObject explosion = Instantiate(lightningEffectPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
            Destroy(explosion, effectDuration);
        }
        else
        {
            Debug.LogWarning("Lightning effect prefab is not assigned!");
        }

        // Destroy the projectile
        Destroy(gameObject);
    }

    private void SpawnLightningBolts(Vector3 targetPosition)
    {
        if (lightningEffectPrefab == null)
        {
            Debug.LogWarning("Cannot spawn lightning bolts: lightningEffectPrefab is null");
            return;
        }

        for (int i = 0; i < lightningBoltsCount; i++)
        {
            // Randomize position slightly around the enemy
            Vector3 offset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(0.5f, 1.5f),  // Slightly above enemy
                Random.Range(-0.5f, 0.5f)
            );

            Vector3 boltPosition = targetPosition + offset;

            // Create lightning bolt effect
            GameObject bolt = Instantiate(lightningEffectPrefab, boltPosition, Quaternion.identity);

            // Randomize bolt rotation for variety
            bolt.transform.rotation = Quaternion.Euler(
                Random.Range(0, 360),
                Random.Range(0, 360),
                Random.Range(0, 360)
            );

            // Scale the bolt randomly for variety
            float randomScale = Random.Range(0.8f, 1.2f);
            bolt.transform.localScale = new Vector3(randomScale, randomScale, randomScale);

            // Destroy after effect duration
            Destroy(bolt, effectDuration);
        }
    }

    // Optional: Visual debugging of AOE radius
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}

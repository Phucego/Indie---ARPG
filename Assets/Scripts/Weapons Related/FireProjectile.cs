using System.Collections;
using UnityEngine;

public class FireProjectile : Projectile
{
    [SerializeField] private float burnDuration = 2f;
    [SerializeField] private float burnDamagePerSecond = 5f;

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        if (collision.gameObject.TryGetComponent(out EnemyHealth enemy))
        {
            enemy.StartCoroutine(ApplyBurn(enemy));
        }
    }

    private IEnumerator ApplyBurn(EnemyHealth enemy)
    {
        float elapsed = 0f;
        while (elapsed < burnDuration)
        {
            enemy.TakeDamage(burnDamagePerSecond * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;

    private float damage;
    private GameObject target;
    private Vector3 startPosition;
    private float maxDistance;
    private Vector3 targetDirection;

    public void Initialize(Vector3 direction, float damage, GameObject target)
    {
        this.damage = damage;
        this.target = target;
        this.startPosition = transform.position;

        maxDistance = PlayerAttack.Instance != null ? PlayerAttack.Instance.rangedAttackRange : 50f;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints.FreezePositionY;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            Debug.LogWarning("Rigidbody component missing on Projectile!", this);
        }

        // Calculate target direction
        if (target != null)
        {
            Vector3 targetPos = target.transform.position;
            targetPos.y = transform.position.y;
            targetDirection = (targetPos - transform.position).normalized;
            if (targetDirection.sqrMagnitude < 0.01f)
            {
                targetDirection = direction.normalized;
            }
        }
        else
        {
            targetDirection = direction.normalized;
        }

        // Ensure direction is horizontal
        targetDirection.y = 0;
        targetDirection = targetDirection.normalized;
        if (targetDirection.sqrMagnitude < 0.01f)
        {
            targetDirection = Vector3.forward;
        }

        // Set rotation: Align to target direction, then apply fixed X=-90, Y=0, Z=0
        Quaternion directionRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
        transform.rotation = directionRotation * Quaternion.Euler(-90f, 0f, 0f);

        if (rb != null)
        {
            rb.velocity = targetDirection * speed;
        }
    }

    private void Update()
    {
        // Destroy if beyond max distance
        if (Vector3.Distance(startPosition, transform.position) > maxDistance)
        {
            Destroy(gameObject);
        }

        // Ensure horizontal movement
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && Mathf.Abs(rb.velocity.y) > 0.01f)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized * speed;
        }

        // Maintain fixed rotation (X=-90, Y=0, Z=0 relative to target direction)
        if (rb != null)
        {
            Quaternion directionRotation = Quaternion.LookRotation(rb.velocity.normalized, Vector3.up);
            transform.rotation = directionRotation * Quaternion.Euler(-90f, 0f, 0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Handle collision with enemies
        if (other.TryGetComponent(out EnemyHealth enemyHealth))
        {
            enemyHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Handle collision with breakable props
        if (other.TryGetComponent(out BreakableProps breakable))
        {
            breakable.OnRangedInteraction(damage);
            Destroy(gameObject);
            return;
        }

        // Handle collision with walls
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
            return;
        }
    }
}
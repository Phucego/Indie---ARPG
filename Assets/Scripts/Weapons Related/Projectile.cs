using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;

    private float damage;
    private GameObject target;
    private Vector3 startPosition;
    private float maxDistance;
    private Vector3 targetDirection; // Store for Update

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
            rb.constraints = RigidbodyConstraints.FreezePositionY; // Freeze Y-position
            rb.velocity = Vector3.zero; // Clear existing velocity
            rb.angularVelocity = Vector3.zero; // Clear angular velocity
        }
        else
        {
            Debug.LogWarning("Rigidbody component missing on Projectile!", this);
        }

        if (target != null)
        {
            Vector3 targetPos = target.transform.position;
            targetPos.y = transform.position.y; // Align Y to projectile position
            targetDirection = (targetPos - transform.position).normalized;
            if (targetDirection.sqrMagnitude < 0.01f)
            {
                targetDirection = direction.normalized;
            }
            Debug.Log($"Projectile targetDirection: {targetDirection}, Target: {target.name}, TargetPos: {target.transform.position}", this);
        }
        else
        {
            targetDirection = direction.normalized;
            Debug.Log($"Projectile fallback direction: {targetDirection}", this);
        }

        // Ensure direction is horizontal
        if (targetDirection.y < -0.5f || targetDirection.y > 0.5f)
        {
            Debug.LogWarning($"Direction has significant Y-component (y={targetDirection.y})! Forcing horizontal.", this);
            targetDirection.y = 0;
            targetDirection = targetDirection.normalized;
            if (targetDirection.sqrMagnitude < 0.01f)
            {
                targetDirection = Vector3.forward;
            }
        }

        // Set rotation to face target, with additional -90 degrees X-axis rotation
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
        transform.rotation = targetRotation * Quaternion.Euler(-90f, 0f, 0f);

        if (rb != null)
        {
            rb.velocity = targetDirection * speed;
            Debug.Log($"Applied velocity: {rb.velocity}, Expected: {targetDirection * speed}, World forward: {transform.forward}, World up: {transform.up}", this);
        }

        Debug.Log($"Projectile rotation: {transform.rotation.eulerAngles}, Position: {transform.position}", this);
    }

    private void Update()
    {
        if (Vector3.Distance(startPosition, transform.position) > maxDistance)
        {
            Destroy(gameObject);
        }

        // Enforce horizontal velocity and log for debugging
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 currentVelocity = rb.velocity;
            if (Mathf.Abs(currentVelocity.y) > 0.01f)
            {
                Debug.LogWarning($"Y-velocity detected (y={currentVelocity.y})! Forcing horizontal velocity.", this);
                rb.velocity = new Vector3(currentVelocity.x, 0, currentVelocity.z).normalized * speed;
            }
            Debug.Log($"Current velocity: {rb.velocity}, Y-Position: {transform.position.y}, Expected Y: {startPosition.y}", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == target && target != null && target.TryGetComponent(out EnemyHealth enemyHealth))
        {
            enemyHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
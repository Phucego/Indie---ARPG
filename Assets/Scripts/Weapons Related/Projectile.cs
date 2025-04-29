using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float homingStrength = 2f; // Controls how strongly the projectile homes (degrees per second)
    public float homingAngleLimit = 45f; // Max angle for homing to prevent sharp turns
    public GameObject impactEffect; // Assign in Inspector for impact particle effect
    public AudioClip impactSound; // Assign in Inspector for impact sound
    public LayerMask collisionMask; // Assign in Inspector (include Enemy and Breakable layers)

    [SerializeField] private float damage;
    private GameObject target;
    private Vector3 startPosition;
    private float maxDistance;
    private Vector3 targetDirection;
    private AudioSource audioSource;
    private System.Action<Vector3> onImpact;
    [SerializeField] private Rigidbody _rb;
    private bool hasCollided = false; // Track if projectile has collided or reached max distance
    private float destroyDelay = 5f; // Time before destroying the projectile after falling
    
    public GameObject impactDust;
    public void Initialize(Vector3 direction, float damage, GameObject target, System.Action<Vector3> onImpact = null)
    {
        this.damage = damage;
        this.target = target;
        this.startPosition = transform.position;
        this.targetDirection = direction.normalized;
        this.onImpact = onImpact;
        maxDistance = PlayerAttack.Instance != null ? PlayerAttack.Instance.rangedAttackRange : 50f;
        _rb = GetComponent<Rigidbody>();
        // Initialize AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false; // No gravity during flight
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Improve collision accuracy
            rb.constraints = RigidbodyConstraints.FreezePositionY; // Lock Y-axis for straight path
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
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
        }
        else
        {
            targetDirection = direction.normalized;
        }

        // Ensure direction is horizontal
        if (targetDirection.y < -0.5f || targetDirection.y > 0.5f)
        {
            targetDirection.y = 0;
            targetDirection = targetDirection.normalized;
            if (targetDirection.sqrMagnitude < 0.01f)
            {
                targetDirection = Vector3.forward;
            }
        }

        // Set rotation to face target, with -90 degrees X-axis rotation
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
        transform.rotation = targetRotation * Quaternion.Euler(-90f, 0f, 0f);

        if (rb != null)
        {
            rb.velocity = targetDirection * speed;
        }
    }

    private void Update()
    {
        if (hasCollided)
        {
            // Skip update logic after collision or max distance
            return;
        }

        if (Vector3.Distance(startPosition, transform.position) > maxDistance)
        {
            StartFalling();
            return;
        }

        // Homing logic
        if (target != null && target.activeInHierarchy)
        {
            Vector3 targetPos = target.transform.position;
            targetPos.y = transform.position.y; // Keep Y aligned
            Vector3 desiredDirection = (targetPos - transform.position).normalized;

            // Check if within homing angle limit
            float angle = Vector3.Angle(targetDirection, desiredDirection);
            if (angle <= homingAngleLimit)
            {
                // Smoothly adjust direction
                targetDirection = Vector3.Slerp(targetDirection, desiredDirection, homingStrength * Time.deltaTime).normalized;
                transform.rotation = Quaternion.LookRotation(targetDirection, Vector3.up) * Quaternion.Euler(-90f, 0f, 0f);

                if (_rb != null)
                {
                    _rb.velocity = targetDirection * speed;
                }
            }
        }

        // Enforce horizontal velocity
        if (_rb != null)
        {
            Vector3 currentVelocity = _rb.velocity;
            if (Mathf.Abs(currentVelocity.y) > 0.01f)
            {
                Debug.LogWarning($"Y-velocity detected (y={currentVelocity.y})! Forcing horizontal velocity.", this);
                _rb.velocity = new Vector3(currentVelocity.x, 0, currentVelocity.z).normalized * speed;
            }
            Debug.Log($"Current velocity: {_rb.velocity}, Y-Position: {transform.position.y}, Expected Y: {startPosition.y}", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only process collisions with objects in collisionMask
        if (((1 << other.gameObject.layer) & collisionMask) != 0 && !hasCollided)
        {
            Debug.Log($"Projectile hit: {other.gameObject.name}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}", this);

            // Call onImpact callback
            onImpact?.Invoke(transform.position);

            // Handle enemy collision
            if (other.gameObject.TryGetComponent(out EnemyHealth enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($"Projectile dealt {damage} damage to enemy: {other.gameObject.name}", this);

                // Spawn impact effect
                if (impactEffect != null)
                {
                    Instantiate(impactEffect, transform.position, Quaternion.identity);
                }

                // Play impact sound
                if (impactSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(impactSound);
                }

                StartFalling();
            }
            // Handle breakable prop collision
            else if (other.gameObject.TryGetComponent(out BreakableProps breakable))
            {
                // Try calling OnRangedInteraction if it exists
                try
                {
                    breakable.OnRangedInteraction(damage);
                    Debug.Log($"Projectile triggered OnRangedInteraction with {damage} damage on prop: {other.gameObject.name}", this);
                }
                catch (System.Exception e)
                {
                    // Fallback to OnMeleeInteraction if OnRangedInteraction is not implemented
                    Debug.LogWarning($"OnRangedInteraction failed on {other.gameObject.name}: {e.Message}. Falling back to OnMeleeInteraction.", this);
                    breakable.OnMeleeInteraction(damage);
                }

                // Spawn impact effect
                if (impactEffect != null)
                {
                    Instantiate(impactEffect, transform.position, Quaternion.identity);
                }

                // Play impact sound
                if (impactSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(impactSound);
                }

                StartFalling();
            }
        }
    }

    private void StartFalling()
    {
        if (hasCollided) return; // Prevent multiple calls

        hasCollided = true;

        if (_rb != null)
        {
            _rb.constraints = RigidbodyConstraints.None; // Remove all constraints
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.useGravity = true;

            // Add a small bounce
            _rb.AddForce(Vector3.up * 2f, ForceMode.Impulse); // Tweak value as needed
            _rb.AddForce(Vector3.down * 10f, ForceMode.Impulse); // Helps pull down faster for realism
        }

        // Spawn dust effect at current position
        if (impactDust != null)
        {
            Instantiate(impactDust, transform.position, Quaternion.identity);
        }

        // Let the projectile settle and then destroy it
        Destroy(gameObject, destroyDelay);
    }


}
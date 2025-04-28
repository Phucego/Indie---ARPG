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


    [SerializeField] private Rigidbody _rb;
    public void Initialize(Vector3 direction, float damage, GameObject target)
    {
        this.damage = damage;
        this.target = target;
        this.startPosition = transform.position;
        this.targetDirection = direction.normalized;
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
            rb.useGravity = false; // Ensure no gravity for straight trajectory
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Improve collision accuracy
            rb.constraints = RigidbodyConstraints.FreezePositionY; // Lock Y-axis for straight path
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
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

        // Set rotation to face target, with -90 degrees X-axis rotation
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
            gameObject.SetActive(false); // Deactivate for pooling
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
        if (((1 << other.gameObject.layer) & collisionMask) != 0)
        {
            // Handle enemy collision
            if (other.gameObject.TryGetComponent(out EnemyHealth enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($"Projectile hit enemy {other.gameObject.name}, applied damage: {damage}");

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

                gameObject.SetActive(false); // Deactivate for pooling
            }
            // Handle breakable prop collision
            else if (other.gameObject.TryGetComponent(out BreakableProps breakable))
            {
                breakable.OnRangedInteraction(damage);
                Debug.Log($"Projectile hit breakable prop {other.gameObject.name}, triggered ranged interaction with damage: {damage}");

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

                gameObject.SetActive(false); // Deactivate for pooling
            }
        }
    }
}
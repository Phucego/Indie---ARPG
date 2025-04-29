using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float homingStrength = 2f;
    public float homingAngleLimit = 45f;
    public GameObject impactEffect;
    public AudioClip impactSound;
    public LayerMask collisionMask;

    [SerializeField] private float damage;
    private GameObject target;
    private Vector3 startPosition;
    private float maxDistance;
    private Vector3 targetDirection;
    private AudioSource audioSource;
    private System.Action<Vector3> onImpact;
    [SerializeField] private Rigidbody _rb;
    private bool hasCollided = false;
    
    [SerializeField]
    private float destroyDelay = 5f;

    public GameObject impactDust;

    public void Initialize(Vector3 direction, float damage, GameObject target, System.Action<Vector3> onImpact = null)
    {
        this.damage = damage;
        this.target = target;
        this.startPosition = transform.position;
        this.targetDirection = direction.normalized;
        this.onImpact = onImpact;
        hasCollided = false;

        if (PlayerAttack.Instance != null)
            maxDistance = PlayerAttack.Instance.rangedAttackRange;
        else
            maxDistance = 50f;

        if (_rb == null)
            _rb = GetComponent<Rigidbody>();

        if (_rb != null)
        {
            _rb.useGravity = false;
            _rb.freezeRotation = true;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rb.constraints = RigidbodyConstraints.FreezePositionY;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Setup target direction
        if (target != null && target.activeInHierarchy)
        {
            Vector3 targetPos = target.transform.position;
            targetPos.y = transform.position.y;
            targetDirection = (targetPos - transform.position).normalized;

            if (targetDirection.sqrMagnitude < 0.01f)
                targetDirection = direction.normalized;
        }

        // Ensure horizontal direction
        if (Mathf.Abs(targetDirection.y) > 0.5f)
        {
            targetDirection.y = 0;
            targetDirection.Normalize();
            if (targetDirection.sqrMagnitude < 0.01f)
                targetDirection = Vector3.forward;
        }

        transform.rotation = Quaternion.LookRotation(targetDirection, Vector3.up) * Quaternion.Euler(-90f, 0f, 0f);

        if (_rb != null)
            _rb.velocity = targetDirection * speed;
    }

    private void Update()
    {
        if (hasCollided)
            return;

        if (Vector3.Distance(startPosition, transform.position) > maxDistance)
        {
            StartFalling();
            return;
        }

        if (target != null && target.activeInHierarchy)
        {
            Vector3 targetPos = target.transform.position;
            targetPos.y = transform.position.y;
            Vector3 desiredDirection = (targetPos - transform.position).normalized;

            float angle = Vector3.Angle(targetDirection, desiredDirection);
            if (angle <= homingAngleLimit)
            {
                targetDirection = Vector3.Slerp(targetDirection, desiredDirection, homingStrength * Time.deltaTime).normalized;
                transform.rotation = Quaternion.LookRotation(targetDirection, Vector3.up) * Quaternion.Euler(-90f, 0f, 0f);

                if (_rb != null)
                    _rb.velocity = targetDirection * speed;
            }
        }

        if (_rb != null)
        {
            Vector3 currentVelocity = _rb.velocity;
            if (Mathf.Abs(currentVelocity.y) > 0.01f)
            {
                Debug.LogWarning($"Y-velocity detected (y={currentVelocity.y})! Forcing horizontal velocity.", this);
                _rb.velocity = new Vector3(currentVelocity.x, 0, currentVelocity.z).normalized * speed;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;

        GameObject other = collision.gameObject;

        if (((1 << other.layer) & collisionMask) != 0)
        {
            Debug.Log($"Projectile hit: {other.name}, Layer: {LayerMask.LayerToName(other.layer)}", this);

            onImpact?.Invoke(transform.position);

            // Handle damage
            if (other.TryGetComponent(out EnemyHealth enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($"Projectile dealt {damage} damage to enemy: {other.name}", this);
            }
            else if (other.TryGetComponent(out BreakableProps breakable))
            {
                try
                {
                    breakable.OnRangedInteraction(damage);
                    Debug.Log($"Triggered OnRangedInteraction on: {other.name}", this);
                }
                catch
                {
                    breakable.OnMeleeInteraction(damage);
                }
            }

            // Instantiate effects
            if (impactEffect != null)
                Instantiate(impactEffect, transform.position, Quaternion.identity);

            // Play sound
            if (impactSound != null && audioSource != null)
                audioSource.PlayOneShot(impactSound);

            // Fall down
            StartFalling();

            // Make sure to nullify the reference if needed
            target = null;
        }
    }

    private void StartFalling()
    {
        if (hasCollided) return;

        hasCollided = true;

        if (_rb != null)
        {
            _rb.constraints = RigidbodyConstraints.None;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.useGravity = true;

            _rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
            _rb.AddForce(Vector3.down * 10f, ForceMode.Impulse);
        }

        if (impactDust != null)
        {
            Instantiate(impactDust, transform.position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        }

        Destroy(gameObject, destroyDelay);
    }

    // private void Deactivate()
    // {
    //     if (gameObject.activeInHierarchy)
    //         gameObject.SetActive(false); // Pooling-friendly
    // }
}

using UnityEngine;

public class Projectile : MonoBehaviour, IInteractable
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

    [SerializeField] private float destroyDelay = 5f;
    public GameObject impactDust;
    [SerializeField] private float dustLifetime = 2f; // Duration before dust is destroyed

    [Header("Pickup Settings")]
    [SerializeField] private int ammoAmount = 1;
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private LayerMask playerLayer;
    private bool isPickable = false;
    private bool isPlayerInRange = false;
    private BoxCollider boxCollider;
    private SphereCollider pickupCollider;
    private Outline outline;
    private float pickupTimer = 0f;

    public void Initialize(Vector3 direction, float damage, GameObject target, System.Action<Vector3> onImpact = null)
    {
        this.damage = damage;
        this.target = target;
        this.startPosition = transform.position;
        this.targetDirection = direction.normalized;
        this.onImpact = onImpact;
        hasCollided = false;
        isPickable = false;
        isPlayerInRange = false;
        pickupTimer = 0f;

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
            _rb.drag = 0f; // Ensure no drag
            _rb.angularDrag = 0f; // Ensure no angular drag
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
            targetDirection = targetDirection.normalized;
            if (targetDirection.sqrMagnitude < 0.01f)
                targetDirection = Vector3.forward;
        }

        transform.rotation = Quaternion.LookRotation(targetDirection, Vector3.up) * Quaternion.Euler(-90f, 0f, 0f);

        if (_rb != null)
            _rb.velocity = targetDirection * speed;

        // Initialize BoxCollider
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(0.2f, 0.2f, 0.5f);
            boxCollider.center = Vector3.zero;
            Debug.Log("BoxCollider added to projectile.", this);
        }
        boxCollider.isTrigger = false;

        // Initialize SphereCollider
        pickupCollider = GetComponent<SphereCollider>();
        if (pickupCollider == null)
        {
            pickupCollider = gameObject.AddComponent<SphereCollider>();
            pickupCollider.radius = pickupRange;
            pickupCollider.center = Vector3.zero;
            Debug.Log("SphereCollider added to projectile.", this);
        }
        pickupCollider.isTrigger = true;
        pickupCollider.enabled = false;

        // Initialize outline
        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.OutlineColor = Color.yellow;
            outline.OutlineWidth = 2f;
            outline.enabled = false;
            Debug.Log("Outline added to projectile.", this);
        }
    }

    private void Update()
    {
        if (hasCollided)
        {
            if (isPickable)
            {
                pickupTimer += Time.deltaTime;
                if (pickupTimer >= destroyDelay)
                {
                    Destroy(gameObject);
                }
            }
            return;
        }

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
            if (Mathf.Abs(currentVelocity.y) > 0.01f && !isPickable)
            {
                Debug.LogWarning($"Y-velocity detected (y={currentVelocity.y})! Forcing horizontal velocity.", this);
                _rb.velocity = targetDirection * speed; // Use targetDirection to maintain speed
            }

            // Debug slow movement
            if (currentVelocity.magnitude < speed * 0.9f)
            {
                Debug.LogWarning($"Arrow speed too low! Expected: {speed}, Actual: {currentVelocity.magnitude}, Direction: {targetDirection}", this);
                _rb.velocity = targetDirection * speed; // Force correct speed
            }
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;

        GameObject other = collision.gameObject;

        if (((1 << other.layer) & collisionMask) != 0)
        {
            onImpact?.Invoke(transform.position);

            if (other.TryGetComponent(out EnemyHealth enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
            }
            else if (other.TryGetComponent(out BreakableProps breakable))
            {
                try
                {
                    breakable.OnRangedInteraction(damage);
                }
                catch
                {
                    breakable.OnMeleeInteraction(damage);
                }
            }

            if (impactEffect != null)
                Instantiate(impactEffect, transform.position, Quaternion.identity);

            if (impactSound != null && audioSource != null)
                audioSource.PlayOneShot(impactSound);

            StartFalling();
            target = null;
        }
    }

    private void StartFalling()
    {
        if (hasCollided) return;

        hasCollided = true;
        isPickable = true;

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
            GameObject dustInstance = Instantiate(impactDust, transform.position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            Destroy(dustInstance, dustLifetime);
        }

        int pickableLayer = LayerMask.NameToLayer("Pickable");
        if (pickableLayer == -1)
        {
            return;
        }
        gameObject.layer = pickableLayer;

        if (boxCollider != null)
        {
            boxCollider.isTrigger = false;
        }

        if (pickupCollider != null)
        {
            pickupCollider.enabled = true;
        }

        if (outline != null)
        {
            outline.enabled = true;
        }
    }

    public void Interact()
    {
        if (!isPickable)
        {
            return;
        }

        if (ArrowAmmoManager.Instance != null)
        {
            ArrowAmmoManager.Instance.AddAmmo(ammoAmount);
        }

        if (outline != null)
            outline.enabled = false;
        Destroy(gameObject);
    }

    public bool IsInRange()
    {
        return isPlayerInRange;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public float GetInteractionRange()
    {
        return pickupRange;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPickable && ((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            isPlayerInRange = true;
            Debug.Log("Player entered pickup range. Auto-picking up.", this);
            Interact();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isPickable && ((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            isPlayerInRange = false;
            if (outline != null)
                outline.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (outline != null)
            outline.enabled = false;
    }
}
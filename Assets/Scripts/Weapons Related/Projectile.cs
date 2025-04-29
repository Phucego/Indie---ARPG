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

    [Header("Pickup Settings")]
    [SerializeField] private int ammoAmount = 1; // Amount of ammo to add when picked up
    [SerializeField] private float pickupRange = 2f; // Interaction range for SphereCollider
    [SerializeField] private LayerMask playerLayer; // Layer to detect the player
    private bool isPickable = false; // Tracks if the projectile is in pickable state
    private bool isPlayerInRange = false; // Tracks if player is within SphereCollider
    private BoxCollider boxCollider; // Collider for physical interactions
    private SphereCollider pickupCollider; // Trigger collider for pickup detection
    private Outline outline; // Outline component for hover feedback
    private float pickupTimer = 0f; // Timer for despawning if not picked up

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

        // Initialize BoxCollider for physical collisions
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(0.2f, 0.2f, 0.5f); // Adjust size to fit arrow model
            boxCollider.center = Vector3.zero;
            Debug.Log("BoxCollider added to projectile.", this);
        }
        boxCollider.isTrigger = false;

        // Initialize SphereCollider for pickup detection
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
                // Update despawn timer
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
            onImpact?.Invoke(transform.position);

            // Handle damage
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

            // Instantiate effects
            if (impactEffect != null)
                Instantiate(impactEffect, transform.position, Quaternion.identity);

            // Play sound
            if (impactSound != null && audioSource != null)
                audioSource.PlayOneShot(impactSound);

            // Transition to pickable state
            StartFalling();

            // Nullify target
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
            Instantiate(impactDust, transform.position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        }

        // Configure for pickup
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

    // IInteractable implementation
    public void Interact()
    {
        if (!isPickable)
        {
           
            return;
        }

        // Add ammo to ArrowAmmoManager
        if (ArrowAmmoManager.Instance != null)
        {
            ArrowAmmoManager.Instance.AddAmmo(ammoAmount);
            
        }
      

        // Disable outline and destroy
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
            Interact(); // Auto-pickup
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
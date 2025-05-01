using UnityEngine;

public class CrossbowPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [Tooltip("Radius of the collision collider for pickup detection")]
    [SerializeField] private float pickupRadius = 0.5f;
    [Tooltip("Distance to show outline effect before collision")]
    [SerializeField] private float outlineActivationDistance = 5f;
    [Tooltip("Layer mask for the player (to detect collision)")]
    [SerializeField] private LayerMask playerLayer;
    [Tooltip("Sound effect played when picking up the crossbow")]
    [SerializeField] private AudioClip pickUpSound;
    [Tooltip("Settings for the outline effect when player is in contact")]
    [SerializeField] private OutlineSettings outlineSettings;

    [System.Serializable]
    private class OutlineSettings
    {
        [Tooltip("Color of the outline effect")]
        public Color outlineColor = Color.yellow;
        [Tooltip("Width of the outline effect")]
        [Range(0f, 10f)]
        public float outlineWidth = 2f;
        [Tooltip("Outline mode (e.g., OutlineAll for full outline)")]
        public Outline.Mode outlineMode = Outline.Mode.OutlineAll;
    }

    private bool isPlayerInContact = false;
    private Collider pickupCollider;
    private Outline outline;
    private GameObject player;

    private void Awake()
    {
        // Setup non-trigger collider
        pickupCollider = GetComponent<Collider>();
        if (pickupCollider == null)
        {
            pickupCollider = gameObject.AddComponent<SphereCollider>();
        }
        pickupCollider.isTrigger = false;

        // Set collider size
        if (pickupCollider is SphereCollider sphereCollider)
        {
            sphereCollider.radius = pickupRadius;
        }

        // Ensure the pickup has a Rigidbody for collision detection
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        // Setup outline component
        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }
        outline.OutlineColor = outlineSettings.outlineColor;
        outline.OutlineWidth = outlineSettings.outlineWidth;
        outline.OutlineMode = outlineSettings.outlineMode;
        outline.enabled = false;

        // Find player
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        // Update outline based on distance
        if (player != null && outline != null)
        {
            float distance = Vector3.Distance(player.transform.position, transform.position);
            outline.enabled = distance <= outlineActivationDistance || isPlayerInContact;
        }

        // Handle pickup
        if (isPlayerInContact && Input.GetMouseButtonDown(0))
        {
            if (PlayerAttack.Instance != null && PlayerAttack.Instance.isAttacking)
            {
                Debug.Log("Cannot pick up crossbow: Player is attacking.", this);
                return;
            }

            if (WeaponManager.Instance != null)
            {
                WeaponManager.Instance.PickupCrossbow();
                if (pickUpSound != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySoundEffect(pickUpSound);
                }
                else if (pickUpSound == null)
                {
                    Debug.LogWarning("PickUpSound is not assigned in CrossbowPickup!", this);
                }
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("WeaponManager instance not found!", this);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (playerLayer == (playerLayer | (1 << collision.gameObject.layer)))
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                isPlayerInContact = true;
                Debug.Log("Click to pick up crossbow");
                if (outline != null)
                {
                    outline.enabled = true;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (playerLayer == (playerLayer | (1 << collision.gameObject.layer)))
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                isPlayerInContact = false;
                Debug.Log("Pickup prompt hidden");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize pickup radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);

        // Visualize outline activation distance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, outlineActivationDistance);
    }
}
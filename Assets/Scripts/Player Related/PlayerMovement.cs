using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    private Animator animator;
    private StaminaManager _staminaManager;

    public static PlayerMovement Instance;
    public string currentAnimation = "";

    [SerializeField] private AudioSource _audioSource;

    private enum MovementState
    {
        Idle,
        Running,
        Dodging,
        Attacking,
    }

    private MovementState currentState = MovementState.Idle;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 15f;
    private Vector3 targetPosition;

    [Header("Dodge Settings")]
    public float dodgeForce = 500f;
    public float dodgeCooldown = 0.5f;
    [SerializeField] private float dodgeStaminaCost = 10f;
    private bool canDodge = true;

    [Header("Audio")]
    [SerializeField] private AudioManager _audioManager;
    private AudioSource audioSource;
    [SerializeField] private AudioClip runningSound;
    [SerializeField] private AudioClip dodgingSound;

    [Header("Movement Animations")]
    public AnimationClip runningForward;
    [SerializeField] private AnimationClip runningBackward;
    [SerializeField] private AnimationClip runningLeft;
    [SerializeField] private AnimationClip runningRight;
    public AnimationClip idleAnimation;
    [SerializeField] private AnimationClip forwardDodgeAnim;
    [SerializeField] private AnimationClip backwardDodgeAnim;
    [SerializeField] private AnimationClip leftDodgeAnim;
    [SerializeField] private AnimationClip rightDodgeAnim;

    public bool IsRunning { get; private set; }
    public bool IsDodging { get; private set; }
    public bool IsIdle { get; private set; }
    public bool canMove = true;

    [Header("Layer Settings")]
    public LayerMask movableLayer; // The layer that defines where the player can move.
    public LayerMask interactableLayer; // The layer for interactable objects.

    private IInteractable currentInteractable;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _staminaManager = GetComponentInChildren<StaminaManager>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canMove) // Left mouse click
        {
            HandleMovement();
            HandleInteraction();
        }
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        switch (currentState)
        {
            case MovementState.Dodging:
            case MovementState.Attacking:
                return;
        }

        SmoothMove();
    }

    void HandleInteraction()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, interactableLayer)) // Raycast hits only interactable objects
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                currentInteractable = interactable;
                currentInteractable.Interact(); // Interact with the object
            }
        }
    }

    void HandleMovement()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, movableLayer)) // Raycast hits only objects in the Movable layer
        {
            targetPosition = hit.point;
            Debug.Log("Target position set to: " + targetPosition);

            // Start running towards the target position
            currentState = MovementState.Running;
            PlayMovementSound(runningSound);
            ChangeAnimation(runningForward);
        }
    }

    void SmoothMove()
    {
        if (currentState != MovementState.Running || rb == null) return;

        Vector3 direction = (targetPosition - transform.position).normalized;

        // Move the player using Rigidbody physics
        Vector3 move = direction * moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + move);

        // Rotate the player towards the target
        RotateToPosition(targetPosition);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentState = MovementState.Idle;
            ChangeAnimation(idleAnimation);
        }
    }

    void RotateToPosition(Vector3 targetPos)
    {
        // Rotate only when moving to the destination
        if (rb.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 direction = (targetPos - transform.position).normalized;
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }

    private void PlayMovementSound(AudioClip clip)
    {
        if (audioSource.clip != clip || !audioSource.isPlaying)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    public void ChangeAnimation(AnimationClip animationClip, float _crossfade = 0.02f)
    {
        if (currentAnimation != animationClip.name)
        {
            currentAnimation = animationClip.name;
            animator.CrossFade(animationClip.name, _crossfade);
        }
    }

    public void StopMovementSound()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            currentInteractable = interactable;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable) && currentInteractable == interactable)
        {
            currentInteractable = null;
        }
    }
}

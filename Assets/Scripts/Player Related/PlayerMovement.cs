using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    private Animator animator;
    private StaminaManager _staminaManager;

    public static PlayerMovement Instance;
    public string currentAnimation = "";

    [SerializeField] private AudioSource _audioSource;
    private Outline currentOutline;

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

    public LayerMask enemyLayer;
    public bool IsRunning { get; private set; }
    public bool IsDodging { get; private set; }
    public bool IsIdle { get; private set; }
    public bool canMove = true;

    [Header("Layer Settings")]
    public LayerMask movableLayer;
    public LayerMask interactableLayer;

    private PlayerStats playerStats;
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

        playerStats = GetComponent<PlayerStats>();
        
    }

    void Update()
    {
        // Skip input processing if the mouse is over a UI element or attacking
        if ((PlayerAttack.Instance != null && PlayerAttack.Instance.isAttacking))
        {
            Debug.Log("Input blocked: UI element or attacking.", this);
            return;
        }

        if (canMove && Input.GetMouseButtonDown(0))
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

    void HandleDirectionalAnimation(Vector3 movementDirection)
    {
        Vector3 localDir = transform.InverseTransformDirection(movementDirection.normalized);

        if (localDir.z > 0.7f)
        {
            ChangeAnimation(runningForward);
        }
        else if (localDir.z < -0.7f)
        {
            ChangeAnimation(runningBackward);
        }
        else if (localDir.x < -0.7f)
        {
            ChangeAnimation(runningLeft);
        }
        else if (localDir.x > 0.7f)
        {
            ChangeAnimation(runningRight);
        }
    }

    void HandleInteraction()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, interactableLayer | enemyLayer))
        {
            Debug.Log($"Interaction raycast hit: {hit.collider.gameObject.name}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}", this);

            if (currentOutline != null)
            {
                currentOutline.enabled = false;
            }

            if (hit.collider.TryGetComponent(out Outline outline))
            {
                outline.enabled = true;
                currentOutline = outline;
            }

            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                currentInteractable = interactable;
                currentInteractable.Interact();
            }
        }
        else
        {
            if (currentOutline != null)
            {
                currentOutline.enabled = false;
                currentOutline = null;
            }
        }
    }

    void HandleMovement()
    {
        if (!canMove)
        {
            Debug.Log("HandleMovement skipped: canMove is false.", this);
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // First, check if the click is on an attackable target (Enemy or Breakable)
        if (PlayerAttack.Instance != null && Physics.Raycast(ray, out hit, 100f, PlayerAttack.Instance.targetLayerMask))
        {
            Debug.Log($"Clicked attackable target: {hit.collider.gameObject.name}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}. Skipping movement.", this);
            return; // Let PlayerAttack handle the attack
        }

        // If not an attackable target, check for movement surface
        if (Physics.Raycast(ray, out hit, 100f, movableLayer))
        {
            Debug.Log($"Movement raycast hit: {hit.collider.gameObject.name}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}", this);
            targetPosition = hit.point;
            Vector3 direction = (targetPosition - transform.position).normalized;

            moveSpeed = playerStats.movementSpeedModifier;

            HandleDirectionalAnimation(direction);
            currentState = MovementState.Running;
            IsRunning = true;
            PlayMovementSound(runningSound);
        }
        else
        {
            Debug.Log("No valid movement surface hit.", this);
        }
    }

    void SmoothMove()
    {
        if (currentState != MovementState.Running || rb == null) return;

        Vector3 direction = (targetPosition - transform.position).normalized;

        HandleDirectionalAnimation(direction);

        Vector3 move = direction * moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + move);

        RotateToPosition(targetPosition);

        if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
        {
            currentState = MovementState.Idle;
            IsRunning = false;
            ChangeAnimation(idleAnimation);
            //StopMovementSound();
        }
    }

    void RotateToPosition(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
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
        if (animationClip == null)
        {
            Debug.LogError("Cannot change animation: AnimationClip is null.", this);
            return;
        }

        if (currentAnimation != animationClip.name)
        {
            currentAnimation = animationClip.name;
            animator.CrossFade(animationClip.name, _crossfade);
        }
    }

    public void MoveTowards(Vector3 direction)
    {
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);

        if (direction.sqrMagnitude > 0.01f)
        {
            direction.y = 0f;
            Quaternion toRotation = Quaternion.LookRotation(direction);
            transform.rotation = toRotation;
        }
    }

    public void StopMoving()
    {
        rb.velocity = Vector3.zero;
        currentState = MovementState.Idle;
        IsRunning = false;
        ChangeAnimation(idleAnimation);
        //StopMovementSound();
    }

    public void MoveToTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;

        Vector3 move = direction * moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + move);

        RotateToPosition(targetPosition);

        if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
        {
            currentState = MovementState.Idle;
            IsRunning = false;
            ChangeAnimation(idleAnimation);
           // StopMovementSound();
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
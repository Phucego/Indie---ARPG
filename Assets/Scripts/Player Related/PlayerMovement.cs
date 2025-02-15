using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    private Animator animator;
    private StaminaManager _staminaManager;
    private LockOnSystem lockOnSystem;

    public static PlayerMovement Instance;
    public string currentAnimation = "";

    [SerializeField] private AudioSource _audioSource;
   
    private enum MovementState
    {
        Idle,
        Running,
        Dodging,
        Blocking,
        Attacking
    }
    private enum MovementDirection
    {
        Forward,
        Backward,
        Left,
        Right,
        Idle
    }

    private MovementState currentState = MovementState.Idle;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float backwardSpeedMultiplier = 0.5f;
    public float acceleration = 20f;
    public float deceleration = 25f;
    public float rotationSpeed = 15f;
    private Vector3 currentVelocity;
    private Vector3 targetVelocity;

    [Header("Dodge Settings")]
    public float dodgeForce = 500f;
    public float dodgeCooldown = 0.5f;
    [SerializeField] private float dodgeStaminaCost = 10f;
    private bool canDodge = true;

    [Header("Movement Smoothing")]
    public float velocitySmoothing = 0.1f;
    private Vector3 smoothVelocity;
    private float currentSpeed;
    private Vector3 lastMoveDirection;

    [Header("Audio")]
    [SerializeField] private AudioManager _audioManager;
    private AudioSource audioSource;
    [SerializeField] private AudioClip walkingSound;
    [SerializeField] private AudioClip runningSound;
    [SerializeField] private AudioClip dodgingSound;
    
    
    [Header("Movement Animations")]
    [SerializeField] private AnimationClip runningForward;
    [SerializeField] private AnimationClip runningBackward;
    [SerializeField] private AnimationClip runningLeft;
    [SerializeField] private AnimationClip runningRight;
    public AnimationClip idleAnimation;
    [SerializeField] private AnimationClip blockingAnimation;
    [SerializeField] private AnimationClip forwardDodgeAnim;

    // Public booleans for external access
    public bool IsRunning { get; private set; }
    public bool IsDodging { get; private set; }
    public bool IsBlocking { get; private set; }
    public bool IsIdle { get; private set; }
    public bool canMove = true;
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
        lockOnSystem = GetComponent<LockOnSystem>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        currentSpeed = 0f;
        lastMoveDirection = Vector3.zero;
    }

    void FixedUpdate()
    {
        if (!canMove) return; // Prevent movement while attacking

        switch (currentState)
        {
            case MovementState.Dodging:
            case MovementState.Blocking:
            case MovementState.Attacking:
                return;
        }

        DodgeInput();
        SmoothMove();
        HandleRotation();
        HandleBlocking();
    }

    void SmoothMove()
    {
        if (!canMove || currentState == MovementState.Dodging) return;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 moveInput = new Vector3(moveX, 0f, moveZ);
        Vector3 moveDirection = moveInput.magnitude > 0.1f ? moveInput.normalized : lastMoveDirection;

        lastMoveDirection = moveDirection;
        float targetSpeed = moveInput.magnitude * moveSpeed;

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, (targetSpeed > currentSpeed ? acceleration : deceleration) * Time.deltaTime);
        targetVelocity = lastMoveDirection * currentSpeed;
        currentVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref smoothVelocity, velocitySmoothing);
        rb.MovePosition(rb.position + currentVelocity * Time.deltaTime);
        HandleMovementAnimationsAndSound(moveX, moveZ, currentSpeed);
    }
    void HandleRotation()
    {
        if (currentState == MovementState.Dodging) return;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

        if (!lockOnSystem.IsLocked() && moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
    void HandleMovementAnimationsAndSound(float moveX, float moveZ, float speed)
    {
        if (!canMove || currentState == MovementState.Attacking) return;

        MovementDirection movementDirection = DetermineMovementDirection(moveX, moveZ);
    
        AnimationClip selectedAnimation = movementDirection switch
        {
            MovementDirection.Forward => runningForward,
            MovementDirection.Backward => runningForward,
            MovementDirection.Left => runningLeft,
            MovementDirection.Right => runningRight,
            _ => idleAnimation
        };

        ChangeAnimation(selectedAnimation);
       // PlayMovementSound(runningSound);
    }
    private MovementDirection DetermineMovementDirection(float moveX, float moveZ)
    {
        if (moveZ > 0) return MovementDirection.Forward;
        if (moveZ < 0) return MovementDirection.Backward;
        if (moveX > 0) return MovementDirection.Right;
        if (moveX < 0) return MovementDirection.Left;
        return MovementDirection.Idle;
    }



    void DodgeInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _staminaManager.currentStamina > dodgeStaminaCost && canDodge)
        {
            StartCoroutine(Dodge());
            _staminaManager.UseStamina(dodgeStaminaCost);
        }
    }

    IEnumerator Dodge()
    {
        currentState = MovementState.Dodging;
        canDodge = false;
        IsDodging = true;
        rb.velocity = Vector3.zero;
        Input.ResetInputAxes();
        rb.AddForce(transform.forward * dodgeForce, ForceMode.Impulse);
        ChangeAnimation(forwardDodgeAnim);
        PlayMovementSound(dodgingSound);
        yield return new WaitForSeconds(0.3f);
        currentState = MovementState.Idle;
        IsDodging = false;
        StopMovementSound();
        ChangeAnimation(idleAnimation);
        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }

    void HandleBlocking()
    {
        if (Input.GetMouseButton(1))
        {
            if (currentState != MovementState.Blocking)
            {
                StartBlocking();
            }
        }
        else if (currentState == MovementState.Blocking)
        {
            StopBlocking();
        }
    }

    void StartBlocking()
    {
        currentState = MovementState.Blocking;
        ChangeAnimation(blockingAnimation);
        StopMovementSound();
        currentVelocity = Vector3.zero;
        targetVelocity = Vector3.zero;
        currentSpeed = 0f;
        IsBlocking = true;
    }

    void StopBlocking()
    {
        currentState = MovementState.Idle;
        ChangeAnimation(idleAnimation);
        IsBlocking = false;
    }
    
    public void StopMovementSound()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
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


}

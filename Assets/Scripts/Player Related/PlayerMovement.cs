using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    private Animator animator;
    private StaminaManager _staminaManager;

    public static PlayerMovement Instance;
    public string currentAnimation = "";

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
    private bool isDodging = false;
    private bool canDodge = true;
    public bool isInvulnerable;
    public bool isBlocking;

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

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        _staminaManager = GetComponentInChildren<StaminaManager>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = true;

        currentSpeed = 0f;
        lastMoveDirection = Vector3.zero;
    }

    void FixedUpdate()
    {
        if (!isDodging && !isBlocking)
        {
            DodgeInput();
            SmoothMove();
            RotateTowardsMovementDirection();
        }
        HandleBlocking();
    }

    void RotateTowardsMovementDirection()
    {
        if (isDodging) return;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    void SmoothMove()
    {
        if (isDodging) return;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveInput = new Vector3(moveX, 0f, moveZ);
        Vector3 moveDirection = Vector3.zero;

        if (moveInput.magnitude > 0.1f)
        {
            moveDirection = moveInput.normalized;
            lastMoveDirection = moveDirection;
        }

        float targetSpeed = moveInput.magnitude * moveSpeed;
    

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            (targetSpeed > currentSpeed ? acceleration : deceleration) * Time.deltaTime
        );

        targetVelocity = lastMoveDirection * currentSpeed;
        currentVelocity = Vector3.SmoothDamp(
            currentVelocity,
            targetVelocity,
            ref smoothVelocity,
            velocitySmoothing
        );

        rb.MovePosition(rb.position + currentVelocity * Time.deltaTime);
        HandleMovementAnimationsAndSound(moveX, moveZ, currentSpeed);
    }

    void HandleMovementAnimationsAndSound(float moveX, float moveZ, float speed)
    {
        if (currentAnimation == "Melee_Slice" || currentAnimation == "Player_GotHit" || 
            currentAnimation == "Block" || currentAnimation == "Blocking")
        {
            StopMovementSound();
            return;
        }

        float movementMagnitude = new Vector2(moveX, moveZ).magnitude;

        if (movementMagnitude > 0.1f)
        {
            if (moveZ > 0f || moveZ < 0f)
            {
                ChangeAnimation("Running_B");
                PlayMovementSound(runningSound);
            }
            else if (moveX > 0f)
            {
                ChangeAnimation("Running_Strafe_Right");
                PlayMovementSound(runningSound);
            }
            else if (moveX < 0f)
            {
                ChangeAnimation("Running_Strafe_Left");
                PlayMovementSound(runningSound);
            }
        }
        else
        {
            ChangeAnimation("Idle");
            StopMovementSound();
        }
    }

    void DodgeInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _staminaManager.currentStamina > dodgeStaminaCost && canDodge)
        {
            StartCoroutine(Dodge());
            _staminaManager.UseStamina(dodgeStaminaCost);
        }
        else if (!isDodging)
        {
            _staminaManager.RegenerateStamina();
        }
    }

    IEnumerator Dodge()
    {
        if (canDodge)
        {
            // Start Dodge
            isDodging = true;
            isInvulnerable = true;
            canDodge = false;

            // Cancel movement by zeroing out the velocity
            rb.velocity = Vector3.zero;

      
            Input.ResetInputAxes(); // This effectively cancels player movement input during dodge.

            // Dodge in the direction the player is facing
            Vector3 dodgeDirection = transform.forward;

            // Apply the dodge force using ForceMode.Impulse for immediate impact
            rb.AddForce(dodgeDirection * dodgeForce, ForceMode.Impulse);

            // Change animation to dodge and play sound
            ChangeAnimation("Dodge_Forward");
            PlayMovementSound(dodgingSound);

            // Wait for the dodge animation duration (0.3f is for timing purposes)
            yield return new WaitForSeconds(0.3f); 

            // After the dodge, reset the dodge state and return to idle animation
            isDodging = false;
            isInvulnerable = false;
            StopMovementSound();
            ChangeAnimation("Idle");

            // Re-enable player movement by restoring input
            // No need to re-enable Input.ResetInputAxes() explicitly because player can resume input after dodge ends.

            // Wait for cooldown before the player can dodge again
            yield return new WaitForSeconds(dodgeCooldown);
            canDodge = true;
        }
    }



    public void ChangeAnimation(string animation, float _crossfade = 0.02f, float time = 0f)
    {
        if (time > 0)
        {
            StartCoroutine(Wait());
        }
        else
        {
            Validate();
        }

        IEnumerator Wait()
        {
            yield return new WaitForSeconds(time - _crossfade);
            Validate();
        }

        void Validate()
        {
            if (currentAnimation != animation)
            {
                currentAnimation = animation;
                if (currentAnimation == "")
                {
                    SmoothMove();
                }
                else
                {
                    animator.CrossFade(animation, _crossfade);
                }
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

    public void StopMovementSound()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    void HandleBlocking()
    {
        if (Input.GetMouseButton(1))
        {
            if (!isBlocking)
            {
                StartBlocking();
            }
        }
        else if (isBlocking)
        {
            StopBlocking();
        }
    }

    void StartBlocking()
    {
        isBlocking = true;
        ChangeAnimation("Blocking");
        StopMovementSound();
        currentVelocity = Vector3.zero;
        targetVelocity = Vector3.zero;
        currentSpeed = 0f;
    }

    void StopBlocking()
    {
        isBlocking = false;
        ChangeAnimation("Idle");
    }
}

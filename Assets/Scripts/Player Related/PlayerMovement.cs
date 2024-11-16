using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Camera mainCamera;
    private Rigidbody rb;
    private Animator animator;
    public static PlayerMovement Instance;
    private StaminaManager _staminaManager;
    
    public string currentAnimation = "";

    public float moveSpeed = 5f;
    public float backwardSpeedMultiplier = 0.5f;
    public float dodgeSpeed = 10f;          // Speed multiplier for dodging
    public float dodgeDuration = 0.2f;      // Duration of the dodge
    private bool isDodging = false;
    public bool isInvulnerable;
    [SerializeField] private float dodgeStaminaCost = 10f;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        _staminaManager = GetComponentInChildren<StaminaManager>();
    }

    void Update()
    {
        if (!isDodging)
        {
            Move();
            RotateTowardsMouse();

            // Check for dodge input
            if (Input.GetKeyDown(KeyCode.Space) && _staminaManager.currentStamina > dodgeStaminaCost)
            {
                StartCoroutine(Dodge());
                _staminaManager.UseStamina(dodgeStaminaCost);
            }
            else
            {
                _staminaManager.RegenerateStamina();
            }
        }
    }

    void RotateTowardsMouse()
    {
        Plane playerPlane = new Plane(Vector3.up, transform.position);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (playerPlane.Raycast(ray, out float distance))
        {
            Vector3 targetPoint = ray.GetPoint(distance);
            Vector3 direction = (targetPoint - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }

    void Move()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        Vector3 moveDirection = (forward * moveZ + right * moveX).normalized;

        float speed = moveSpeed;
        if (moveZ < 0)
        {
            speed *= backwardSpeedMultiplier;
        }

        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        // Detect movement direction and set animations
        if (moveZ > 0f) // Moving forward
        {
            ChangeAnimation("Running_B");
        }
        else if (moveZ < 0f) // Moving backward
        {
            ChangeAnimation("Walking_Backwards");
        }
        else if (moveX > 0f) // Moving to the right
        {
            ChangeAnimation("Running_Strafe_Right");
        }
        else if (moveX < 0f) // Moving to the left
        {
            ChangeAnimation("Running_Strafe_Left");
        }
        else
        {
            ChangeAnimation("Idle");
        }
    }

    IEnumerator Dodge()
    {
        if (_staminaManager.HasEnoughStamina(_staminaManager.currentStamina))
        {
            isDodging = true;
            isInvulnerable = true;
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            Vector3 dodgeDirection = (transform.forward * moveZ + transform.right * moveX).normalized;

            if (dodgeDirection == Vector3.zero)
            {
                dodgeDirection = transform.forward; // Default dodge forward if no movement input
            }

            // Set dodge animation based on direction
            if (dodgeDirection.z > 0)
            {
                ChangeAnimation("Dodge_Forward");
            }
            else if (dodgeDirection.z < 0)
            {
                ChangeAnimation("Dodge_Backward");
            }
            else if (dodgeDirection.x > 0)
            {
                ChangeAnimation("Dodge_Right");
            }
            else if (dodgeDirection.x < 0)
            {
                ChangeAnimation("Dodge_Left");
            }

            float elapsed = 0;

            while (elapsed < dodgeDuration)
            {
                transform.Translate(dodgeDirection * dodgeSpeed * Time.deltaTime, Space.World);
                elapsed += Time.deltaTime;
                yield return null;
            }
            isDodging = false;
            isInvulnerable = false;  // Reset invulnerability after dodge 
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
                    Move();
                }
                else
                {
                    animator.CrossFade(animation, _crossfade);
                }
            }
        }
    }
}

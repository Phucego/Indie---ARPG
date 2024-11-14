using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Camera mainCamera;
    private Rigidbody rb;
    private Animator animator;
    public static PlayerMovement Instance;

    public string currentAnimation = "";

    public float moveSpeed = 5f;
    public float backwardSpeedMultiplier = 0.5f;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Move();
        RotateTowardsMouse();
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

using System;
using System.Collections;
using System.ComponentModel.Design.Serialization;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
   
    private Camera mainCamera;
    private Rigidbody rb;
    private Animator animator;
    public static PlayerMovement Instance;
    
    
    public string currentAnimation = "";
   
    public float moveSpeed = 5f;
    private float moveX;
    private float moveZ;
    public float backwardSpeedMultiplier = 0.5f;
    
    
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Debug.Log(Camera.current);
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
        // Define a plane at the player's y-position to intersect with the mouse position in world space
        Plane playerPlane = new Plane(Vector3.up, transform.position);

        // Create a ray from the mouse position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Calculate the point where the mouse ray intersects with the player's plane
        if (playerPlane.Raycast(ray, out float distance))
        {
            Vector3 targetPoint = ray.GetPoint(distance);

            // Calculate direction to the target point (mouse position projected onto the plane)
            Vector3 direction = (targetPoint - transform.position).normalized;
            direction.y = 0; // Keep rotation on the horizontal plane

            // Rotate the player to face the target point
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }

    void Move()
    {
        // Get input axes for movement
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Calculate movement direction based on the character's forward direction
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        // Calculate the movement direction relative to the player's forward
        Vector3 moveDirection = (forward * moveZ + right * moveX).normalized;

        // Adjust speed if moving backward
        float speed = moveSpeed;
        if (moveZ < 0) // When pressing 'S'
        {
            speed *= backwardSpeedMultiplier;
        }

        // Apply movement in world space
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        //Movements animations check
        if (moveZ > 0f)
        {
            ChangeAnimation("Running_B");
        }
        else if (moveZ < 0f)
        {
            ChangeAnimation("Walking_Backwards");
        }
        else if (moveX > 0)
        {
            ChangeAnimation("Running_Strafe_Right");
        }
        else if (moveX < 0)
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
    


    

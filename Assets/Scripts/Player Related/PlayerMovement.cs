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
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Move();
        RotateTowardsMouse();
        CheckAnimation();
     
    }

    void RotateTowardsMouse()
    {
        // Ray from the camera to the mouse position in the world
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            // Calculate direction to the hit point (mouse position on the ground)
            Vector3 direction = (hitInfo.point - transform.position).normalized;
            direction.y = 0; 

            // Rotate the player to face the mouse position
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }

    void Move()
    {
        // Get the input axes for movement
        moveX = Input.GetAxis("Horizontal");
        moveZ = Input.GetAxis("Vertical");

        // Calculate movement direction relative to the camera's orientation
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        // Zero out the y components to keep movement on the horizontal plane
        cameraForward.y = 0;
        cameraRight.y = 0;

        // Calculate the movement direction relative to the camera
        Vector3 moveDirection = (cameraForward * moveZ + cameraRight * moveX).normalized;

        // Reduce the player move speed when moving backwards
        float speed = moveSpeed;
        if (moveZ < 0) // When pressing 'S'
        {
            speed *= backwardSpeedMultiplier;
        }

        // Apply movement in world space
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
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
                    CheckAnimation();
                }
                else
                {
                    animator.CrossFade(animation, _crossfade);
                }
            }
        }
    }

    public void CheckAnimation()
    {
        //Movements
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


}
    


    

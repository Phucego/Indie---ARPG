using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private LayerMask pickableLayer; // Layer for pickable objects
    [SerializeField] private float interactionCheckDistance = 100f; // Max distance for raycast
    [SerializeField] private AnimationClip pickupAnimation; // Animation to play when picking up

    private Animator animator;
    private PlayerMovement playerMovement;
    private IInteractable currentInteractable;
    private Outline currentOutline;
    private bool isInteracting = false;
    private Vector3 interactionTargetPosition;

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();

        if (pickupAnimation == null)
        {
            Debug.LogWarning("PickupAnimation is not assigned in PlayerInteraction.", this);
        }
    }

    private void Update()
    {
        // Skip input processing if attacking or already interacting
        if ((PlayerAttack.Instance != null && PlayerAttack.Instance.isAttacking) || isInteracting)
        {
            return;
        }

        // Handle hover outline
        HandleHoverOutline();

        // Handle interaction input
        if (playerMovement.canMove && Input.GetMouseButtonDown(0))
        {
            HandleInteraction();
        }

        // Move to interaction target if interacting
        if (isInteracting && currentInteractable != null)
        {
            MoveToInteractionTarget();
        }
    }

    private void HandleHoverOutline()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionCheckDistance, pickableLayer))
        {
            if (hit.collider.TryGetComponent(out Outline outline))
            {
                if (currentOutline != outline)
                {
                    if (currentOutline != null)
                    {
                        currentOutline.enabled = false;
                    }
                    currentOutline = outline;
                    currentOutline.enabled = true;
                }
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

    private void HandleInteraction()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Check for pickable objects
        if (Physics.Raycast(ray, out hit, interactionCheckDistance, pickableLayer))
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                // Skip click-based interaction for Projectiles (handled by auto-pickup)
                if (hit.collider.GetComponent<Projectile>() != null)
                {
                    Debug.Log("Clicked on Projectile, but auto-pickup will handle it.", this);
                    return;
                }

                currentInteractable = interactable;
                interactionTargetPosition = currentInteractable.GetPosition();

                // Check if already in range
                if (currentInteractable.IsInRange())
                {
                    PerformInteraction();
                }
                else
                {
                    // Start moving to the target
                    isInteracting = true;
                    playerMovement.MoveToTarget(interactionTargetPosition);
                }
            }
        }
    }

    private void MoveToInteractionTarget()
    {
        if (currentInteractable == null)
        {
            isInteracting = false;
            return;
        }

        // Check if in range
        if (currentInteractable.IsInRange())
        {
            // Stop moving and perform interaction
            playerMovement.StopMoving();
            PerformInteraction();
        }
    }

    private void PerformInteraction()
    {
        if (currentInteractable == null)
        {
            isInteracting = false;
            return;
        }

        // Play pickup animation
        if (pickupAnimation != null)
        {
            animator.CrossFade(pickupAnimation.name, 0.2f);
            // Wait for animation to complete before interacting
            Invoke(nameof(CompleteInteraction), pickupAnimation.length);
        }
        else
        {
            // No animation, interact immediately
            CompleteInteraction();
        }
    }

    private void CompleteInteraction()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
        currentInteractable = null;
        isInteracting = false;

        // Ensure player returns to idle state if not attacking
        if (PlayerAttack.Instance == null || !PlayerAttack.Instance.isAttacking)
        {
            playerMovement.ChangeAnimation(playerMovement.idleAnimation);
        }
    }

    private void OnDisable()
    {
        // Clean up outline when disabled
        if (currentOutline != null)
        {
            currentOutline.enabled = false;
            currentOutline = null;
        }
    }
}
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionRange = 2f; // Size of the player's trigger collider
    private IInteractable currentInteractable; // Track the current interactable in the trigger area
    private Collider triggerCollider; // Player's trigger collider

    private void Awake()
    {
        // Ensure the player has a trigger collider
        triggerCollider = GetComponent<SphereCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<SphereCollider>();
            triggerCollider.isTrigger = true;
        }
        else
        {
            triggerCollider.isTrigger = true;
        }

        // Set the collider size based on interactionRange
        if (triggerCollider is SphereCollider sphereCollider)
        {
            sphereCollider.radius = interactionRange;
        }
        else if (triggerCollider is CapsuleCollider capsuleCollider)
        {
            capsuleCollider.radius = interactionRange;
            capsuleCollider.height = interactionRange * 2f; // Adjust height for capsule
        }
        else
        {
            Debug.LogWarning("Unsupported collider type for PlayerInteraction. Using default size.", this);
        }
    }

    private void Update()
    {
        // Handle press (interaction)
        if (Input.GetMouseButtonDown(0) && currentInteractable != null && currentInteractable.IsInRange())
        {
            currentInteractable.Interact();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Skip if the other object is null or destroyed
        if (other == null || !other.gameObject.activeInHierarchy)
        {
            return;
        }

        // Check if the collider has an IInteractable component
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null && interactable != currentInteractable)
        {
            // Exit previous interactable if it exists and is still valid
            if (currentInteractable != null)
            {
                MonoBehaviour previousMono = currentInteractable as MonoBehaviour;
                if (previousMono != null && previousMono.gameObject.activeInHierarchy)
                {
                    PortalDoorInteractable portal = previousMono.GetComponent<PortalDoorInteractable>();
                    portal?.OnHoverExit();
                }
            }

            // Enter new interactable
            currentInteractable = interactable;
            MonoBehaviour currentMono = currentInteractable as MonoBehaviour;
            if (currentMono != null && currentMono.gameObject.activeInHierarchy)
            {
                PortalDoorInteractable portal = currentMono.GetComponent<PortalDoorInteractable>();
                portal?.OnHoverEnter();
            }
            else
            {
                // Clear currentInteractable if the object is invalid
                currentInteractable = null;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the exiting collider is the current interactable
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null && interactable == currentInteractable)
        {
            // Exit the interactable
            MonoBehaviour currentMono = currentInteractable as MonoBehaviour;
            if (currentMono != null && currentMono.gameObject.activeInHierarchy)
            {
                PortalDoorInteractable portal = currentMono.GetComponent<PortalDoorInteractable>();
                portal?.OnHoverExit();
            }
            currentInteractable = null;
        }
    }
}
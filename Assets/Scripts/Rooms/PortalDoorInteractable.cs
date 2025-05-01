using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Events;

public class PortalDoorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string targetRoomID; // The room to teleport to
    [SerializeField] private float interactionRange = 2f; // Range for interaction
    [SerializeField] private Vector3 teleportOffset = Vector3.zero; // Offset for teleport position
    [SerializeField] private Image fadeImage; // UI Image for fade effect
    [SerializeField] private float fadeDuration = 1f; // Duration of fade in/out
    [SerializeField] private Outline outline; // Reference to the Outline component

    [Header("Events")]
    public UnityEvent OnTeleport; // Event triggered when player teleports

    private Transform player;
    private bool isFading = false;
    private bool isHovered = false;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0); // Ensure fade image is transparent initially
        }
        if (outline != null)
        {
            outline.enabled = false; // Disable outline initially
        }
    }

    public void Interact()
    {
        if (isFading) return;

        isFading = true;
        if (outline != null)
        {
            outline.enabled = true; // Keep outline on during press
        }

        // Create a DOTween sequence for fade and teleport
        Sequence fadeSequence = DOTween.Sequence();
        fadeSequence.Append(fadeImage.DOFade(1f, fadeDuration * 0.5f)) // Fade out halfway
                    .AppendCallback(() => TeleportPlayer()) // Teleport during fade
                    .Append(fadeImage.DOFade(1f, fadeDuration * 0.25f)) // Complete fade out
                    .Append(fadeImage.DOFade(0f, fadeDuration * 0.75f)) // Fade in
                    .OnComplete(() =>
                    {
                        isFading = false;
                        if (outline != null)
                        {
                            outline.enabled = isHovered; // Restore outline state based on hover
                        }
                    });
    }

    public void OnHoverEnter()
    {
        isHovered = true;
        if (outline != null && !isFading)
        {
            outline.enabled = true; // Enable outline on hover
        }
    }

    public void OnHoverExit()
    {
        isHovered = false;
        if (outline != null && !isFading)
        {
            outline.enabled = false; // Disable outline when not hovering
        }
    }

    private void TeleportPlayer()
    {
        // Switch to the target room using RoomCameraSwitcher
        RoomCameraSwitcher roomSwitcher = FindObjectOfType<RoomCameraSwitcher>();
        if (roomSwitcher != null)
        {
            roomSwitcher.SwitchRoomProgrammatically(targetRoomID, teleportOffset);
            OnTeleport.Invoke(); // Notify teleportation
        }
    }

    public bool IsInRange()
    {
        return Vector3.Distance(player.position, GetPosition()) <= interactionRange;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public float GetInteractionRange()
    {
        return interactionRange;
    }
}
using UnityEngine;

public interface IInteractable
{
    void Interact();
    bool IsInRange();
    Vector3 GetPosition();
    float GetInteractionRange();
}
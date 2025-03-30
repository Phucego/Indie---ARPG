using UnityEngine;

public class WeaponPickup : MonoBehaviour, IInteractable
{
    public Weapon weapon; // Assign weapon in Inspector

    public void Interact()
    {
        InventoryManager inventory = FindObjectOfType<InventoryManager>();
        if (inventory != null)
        {
            inventory.AddWeapon(weapon);
          
            Destroy(gameObject); // Remove the pickup object
        }
    }
}
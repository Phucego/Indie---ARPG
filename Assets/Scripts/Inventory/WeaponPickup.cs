using FarrokhGames.Inventory.Examples;
using UnityEngine;

public class WeaponPickup : MonoBehaviour, IInteractable
{
    public ItemDefinition weapon; // Assign weapon in Inspector

    public void Interact()
    {
        WeaponInventoryManager inventory = FindObjectOfType<WeaponInventoryManager>();
        if (inventory != null)
        {
            inventory.AddWeapon(weapon);
          
            Destroy(gameObject); // Remove the pickup object
        }
    }
}
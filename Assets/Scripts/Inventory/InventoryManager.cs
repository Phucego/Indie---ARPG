using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Settings")]
    public List<Weapon> inventory = new List<Weapon>(); // Stores weapons
    public Button[] hotbarButtons; // UI buttons for hotbar slots
    private int equippedRightHandIndex = -1;
    private int equippedLeftHandIndex = -1;

    [Header("References")]
    public WeaponManager weaponManager;

    private void Start()
    {
        // Assign button events dynamically
        for (int i = 0; i < hotbarButtons.Length; i++)
        {
            int index = i;
            hotbarButtons[i].onClick.AddListener(() => EquipWeapon(index));
        }
    }

    public void AddWeapon(Weapon newWeapon)
    {
        if (inventory.Count < hotbarButtons.Length)
        {
            inventory.Add(newWeapon);
            UpdateHotbar();
        }
        else
        {
            Debug.Log("Inventory full!");
        }
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= inventory.Count) return;

        Weapon selectedWeapon = inventory[index];

        // Check if it's a two-handed weapon
        if (selectedWeapon.isTwoHanded)
        {
            // Equip in the right hand and unequip any left-hand weapon
            weaponManager.EquipWeapon(selectedWeapon, true);
            weaponManager.UnequipWeapon(false);
            equippedRightHandIndex = index;
            equippedLeftHandIndex = -1; // Ensure the left hand is empty
        }
        else
        {
            // Toggle between hands
            if (equippedRightHandIndex == -1)
            {
                weaponManager.EquipWeapon(selectedWeapon, true);
                equippedRightHandIndex = index;
            }
            else if (equippedLeftHandIndex == -1)
            {
                weaponManager.EquipWeapon(selectedWeapon, false);
                equippedLeftHandIndex = index;
            }
            else
            {
                Debug.Log("Both hands are already equipped. Unequip a weapon first.");
            }
        }

        UpdateHotbar();
    }

    public void UnequipWeapon(bool isRightHand)
    {
        if (isRightHand)
        {
            weaponManager.UnequipWeapon(true);
            equippedRightHandIndex = -1;
        }
        else
        {
            weaponManager.UnequipWeapon(false);
            equippedLeftHandIndex = -1;
        }

        UpdateHotbar();
    }

    void UpdateHotbar()
    {
        for (int i = 0; i < hotbarButtons.Length; i++)
        {
            if (i < inventory.Count)
            {
                hotbarButtons[i].GetComponentInChildren<Text>().text = inventory[i].weaponName;
            }
            else
            {
                hotbarButtons[i].GetComponentInChildren<Text>().text = "Empty";
            }
        }
    }
}

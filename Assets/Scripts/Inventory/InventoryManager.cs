using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Settings")]
    public List<Weapon> inventory = new List<Weapon>(); // List of weapons
    public Button[] hotbarButtons; // UI buttons for weapon slots
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
        if (!inventory.Contains(newWeapon))
        {
            inventory.Add(newWeapon);
            UpdateHotbar();
        }
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= inventory.Count) return;

        Weapon selectedWeapon = inventory[index];

        // If it's a two-handed weapon, unequip both hands
        if (selectedWeapon.weaponData.isTwoHanded)
        {
            weaponManager.EquipWeapon(selectedWeapon, true); // Equip in right hand
            equippedRightHandIndex = index;
            equippedLeftHandIndex = -1; // Left hand must be empty
        }
        else
        {
            // Check which hand to equip to
            if (weaponManager.isRightHandEmpty)
            {
                weaponManager.EquipWeapon(selectedWeapon, true);
                equippedRightHandIndex = index;
            }
            else if (weaponManager.isLeftHandEmpty)
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
                hotbarButtons[i].GetComponentInChildren<Text>().text = inventory[i].weaponData.weaponName;
            }
            else
            {
                hotbarButtons[i].GetComponentInChildren<Text>().text = "Empty";
            }
        }
    }
}

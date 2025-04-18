using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponInventoryManager : MonoBehaviour
{
    [Header("Inventory Settings")]
    public List<Weapon> inventory = new List<Weapon>(); // List of weapons
    public Button[] inventoryButtons; // UI buttons for weapon slots in the inventory panel
    public Button[] hotbarButtons; // UI buttons for weapon slots in the hotbar
    private int equippedRightHandIndex = -1;
    private int equippedLeftHandIndex = -1;

    [Header("References")]
    public WeaponManager weaponManager;
    public GameObject inventoryPanel; // Inventory panel UI
    public GameObject hotbarPanel; // Hotbar UI (different from inventory panel)

    private void Start()
    {
        // Hide the inventory panel at the start
        inventoryPanel.SetActive(false);

        // Assign button events dynamically
        for (int i = 0; i < inventoryButtons.Length; i++)
        {
            int index = i;
            inventoryButtons[i].onClick.AddListener(() => EquipWeaponFromInventory(index));
        }

        for (int i = 0; i < hotbarButtons.Length; i++)
        {
            int index = i;
            hotbarButtons[i].onClick.AddListener(() => EquipWeapon(index));
        }
    }

    // Toggle the visibility of the inventory panel
    public void ToggleInventoryPanel()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        hotbarPanel.SetActive(!hotbarPanel.activeSelf);
    }

    public void AddWeapon(Weapon newWeapon)
    {
        if (!inventory.Contains(newWeapon))
        {
            inventory.Add(newWeapon);
            UpdateInventoryPanel();
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

    // Equip weapon from the inventory panel (different from hotbar)
    public void EquipWeaponFromInventory(int index)
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

        UpdateInventoryPanel();
    }

    // Update the inventory panel UI
    void UpdateInventoryPanel()
    {
        for (int i = 0; i < inventoryButtons.Length; i++)
        {
            if (i < inventory.Count)
            {
                inventoryButtons[i].GetComponentInChildren<Text>().text = inventory[i].weaponData.weaponName;
            }
            else
            {
                inventoryButtons[i].GetComponentInChildren<Text>().text = "Empty";
            }
        }
    }

    // Update the hotbar UI
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

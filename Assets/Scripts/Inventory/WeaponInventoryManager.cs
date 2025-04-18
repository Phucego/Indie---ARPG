using UnityEngine;
using UnityEngine.UI;
using FarrokhGames.Inventory.Examples;
using System.Collections.Generic;
using TMPro; // For TextMeshPro support

public class WeaponInventoryManager : MonoBehaviour
{
    [Header("Inventory Settings")]
    public List<ItemDefinition> inventory = new List<ItemDefinition>(); // List of weapons
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
        if (inventoryPanel != null)
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

        UpdateInventoryPanel();
        UpdateHotbar();
    }

    // Toggle the visibility of the inventory panel
    public void ToggleInventoryPanel()
    {
        if (inventoryPanel != null && hotbarPanel != null)
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
            hotbarPanel.SetActive(!hotbarPanel.activeSelf);
        }
    }

    public void AddWeapon(ItemDefinition newWeapon)
    {
        if (newWeapon != null && newWeapon.Type == ItemType.Weapons && !inventory.Contains(newWeapon))
        {
            inventory.Add(newWeapon);
            UpdateInventoryPanel();
            UpdateHotbar();
            Debug.Log($"Added {newWeapon.Name} to inventory");
        }
        else
        {
            Debug.LogWarning("Cannot add item: Not a weapon or already in inventory.");
        }
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= inventory.Count) return;

        ItemDefinition selectedWeapon = inventory[index];

        if (selectedWeapon.Type != ItemType.Weapons)
        {
            Debug.LogWarning($"Cannot equip {selectedWeapon.Name}: Not a weapon.");
            return;
        }

        // If it's a two-handed weapon, unequip both hands
        if (selectedWeapon.IsTwoHanded)
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

        ItemDefinition selectedWeapon = inventory[index];

        if (selectedWeapon.Type != ItemType.Weapons)
        {
            Debug.LogWarning($"Cannot equip {selectedWeapon.Name}: Not a weapon.");
            return;
        }

        // If it's a two-handed weapon, unequip both hands
        if (selectedWeapon.IsTwoHanded)
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
    private void UpdateInventoryPanel()
    {
        for (int i = 0; i < inventoryButtons.Length; i++)
        {
            if (i < inventory.Count)
            {
                // Support both Text and TMP_Text
                var textComponent = inventoryButtons[i].GetComponentInChildren<Text>();
                var tmpTextComponent = inventoryButtons[i].GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                    textComponent.text = inventory[i].Name;
                else if (tmpTextComponent != null)
                    tmpTextComponent.text = inventory[i].Name;
            }
            else
            {
                var textComponent = inventoryButtons[i].GetComponentInChildren<Text>();
                var tmpTextComponent = inventoryButtons[i].GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                    textComponent.text = "Empty";
                else if (tmpTextComponent != null)
                    tmpTextComponent.text = "Empty";
            }
        }
    }

    // Update the hotbar UI
    private void UpdateHotbar()
    {
        for (int i = 0; i < hotbarButtons.Length; i++)
        {
            if (i < inventory.Count)
            {
                // Support both Text and TMP_Text
                var textComponent = hotbarButtons[i].GetComponentInChildren<Text>();
                var tmpTextComponent = hotbarButtons[i].GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                    textComponent.text = inventory[i].Name;
                else if (tmpTextComponent != null)
                    tmpTextComponent.text = inventory[i].Name;
            }
            else
            {
                var textComponent = hotbarButtons[i].GetComponentInChildren<Text>();
                var tmpTextComponent = hotbarButtons[i].GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                    textComponent.text = "Empty";
                else if (tmpTextComponent != null)
                    tmpTextComponent.text = "Empty";
            }
        }
    }
}
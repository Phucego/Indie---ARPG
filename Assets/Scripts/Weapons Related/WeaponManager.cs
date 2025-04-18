using UnityEngine;
using FarrokhGames.Inventory;
using FarrokhGames.Inventory.Examples;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Inventory")]
    public ItemDefinition equippedRightHandWeapon;
    public ItemDefinition equippedLeftHandWeapon;

    [Header("Default Fist Weapons (Used if no weapon is equipped)")]
    public ItemDefinition rightFist;
    public ItemDefinition leftFist;

    [Header("Hand Transforms")]
    public Transform rightHandHolder;
    public Transform leftHandHolder;

    [Header("References")]
    public WeaponInventoryManager inventoryManager;
    public LayerMask enemyLayer;

    public bool isRightHandOneHanded { get; private set; }
    public bool isLeftHandOneHanded { get; private set; }
    public bool isTwoHandedEquipped { get; private set; }
    public bool isRightHandEmpty { get; private set; } = true;
    public bool isLeftHandEmpty { get; private set; } = true;
    public bool isWieldingOneHand { get; private set; }

    private GameObject currentRightHandWeaponInstance;
    private GameObject currentLeftHandWeaponInstance;
    private Dictionary<ItemDefinition, float> modifiedWeaponDamage = new Dictionary<ItemDefinition, float>(); // Store runtime-modified damage

    private PlayerStats playerStats;

    public static WeaponManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();

        // Assign tutorial values to fists
        if (rightFist != null && rightFist.Type == ItemType.Weapons)
        {
            modifiedWeaponDamage[rightFist] = 1f; // Default fist damage
        }

        if (leftFist != null && leftFist.Type == ItemType.Weapons)
        {
            modifiedWeaponDamage[leftFist] = 1f; // Default fist damage
        }

        EquipWeapon(rightFist, true);
        EquipWeapon(leftFist, false);

        // Subscribe to inventory events to handle automatic equipping
        if (inventoryManager != null && inventoryManager.inventory != null)
        {
            var inventoryController = inventoryManager.GetComponent<InventoryController>();
            if (inventoryController != null)
            {
                inventoryController.onItemAdded += OnWeaponAddedToInventory;
            }
        }

        // Subscribe to PlayerStats changes
        if (playerStats != null)
        {
            playerStats.OnStatsChanged += UpdateEquippedWeaponStats;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from inventory events to prevent memory leaks
        if (inventoryManager != null && inventoryManager.inventory != null)
        {
            var inventoryController = inventoryManager.GetComponent<InventoryController>();
            if (inventoryController != null)
            {
                inventoryController.onItemAdded -= OnWeaponAddedToInventory;
            }
        }

        // Unsubscribe from PlayerStats changes
        if (playerStats != null)
        {
            playerStats.OnStatsChanged -= UpdateEquippedWeaponStats;
        }
    }

    public void EquipWeapon(ItemDefinition newWeapon, bool isRightHand)
    {
        if (newWeapon == null || newWeapon.Type != ItemType.Weapons || newWeapon.WeaponPrefab == null)
        {
            Debug.LogWarning("Cannot equip: Item is not a weapon or has no prefab.");
            return;
        }

        // Unequip logic
        if (newWeapon.IsTwoHanded || IsTwoHandedWeaponEquipped())
        {
            UnequipWeapon(true);
            UnequipWeapon(false);
        }
        else
        {
            UnequipWeapon(isRightHand);
        }

        // Equip
        if (isRightHand)
        {
            equippedRightHandWeapon = newWeapon;
            currentRightHandWeaponInstance = Instantiate(newWeapon.WeaponPrefab, rightHandHolder);
            currentRightHandWeaponInstance.transform.localPosition = Vector3.zero; // Adjust as needed
            currentRightHandWeaponInstance.transform.localRotation = Quaternion.identity; // Adjust as needed
            isRightHandEmpty = false;

            if (newWeapon.IsTwoHanded)
            {
                equippedLeftHandWeapon = newWeapon;
                isLeftHandEmpty = false;
                isTwoHandedEquipped = true;
                isWieldingOneHand = false;
            }
            else
            {
                isRightHandOneHanded = true;
                isTwoHandedEquipped = false;
                UpdateWieldingState();
            }

            ModifyWeaponStats(newWeapon);
        }
        else
        {
            equippedLeftHandWeapon = newWeapon;
            currentLeftHandWeaponInstance = Instantiate(newWeapon.WeaponPrefab, leftHandHolder);
            currentLeftHandWeaponInstance.transform.localPosition = Vector3.zero; // Adjust as needed
            currentLeftHandWeaponInstance.transform.localRotation = Quaternion.identity; // Adjust as needed
            isLeftHandEmpty = false;

            if (newWeapon.IsTwoHanded)
            {
                equippedRightHandWeapon = newWeapon;
                isRightHandEmpty = false;
                isTwoHandedEquipped = true;
                isWieldingOneHand = false;
            }
            else
            {
                isLeftHandOneHanded = true;
                isTwoHandedEquipped = false;
                UpdateWieldingState();
            }

            ModifyWeaponStats(newWeapon);
        }

        Debug.Log($"Equipped {newWeapon.Name} in {(isRightHand ? "Right" : "Left")} Hand");
    }

    public void UnequipWeapon(bool isRightHand)
    {
        if (isRightHand)
        {
            if (currentRightHandWeaponInstance != null) Destroy(currentRightHandWeaponInstance);
            equippedRightHandWeapon = null;
            isRightHandOneHanded = false;
            isRightHandEmpty = true;

            if (equippedLeftHandWeapon != null && equippedLeftHandWeapon.IsTwoHanded)
            {
                equippedLeftHandWeapon = null;
                isLeftHandEmpty = true;
                isTwoHandedEquipped = false;
            }
        }
        else
        {
            if (currentLeftHandWeaponInstance != null) Destroy(currentLeftHandWeaponInstance);
            equippedLeftHandWeapon = null;
            isLeftHandOneHanded = false;
            isLeftHandEmpty = true;

            if (equippedRightHandWeapon != null && equippedRightHandWeapon.IsTwoHanded)
            {
                equippedRightHandWeapon = null;
                isRightHandEmpty = true;
                isTwoHandedEquipped = false;
            }
        }

        UpdateWieldingState();
    }

    private void ModifyWeaponStats(ItemDefinition weapon)
    {
        if (playerStats == null || weapon == null || weapon.Type != ItemType.Weapons) return;

        float modifiedDamage = weapon.BaseDamage + playerStats.attackPower + playerStats.damageBonus;
        modifiedWeaponDamage[weapon] = modifiedDamage; // Store modified damage
        Debug.Log($"Modified damage for {weapon.Name}: {modifiedDamage}");
    }

    private void UpdateEquippedWeaponStats()
    {
        if (equippedRightHandWeapon != null) ModifyWeaponStats(equippedRightHandWeapon);
        if (equippedLeftHandWeapon != null && equippedLeftHandWeapon != equippedRightHandWeapon)
            ModifyWeaponStats(equippedLeftHandWeapon);
    }

    public float GetModifiedDamage(ItemDefinition weapon)
    {
        return modifiedWeaponDamage.ContainsKey(weapon) ? modifiedWeaponDamage[weapon] : weapon.BaseDamage;
    }

    public bool BothHandsOccupied() =>
        equippedRightHandWeapon != null && equippedLeftHandWeapon != null;

    public bool IsTwoHandedWeaponEquipped()
    {
        if (equippedRightHandWeapon == null && equippedLeftHandWeapon == null) return false;
        return equippedRightHandWeapon != null && equippedRightHandWeapon.IsTwoHanded;
    }

    public void AddWeaponToInventory(ItemDefinition weapon)
    {
        if (weapon.Type != ItemType.Weapons)
        {
            Debug.LogWarning("Cannot add non-weapon item to inventory.");
            return;
        }

        if (inventoryManager != null && !inventoryManager.inventory.Contains(weapon))
        {
            inventoryManager.AddWeapon(weapon); // Directly pass ItemDefinition
            Debug.Log($"Added {weapon.Name} to inventory");
        }
        else
        {
            Debug.LogWarning("Cannot add weapon: Inventory manager is null or weapon already exists.");
        }
    }

    public bool CanUseTwoHandedSkill() => isTwoHandedEquipped;

    public ItemDefinition GetCurrentWeapon()
    {
        return equippedRightHandWeapon ?? equippedLeftHandWeapon;
    }

    private void UpdateWieldingState()
    {
        isWieldingOneHand = (isRightHandOneHanded && isLeftHandEmpty) || (isLeftHandOneHanded && isRightHandEmpty);
        isTwoHandedEquipped = BothHandsOccupied()
            && equippedRightHandWeapon == equippedLeftHandWeapon
            && equippedRightHandWeapon.IsTwoHanded;
    }

    private void OnWeaponAddedToInventory(IInventoryItem item)
    {
        // Check if the item is a Weapon and the right hand is empty
        if (item is ItemDefinition weapon && weapon.Type == ItemType.Weapons && isRightHandEmpty)
        {
            // Equip the weapon to the right hand
            EquipWeapon(weapon, true);
            Debug.Log($"Auto-equipped {weapon.Name} to Right Hand from inventory");
        }
    }
}
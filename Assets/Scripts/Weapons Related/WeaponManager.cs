using System;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Inventory")]
    public Weapon equippedRightHandWeapon;
    public Weapon equippedLeftHandWeapon;

    [Header("References")]
    public Transform rightHandHolder;
    public Transform leftHandHolder;
    public InventoryManager inventoryManager;

    private GameObject currentRightHandWeaponInstance;
    private GameObject currentLeftHandWeaponInstance;

    public static WeaponManager Instance;

    // Hand state tracking
    public bool isRightHandOneHanded { get; private set; }
    public bool isLeftHandOneHanded { get; private set; }
    public bool isTwoHandedEquipped { get; private set; }
    public bool isRightHandEmpty { get; private set; } = true;
    public bool isLeftHandEmpty { get; private set; } = true;
    public bool isWieldingOneHand { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (equippedRightHandWeapon != null)
        {
            EquipWeapon(equippedRightHandWeapon, true);
        }
        if (equippedLeftHandWeapon != null)
        {
            EquipWeapon(equippedLeftHandWeapon, false);
        }
    }

    public void EquipWeapon(Weapon newWeapon, bool isRightHand)
    {
        if (newWeapon == null || newWeapon.weaponData == null) return;

        // Unequip both hands if a two-handed weapon is involved
        if (newWeapon.weaponData.isTwoHanded || IsTwoHandedWeaponEquipped())
        {
            UnequipWeapon(true);
            UnequipWeapon(false);
        }
        else
        {
            // Unequip only the selected hand
            UnequipWeapon(isRightHand);
        }

        // Instantiate and assign weapon
        if (isRightHand)
        {
            equippedRightHandWeapon = newWeapon;
            currentRightHandWeaponInstance = Instantiate(newWeapon.weaponPrefab, rightHandHolder);
            isRightHandEmpty = false;

            if (newWeapon.weaponData.isTwoHanded)
            {
                equippedLeftHandWeapon = newWeapon; // Mark left hand as occupied
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
        }
        else
        {
            equippedLeftHandWeapon = newWeapon;
            currentLeftHandWeaponInstance = Instantiate(newWeapon.weaponPrefab, leftHandHolder);
            isLeftHandEmpty = false;

            if (newWeapon.weaponData.isTwoHanded)
            {
                equippedRightHandWeapon = newWeapon; // Mark right hand as occupied
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
        }

        Debug.Log($"Equipped {newWeapon.weaponData.weaponName} in {(isRightHand ? "Right" : "Left")} Hand");
    }

    public void UnequipWeapon(bool isRightHand)
    {
        if (isRightHand)
        {
            if (currentRightHandWeaponInstance != null)
            {
                Destroy(currentRightHandWeaponInstance);
            }
            equippedRightHandWeapon = null;
            isRightHandOneHanded = false;
            isRightHandEmpty = true;

            // If this was part of a two-handed weapon, clear both hands
            if (equippedLeftHandWeapon != null && equippedLeftHandWeapon.weaponData.isTwoHanded)
            {
                equippedLeftHandWeapon = null;
                isLeftHandEmpty = true;
                isTwoHandedEquipped = false;
            }
        }
        else
        {
            if (currentLeftHandWeaponInstance != null)
            {
                Destroy(currentLeftHandWeaponInstance);
            }
            equippedLeftHandWeapon = null;
            isLeftHandOneHanded = false;
            isLeftHandEmpty = true;

            // If this was part of a two-handed weapon, clear both hands
            if (equippedRightHandWeapon != null && equippedRightHandWeapon.weaponData.isTwoHanded)
            {
                equippedRightHandWeapon = null;
                isRightHandEmpty = true;
                isTwoHandedEquipped = false;
            }
        }

        UpdateWieldingState();
    }

    public bool BothHandsOccupied()
    {
        return equippedRightHandWeapon != null && equippedLeftHandWeapon != null;
    }

    public bool IsTwoHandedWeaponEquipped()
    {
        return equippedRightHandWeapon != null && equippedRightHandWeapon.weaponData.isTwoHanded;
    }

    public void AddWeaponToInventory(Weapon weapon)
    {
        if (!inventoryManager.inventory.Contains(weapon))
        {
            inventoryManager.AddWeapon(weapon);
        }
    }

    private void UpdateWieldingState()
    {
        // If a weapon is in one hand and the other hand is empty -> One-Handed Wielding
        isWieldingOneHand = (isRightHandOneHanded && isLeftHandEmpty) || (isLeftHandOneHanded && isRightHandEmpty);

        // If both hands are occupied, set two-handed wielding
        isTwoHandedEquipped = BothHandsOccupied();
    }
}

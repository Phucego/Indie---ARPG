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

        // If equipping a two-handed weapon, unequip both hands first
        if (newWeapon.weaponData.isTwoHanded)
        {
            UnequipWeapon(true);
            UnequipWeapon(false);
        }
        else
        {
            // If a two-handed weapon is already equipped, remove it first
            if (IsTwoHandedWeaponEquipped())
            {
                UnequipWeapon(true);
                UnequipWeapon(false);
            }

            // Unequip the weapon in the selected hand before equipping a new one
            UnequipWeapon(isRightHand);
        }

        // Instantiate the weapon and store reference
        if (isRightHand)
        {
            equippedRightHandWeapon = newWeapon;
            currentRightHandWeaponInstance = Instantiate(newWeapon.weaponPrefab, rightHandHolder);
        }
        else
        {
            equippedLeftHandWeapon = newWeapon;
            currentLeftHandWeaponInstance = Instantiate(newWeapon.weaponPrefab, leftHandHolder);
        }

        Debug.Log($"Equipped {newWeapon.weaponData.weaponName} in {(isRightHand ? "Right" : "Left")} Hand");
    }
    public bool IsHoldingShield()
    {
        return (equippedRightHandWeapon != null && equippedRightHandWeapon.weaponData.weaponType == WeaponType.Shield) ||
               (equippedLeftHandWeapon != null && equippedLeftHandWeapon.weaponData.weaponType == WeaponType.Shield);
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
        }
        else
        {
            if (currentLeftHandWeaponInstance != null)
            {
                Destroy(currentLeftHandWeaponInstance);
            }
            equippedLeftHandWeapon = null;
        }
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
}

using UnityEngine;
using FarrokhGames.Inventory.Examples;
using FarrokhGames.Inventory;
public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Inventory")]
    public Weapon equippedRightHandWeapon;
    public Weapon equippedLeftHandWeapon;

    [Header("Default Fist Weapons (Used if no weapon is equipped)")]
    public Weapon rightFist;
    public Weapon leftFist;

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
        if (rightFist != null && rightFist.weaponData != null)
            rightFist.weaponData.baseDamage = 1;

        if (leftFist != null && leftFist.weaponData != null)
            leftFist.weaponData.baseDamage = 1;

        EquipWeapon(rightFist, true);
        EquipWeapon(leftFist, false);
    }

    public void EquipWeapon(Weapon newWeapon, bool isRightHand)
    {
        if (newWeapon == null || newWeapon.weaponData == null) return;

        // Unequip logic
        if (newWeapon.weaponData.isTwoHanded || IsTwoHandedWeaponEquipped())
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
            currentRightHandWeaponInstance = Instantiate(newWeapon.weaponPrefab, rightHandHolder);
            isRightHandEmpty = false;

            if (newWeapon.weaponData.isTwoHanded)
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
            currentLeftHandWeaponInstance = Instantiate(newWeapon.weaponPrefab, leftHandHolder);
            isLeftHandEmpty = false;

            if (newWeapon.weaponData.isTwoHanded)
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

        Debug.Log($"Equipped {newWeapon.weaponData.weaponName} in {(isRightHand ? "Right" : "Left")} Hand");
    }

    public void UnequipWeapon(bool isRightHand)
    {
        if (isRightHand)
        {
            if (currentRightHandWeaponInstance != null) Destroy(currentRightHandWeaponInstance);
            equippedRightHandWeapon = null;
            isRightHandOneHanded = false;
            isRightHandEmpty = true;

            if (equippedLeftHandWeapon != null && equippedLeftHandWeapon.weaponData.isTwoHanded)
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

            if (equippedRightHandWeapon != null && equippedRightHandWeapon.weaponData.isTwoHanded)
            {
                equippedRightHandWeapon = null;
                isRightHandEmpty = true;
                isTwoHandedEquipped = false;
            }
        }

        UpdateWieldingState();
    }

    private void ModifyWeaponStats(Weapon weapon)
    {
        if (playerStats == null || weapon == null || weapon.weaponData == null) return;

        float modifiedDamage = weapon.weaponData.baseDamage + playerStats.attackPower + playerStats.damageBonus;
        weapon.ModifyWeaponData(modifiedDamage);
    }

    public bool BothHandsOccupied() =>
        equippedRightHandWeapon != null && equippedLeftHandWeapon != null;

    public bool IsTwoHandedWeaponEquipped()
    {
        if (equippedRightHandWeapon == null && equippedLeftHandWeapon == null) return false;
        return equippedRightHandWeapon != null && equippedRightHandWeapon.weaponData.isTwoHanded;
    }

    public void AddWeaponToInventory(Weapon weapon)
    {
        if (!inventoryManager.inventory.Contains(weapon))
            inventoryManager.AddWeapon(weapon);
    }

    public bool CanUseTwoHandedSkill() => isTwoHandedEquipped;

    public Weapon GetCurrentWeapon()
    {
        return equippedRightHandWeapon ?? equippedLeftHandWeapon;
    }

    private void UpdateWieldingState()
    {
        isWieldingOneHand = (isRightHandOneHanded && isLeftHandEmpty) || (isLeftHandOneHanded && isRightHandEmpty);
        isTwoHandedEquipped = BothHandsOccupied()
            && equippedRightHandWeapon == equippedLeftHandWeapon
            && equippedRightHandWeapon.weaponData.isTwoHanded;
    }
}

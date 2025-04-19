using UnityEngine;
using FarrokhGames.Inventory;
using FarrokhGames.Inventory.Examples;
using System.Collections.Generic;
using DG.Tweening;

public class WeaponManager : MonoBehaviour
{
    // [Previous fields unchanged]
    [Header("Weapon Inventory")]
    public ItemDefinition equippedRightHandWeapon;
    public ItemDefinition equippedLeftHandWeapon;

    [Header("Default Fist Weapons (Used if no weapon is equipped)")]
    public ItemDefinition rightFist;
    public ItemDefinition leftFist;

    [Header("Fallback Prefab")]
    [Tooltip("Default prefab to use if an ItemDefinition's WeaponPrefab is null")]
    public GameObject defaultWeaponPrefab;

    [Header("Hand Transforms")]
    public Transform rightHandHolder;
    public Transform leftHandHolder;
    [Tooltip("Child Transform in right hand hierarchy for weapon attachment")]
    public Transform rightHandAttachment;
    [Tooltip("Child Transform in left hand hierarchy for weapon attachment")]
    public Transform leftHandAttachment;

    
    public InventoryManager inventoryManager;
    public LayerMask enemyLayer;

    [Header("Drop Animation Settings")]
    [Tooltip("Height above player where dropped item spawns")]
    public float dropSpawnHeight = 1.5f;
    [Tooltip("Horizontal spread range for drop landing position")]
    public float dropSpreadRange = 1f;
    [Tooltip("Duration of the drop animation")]
    public float dropAnimationDuration = 0.6f;
    [Tooltip("Scale multiplier for item during drop animation")]
    public float dropScaleMultiplier = 1.2f;

    public bool isRightHandOneHanded { get; private set; }
    public bool isLeftHandOneHanded { get; private set; }
    public bool isTwoHandedEquipped { get; private set; }
    public bool isRightHandEmpty { get; private set; } = true;
    public bool isLeftHandEmpty { get; private set; } = true;
    public bool isWieldingOneHand { get; private set; }

    private GameObject currentRightHandWeaponInstance;
    private GameObject currentLeftHandWeaponInstance;
    private Dictionary<ItemDefinition, float> modifiedWeaponDamage = new Dictionary<ItemDefinition, float>();

    private PlayerStats playerStats;

    public static WeaponManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();

        // Validate fist weapons
        if (rightFist == null || rightFist.Type != ItemType.Weapons)
            Debug.LogWarning($"RightFist is {(rightFist == null ? "not assigned" : $"not a weapon type (Type: {rightFist.Type})")}.");
        if (leftFist == null || leftFist.Type != ItemType.Weapons)
            Debug.LogWarning($"LeftFist is {(leftFist == null ? "not assigned" : $"not a weapon type (Type: {leftFist.Type})")}.");
        if (rightFist != null && rightFist.WeaponPrefab == null)
            Debug.LogWarning("RightFist has no WeaponPrefab assigned.");
        if (leftFist != null && leftFist.WeaponPrefab == null)
            Debug.LogWarning("LeftFist has no WeaponPrefab assigned.");

        // Validate hand transforms
        if (rightHandHolder == null)
            Debug.LogWarning("RightHandHolder is not assigned.");
        if (leftHandHolder == null)
            Debug.LogWarning("LeftHandHolder is not assigned.");
        if (rightHandAttachment == null)
            Debug.LogWarning("RightHandAttachment is not assigned; using RightHandHolder as parent.");
        if (leftHandAttachment == null)
            Debug.LogWarning("LeftHandAttachment is not assigned; using LeftHandHolder as parent.");

        // Assign tutorial values to fists and equip
        if (rightFist != null && rightFist.Type == ItemType.Weapons)
        {
            modifiedWeaponDamage[rightFist] = 1f;
            EquipWeapon(rightFist, true);
        }

        if (leftFist != null && leftFist.Type == ItemType.Weapons)
        {
            modifiedWeaponDamage[leftFist] = 1f;
            EquipWeapon(leftFist, false);
        }

        // Subscribe to inventory events
        if (inventoryManager != null)
        {
            inventoryManager.onItemAdded += OnWeaponAddedToInventory;
            inventoryManager.onItemDropped += OnWeaponDropped;
        }
        else
        {
            Debug.LogError("InventoryManager is not assigned. Cannot subscribe to inventory events.");
        }

        // Subscribe to PlayerStats changes
        if (playerStats != null)
        {
            playerStats.OnStatsChanged += UpdateEquippedWeaponStats;
        }
    }

    private void OnDestroy()
    {
        if (inventoryManager != null)
        {
            inventoryManager.onItemAdded -= OnWeaponAddedToInventory;
            inventoryManager.onItemDropped -= OnWeaponDropped;
        }

        if (playerStats != null)
        {
            playerStats.OnStatsChanged -= UpdateEquippedWeaponStats;
        }
    }

    public void EquipWeapon(ItemDefinition newWeapon, bool isRightHand)
    {
        if (newWeapon == null || newWeapon.Type != ItemType.Weapons)
        {
            Debug.LogWarning($"Cannot equip: {newWeapon?.Name ?? "null"} is not a weapon. Type: {newWeapon?.Type}");
            return;
        }

        GameObject weaponPrefab = newWeapon.WeaponPrefab;
        Debug.Log($"Equipping {newWeapon.Name}. WeaponPrefab: {(weaponPrefab != null ? weaponPrefab.name : "null")}");

        if (weaponPrefab == null)
        {
            if (defaultWeaponPrefab != null)
            {
                weaponPrefab = defaultWeaponPrefab;
                Debug.LogWarning($"No WeaponPrefab for {newWeapon.Name}. Using default prefab: {defaultWeaponPrefab.name}");
            }
            else
            {
                Debug.LogWarning($"No WeaponPrefab or default prefab for {newWeapon.Name}. Equipping without visual.");
            }
        }

        if (newWeapon.IsTwoHanded || IsTwoHandedWeaponEquipped())
        {
            UnequipWeapon(true);
            UnequipWeapon(false);
        }
        else
        {
            UnequipWeapon(isRightHand);
        }

        Transform parentTransform = isRightHand ? (rightHandAttachment != null ? rightHandAttachment : rightHandHolder) : (leftHandAttachment != null ? leftHandAttachment : leftHandHolder);
        if (parentTransform == null)
        {
            Debug.LogWarning($"Cannot equip {newWeapon.Name}: No parent Transform available for {(isRightHand ? "right" : "left")} hand.");
            return;
        }

        if (isRightHand)
        {
            equippedRightHandWeapon = newWeapon;
            if (weaponPrefab != null)
            {
                currentRightHandWeaponInstance = Instantiate(weaponPrefab, parentTransform);
                currentRightHandWeaponInstance.transform.localPosition = Vector3.zero;
                currentRightHandWeaponInstance.transform.localRotation = Quaternion.identity;
                Debug.Log($"Instantiated {newWeapon.Name} in right hand at {parentTransform.name}");
            }
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
            if (weaponPrefab != null)
            {
                currentLeftHandWeaponInstance = Instantiate(weaponPrefab, parentTransform);
                currentLeftHandWeaponInstance.transform.localPosition = Vector3.zero;
                currentLeftHandWeaponInstance.transform.localRotation = Quaternion.identity;
                Debug.Log($"Instantiated {newWeapon.Name} in left hand at {parentTransform.name}");
            }
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

        Debug.Log($"Equipped {newWeapon.Name} in {(isRightHand ? "Right" : "Left")} Hand under {parentTransform.name}");
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
        modifiedWeaponDamage[weapon] = modifiedDamage;
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

    public void AddItemToInventory(ItemDefinition item)
    {
        if (item == null)
        {
            Debug.LogWarning("Cannot add null item to inventory.");
            return;
        }

        if (item.Type != ItemType.Weapons)
        {
            Debug.LogWarning($"Cannot add {item.Name}: Only weapons can be added to inventory.");
            return;
        }

        if (inventoryManager != null)
        {
            if (inventoryManager.TryAdd(item))
            {
                Debug.Log($"Added {item.Name} to inventory");
            }
            else
            {
                Debug.LogWarning($"Failed to add {item.Name}: Inventory is full or item doesn't fit.");
            }
        }
        else
        {
            Debug.LogWarning($"Cannot add {item.Name}: Inventory manager is null.");
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
        if (item is ItemDefinition weapon && weapon.Type == ItemType.Weapons && isRightHandEmpty)
        {
            EquipWeapon(weapon, true);
            Debug.Log($"Auto-equipped {weapon.Name} to Right Hand from inventory");
        }
    }

    private void OnWeaponDropped(IInventoryItem item)
    {
        if (item is ItemDefinition droppedWeapon && droppedWeapon.Type == ItemType.Weapons)
        {
            GameObject weaponPrefab = droppedWeapon.WeaponPrefab;
            Debug.Log($"Dropping {droppedWeapon.Name}. WeaponPrefab: {(weaponPrefab != null ? weaponPrefab.name : "null")}");

            if (weaponPrefab == null)
            {
                if (defaultWeaponPrefab != null)
                {
                    weaponPrefab = defaultWeaponPrefab;
                    Debug.LogWarning($"No WeaponPrefab for dropped {droppedWeapon.Name}. Using default prefab: {defaultWeaponPrefab.name}");
                }
                else
                {
                    Debug.LogWarning($"No WeaponPrefab or default prefab for dropped {droppedWeapon.Name}. Skipping drop instantiation.");
                    return;
                }
            }

            Vector3 playerPos = transform.position;
            Vector3 spawnPos = playerPos + Vector3.up * dropSpawnHeight;
            Vector3 landingPos = playerPos + new Vector3(
                Random.Range(-dropSpreadRange, dropSpreadRange),
                0f,
                Random.Range(-dropSpreadRange, dropSpreadRange)
            );

            GameObject droppedInstance = Instantiate(weaponPrefab, spawnPos, Quaternion.identity);
            droppedInstance.name = droppedWeapon.Name;

            PlayDropAnimation(droppedInstance, spawnPos, landingPos);

            Debug.Log($"Dropped {droppedWeapon.Name} at {landingPos}");
        }
        else
        {
            
        }
    }

    private void PlayDropAnimation(GameObject droppedItem, Vector3 startPos, Vector3 endPos)
    {
        if (droppedItem == null)
        {
            Debug.LogWarning("Dropped item is null. Cannot play drop animation.");
            return;
        }

        Transform itemTransform = droppedItem.transform;
        if (itemTransform == null)
        {
            Debug.LogWarning("Dropped item has no transform. Destroying object.");
            Destroy(droppedItem);
            return;
        }

        if (!droppedItem.GetComponent<Renderer>() && !droppedItem.GetComponent<SpriteRenderer>())
        {
            Debug.LogWarning($"Dropped item {droppedItem.name} has no Renderer or SpriteRenderer. May not be visible.");
        }

        itemTransform.position = startPos;
        Vector3 originalScale = itemTransform.localScale;

        Sequence dropSequence = DOTween.Sequence();
        dropSequence.Append(itemTransform.DOScale(originalScale * dropScaleMultiplier, dropAnimationDuration * 0.2f));
        dropSequence.Join(itemTransform.DOJump(endPos, 0.5f, 1, dropAnimationDuration).SetEase(Ease.OutQuad));
        dropSequence.Join(itemTransform.DORotate(new Vector3(0f, Random.Range(-30f, 30f), 0f), dropAnimationDuration).SetEase(Ease.Linear));
        dropSequence.Append(itemTransform.DOScale(originalScale, dropAnimationDuration * 0.2f));
        dropSequence.Play();
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using FarrokhGames.Inventory;
using FarrokhGames.Inventory.Examples;

public class WeaponUISlot : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField, Tooltip("Image to display the weapon's sprite")]
    private Image weaponImage;

    [SerializeField, Tooltip("Text to display the weapon's name")]
    private TextMeshProUGUI weaponNameText;

    [SerializeField, Tooltip("Reference to the WeaponManager")]
    private WeaponManager weaponManager;

    [SerializeField, Tooltip("Is this slot for the right hand? (If false, it's for the left hand)")]
    private bool isRightHandSlot = true;

    private IInventoryItem currentWeapon; // Stored as IInventoryItem for inventory compatibility
    public ItemDefinition CurrentWeapon { get; private set; } // Exposed for WeaponManager
    private bool isDragging;
    private bool isProcessingDrop; // Prevent concurrent drops

    private void Awake()
    {
        if (weaponImage == null)
        {
            Debug.LogWarning($"WeaponImage not assigned in WeaponUISlot on {gameObject.name}.", this);
        }
        if (weaponNameText == null)
        {
            Debug.LogWarning($"WeaponNameText not assigned in WeaponUISlot on {gameObject.name}.", this);
        }
        if (weaponManager == null)
        {
            weaponManager = FindObjectOfType<WeaponManager>();
            if (weaponManager == null)
            {
                Debug.LogError($"WeaponManager not found in scene for WeaponUISlot on {gameObject.name}.", this);
            }
        }
        ClearSlot();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (isProcessingDrop)
        {
            Debug.LogWarning($"Drop ignored in WeaponUISlot on {gameObject.name}: Another drop is being processed.", this);
            return;
        }

        if (eventData.pointerDrag == null)
        {
            Debug.LogWarning($"No dragged object detected in OnDrop for WeaponUISlot on {gameObject.name}.", this);
            return;
        }

        var draggedItem = InventoryController.GetDraggedItem();
        if (draggedItem == null || draggedItem.item == null)
        {
            Debug.LogWarning($"Dropped item is not a valid inventory item in WeaponUISlot on {gameObject.name}.", this);
            return;
        }

        // Ensure drop is directly on this slot
        if (eventData.pointerEnter == null || (eventData.pointerEnter != gameObject && !eventData.pointerEnter.transform.IsChildOf(transform)))
        {
            Debug.LogWarning($"Drop ignored in WeaponUISlot on {gameObject.name}: Item was dropped outside this slot.", this);
            return;
        }

        // Verify drag originates from an inventory
        InventoryRenderer inventoryRenderer = eventData.pointerDrag.GetComponentInParent<InventoryRenderer>();
        if (inventoryRenderer == null)
        {
            Debug.LogWarning($"Dropped item in WeaponUISlot on {gameObject.name} does not originate from an inventory.", this);
            return;
        }

        ItemDefinition droppedWeapon = draggedItem.item as ItemDefinition;
        if (droppedWeapon == null || droppedWeapon.Type != ItemType.Weapons)
        {
            Debug.LogWarning($"Cannot equip {droppedWeapon?.Name ?? "null"}: Not a weapon in WeaponUISlot on {gameObject.name}.", this);
            return;
        }

        isProcessingDrop = true;

        try
        {
            // Unequip current weapon if present
            if (currentWeapon != null)
            {
                ItemDefinition currentWeaponDef = currentWeapon as ItemDefinition;
                if (currentWeaponDef != null)
                {
                    weaponManager.UnequipWeapon(isRightHandSlot);
                    if (weaponManager.inventoryManager != null)
                    {
                        if (!weaponManager.inventoryManager.TryAdd(currentWeapon))
                        {
                            Debug.LogWarning($"Failed to add {currentWeaponDef.Name} back to inventory in WeaponUISlot on {gameObject.name}.", this);
                        }
                    }
                }
            }

            // Update slot before equipping
            currentWeapon = droppedWeapon;
            CurrentWeapon = droppedWeapon;
            UpdateSlot(droppedWeapon);

            // Equip the weapon
            weaponManager.EquipWeapon(droppedWeapon, isRightHandSlot);

            // Remove from inventory
            if (weaponManager.inventoryManager != null)
            {
                if (!weaponManager.inventoryManager.TryRemove(droppedWeapon))
                {
                    Debug.LogWarning($"Failed to remove {droppedWeapon.Name} from inventory in WeaponUISlot on {gameObject.name}. Reverting slot state.", this);
                    weaponManager.UnequipWeapon(isRightHandSlot);
                    ClearSlot();
                    return;
                }
            }

            // Update other slot if two-handed
            if (droppedWeapon.IsTwoHanded && weaponManager != null)
            {
                var otherSlot = isRightHandSlot ? weaponManager.leftHandSlot : weaponManager.rightHandSlot;
                if (otherSlot != null)
                {
                    otherSlot.UpdateSlot(droppedWeapon);
                }
            }
        }
        finally
        {
            isProcessingDrop = false;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentWeapon == null)
        {
            return;
        }

        ItemDefinition currentWeaponDef = currentWeapon as ItemDefinition;
        if (currentWeaponDef == null)
        {
            Debug.LogWarning($"Current weapon is not an ItemDefinition in WeaponUISlot on {gameObject.name}.", this);
            return;
        }

        // Prevent dragging equipped or fist weapons
        if (currentWeaponDef == weaponManager.equippedRightHandWeapon ||
            currentWeaponDef == weaponManager.equippedLeftHandWeapon ||
            weaponManager.isTwoHandedEquipped ||
            currentWeaponDef == weaponManager.rightFist ||
            currentWeaponDef == weaponManager.leftFist)
        {
            Debug.LogWarning($"Cannot drag {currentWeaponDef.Name}: Item is currently equipped, two-handed, or a default fist weapon.", this);
            return;
        }

        isDragging = true;

        // Initialize dragging via InventoryController
        var inventoryController = FindObjectOfType<InventoryController>();
        if (inventoryController == null)
        {
            Debug.LogError($"InventoryController not found in scene for WeaponUISlot on {gameObject.name}.", this);
            isDragging = false;
            return;
        }

        // Create a dragged item
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError($"No Canvas found in parent hierarchy for WeaponUISlot on {gameObject.name}.", this);
            isDragging = false;
            return;
        }

        InventoryController.SetDraggedItem(new InventoryDraggedItem(
            canvas,
            inventoryController,
            Vector2Int.zero, // Origin point not used for UI slot
            currentWeapon,
            Vector2.zero // Offset adjusted by InventoryDraggedItem
        ));
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
        {
            return;
        }

        var draggedItem = InventoryController.GetDraggedItem();
        if (draggedItem != null)
        {
            draggedItem.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging || currentWeapon == null)
        {
            isDragging = false;
            return;
        }

        ItemDefinition currentWeaponDef = currentWeapon as ItemDefinition;
        if (currentWeaponDef == null)
        {
            Debug.LogWarning($"Current weapon is not an ItemDefinition in WeaponUISlot on {gameObject.name}.", this);
            isDragging = false;
            return;
        }

        isDragging = false;
        var draggedItem = InventoryController.GetDraggedItem();
        if (draggedItem == null)
        {
            Debug.LogWarning($"No dragged item found in OnEndDrag for WeaponUISlot on {gameObject.name}.", this);
            return;
        }

        // Check if dropped onto an inventory
        bool droppedInInventory = false;
        if (eventData.pointerEnter != null)
        {
            var inventoryRenderer = eventData.pointerEnter.GetComponentInParent<InventoryRenderer>();
            if (inventoryRenderer != null && weaponManager.inventoryManager != null)
            {
                // Try to add to inventory
                if (weaponManager.inventoryManager.TryAdd(currentWeapon))
                {
                    droppedInInventory = true;
                    weaponManager.UnequipWeapon(isRightHandSlot);
                    ClearSlot();

                    // Update other slot if two-handed
                    if (currentWeaponDef.IsTwoHanded && weaponManager != null)
                    {
                        var otherSlot = isRightHandSlot ? weaponManager.leftHandSlot : weaponManager.rightHandSlot;
                        if (otherSlot != null)
                        {
                            otherSlot.ClearSlot();
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Failed to add {currentWeaponDef.Name} to inventory in WeaponUISlot on {gameObject.name}.", this);
                }
            }
        }

        // If not dropped in inventory, restore or clear slot based on WeaponManager state
        if (!droppedInInventory)
        {
            ItemDefinition equippedWeapon = isRightHandSlot ? weaponManager.equippedRightHandWeapon : weaponManager.equippedLeftHandWeapon;
            if (equippedWeapon == currentWeaponDef)
            {
                UpdateSlot(currentWeapon);
            }
            else
            {
                ClearSlot();
            }
        }

        // Clear the dragged item
        InventoryController.ClearDraggedItem();
    }

    public void UpdateSlot(IInventoryItem weapon)
    {
        if (weapon == null)
        {
            ClearSlot();
            return;
        }

        ItemDefinition weaponDef = weapon as ItemDefinition;
        if (weaponDef == null)
        {
            Debug.LogWarning($"Weapon is not an ItemDefinition in UpdateSlot for WeaponUISlot on {gameObject.name}.", this);
            return;
        }

        currentWeapon = weapon;
        CurrentWeapon = weaponDef;

        if (weaponImage != null)
        {
            weaponImage.sprite = weapon.sprite;
            weaponImage.enabled = weapon.sprite != null;
        }
        if (weaponNameText != null)
        {
            weaponNameText.text = weaponDef.Name;
        }
        Debug.Log($"WeaponUISlot on {gameObject.name} updated with {weaponDef.Name}.", this);
    }

    public void ClearSlot()
    {
        currentWeapon = null;
        CurrentWeapon = null;
        if (weaponImage != null)
        {
            weaponImage.sprite = null;
            weaponImage.enabled = false;
        }
        if (weaponNameText != null)
        {
            weaponNameText.text = "";
        }
        Debug.Log($"WeaponUISlot on {gameObject.name} cleared.", this);
    }

    public void OnWeaponUnequipped()
    {
        // Only clear if the equipped weapon matches this slot's weapon
        ItemDefinition equippedWeapon = isRightHandSlot ? weaponManager.equippedRightHandWeapon : weaponManager.equippedLeftHandWeapon;
        if (equippedWeapon == null || equippedWeapon != CurrentWeapon)
        {
            ClearSlot();
        }
    }
}
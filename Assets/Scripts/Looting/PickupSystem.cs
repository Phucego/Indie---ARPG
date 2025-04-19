using UnityEngine;
using FarrokhGames.Inventory.Examples;
using System.Collections.Generic;

public class PickupSystem : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField, Tooltip("Layer for pickable objects")]
    private LayerMask pickableLayer;

    [SerializeField, Tooltip("Radius of the trigger sphere for detecting pickable items")]
    private float detectionRadius = 2f;

    [Header("References")]
    [SerializeField, Tooltip("Reference to the WeaponManager")]
    private WeaponManager weaponManager;

    private List<PickableItem> nearbyItems = new List<PickableItem>();
    private PickableItem hoveredItem;
    private Camera mainCamera;

    private void Awake()
    {
        if (weaponManager == null)
        {
            weaponManager = GetComponent<WeaponManager>();
            if (weaponManager == null)
            {
                Debug.LogError("WeaponManager not assigned and not found on GameObject.");
            }
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found. Please tag a camera as MainCamera.");
        }
    }

    private void Update()
    {
        // Detect hovered item via raycast
        UpdateHoveredItem();

        // Update outline and text display
        foreach (var item in nearbyItems)
        {
            bool isHovered = item == hoveredItem;
            item.Highlight(isHovered);
            if (isHovered)
            {
                item.ShowItemName();
            }
            else
            {
                item.HideItemName();
            }
        }

        // Handle pickup with left mouse button
        if (Input.GetMouseButtonDown(0) && hoveredItem != null && IsWithinDetectionRadius(hoveredItem))
        {
            PickupItem(hoveredItem);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & pickableLayer) != 0)
        {
            PickableItem item = other.GetComponent<PickableItem>();
            if (item != null && !nearbyItems.Contains(item))
            {
                nearbyItems.Add(item);
                Debug.Log($"Detected pickable item: {item.ItemDefinition?.Name ?? "null"}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & pickableLayer) != 0)
        {
            PickableItem item = other.GetComponent<PickableItem>();
            if (item != null && nearbyItems.Contains(item))
            {
                nearbyItems.Remove(item);
                item.Highlight(false);
                item.HideItemName();
                Debug.Log($"Pickable item exited: {item.ItemDefinition?.Name ?? "null"}");
            }
        }
    }

    private void UpdateHoveredItem()
    {
        PickableItem previousHoveredItem = hoveredItem;
        hoveredItem = null;

        // Raycast from mouse position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, pickableLayer))
        {
            PickableItem item = hit.collider.GetComponent<PickableItem>();
            if (item != null && nearbyItems.Contains(item) && IsWithinDetectionRadius(item))
            {
                hoveredItem = item;
            }
        }

        // Hide text for previously hovered item if it changed
        if (previousHoveredItem != null && previousHoveredItem != hoveredItem)
        {
            previousHoveredItem.HideItemName();
        }
    }

    private bool IsWithinDetectionRadius(PickableItem item)
    {
        if (item == null) return false;
        return Vector3.Distance(transform.position, item.transform.position) <= detectionRadius;
    }

    private void PickupItem(PickableItem item)
    {
        if (item == null || item.ItemDefinition == null)
        {
            Debug.LogWarning("Cannot pick up: Item or ItemDefinition is null.");
            return;
        }

        ItemDefinition itemDef = item.ItemDefinition;
        if (itemDef.Type != ItemType.Weapons)
        {
            Debug.LogWarning($"Cannot pick up {itemDef.Name}: Only weapons can be picked up.");
            item.Pickup();
            return;
        }

        // Add to inventory
        weaponManager.AddItemToInventory(itemDef);

        // Equip to right hand if empty
        if (weaponManager.isRightHandEmpty)
        {
            weaponManager.EquipWeapon(itemDef, true);
            Debug.Log($"Equipped {itemDef.Name} to right hand on pickup.");
        }
        else
        {
            Debug.Log($"Added {itemDef.Name} to inventory. Right hand is not empty.");
        }

        // Destroy the pickable object
        item.Pickup();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
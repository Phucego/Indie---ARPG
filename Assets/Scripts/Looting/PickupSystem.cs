using UnityEngine;
using TMPro;
using DG.Tweening;
using FarrokhGames.Inventory;

public class PickupSystem : MonoBehaviour
{
    [SerializeField] private Camera mainCamera; // Main camera for raycasting
    [SerializeField] private float pickupRange = 2f; // Range to pick up items
    [SerializeField] private LayerMask pickableLayer; // Layer for pickable objects
    [SerializeField] private InventoryManager inventory; // Reference to InventoryManager
    [SerializeField] private Transform leftHandSlot; // Transform for left hand equipment
    [SerializeField] private Transform rightHandSlot; // Transform for right hand equipment
    [SerializeField] private float hoverNameHeight = 1f; // Height for hover name pop-up
    [SerializeField] private Canvas uiCanvas; // Canvas for UI elements
    [SerializeField] private WeaponManager weaponManager; // Reference to WeaponManager
    [SerializeField] private float moveSpeed = 5f; // Speed for moving toward item
    [SerializeField] private TMP_FontAsset customFont; // Custom TMP font for PickupUI
    private InventoryItem currentHoveredItem; // Currently hovered pickable item
    private PickupUI currentPickupUI; // UI for displaying item name
    private bool isMovingToItem; // Flag to track if moving to pick up an item
    private InventoryItem targetItem; // Item to pick up after moving

    private void Awake()
    {
        if (!mainCamera) mainCamera = Camera.main;
        if (!inventory) Debug.LogError("InventoryManager reference missing.");
        if (!uiCanvas) Debug.LogError("UI Canvas reference missing.");
        if (!weaponManager) Debug.LogError("WeaponManager reference missing.");
        if (!customFont) Debug.LogWarning("Custom TMP font not assigned, PickupUI will use default TMP font.");
    }

    private void Update()
    {
        HandleHover();
        HandlePickupInput();
        HandleMovement();
    }

    private void HandleHover()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        bool hitPickable = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, pickableLayer);

        InventoryItem newHoveredItem = hitPickable ? hit.collider.GetComponent<InventoryItem>() : null;

        if (newHoveredItem != currentHoveredItem)
        {
            if (currentPickupUI)
            {
                currentPickupUI.FadeOut(() => Destroy(currentPickupUI.gameObject));
                currentPickupUI = null;
            }

            currentHoveredItem = newHoveredItem;

            if (currentHoveredItem)
            {
                GameObject uiObject = new GameObject("PickupUI");
                uiObject.transform.SetParent(uiCanvas.transform, false);
                currentPickupUI = uiObject.AddComponent<PickupUI>();
                currentPickupUI.Initialize(currentHoveredItem.itemName, currentHoveredItem.transform, hoverNameHeight);
            }
        }
    }

    private void HandlePickupInput()
    {
        if (Input.GetMouseButtonDown(0) && currentHoveredItem && !isMovingToItem)
        {
            float distance = Vector3.Distance(transform.position, currentHoveredItem.transform.position);
            if (distance <= pickupRange)
            {
                PickupItem(currentHoveredItem);
            }
            else
            {
                isMovingToItem = true;
                targetItem = currentHoveredItem;
            }
        }
    }

    private void HandleMovement()
    {
        if (isMovingToItem && targetItem)
        {
            Vector3 targetPosition = targetItem.transform.position;
            float distance = Vector3.Distance(transform.position, targetPosition);

            if (distance <= pickupRange)
            {
                PickupItem(targetItem);
                isMovingToItem = false;
                targetItem = null;
            }
            else
            {
                Vector3 direction = (targetPosition - transform.position).normalized;
                transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

                // Optional: Face the target
                transform.LookAt(targetPosition);
            }
        }
    }

    private void PickupItem(InventoryItem item)
    {
        if (inventory.TryAdd(item))
        {
            if (currentPickupUI)
            {
                currentPickupUI.FadeOut(() => Destroy(currentPickupUI.gameObject));
                currentPickupUI = null;
            }

            bool isLeftHandEmpty = weaponManager.isLeftHandEmpty;
            bool isRightHandEmpty = weaponManager.isRightHandEmpty;

            if (isLeftHandEmpty && isRightHandEmpty)
            {
                weaponManager.EquipWeaponFromPickup(item);
            }

            item.gameObject.SetActive(false);
            currentHoveredItem = null;
        }
        else
        {
            Debug.Log("Inventory full, cannot pick up " + item.itemName);
        }
    }
}
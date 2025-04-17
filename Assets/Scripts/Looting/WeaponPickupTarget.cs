using UnityEngine;
using TMPro;
using FarrokhGames.Inventory;
using FarrokhGames.Inventory.Examples;

[RequireComponent(typeof(Collider))]
public class WeaponPickupTarget : MonoBehaviour, IInteractable
{
    [Header("Pickup Config")]
    public ItemDefinition itemDefinition;
    public Outline outline;
    public Transform infoCanvas;
    public TextMeshProUGUI infoText;
    public float pickupRange = 1.5f;

    private bool isBeingInteractedWith = false;
    private Transform player;
    private PlayerMovement playerMovement;

    private InventoryManager inventoryManager;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerMovement = player?.GetComponent<PlayerMovement>();

        inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager not found in the scene.");
        }
        else
        {
            Debug.Log("InventoryManager found successfully.");
        }

        if (infoCanvas != null) infoCanvas.gameObject.SetActive(false);
        if (outline != null) outline.enabled = false;
    }


    private void Update()
    {
        if (isBeingInteractedWith && player != null)
        {
            float dist = Vector3.Distance(player.position, transform.position);
            if (dist <= pickupRange)
            {
                TryPickup();
                isBeingInteractedWith = false;
            }
            else
            {
                playerMovement?.MoveToTarget(transform.position);
            }

            if (infoCanvas != null && Camera.main != null)
            {
                infoCanvas.LookAt(Camera.main.transform.position);
                infoCanvas.Rotate(0f, 180f, 0f);
            }
        }
    }

    public void Interact()
    {
        isBeingInteractedWith = true;
    }

    private void TryPickup()
    {
        if (itemDefinition == null)
        {
            Debug.LogWarning("ItemDefinition is missing.");
            return;
        }

        if (inventoryManager == null)
        {
            Debug.LogWarning("InventoryManager is not found.");
            return;
        }

        // Create the item instance from the ItemDefinition
        IInventoryItem instance = itemDefinition.CreateInstance();

        // Try to add the item to the inventory
        if (inventoryManager.TryAddItem(instance))
        {
            Weapon weaponToEquip = itemDefinition.WeaponPrefabReference;

            if (weaponToEquip != null)
            {
                // Ensure WeaponManager.Instance is not null
                if (WeaponManager.Instance != null)
                {
                    WeaponManager.Instance.AddWeaponToInventory(weaponToEquip);

                    // Equip logic based on available hands
                    bool rightEmpty = WeaponManager.Instance.isRightHandEmpty;
                    bool leftEmpty = WeaponManager.Instance.isLeftHandEmpty;

                    if (weaponToEquip.weaponData.isTwoHanded)
                    {
                        if (rightEmpty && leftEmpty)
                        {
                            WeaponManager.Instance.EquipWeapon(weaponToEquip, true);
                            Debug.Log($"Two-handed weapon equipped: {itemDefinition.Name}");
                        }
                    }
                    else
                    {
                        if (rightEmpty)
                        {
                            WeaponManager.Instance.EquipWeapon(weaponToEquip, true);
                            Debug.Log($"One-handed weapon equipped in right hand: {itemDefinition.Name}");
                        }
                        else if (leftEmpty)
                        {
                            WeaponManager.Instance.EquipWeapon(weaponToEquip, false);
                            Debug.Log($"One-handed weapon equipped in left hand: {itemDefinition.Name}");
                        }
                    }
                }
                else
                {
                    Debug.LogError("WeaponManager.Instance is null. Make sure the WeaponManager is set up correctly.");
                }

                Debug.Log($"Picked up: {itemDefinition.Name}");
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.Log("Failed to add item to inventory. Maybe it's full.");
        }
    }

    private void OnMouseEnter()
    {
        if (outline != null) outline.enabled = true;
        if (infoCanvas != null && infoText != null)
        {
            infoCanvas.gameObject.SetActive(true);
            infoText.text = $"[LMB] Pick up {itemDefinition?.Name ?? "Item"}";
        }
    }

    private void OnMouseExit()
    {
        if (outline != null) outline.enabled = false;
        if (infoCanvas != null) infoCanvas.gameObject.SetActive(false);
    }

    private void OnMouseDown()
    {
        Interact();
    }
}

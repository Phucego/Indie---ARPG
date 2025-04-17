using UnityEngine;
using TMPro;
using FarrokhGames.Inventory;
using FarrokhGames.Inventory.Examples;

[RequireComponent(typeof(Collider))]
public class WeaponPickup : MonoBehaviour, IInteractable
{
    [Header("Pickup Configuration")]
    [Tooltip("The ItemDefinition that this pickup represents")]
    public ItemDefinition itemDefinition;

    [Header("Visuals & UI")]
    public Outline outline;
    public Transform infoCanvas;
    public TextMeshProUGUI infoText;

    [Header("Pickup Settings")]
    public float pickupRange = 1.5f;

    private Transform player;
    private PlayerMovement playerMovement;
    private bool isHovered = false;
    private bool isApproachingToPickup = false;

    private InventoryManager inventoryManager;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerMovement = player?.GetComponent<PlayerMovement>();

        inventoryManager = FindObjectOfType<InventoryManager>();


        if (inventoryManager == null)
        {
            Debug.LogError("[WeaponPickup] InventoryManager is NULL! Is SizeInventoryExample initialized first?");
        }

        if (itemDefinition == null)
        {
            Debug.LogError($"[WeaponPickup] ItemDefinition is not assigned on {gameObject.name}");
        }

        if (infoCanvas != null)
            infoCanvas.gameObject.SetActive(false);

        if (outline != null)
            outline.enabled = false;
    }

    private void Update()
    {
        if (isApproachingToPickup && player != null)
        {
            float dist = Vector3.Distance(player.position, transform.position);
            if (dist <= pickupRange)
            {
                TryPickup();
                isApproachingToPickup = false;
            }
            else
            {
                playerMovement?.MoveToTarget(transform.position);
            }
        }

        if (isHovered && infoCanvas != null && Camera.main != null)
        {
            infoCanvas.rotation = Quaternion.LookRotation(infoCanvas.position - Camera.main.transform.position);
        }
    }

    private void OnMouseEnter()
    {
        isHovered = true;

        if (outline != null)
            outline.enabled = true;

        if (infoCanvas != null && infoText != null)
        {
            infoCanvas.gameObject.SetActive(true);
            infoText.text = $"[LMB] Pick up {itemDefinition?.Name ?? "Unknown Item"}";
        }
    }

    private void OnMouseExit()
    {
        isHovered = false;

        if (outline != null)
            outline.enabled = false;

        if (infoCanvas != null)
            infoCanvas.gameObject.SetActive(false);
    }

    private void OnMouseDown()
    {
        isApproachingToPickup = true;
    }

    private void TryPickup()
    {
        if (itemDefinition == null)
        {
            Debug.LogWarning("[WeaponPickup] ItemDefinition is missing.");
            return;
        }

        if (inventoryManager == null)
        {
            Debug.LogWarning("[WeaponPickup] InventoryManager is not found.");
            return;
        }

        IInventoryItem instance = itemDefinition.CreateInstance();

        if (instance == null)
        {
            Debug.LogWarning($"[WeaponPickup] Failed to create instance from ItemDefinition: {itemDefinition.name}");
            return;
        }

        if (inventoryManager.TryAddItem(instance))
        {
            Debug.Log($"[WeaponPickup] Added to inventory: {itemDefinition.Name}");

            if (WeaponManager.Instance == null)
            {
                Debug.LogError("[WeaponPickup] WeaponManager.Instance is null!");
                return;
            }

            Weapon weaponPrefab = itemDefinition.WeaponPrefabReference;

            if (weaponPrefab != null)
            {
                WeaponManager.Instance.AddWeaponToInventory(weaponPrefab);

                bool isTwoHanded = weaponPrefab.weaponData.isTwoHanded;
                bool rightEmpty = WeaponManager.Instance.isRightHandEmpty;
                bool leftEmpty = WeaponManager.Instance.isLeftHandEmpty;

                if (isTwoHanded)
                {
                    if (rightEmpty && leftEmpty)
                    {
                        WeaponManager.Instance.EquipWeapon(weaponPrefab, true);
                        Debug.Log($"[WeaponPickup] Two-handed weapon equipped: {itemDefinition.Name}");
                    }
                }
                else
                {
                    if (rightEmpty)
                    {
                        WeaponManager.Instance.EquipWeapon(weaponPrefab, true);
                        Debug.Log($"[WeaponPickup] One-handed weapon equipped in right hand: {itemDefinition.Name}");
                    }
                    else if (leftEmpty)
                    {
                        WeaponManager.Instance.EquipWeapon(weaponPrefab, false);
                        Debug.Log($"[WeaponPickup] One-handed weapon equipped in left hand: {itemDefinition.Name}");
                    }
                }
            }
            else
            {
                Debug.Log($"[WeaponPickup] Item {itemDefinition.Name} has no WeaponPrefabReference assigned.");
            }

            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning($"[WeaponPickup] Failed to add item '{itemDefinition.Name}' to inventory. Maybe it's full?");
        }
    }

    public void Interact()
    {
        isApproachingToPickup = true;
    }
}

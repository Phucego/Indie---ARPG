using UnityEngine;

namespace FarrokhGames.Inventory.Examples
{
    [RequireComponent(typeof(InventoryRenderer))]
    public class SizeInventoryExample : MonoBehaviour
    {
        [SerializeField] private InventoryRenderMode _renderMode = InventoryRenderMode.Grid;
        [SerializeField] private int _maximumAllowedItemCount = -1;
        [SerializeField] private ItemType _allowedItem = ItemType.Weapons; // Restrict to weapons
        [SerializeField] private int _width = 8;
        [SerializeField] private int _height = 4;
        [SerializeField] private ItemDefinition[] _definitions = null;
        [SerializeField] private bool _fillRandomly = true;
        [SerializeField] private bool _fillEmpty = false;

        [SerializeField] private WeaponManager weaponManager; // Reference to WeaponManager

        void Start()
        {
            var provider = new InventoryProvider(_renderMode, _maximumAllowedItemCount, _allowedItem);

            // Create inventory
            var inventory = new InventoryManager(provider, _width, _height);

            // Assign to WeaponManager
            if (weaponManager != null)
            {
                weaponManager.inventoryManager = inventory;
            }

            // Fill inventory with random items
            if (_fillRandomly)
            {
                var tries = (_width * _height) / 3;
                for (var i = 0; i < tries; i++)
                {
                    var item = _definitions[Random.Range(0, _definitions.Length)];
                    inventory.TryAdd(item.CreateInstance());
                }
            }

            // Fill empty slots with first (1x1) item
            if (_fillEmpty)
            {
                for (var i = 0; i < _width * _height; i++)
                {
                    var item = _definitions[0];
                    inventory.TryAdd(item.CreateInstance());
                }
            }

            // Set the renderer's inventory to trigger drawing
            GetComponent<InventoryRenderer>().SetInventory(inventory, provider.inventoryRenderMode);

            // Log items being dropped on the ground
            inventory.onItemDropped += (item) =>
            {
                Debug.Log((item as ItemDefinition).Name + " was dropped on the ground");
            };

            inventory.onItemDroppedFailed += (item) =>
            {
                Debug.Log($"You're not allowed to drop {(item as ItemDefinition).Name} on the ground");
            };

            inventory.onItemAddedFailed += (item) =>
            {
                Debug.Log($"You can't put {(item as ItemDefinition).Name} there!");
            };
        }
    }
}
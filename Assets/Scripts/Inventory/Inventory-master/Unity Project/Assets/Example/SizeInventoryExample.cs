using UnityEngine;

namespace FarrokhGames.Inventory.Examples
{
    [RequireComponent(typeof(InventoryRenderer))]
    public class SizeInventoryExample : MonoBehaviour
    {
        [SerializeField] private InventoryRenderMode _renderMode = InventoryRenderMode.Grid;
        [SerializeField] private int _maximumAlowedItemCount = -1;
        [SerializeField] private ItemType _allowedItem = ItemType.Any;
        [SerializeField] private int _width = 8;
        [SerializeField] private int _height = 4;

        // Make inventory static so it can be accessed globally
        public static InventoryManager Inventory { get; private set; }

        void Start()
        {
            var provider = new InventoryProvider(_renderMode, _maximumAlowedItemCount, _allowedItem);
            Inventory = new InventoryManager(provider, _width, _height);

            // Attach to renderer
            GetComponent<InventoryRenderer>().SetInventory(Inventory, provider.inventoryRenderMode);

            // Debug logs
            Inventory.onItemDropped += (item) =>
            {
                Debug.Log((item as ItemDefinition).Name + " was dropped on the ground");
            };

            Inventory.onItemDroppedFailed += (item) =>
            {
                Debug.Log($"You're not allowed to drop {(item as ItemDefinition).Name} on the ground");
            };

            Inventory.onItemAddedFailed += (item) =>
            {
                Debug.Log($"You can't put {(item as ItemDefinition).Name} there!");
            };
        }
    }
}
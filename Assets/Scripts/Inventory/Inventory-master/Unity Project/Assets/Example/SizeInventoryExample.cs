using UnityEngine;
using FarrokhGames.Inventory.Examples;
using System.Collections.Generic;
using System.Linq;

namespace FarrokhGames.Inventory.Examples
{
    [RequireComponent(typeof(InventoryRenderer))]
    public class SizeInventoryExample : MonoBehaviour
    {
        [SerializeField, Tooltip("Render mode for the inventory")]
        private InventoryRenderMode _renderMode = InventoryRenderMode.Grid;

        [SerializeField, Tooltip("Maximum allowed item count (-1 for unlimited)")]
        private int _maximumAllowedItemCount = -1;

        [SerializeField, Tooltip("Allowed item type")]
        private ItemType _allowedItem = ItemType.Weapons;

        [SerializeField, Tooltip("Inventory width")]
        private int _width = 8;

        [SerializeField, Tooltip("Inventory height")]
        private int _height = 4;

        [SerializeField, Tooltip("Initial item definitions for populating inventory")]
        private ItemDefinition[] _definitions = null;

        [SerializeField, Tooltip("Fill inventory with random items on start")]
        private bool _fillRandomly = false;

        [SerializeField, Tooltip("Fill empty slots with first definition on start")]
        private bool _fillEmpty = false;

        [SerializeField, Tooltip("Reference to the WeaponManager")]
        private WeaponManager weaponManager;

        [SerializeField, Tooltip("Reference to the right hand WeaponUISlot")]
        private WeaponUISlot weaponUISlot;

        private InventoryManager inventory;
        private List<ItemDefinition> dynamicDefinitions; // Dynamic list for runtime updates

        void Start()
        {
            // Initialize inventory
            var provider = new InventoryProvider(_renderMode, _maximumAllowedItemCount, _allowedItem);
            inventory = new InventoryManager(provider, _width, _height);

            // Initialize dynamic definitions from serialized array
            dynamicDefinitions = _definitions != null ? _definitions.ToList() : new List<ItemDefinition>();

            // Assign inventory to WeaponManager
            if (weaponManager != null)
            {
                weaponManager.inventoryManager = inventory;
            }
            else
            {
                Debug.LogWarning("WeaponManager is not assigned in SizeInventoryExample.", this);
            }

            // Fill inventory randomly if enabled
            if (_fillRandomly && dynamicDefinitions != null && dynamicDefinitions.Count > 0)
            {
                var tries = (_width * _height) / 3;
                for (var i = 0; i < tries; i++)
                {
                    var item = dynamicDefinitions[Random.Range(0, dynamicDefinitions.Count)];
                    inventory.TryAdd(item.CreateInstance());
                }
            }

            // Fill empty slots if enabled
            if (_fillEmpty && dynamicDefinitions != null && dynamicDefinitions.Count > 0)
            {
                for (var i = 0; i < _width * _height; i++)
                {
                    var item = dynamicDefinitions[0];
                    inventory.TryAdd(item.CreateInstance());
                }
            }

            // Set up renderer
            var renderer = GetComponent<InventoryRenderer>();
            renderer.SetInventory(inventory, provider.inventoryRenderMode);

            // Subscribe to inventory events
            inventory.onItemDropped += OnItemDropped;
            inventory.onItemDroppedFailed += OnItemDroppedFailed;
            inventory.onItemAdded += OnItemAdded;
            inventory.onItemAddedFailed += OnItemAddedFailed;

            // Sync definitions to serialized field (for Inspector visibility)
            SyncDefinitionsToSerializedField();
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.onItemDropped -= OnItemDropped;
                inventory.onItemDroppedFailed -= OnItemDroppedFailed;
                inventory.onItemAdded -= OnItemAdded;
                inventory.onItemAddedFailed -= OnItemAddedFailed;
            }
        }

        private void OnItemAdded(IInventoryItem item)
        {
            ItemDefinition itemDef = item as ItemDefinition;
            if (itemDef != null)
            {
                // Add to dynamicDefinitions if not already present
                if (!dynamicDefinitions.Contains(itemDef))
                {
                    dynamicDefinitions.Add(itemDef);
                    Debug.Log($"Added {itemDef.Name} to dynamic definitions.", this);
                    SyncDefinitionsToSerializedField();
                }

                // Unequip if the item is equipped
                if (weaponManager != null && weaponManager.equippedRightHandWeapon == item)
                {
                    weaponManager.UnequipWeapon(true);
                    if (weaponUISlot != null)
                    {
                        weaponUISlot.OnWeaponUnequipped();
                    }
                }
            }
        }

        private void OnItemDropped(IInventoryItem item)
        {
            ItemDefinition itemDef = item as ItemDefinition;
            if (itemDef != null)
            {
                Debug.Log($"{itemDef.Name} was dropped on the ground", this);

                // Unequip if the item is equipped
                if (weaponManager != null && weaponManager.equippedRightHandWeapon == item)
                {
                    weaponManager.UnequipWeapon(true);
                    if (weaponUISlot != null)
                    {
                        weaponUISlot.OnWeaponUnequipped();
                    }
                }
            }
        }

        private void OnItemDroppedFailed(IInventoryItem item)
        {
            ItemDefinition itemDef = item as ItemDefinition;
            if (itemDef != null)
            {
                Debug.Log($"You're not allowed to drop {itemDef.Name} on the ground", this);
            }
        }

        private void OnItemAddedFailed(IInventoryItem item)
        {
            ItemDefinition itemDef = item as ItemDefinition;
            if (itemDef != null)
            {
                Debug.Log($"You can't put {itemDef.Name} there!", this);
            }
        }

        private void SyncDefinitionsToSerializedField()
        {
            // Update the serialized field for Inspector visibility
            _definitions = dynamicDefinitions.ToArray();
        }
    }
}
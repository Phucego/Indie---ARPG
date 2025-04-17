using UnityEngine;
using FarrokhGames.Inventory;
using FarrokhGames.Inventory.Examples; // Ensure the correct namespace is being used

public class InventoryAccess : MonoBehaviour
{
    public static InventoryAccess Instance { get; private set; }

    public InventoryManager Inventory { get; private set; }  // Correct reference for InventoryManager
    public InventoryProvider Provider { get; private set; }  // Correct reference for InventoryProvider

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);  // Ensures only one instance is kept
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);  // Keeps the singleton across scenes
    }

    // Register method to assign the InventoryManager and InventoryProvider
    public void Register(InventoryManager inventory, InventoryProvider provider)
    {
        Inventory = inventory;
        Provider = provider;
    }
}

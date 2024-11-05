using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public InventoryObject _playerInventory;

    public void OnTriggerEnter(Collider other)
    {
        var item = other.GetComponent<ItemClass>();
        Debug.Log(item);
        if (item)
        {
            _playerInventory.AddItem(item.item, 1);
            Destroy(other.gameObject);
        }
    }
//TODO: Clear all items in inventory
    private void OnApplicationQuit()
    {
        _playerInventory.itemContainer.Clear();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LootBag : MonoBehaviour
{
    public GameObject droppedLootPrefab;
    public List<Loot> lootList = new List<Loot>();

    Loot GetDroppedItem()
    {
        int randNumber = Random.Range(1, 101);

        List<Loot> possibleItems = new List<Loot>();

        foreach (Loot item in lootList)
        {
            if (randNumber <= item.dropChance)
            {
                possibleItems.Add(item);
             
            }
        }

        if (possibleItems.Count > 0)
        {
            Loot droppedItem = possibleItems[Random.Range(0, possibleItems.Count)];
        }   
        Debug.Log("No Loot Dropped");
        return null;
    }

    public void InstantiateLoot(Vector3 spawnPos)
    {
        Loot droppedItem = GetDroppedItem();
            
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LootSO", menuName = "LootData/Loot")]
public class Loot : ScriptableObject
{
    public GameObject lootPrefab;
    public string name;
    public int dropChance;

    public Loot(string name, int dropChance)
    {
        this.name = name;
        this.dropChance = dropChance;
    }
}

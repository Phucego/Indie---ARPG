using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public enum ItemType{ Potions, Equipment, Default }
public abstract class ItemObject : ScriptableObject
{
    public GameObject prefab;
    public ItemType itemType;
    
    [TextArea(15, 20)]
    public string description;

}

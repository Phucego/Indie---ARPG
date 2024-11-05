using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "New Default Object", menuName = "Inventory/Items/Default")]
public class DefaultObject : ItemObject
{
    public void Awake()
    {
        itemType = ItemType.Default;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Default Object", menuName = "Inventory/Items/Equipment")]
public class EquipmentObject : ItemObject
{
    public float attackBonus;
    public float defenseBonus;
    public void Awake()
    {
        itemType = ItemType.Equipment;
    }
}

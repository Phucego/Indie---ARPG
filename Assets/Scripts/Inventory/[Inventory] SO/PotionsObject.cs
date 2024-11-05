using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Default Object", menuName = "Inventory/Items/Potions")]
public class PotionsObject : ItemObject
{
    public int restoreHPValue;
    public int restoreManaValue;
    public int restoreStaminaValue;
    public void Awake()
    {
        itemType = ItemType.Potions;
    }
}

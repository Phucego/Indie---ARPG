using FarrokhGames.Inventory;
using UnityEngine;
using FarrokhGames.Inventory.Examples;

public class Weapon : MonoBehaviour, IInventoryItem
{
    public ItemDefinition itemDefinition;

    public string Name => itemDefinition.Name;
    public ItemType Type => itemDefinition.Type;
    public Sprite sprite => itemDefinition.sprite;
    public int width => itemDefinition.width;
    public int height => itemDefinition.height;
    public Vector2Int position
    {
        get => itemDefinition.position;
        set => itemDefinition.position = value;
    }
    public bool canDrop => itemDefinition.canDrop;

    public bool IsPartOfShape(Vector2Int localPosition)
    {
        return itemDefinition.IsPartOfShape(localPosition);
    }

    // Additional properties
    public GameObject weaponPrefab => itemDefinition.WeaponPrefab;
    public bool isTwoHanded => itemDefinition.IsTwoHanded;
    public float baseDamage => itemDefinition.BaseDamage;
}
using UnityEngine;
using FarrokhGames.Inventory;

public class InventoryItem : MonoBehaviour, IInventoryItem, ItemDefinition
{
    [SerializeField] private string _itemName;
    [SerializeField] private Sprite _sprite;
    [SerializeField] private int _width = 1;
    [SerializeField] private int _height = 1;
    [SerializeField] private bool _canDrop = true;
    [SerializeField] private ItemType _type = ItemType.Any; // Add ItemType field
    private InventoryShape _shape;
    private Vector2Int _position;

    public ItemType Type => _type; // Implement ItemDefinition
    public string itemName => _itemName;
    public Sprite sprite => _sprite;
    public int width => _width;
    public int height => _height;
    public bool canDrop => _canDrop;
    public Vector2Int position { get => _position; set => _position = value; }

    private void Awake()
    {
        _shape = new InventoryShape(_width, _height);
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _shape.IsPartOfShape(new Vector2Int(x, y));
            }
        }
    }

    public bool IsPartOfShape(Vector2Int localPosition)
    {
        return _shape.IsPartOfShape(localPosition);
    }
}

public interface ItemDefinition
{
    ItemType Type { get; }
}

public enum ItemType
{
    Any,
    Weapon,
    Armor,
    Consumable
}
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public float attackRange;
    public float weaponHitRadius;
    public WeaponType weaponType;
    public Transform weaponTipPrefab;
    
    public bool isTwoHanded;
}

public enum WeaponType
{
    Sword,
    Axe,
    Spear,
    Hammer,
    Shield
}
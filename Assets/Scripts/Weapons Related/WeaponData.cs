using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName;
    public float attackRange;
    public float weaponHitRadius;
    public WeaponType weaponType;
    public Transform weaponTipPrefab;

    [Header("Weapon Handling")]
    public bool isTwoHanded;

    // New property to determine if the weapon is one-handed
    public bool IsOneHanded => !isTwoHanded;
}

public enum WeaponType
{
    Sword,
    Axe,
    Spear,
    Hammer,
    Fist
}
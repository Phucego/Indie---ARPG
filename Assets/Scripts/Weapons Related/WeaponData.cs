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

    [Header("Weapon Stats")]
    public float baseDamage = 10f; // Add base damage here
    public float damageBonus = 0f;  // New damage bonus property

    public float critChance = 0f;
    // Method to calculate the total damage with the damage bonus
    public float GetTotalDamage()
    {
        return baseDamage + damageBonus;  // Base damage + damage bonus
    }
}

public enum WeaponType
{
    Sword,
    Axe,
    Spear,
    Hammer,
    Fist
}
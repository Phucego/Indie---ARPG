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
    public float damageBonus = 0f;  // New damage bonus property

    // Method to calculate the total damage with the damage bonus
    public float GetTotalDamage(float baseDamage)
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
using UnityEngine;

[System.Serializable]
public class Weapon
{
    public string weaponName;
    public GameObject weaponPrefab;
    public bool isTwoHanded;

    [Header("Weapon Stats")]
    public WeaponData weaponData;

    [Header("Damage Stats")]
    public float damageBonus = 0f;

    public Weapon(string name, GameObject prefab, bool twoHanded, WeaponData data, float damageBonus = 0f)
    {
        weaponName = name;
        weaponPrefab = prefab;
        isTwoHanded = twoHanded;
        weaponData = data;
        this.damageBonus = damageBonus;
    }

    // Method to get the weapon's base damage (from WeaponData)
    public float GetWeaponDamage()
    {
        if (weaponData != null)
        {
            return weaponData.baseDamage;
        }
        return 0f; // Return 0 if weaponData is null
    }

    // Method to modify weapon's base damage and crit chance
    public void ModifyWeaponData(float newBaseDamage, float newCritChance = -1f)
    {
        if (weaponData == null) return;

        weaponData.baseDamage = newBaseDamage;

        // Update crit chance only if a valid value is passed
        if (newCritChance >= 0f)
            weaponData.critChance = newCritChance;
    }

    // Method to calculate total damage based on weapon stats and player bonus
    public float GetTotalDamage(float baseDamage)
    {
        float weaponDamage = GetWeaponDamage();
        return weaponDamage + damageBonus + baseDamage;
    }
}
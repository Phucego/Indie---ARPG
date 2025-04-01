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
    public float damageBonus = 0f;  // New damage bonus variable

    public Weapon(string name, GameObject prefab, bool twoHanded, WeaponData data, float damageBonus = 0f)
    {
        weaponName = name;
        weaponPrefab = prefab;
        isTwoHanded = twoHanded;
        weaponData = data;
        this.damageBonus = damageBonus;
    }

    // You can now access damage bonus directly from weapon instances
    public float GetTotalDamage(float baseDamage)
    {
        return baseDamage + damageBonus;  // Base damage + bonus
    }
}
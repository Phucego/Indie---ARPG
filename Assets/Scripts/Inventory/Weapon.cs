using UnityEngine;

[System.Serializable]
public class Weapon
{
    public string weaponName;
    public GameObject weaponPrefab;
    public bool isTwoHanded;
    
    public WeaponData weaponData;
}
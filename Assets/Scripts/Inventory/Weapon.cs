using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField, Tooltip("Name of the weapon (e.g., 'Crossbow', 'Dagger', 'Left Fist')")]
    private string weaponName;

    [SerializeField, Tooltip("Prefab of the weapon (matches WeaponManager's crossbowPrefab, daggerPrefab, or leftFistPrefab)")]
    private GameObject weaponPrefab;

    [SerializeField, Tooltip("Damage dealt by the weapon")]
    private float damage;

    [SerializeField, Tooltip("Is the weapon two-handed? (e.g., true for Dagger, false for Crossbow and Left Fist)")]
    private bool isTwoHanded;

    [SerializeField, Tooltip("Sprite for UI display (optional, for WeaponUISlot)")]
    private Sprite uiSprite;

    public string WeaponName => weaponName;
    public GameObject WeaponPrefab => weaponPrefab;
    public float Damage => damage;
    public bool IsTwoHanded => isTwoHanded;
    public Sprite UISprite => uiSprite;

    private void Awake()
    {
        if (string.IsNullOrEmpty(weaponName))
        {
            Debug.LogWarning($"Weapon on {gameObject.name} has no weaponName assigned.");
        }

        if (weaponPrefab == null)
        {
            Debug.LogWarning($"Weapon on {gameObject.name} has no weaponPrefab assigned.");
        }

        if (damage < 0)
        {
            Debug.LogWarning($"Weapon on {gameObject.name} has negative damage ({damage}). Setting to 0.");
            damage = 0;
        }
    }
}
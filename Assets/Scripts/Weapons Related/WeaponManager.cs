using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Crossbow Weapon")]
    [Tooltip("Prefab for the one-handed crossbow")]
    public GameObject crossbowPrefab;
    [Tooltip("Damage dealt by the crossbow")]
    public float crossbowDamage = 10f;
    [Tooltip("Prefab for the crossbow bolt projectile")]
   
    [Header("Projectile Prefabs")]
    public GameObject boltPrefab; // Your current prefab
    public GameObject fireBoltPrefab;
    public GameObject explosiveBoltPrefab;

    [Header("Hand Transforms")]
    public Transform rightHandHolder;
    public Transform leftHandHolder;
    [Tooltip("Child Transform in right hand hierarchy for weapon attachment")]
    public Transform rightHandAttachment;
    [Tooltip("Child Transform in left hand hierarchy for weapon attachment")]
    public Transform leftHandAttachment;

    public bool isRightHandOneHanded { get; private set; }
    public bool isRightHandEmpty { get; private set; } = true;
    public bool isLeftHandEmpty { get; private set; } = true;
    public bool IsRangedWeaponEquipped => currentWeaponType == WeaponType.Crossbow;

    private GameObject currentRightHandWeaponInstance;
    private GameObject currentWeapon;
    private float currentWeaponDamage;

    private enum WeaponType { None, Crossbow } // Added None for fist (no weapon)
    private WeaponType currentWeaponType = WeaponType.None; // Start with no weapon

    public static WeaponManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple WeaponManager instances detected. Destroying this one.", this);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Validate all required references
        if (crossbowPrefab == null)
            Debug.LogError("CrossbowPrefab is not assigned in WeaponManager.", this);
        if (boltPrefab == null)
            Debug.LogError("BoltPrefab is not assigned in WeaponManager.", this);

        if (rightHandHolder == null)
            Debug.LogError("RightHandHolder is not assigned in WeaponManager.", this);
        if (leftHandHolder == null)
            Debug.LogError("LeftHandHolder is not assigned in WeaponManager.", this);
        if (rightHandAttachment == null)
            Debug.LogWarning("RightHandAttachment is not assigned; using RightHandHolder as parent.", this);
        if (leftHandAttachment == null)
            Debug.LogWarning("LeftHandAttachment is not assigned; using LeftHandHolder as parent.", this);

        // Start with no weapon equipped (fist)
        isRightHandEmpty = true;
        isLeftHandEmpty = true;
        currentWeaponType = WeaponType.None;
        Debug.Log("Player starts with fists (no weapon equipped).", this);
    }

    public void PickupCrossbow()
    {
        // Equip crossbow when picked up (e.g., during tutorial)
        EquipCrossbow();
        Debug.Log("Crossbow picked up and equipped.", this);
    }

    public void EquipCrossbow()
    {
        if (crossbowPrefab == null)
        {
            Debug.LogError("Cannot equip crossbow: CrossbowPrefab is null.", this);
            return;
        }

        UnequipWeapon(true);

        Transform parentTransform = rightHandAttachment != null ? rightHandAttachment : rightHandHolder;
        if (parentTransform != null)
        {
            currentRightHandWeaponInstance = Instantiate(crossbowPrefab, parentTransform);
            currentRightHandWeaponInstance.transform.localPosition = Vector3.zero;
            currentRightHandWeaponInstance.transform.localRotation = Quaternion.identity;
            isRightHandEmpty = false;
            isRightHandOneHanded = true;
            isLeftHandEmpty = true;
            Debug.Log("Equipped crossbow in right hand.", this);
        }
        else
        {
            Debug.LogError("Cannot equip crossbow: RightHandHolder or RightHandAttachment is null.", this);
        }

        currentWeapon = crossbowPrefab;
        currentWeaponDamage = crossbowDamage;
        currentWeaponType = WeaponType.Crossbow;
    }

    public void UnequipWeapon(bool isRightHand)
    {
        if (isRightHand && currentRightHandWeaponInstance != null)
        {
            Destroy(currentRightHandWeaponInstance);
            currentRightHandWeaponInstance = null;
            isRightHandEmpty = true;
            isRightHandOneHanded = false;
            currentWeaponType = WeaponType.None; // Reset to no weapon
            currentWeapon = null;
            currentWeaponDamage = 0f;
        }
    }

    public GameObject GetCurrentWeapon()
    {
        return currentWeapon;
    }

    public float GetCurrentWeaponDamage()
    {
        return currentWeaponDamage;
    }

    public void SetProjectile(GameObject newBolt)
    {
        boltPrefab = newBolt;
    }
}
using UnityEngine;
using DG.Tweening;

public class WeaponManager : MonoBehaviour
{
    [Header("Rogue Weapons")]
    [Tooltip("Prefab for the one-handed crossbow")]
    public GameObject crossbowPrefab;
    [Tooltip("Damage dealt by the crossbow")]
    public float crossbowDamage = 10f;
    [Tooltip("Prefab for the two-handed dagger")]
    public GameObject daggerPrefab;
    [Tooltip("Damage dealt by the dagger")]
    public float daggerDamage = 15f;

    [Header("Default Fist Weapon")]
    [Tooltip("Prefab for the left fist (used with crossbow)")]
    public GameObject leftFistPrefab;
    [Tooltip("Damage dealt by the left fist")]
    public float leftFistDamage = 5f;

    [Header("Hand Transforms")]
    public Transform rightHandHolder;
    public Transform leftHandHolder;
    [Tooltip("Child Transform in right hand hierarchy for weapon attachment")]
    public Transform rightHandAttachment;
    [Tooltip("Child Transform in left hand hierarchy for weapon attachment")]
    public Transform leftHandAttachment;

    public bool isRightHandOneHanded { get; private set; }
    public bool isLeftHandOneHanded { get; private set; }
    public bool isTwoHandedEquipped { get; private set; }
    public bool isRightHandEmpty { get; private set; } = true;
    public bool isLeftHandEmpty { get; private set; } = true;
    public bool isWieldingOneHand { get; private set; }

    private GameObject currentRightHandWeaponInstance;
    private GameObject currentLeftHandWeaponInstance;
    private GameObject currentWeapon; // Tracks the equipped weapon (crossbow or dagger)
    private float currentWeaponDamage; // Damage of the equipped weapon

    private enum WeaponType { Crossbow, Dagger }
    private WeaponType currentWeaponType = WeaponType.Crossbow; // Start with crossbow

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
        // Validate weapon prefabs
        if (crossbowPrefab == null)
            Debug.LogWarning("CrossbowPrefab is not assigned.");
        if (daggerPrefab == null)
            Debug.LogWarning("DaggerPrefab is not assigned.");
        if (leftFistPrefab == null)
            Debug.LogWarning("LeftFistPrefab is not assigned.");

        // Validate hand transforms
        if (rightHandHolder == null)
            Debug.LogWarning("RightHandHolder is not assigned.");
        if (leftHandHolder == null)
            Debug.LogWarning("LeftHandHolder is not assigned.");
        if (rightHandAttachment == null)
            Debug.LogWarning("RightHandAttachment is not assigned; using RightHandHolder as parent.");
        if (leftHandAttachment == null)
            Debug.LogWarning("LeftHandAttachment is not assigned; using LeftHandHolder as parent.");

        // Equip the crossbow by default
        EquipCrossbow();
    }

    private void Update()
    {
        // Switch weapons when pressing 'Q', but only if not attacking or in dialogue
        if (Input.GetKeyDown(KeyCode.Q) && PlayerAttack.Instance != null && !PlayerAttack.Instance.isAttacking &&
            (DialogueDisplay.Instance == null || !DialogueDisplay.Instance.IsDialogueActive))
        {
            ToggleWeapon();
        }
    }

    private void ToggleWeapon()
    {
        if (currentWeaponType == WeaponType.Crossbow)
        {
            EquipDagger();
        }
        else
        {
            EquipCrossbow();
        }
    }

    public void EquipCrossbow()
    {
        if (crossbowPrefab == null)
        {
            Debug.LogWarning("Cannot equip crossbow: CrossbowPrefab is null.");
            return;
        }

        // Unequip current weapons
        UnequipWeapon(true);
        UnequipWeapon(false);

        // Equip crossbow in right hand
        Transform parentTransform = rightHandAttachment != null ? rightHandAttachment : rightHandHolder;
        if (parentTransform != null)
        {
            currentRightHandWeaponInstance = Instantiate(crossbowPrefab, parentTransform);
            currentRightHandWeaponInstance.transform.localPosition = Vector3.zero;
            currentRightHandWeaponInstance.transform.localRotation = Quaternion.identity;
            isRightHandEmpty = false;
            isRightHandOneHanded = true;
            Debug.Log("Equipped crossbow in right hand.");
        }

        // Equip left fist in left hand
        if (leftFistPrefab != null)
        {
            parentTransform = leftHandAttachment != null ? leftHandAttachment : leftHandHolder;
            if (parentTransform != null)
            {
                currentLeftHandWeaponInstance = Instantiate(leftFistPrefab, parentTransform);
                currentLeftHandWeaponInstance.transform.localPosition = Vector3.zero;
                currentLeftHandWeaponInstance.transform.localRotation = Quaternion.identity;
                isLeftHandEmpty = false;
                isLeftHandOneHanded = true;
                Debug.Log("Equipped left fist in left hand.");
            }
        }

        // Update state
        currentWeapon = crossbowPrefab;
        currentWeaponDamage = crossbowDamage;
        currentWeaponType = WeaponType.Crossbow;
        isTwoHandedEquipped = false;
        isWieldingOneHand = true;
    }

    public void EquipDagger()
    {
        if (daggerPrefab == null)
        {
            Debug.LogWarning("Cannot equip dagger: DaggerPrefab is null.");
            return;
        }

        // Unequip current weapons
        UnequipWeapon(true);
        UnequipWeapon(false);

        // Equip dagger in right hand (two-handed)
        Transform parentTransform = rightHandAttachment != null ? rightHandAttachment : rightHandHolder;
        if (parentTransform != null)
        {
            currentRightHandWeaponInstance = Instantiate(daggerPrefab, parentTransform);
            currentRightHandWeaponInstance.transform.localPosition = Vector3.zero;
            currentRightHandWeaponInstance.transform.localRotation = Quaternion.identity;
            isRightHandEmpty = false;
            Debug.Log("Equipped dagger in right hand.");
        }

        // For two-handed, left hand mirrors right hand
        parentTransform = leftHandAttachment != null ? leftHandAttachment : leftHandHolder;
        if (parentTransform != null)
        {
            currentLeftHandWeaponInstance = Instantiate(daggerPrefab, parentTransform);
            currentLeftHandWeaponInstance.transform.localPosition = Vector3.zero;
            currentLeftHandWeaponInstance.transform.localRotation = Quaternion.identity;
            isLeftHandEmpty = false;
            Debug.Log("Equipped dagger in left hand (two-handed).");
        }

        // Update state
        currentWeapon = daggerPrefab;
        currentWeaponDamage = daggerDamage;
        currentWeaponType = WeaponType.Dagger;
        isTwoHandedEquipped = true;
        isWieldingOneHand = false;
        isRightHandOneHanded = false;
        isLeftHandOneHanded = false;
    }

    public void UnequipWeapon(bool isRightHand)
    {
        if (isRightHand)
        {
            if (currentRightHandWeaponInstance != null)
            {
                Destroy(currentRightHandWeaponInstance);
                currentRightHandWeaponInstance = null;
            }
            isRightHandEmpty = true;
            isRightHandOneHanded = false;

            // If dagger (two-handed) is equipped, unequip left hand too
            if (isTwoHandedEquipped)
            {
                if (currentLeftHandWeaponInstance != null)
                {
                    Destroy(currentLeftHandWeaponInstance);
                    currentLeftHandWeaponInstance = null;
                }
                isLeftHandEmpty = true;
                isTwoHandedEquipped = false;
            }
        }
        else
        {
            if (currentLeftHandWeaponInstance != null)
            {
                Destroy(currentLeftHandWeaponInstance);
                currentLeftHandWeaponInstance = null;
            }
            isLeftHandEmpty = true;
            isLeftHandOneHanded = false;

            // If dagger (two-handed) is equipped, unequip right hand too
            if (isTwoHandedEquipped)
            {
                if (currentRightHandWeaponInstance != null)
                {
                    Destroy(currentRightHandWeaponInstance);
                    currentRightHandWeaponInstance = null;
                }
                isRightHandEmpty = true;
                isTwoHandedEquipped = false;
            }
        }

        UpdateWieldingState();
    }

    private void UpdateWieldingState()
    {
        isWieldingOneHand = (isRightHandOneHanded && isLeftHandEmpty) || (isLeftHandOneHanded && isRightHandEmpty);
    }

    public GameObject GetCurrentWeapon()
    {
        return currentWeapon;
    }

    public float GetCurrentWeaponDamage()
    {
        return currentWeaponDamage;
    }

    public bool CanUseTwoHandedSkill()
    {
        return isTwoHandedEquipped;
    }
}
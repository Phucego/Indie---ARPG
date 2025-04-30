using UnityEngine;

public class RangedAttackUpgrade : MonoBehaviour
{
    [Header("Upgrade Settings")]
    [SerializeField] private string upgradeID = "RangedAttackUpgrade1";
    [SerializeField] private float damageIncrease = 5f;
    [SerializeField] private float rangeIncrease = 5f;
    [SerializeField] private float projectileSpeedIncrease = 5f;
    [SerializeField] private float homingStrengthIncrease = 1f;
    [SerializeField] private float homingAngleIncrease = 15f;

    [Header("Dialogue Condition")]
    [SerializeField] private DialogueTrigger dialogueTrigger;

    [Header("References")]
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private Projectile projectilePrefab;

    [Header("Projectile Types")]
    [SerializeField] private GameObject fireProjectilePrefab;
    [SerializeField] private GameObject explosiveProjectilePrefab;

    [SerializeField] private bool useFireProjectile = false;
    [SerializeField] private bool useExplosiveProjectile = false;

    private bool hasUpgraded = false;

    private void Awake()
    {
        // Auto-assign references if not set
        if (dialogueTrigger == null)
        {
            dialogueTrigger = GetComponent<DialogueTrigger>();
            if (dialogueTrigger == null)
                Debug.LogError("DialogueTrigger not found on this GameObject.", this);
        }

        if (weaponManager == null)
        {
            weaponManager = FindObjectOfType<WeaponManager>();
            if (weaponManager == null)
                Debug.LogError("WeaponManager not found in the scene.", this);
        }

        if (playerAttack == null)
        {
            playerAttack = FindObjectOfType<PlayerAttack>();
            if (playerAttack == null)
                Debug.LogError("PlayerAttack not found in the scene.", this);
        }

        if (projectilePrefab == null)
        {
            projectilePrefab = Resources.Load<Projectile>("ArrowCrossbow");
            if (projectilePrefab == null)
                Debug.LogError("Projectile prefab not assigned or not found in Resources.");
        }

        hasUpgraded = PlayerPrefs.GetInt(upgradeID, 0) == 1;
    }

    private void OnEnable()
    {
        if (DialogueDisplay.Instance != null)
        {
            DialogueDisplay.Instance.OnDialogueEnded += HandleDialogueEnded;
        }
    }

    private void OnDisable()
    {
        if (DialogueDisplay.Instance != null)
        {
            DialogueDisplay.Instance.OnDialogueEnded -= HandleDialogueEnded;
        }
    }

    private void HandleDialogueEnded(Dialogue dialogue)
    {
        if (hasUpgraded || dialogueTrigger == null)
            return;

        Dialogue expectedDialogue = dialogueTrigger.GetCurrentDialogue(); // ✅ Fix: Get the currently valid dialogue
        if (dialogue != expectedDialogue)
            return;

        ApplyUpgrade();
    }

    public void EnableFireProjectile()
    {
        useFireProjectile = true;
        useExplosiveProjectile = false;
        Debug.Log("Fire projectile selected.");
    }

    public void EnableExplosiveProjectile()
    {
        useExplosiveProjectile = true;
        useFireProjectile = false;
        Debug.Log("Explosive projectile selected.");
    }

    private void ApplyUpgrade()
    {
        Debug.Log("Applying ranged attack upgrade...");

        // Upgrade weapon damage
        if (weaponManager != null)
        {
            weaponManager.crossbowDamage += damageIncrease;
            weaponManager.EquipCrossbow();

            if (useFireProjectile && fireProjectilePrefab != null)
            {
                weaponManager.SetProjectile(fireProjectilePrefab);
                Debug.Log("Projectile set to Fire type.");
            }
            else if (useExplosiveProjectile && explosiveProjectilePrefab != null)
            {
                weaponManager.SetProjectile(explosiveProjectilePrefab);
                Debug.Log("Projectile set to Explosive type.");
            }
        }

        // Upgrade player ranged range
        if (playerAttack != null)
        {
            playerAttack.rangedAttackRange += rangeIncrease;
        }

        // Upgrade projectile attributes
        if (projectilePrefab != null)
        {
            projectilePrefab.speed += projectileSpeedIncrease;
            projectilePrefab.homingStrength += homingStrengthIncrease;
            projectilePrefab.homingAngleLimit += homingAngleIncrease;
        }

        hasUpgraded = true;
        PlayerPrefs.SetInt(upgradeID, 1);
        PlayerPrefs.Save();

        Debug.Log("Ranged attack upgrade complete.");
    }
}

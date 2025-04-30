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
    [SerializeField] private DialogueTrigger dialogueTrigger;

    [Header("References")]
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private Projectile projectilePrefab;

    private bool hasUpgraded = false;
    
    [SerializeField] private GameObject fireProjectilePrefab;
    [SerializeField] private GameObject explosiveProjectilePrefab;
    [SerializeField] private bool useFireProjectile = false;
    [SerializeField] private bool useExplosiveProjectile = false;

    private void Awake()
    {
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
            projectilePrefab = Resources.Load<Projectile>("ArrowCrossbow"); // Adjust if needed
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
        if (hasUpgraded || dialogueTrigger == null || dialogue != dialogueTrigger.dialogue)
        {
            return;
        }

        ApplyUpgrade();
    }

    private void ApplyUpgrade()
    {
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


        hasUpgraded = true;
        PlayerPrefs.SetInt(upgradeID, 1);
        PlayerPrefs.Save();
    }
}

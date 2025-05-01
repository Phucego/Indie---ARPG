using UnityEngine;
using DG.Tweening;

public class ArrowAmmoManager : MonoBehaviour
{
    public static ArrowAmmoManager Instance;

    [Header("Ammo Settings")]
    [SerializeField] private int maxAmmo = 5;
    [SerializeField] private int currentAmmo = 5;
    [SerializeField] private GameObject arrowDropPrefab; // Prefab for the dropped arrow

    [Header("Drop Animation Settings")]
    [SerializeField] private float dropHeight = 1f; // Height from which the arrow drops
    [SerializeField] private float dropDuration = 0.5f; // Duration of the drop animation
    [SerializeField] private float fadeDuration = 2f; // Duration before the arrow fades out

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple ArrowAmmoManager instances detected. Destroying this one.", this);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool CanShoot()
    {
        return currentAmmo > 0;
    }

    public void ConsumeAmmo()
    {
        if (currentAmmo > 0)
        {
            currentAmmo--;
            Debug.Log($"Ammo consumed. Current ammo: {currentAmmo}", this);
            ArrowAmmoUI.Instance?.UpdateAmmoUI(currentAmmo, maxAmmo);
        }
    }

    public void AddAmmo(int amount)
    {
        int newAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
        if (newAmmo == currentAmmo)
        {
            Debug.Log("Ammo not added: Already at maximum capacity.", this);
        }
        else
        {
            currentAmmo = newAmmo;
            Debug.Log($"Ammo added. Current ammo: {currentAmmo}", this);
            ArrowAmmoUI.Instance?.UpdateAmmoUI(currentAmmo, maxAmmo);
        }
    }

    public void DropArrow(Vector3 position)
    {
        if (arrowDropPrefab == null)
        {
            Debug.LogWarning("Cannot drop arrow: ArrowDropPrefab is null.", this);
            return;
        }

        GameObject droppedArrow = Instantiate(arrowDropPrefab, position + Vector3.up * dropHeight, Quaternion.identity);
        droppedArrow.transform.DORotate(new Vector3(0, Random.Range(0, 360), 0), dropDuration, RotateMode.Fast)
            .SetEase(Ease.OutBounce);
        droppedArrow.transform.DOMoveY(position.y, dropDuration)
            .SetEase(Ease.OutBounce)
            .OnComplete(() =>
            {
                droppedArrow.transform.DOScale(Vector3.zero, fadeDuration)
                    .SetEase(Ease.InOutQuad)
                    .OnComplete(() => Destroy(droppedArrow));
            });

        Debug.Log($"Arrow dropped at position: {position}", this);
    }

    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public int GetMaxAmmo()
    {
        return maxAmmo;
    }
}
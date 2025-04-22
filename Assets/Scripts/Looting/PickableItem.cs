using UnityEngine;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(Outline))]
public class PickableItem : MonoBehaviour
{
    [SerializeField, Tooltip("Name of the item (e.g., 'Crossbow', 'Dagger')")]
    private string itemName;

    [SerializeField, Tooltip("Prefab of the item (must match WeaponManager's crossbowPrefab or daggerPrefab)")]
    private GameObject itemPrefab;

    [SerializeField, Tooltip("Canvas for displaying the item name")]
    private Canvas uiCanvas;

    [SerializeField, Tooltip("TextMeshProUGUI prefab for the item name")]
    private TextMeshProUGUI itemNameTextPrefab;

    [SerializeField, Tooltip("Offset above item for text display (world space)")]
    private Vector3 textOffset = new Vector3(0f, 0.5f, 0f);

    [SerializeField, Tooltip("Duration for text fade-in/out animation")]
    private float textFadeDuration = 0.3f;

    [SerializeField, Tooltip("Scale multiplier for text pop-in animation")]
    private float textScaleMultiplier = 1.2f;

    private Outline outline;
    private TextMeshProUGUI currentItemNameText;
    private Camera mainCamera;
    private bool isHovered;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline == null)
        {
            Debug.LogWarning($"PickableItem on {gameObject.name} is missing an Outline component.");
        }
        else
        {
            outline.enabled = false;
        }

        if (string.IsNullOrEmpty(itemName))
        {
            Debug.LogWarning($"PickableItem on {gameObject.name} has no itemName assigned.");
        }

        if (itemPrefab == null)
        {
            Debug.LogWarning($"PickableItem on {gameObject.name} has no itemPrefab assigned.");
        }

        if (uiCanvas == null)
        {
            Debug.LogWarning($"PickableItem on {gameObject.name} has no Canvas assigned.");
        }

        if (itemNameTextPrefab == null)
        {
            Debug.LogWarning($"PickableItem on {gameObject.name} has no TextMeshProUGUI prefab assigned.");
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning($"PickableItem on {gameObject.name} cannot find MainCamera.");
        }
    }

    private void Update()
    {
        // Skip if mouse is over UI, in dialogue, or attacking
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() ||
            (DialogueDisplay.Instance != null && DialogueDisplay.Instance.isDialogueActive) ||
            (PlayerAttack.Instance != null && PlayerAttack.Instance.isAttacking))
        {
            if (isHovered)
            {
                Highlight(false);
                HideItemName();
                isHovered = false;
            }
            return;
        }

        // Raycast from mouse position to detect this item
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        bool hitThisItem = Physics.Raycast(ray, out hit, 100f) && hit.collider.gameObject == gameObject;

        if (hitThisItem && !isHovered)
        {
            Highlight(true);
            ShowItemName();
            isHovered = true;
        }
        else if (!hitThisItem && isHovered)
        {
            Highlight(false);
            HideItemName();
            isHovered = false;
        }

        // Pick up item on left-click
        if (hitThisItem && Input.GetMouseButtonDown(0))
        {
            Pickup();
        }
    }

    public void Highlight(bool enable)
    {
        if (outline != null)
        {
            outline.enabled = enable;
        }
    }

    public void ShowItemName()
    {
        if (string.IsNullOrEmpty(itemName) || itemNameTextPrefab == null || uiCanvas == null || mainCamera == null)
        {
            return;
        }

        if (currentItemNameText == null)
        {
            currentItemNameText = Instantiate(itemNameTextPrefab, uiCanvas.transform);
            currentItemNameText.text = itemName;

            var canvasGroup = currentItemNameText.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = currentItemNameText.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
            currentItemNameText.transform.localScale = Vector3.one * 0.8f;

            Sequence textSequence = DOTween.Sequence();
            textSequence.Append(canvasGroup.DOFade(1f, textFadeDuration));
            textSequence.Join(currentItemNameText.transform.DOScale(Vector3.one * textScaleMultiplier, textFadeDuration * 0.5f));
            textSequence.Append(currentItemNameText.transform.DOScale(Vector3.one, textFadeDuration * 0.5f));
            textSequence.Play();
        }

        Vector3 worldPos = transform.position + textOffset;
        Vector2 screenPos = mainCamera.WorldToScreenPoint(worldPos);
        currentItemNameText.transform.position = screenPos;
    }

    public void HideItemName()
    {
        if (currentItemNameText == null)
        {
            return;
        }

        var canvasGroup = currentItemNameText.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            Sequence textSequence = DOTween.Sequence();
            textSequence.Append(canvasGroup.DOFade(0f, textFadeDuration));
            textSequence.Join(currentItemNameText.transform.DOScale(Vector3.one * 0.8f, textFadeDuration));
            textSequence.OnComplete(() =>
            {
                if (currentItemNameText != null)
                {
                    Destroy(currentItemNameText.gameObject);
                    currentItemNameText = null;
                }
            });
            textSequence.Play();
        }
    }

    public void Pickup()
    {
        if (itemPrefab == null || WeaponManager.Instance == null)
        {
            Debug.LogWarning($"Cannot pick up {itemName}: itemPrefab or WeaponManager is null.");
            return;
        }

        // Equip the corresponding weapon based on the itemPrefab
        if (itemPrefab == WeaponManager.Instance.crossbowPrefab)
        {
            WeaponManager.Instance.EquipCrossbow();
            Debug.Log($"Picked up and equipped {itemName} (Crossbow).");
        }
        else if (itemPrefab == WeaponManager.Instance.daggerPrefab)
        {
            WeaponManager.Instance.EquipDagger();
            Debug.Log($"Picked up and equipped {itemName} (Dagger).");
        }
        else
        {
            Debug.LogWarning($"Cannot equip {itemName}: itemPrefab does not match crossbow or dagger.");
            return;
        }

        HideItemName();
        Destroy(gameObject);
    }

    public bool isDialogueActive
    {
        get
        {
            if (DialogueDisplay.Instance != null)
            {
                return DialogueDisplay.Instance.isDialogueActive;
            }
            return false;
        }
    }
}
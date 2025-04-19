using UnityEngine;
using FarrokhGames.Inventory.Examples;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(Outline))]
public class PickableItem : MonoBehaviour
{
    [SerializeField, Tooltip("The ItemDefinition for this pickable object")]
    private ItemDefinition itemDefinition;

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

    public ItemDefinition ItemDefinition => itemDefinition;

    private Outline outline;
    private TextMeshProUGUI currentItemNameText;
    private Camera mainCamera;

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

        if (itemDefinition == null)
        {
            Debug.LogWarning($"PickableItem on {gameObject.name} has no ItemDefinition assigned.");
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

    public void Highlight(bool enable)
    {
        if (outline != null)
        {
            outline.enabled = enable;
        }
    }

    public void ShowItemName()
    {
        if (itemDefinition == null || itemNameTextPrefab == null || uiCanvas == null || mainCamera == null)
        {
            return;
        }

        if (currentItemNameText == null)
        {
            currentItemNameText = Instantiate(itemNameTextPrefab, uiCanvas.transform);
            currentItemNameText.text = itemDefinition.Name;

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
        HideItemName();
        Destroy(gameObject);
    }
}
using UnityEngine;
using DG.Tweening;

public class InventorySlideUI : MonoBehaviour
{
    [SerializeField] private RectTransform inventoryPanel;
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float hiddenX = 1000f; 
    [SerializeField] private float visibleX = 0f;

    private bool isVisible = false;

    private void Start()
    {
        // Start hidden
        inventoryPanel.anchoredPosition = new Vector2(hiddenX, inventoryPanel.anchoredPosition.y);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        isVisible = !isVisible;

        float targetX = isVisible ? visibleX : hiddenX;

        inventoryPanel.DOAnchorPosX(targetX, slideDuration).SetEase(Ease.OutCubic);
    }
}
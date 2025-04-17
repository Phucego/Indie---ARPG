using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class PickupUI : MonoBehaviour
{
    private Text nameText;
    private Transform targetObject;
    private float heightOffset;
    private RectTransform rectTransform;

    public void Initialize(string itemName, Transform target, float height)
    {
        rectTransform = GetComponent<RectTransform>();
        nameText = gameObject.AddComponent<Text>();
        nameText.text = itemName;
        nameText.fontSize = 20;
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.color = Color.white;
        nameText.raycastTarget = false;

        targetObject = target;
        heightOffset = height;

        rectTransform.sizeDelta = new Vector2(200, 50);
        UpdatePosition();

        // Pop-up animation
        rectTransform.anchoredPosition += Vector2.down * 20;
        rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + 20, 0.5f).SetEase(Ease.OutBack);
        nameText.DOFade(0, 0).SetDelay(0.5f).OnComplete(() => nameText.DOFade(1, 0.5f));
    }

    private void Update()
    {
        if (targetObject)
        {
            UpdatePosition();
        }
    }

    private void UpdatePosition()
    {
        Vector3 worldPos = targetObject.position + Vector3.up * heightOffset;
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform,
            screenPos,
            null,
            out Vector2 localPoint
        );
        rectTransform.anchoredPosition = localPoint;
    }

    public void FadeOut(Action onComplete)
    {
        nameText.DOFade(0, 0.3f).OnComplete(() => onComplete?.Invoke());
    }
}
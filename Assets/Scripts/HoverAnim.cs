using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverAnim : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    [SerializeField] private float scaleChange = 1.1f;
    [SerializeField] private Color hoverColor = new Color(1f, 0.8f, 0.4f); // Orange tint for hover
    private Vector3 originalScale;
    private Color originalColor;
    private bool isKeyboardSelected = false;
    private Button button;

    private void Start()
    {
        originalScale = transform.localScale;
        button = GetComponent<Button>();

        if (button != null)
        {
            originalColor = button.colors.normalColor;
        }
    }

    public void ApplyHoverEffect()
    {
        if (!isKeyboardSelected) // Prevents conflict with keyboard selection
        {
            transform.localScale = originalScale * scaleChange;
            ChangeButtonColor(hoverColor);
            AudioManager.Instance.PlaySoundEffect("Hover_SFX");
        }
    }

    public void RemoveHoverEffect()
    {
        if (!isKeyboardSelected) // Only reset if not selected via keyboard
        {
            transform.localScale = originalScale;
            ChangeButtonColor(originalColor);
        }
    }

    public void ApplyKeyboardSelection()
    {
        isKeyboardSelected = true;
        transform.localScale = originalScale * scaleChange;
        ChangeButtonColor(hoverColor);
    }

    public void RemoveKeyboardSelection()
    {
        isKeyboardSelected = false;
        transform.localScale = originalScale;
        ChangeButtonColor(originalColor);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ApplyHoverEffect();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RemoveHoverEffect();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        RemoveHoverEffect();
    }
//NORMAL = GRAY, HIGHLIGHTED = WHITE
    private void ChangeButtonColor(Color newColor)
    {
        if (button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = newColor;
            button.colors = colors;
        }
    }
}

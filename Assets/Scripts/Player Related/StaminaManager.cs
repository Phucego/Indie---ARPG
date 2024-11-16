using UnityEngine;
using UnityEngine.UI;

public class StaminaManager : MonoBehaviour
{
    public float maxStamina = 100f;  
    public float currentStamina;     
    public float staminaRegenRate = 5f;
    public float regenCooldown = 2f;
   [SerializeField]
    private float baseStaminaBarWidth;
    private float lastStaminaUseTime;  
    
    
    public Slider staminaSlider;
    public Slider easeHealthSlider;
    
    [SerializeField] private float lerpSpeed;
    void Start()
    {
        currentStamina = maxStamina;  // Initialize stamina to max
        UpdateStaminaBarSize();
    }

    void Update()
    {
        //TODO: Setting the max value of the slider equal to the max stamina
        staminaSlider.maxValue = maxStamina;
        easeHealthSlider.maxValue = maxStamina;
        if (Time.time >= lastStaminaUseTime + regenCooldown)
        {
            RegenerateStamina();
        }
        if (staminaSlider.value != currentStamina)
        {
            staminaSlider.value = currentStamina;
        }

        if (staminaSlider.value != easeHealthSlider.value)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, currentStamina, lerpSpeed);
        }
    }

    public bool HasEnoughStamina(float amount)
    {
        return currentStamina >= amount;
    }

    public void UseStamina(float amount)
    {
       
        currentStamina = Mathf.Max(currentStamina - amount, 0);
        lastStaminaUseTime = Time.time;
    }

    public void RegenerateStamina()
    {
        currentStamina = Mathf.Min(currentStamina + staminaRegenRate * Time.deltaTime, maxStamina);
    }
    private void UpdateStaminaBarSize()
    {
        float staminaBarWidth = baseStaminaBarWidth * maxStamina; // Scale width based on maxHealth

        RectTransform staminaRectTransform = staminaSlider.GetComponent<RectTransform>();
        RectTransform easeHealthRectTransform = easeHealthSlider.GetComponent<RectTransform>();

        // Set anchor points and pivot to (0, 0.5) to expand from the left
        staminaRectTransform.anchorMin = new Vector2(0, 0.5f);
        staminaRectTransform.anchorMax = new Vector2(0, 0.5f);
        staminaRectTransform.pivot = new Vector2(0, 0.5f);

        easeHealthRectTransform.anchorMin = new Vector2(0, 0.5f);
        easeHealthRectTransform.anchorMax = new Vector2(0, 0.5f);
        easeHealthRectTransform.pivot = new Vector2(0, 0.5f);

        // Update size to expand from the left side
        staminaRectTransform.sizeDelta = new Vector2(staminaBarWidth, staminaRectTransform.sizeDelta.y);
        easeHealthRectTransform.sizeDelta = new Vector2(staminaBarWidth, easeHealthRectTransform.sizeDelta.y);

    }
    
}
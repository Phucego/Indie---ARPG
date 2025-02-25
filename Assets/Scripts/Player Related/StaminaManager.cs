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
   
    [SerializeField] private float lerpSpeed;
    void Start()
    {
        currentStamina = maxStamina;  // Initialize stamina to max
        //UpdateStaminaBarSize();
    }

    void Update()
    {
        //TODO: Setting the max value of the slider equal to the max stamina
        staminaSlider.maxValue = maxStamina;
      
        if (Time.time >= lastStaminaUseTime + regenCooldown)
        {
            RegenerateStamina();
        }
        if (staminaSlider.value != currentStamina)
        {
            staminaSlider.value = currentStamina;
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
    /*private void UpdateStaminaBarSize()
    {
        float staminaBarWidth = baseStaminaBarWidth * maxStamina; // Scale width based on maxHealth

        RectTransform staminaRectTransform = staminaSlider.GetComponent<RectTransform>();
        // Set anchor points and pivot to (0, 0.5) to expand from the left
        staminaRectTransform.anchorMin = new Vector2(0, 0.5f);
        staminaRectTransform.anchorMax = new Vector2(0, 0.5f);
        staminaRectTransform.pivot = new Vector2(0, 0.5f);

    }*/
    
}
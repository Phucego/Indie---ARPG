using UnityEngine;
using UnityEngine.UI;

public class StaminaManager : MonoBehaviour
{
    public float maxStamina = 100f;  
    public float currentStamina;     
    public float staminaRegenRate = 5f;  
    public float regenCooldown = 2f;  

    private float lastStaminaUseTime;  
    public Slider staminaSlider;
    public Slider easeHealthSlider;
    
    [SerializeField] private float lerpSpeed;
    void Start()
    {
        currentStamina = maxStamina;  // Initialize stamina to max
    }

    void Update()
    {
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

    private void RegenerateStamina()
    {
        currentStamina = Mathf.Min(currentStamina + staminaRegenRate * Time.deltaTime, maxStamina);
    }
    
}
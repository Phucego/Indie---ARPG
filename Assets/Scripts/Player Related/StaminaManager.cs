using UnityEngine;

public class StaminaManager : MonoBehaviour
{
    public float maxStamina = 100f;  
    public float currentStamina;     
    public float staminaRegenRate = 5f;  
    public float regenCooldown = 2f;  

    private float lastStaminaUseTime;  
    
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
using UnityEngine;
using UnityEngine.UI;

public class PlayerLevel : MonoBehaviour
{
    [Header("Level Info")]
    public int currentLevel = 1;
    public float currentExp = 0f;
    public float expToLevelUp = 100f;
    public float expMultiplier = 1.1f;
    public PlayerStats playerStats;

    [Header("EXP UI")]
    [SerializeField] private Slider expSlider;
    [SerializeField] private float lerpSpeed = 5f;
    private float targetSliderValue;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();

        if (expSlider != null)
        {
            expSlider.minValue = 0;
            expSlider.maxValue = expToLevelUp;
            expSlider.value = 0;
        }
    }

    void Update()
    {
        GainExperience(Time.deltaTime * 5); // For testing

        if (expSlider != null)
        {
            targetSliderValue = Mathf.Clamp(currentExp, 0, expToLevelUp);
            expSlider.value = Mathf.Lerp(expSlider.value, targetSliderValue, Time.deltaTime * lerpSpeed);
        }
    }

    public void GainExperience(float amount)
    {
        if (currentLevel >= 100) return;

        currentExp += amount;

        while (currentExp >= expToLevelUp)
        {
            currentExp -= expToLevelUp;
            LevelUp();
        }
    }

    void LevelUp()
    {
        currentLevel++;

        playerStats.attackPower += 10f;
        playerStats.maxHP += 20f;
        playerStats.currentHP = playerStats.maxHP;

        expToLevelUp *= expMultiplier;

        if (expSlider != null)
        {
            expSlider.maxValue = expToLevelUp;
        }

        Debug.Log("Level Up! Now level " + currentLevel);
    }
}
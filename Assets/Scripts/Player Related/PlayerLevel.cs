using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

    [Header("Level UI")]
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Level Up VFX")]
    [SerializeField] private ParticleSystem levelUpVFX;

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();

        if (expSlider != null)
        {
            expSlider.minValue = 0;
            expSlider.maxValue = expToLevelUp;
            expSlider.value = 0;
        }

        UpdateLevelText();
    }

    public void GainExperience(float amount)
    {
        if (currentLevel >= 100) return;

        currentExp += amount;
        UpdateExpUI();

        while (currentExp >= expToLevelUp)
        {
            currentExp -= expToLevelUp;
            LevelUp();
        }
    }

    private void UpdateExpUI()
    {
        if (expSlider != null)
        {
            expSlider.DOValue(currentExp, 0.5f).SetEase(Ease.OutCubic);
        }
    }

    private void LevelUp()
    {
        currentLevel++;

        // Update player stats
        playerStats.attackPower += 10f;
        playerStats.maxHP += 20f;
        playerStats.currentHP = playerStats.maxHP;

        // Scale required exp
        expToLevelUp *= expMultiplier;

        // Update slider max and current value
        if (expSlider != null)
        {
            expSlider.maxValue = expToLevelUp;
            expSlider.value = currentExp;
        }

        UpdateLevelText();
        PlayLevelUpVFX();
    }

    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = currentLevel.ToString();
            levelText.transform
                .DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 1)
                .SetEase(Ease.OutBack);
        }
    }

    private void PlayLevelUpVFX()
    {
        if (levelUpVFX != null)
        {
            // Instantiate VFX as child of the player
            ParticleSystem vfxInstance = Instantiate(levelUpVFX, transform.position, Quaternion.identity, transform);
            vfxInstance.transform.localPosition = Vector3.zero; // Align to player center
            vfxInstance.Play();

            // Destroy after VFX finishes
            float totalDuration = vfxInstance.main.duration + vfxInstance.main.startLifetime.constantMax;
            Destroy(vfxInstance.gameObject, totalDuration);
        }
    }
}

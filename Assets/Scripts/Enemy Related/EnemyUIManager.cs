using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject enemyHealthUI; // Panel for enemy health UI
    [SerializeField] private TextMeshProUGUI enemyNameText;
    [SerializeField] private Slider enemyHealthSlider;

    private void Start()
    {
        HideEnemyHealthBar();
    }

    public void ShowEnemyHealthBar(string enemyName, float currentHealth, float maxHealth)
    {
        enemyHealthUI.SetActive(true);
        enemyNameText.text = enemyName;
        enemyHealthSlider.maxValue = maxHealth;
        enemyHealthSlider.value = currentHealth;
    }

    public void HideEnemyHealthBar()
    {
        enemyHealthUI.SetActive(false);
    }
}
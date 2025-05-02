using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Pause Menu UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitToMainMenuButton;
    [SerializeField] private Button quitGameButton;
    [SerializeField] private SceneField mainMenuSceneName;

    [Header("Lose Screen UI")]
    [SerializeField] private GameObject loseScreenPanel;
    [SerializeField] private Button reloadLastRoomButton;
    [SerializeField] private Button loseQuitToMainMenuButton;

    [Header("Win Screen UI")]
    [SerializeField] private GameObject winScreenPanel;
    [SerializeField] private TextMeshProUGUI winScreenText;
    [SerializeField] private Button winQuitToMainMenuButton;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float scaleDuration = 0.3f;
    [SerializeField] private float popScale = 1.1f;
    [SerializeField] private Ease fadeEase = Ease.InOutQuad;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    private bool isPaused = false;
    private CanvasGroup pauseMenuCanvasGroup;
    private CanvasGroup loseScreenCanvasGroup;
    private CanvasGroup winScreenCanvasGroup;
    private Vector3 originalScale;
    private string lastRoomSceneName;

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        // Initialize CanvasGroup for pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuCanvasGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
            if (pauseMenuCanvasGroup == null)
            {
                pauseMenuCanvasGroup = pauseMenuPanel.AddComponent<CanvasGroup>();
            }
            pauseMenuCanvasGroup.alpha = 0f;
            pauseMenuPanel.SetActive(false);
            originalScale = pauseMenuPanel.transform.localScale;
        }

        // Initialize CanvasGroup for lose screen
        if (loseScreenPanel != null)
        {
            loseScreenCanvasGroup = loseScreenPanel.GetComponent<CanvasGroup>();
            if (loseScreenCanvasGroup == null)
            {
                loseScreenCanvasGroup = loseScreenPanel.AddComponent<CanvasGroup>();
            }
            loseScreenCanvasGroup.alpha = 0f;
            loseScreenPanel.SetActive(false);
        }

        // Initialize CanvasGroup for win screen
        if (winScreenPanel != null)
        {
            winScreenCanvasGroup = winScreenPanel.GetComponent<CanvasGroup>();
            if (winScreenCanvasGroup == null)
            {
                winScreenCanvasGroup = winScreenPanel.AddComponent<CanvasGroup>();
            }
            winScreenCanvasGroup.alpha = 0f;
            winScreenPanel.active = false;
        }
    }

    private void Start()
    {
        // Assign button listeners
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        if (quitToMainMenuButton != null)
            quitToMainMenuButton.onClick.AddListener(QuitToMainMenu);
        if (quitGameButton != null)
            quitGameButton.onClick.AddListener(QuitGame);
        if (reloadLastRoomButton != null)
            reloadLastRoomButton.onClick.AddListener(ReloadLastRoom);
        if (loseQuitToMainMenuButton != null)
            loseQuitToMainMenuButton.onClick.AddListener(QuitToMainMenu);
        if (winQuitToMainMenuButton != null)
            winQuitToMainMenuButton.onClick.AddListener(QuitToMainMenu);

        // Set win screen text
        if (winScreenText != null)
        {
            winScreenText.text = "Lord Dalk is dead, Peace has been restored.";
        }

        // Store the initial room
        lastRoomSceneName = SceneManager.GetActiveScene().name;

        // Subscribe to PlayerHealth event
        if (PlayerHealth.instance != null)
        {
            PlayerHealth.instance.OnPlayerDeath += ShowLoseScreen;
        }

        // Subscribe to boss death event
        EnemyHealth[] enemies = FindObjectsOfType<EnemyHealth>();
        foreach (var enemy in enemies)
        {
            if (enemy.isBoss)
            {
                enemy.OnBossDeath.AddListener(ShowWinScreen);
            }
        }
    }

    private void Update()
    {
        // Toggle pause menu with Escape key, but only if lose/win screens are not active
        if (Input.GetKeyDown(KeyCode.Escape) && !loseScreenPanel.activeSelf && !winScreenPanel.activeSelf)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void UpdateLastRoom(string sceneName)
    {
        lastRoomSceneName = sceneName;
        PlayerPrefs.SetString("LastRoom", sceneName);
        PlayerPrefs.Save();
        Debug.Log($"[UIManager] Updated last room to: {sceneName}");
    }

    private void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            pauseMenuCanvasGroup.alpha = 0f;
            pauseMenuPanel.transform.localScale = originalScale * 0.8f;

            Sequence showSequence = DOTween.Sequence();
            showSequence.Append(pauseMenuCanvasGroup.DOFade(1f, fadeDuration).SetEase(fadeEase))
                        .Join(pauseMenuPanel.transform.DOScale(originalScale * popScale, scaleDuration).SetEase(scaleEase))
                        .Append(pauseMenuPanel.transform.DOScale(originalScale, scaleDuration * 0.2f).SetEase(scaleEase))
                        .SetUpdate(true);
        }
    }

    private void ResumeGame()
    {
        isPaused = false;

        if (pauseMenuPanel != null)
        {
            Sequence hideSequence = DOTween.Sequence();
            hideSequence.Append(pauseMenuCanvasGroup.DOFade(0f, fadeDuration).SetEase(fadeEase))
                        .Join(pauseMenuPanel.transform.DOScale(originalScale * 0.8f, scaleDuration).SetEase(scaleEase))
                        .OnComplete(() =>
                        {
                            pauseMenuPanel.SetActive(false);
                            Time.timeScale = 1f;
                        })
                        .SetUpdate(true);
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public void ShowLoseScreen()
    {
        // Ensure other screens are hidden
        if (pauseMenuPanel.activeSelf)
            ResumeGame();
        if (winScreenPanel.activeSelf)
            winScreenPanel.SetActive(false);

        isPaused = true;
        Time.timeScale = 0f;

        if (loseScreenPanel != null)
        {
            loseScreenPanel.SetActive(true);
            loseScreenCanvasGroup.alpha = 0f;
            loseScreenPanel.transform.localScale = originalScale * 0.8f;

            Sequence showSequence = DOTween.Sequence();
            showSequence.Append(loseScreenCanvasGroup.DOFade(1f, fadeDuration).SetEase(fadeEase))
                .Join(loseScreenPanel.transform.DOScale(originalScale * popScale, scaleDuration).SetEase(scaleEase))
                .Append(loseScreenPanel.transform.DOScale(originalScale, scaleDuration * 0.2f).SetEase(scaleEase))
                .SetUpdate(true);
        }
    }


    public void ShowWinScreen()
    {
        // Ensure other screens are hidden
        if (pauseMenuPanel.activeSelf)
            ResumeGame();
        if (loseScreenPanel.activeSelf)
            loseScreenPanel.SetActive(false);

        isPaused = true;
        Time.timeScale = 0f;

        // Only show win screen when the boss dies
        if (winScreenPanel != null && PlayerHealth.instance.currentHealth > 0) // Added check for player health
        {
            winScreenPanel.SetActive(true);
            winScreenCanvasGroup.alpha = 0f;
            winScreenPanel.transform.localScale = originalScale * 0.8f;

            Sequence showSequence = DOTween.Sequence();
            showSequence.Append(winScreenCanvasGroup.DOFade(1f, fadeDuration).SetEase(fadeEase))
                .Join(winScreenPanel.transform.DOScale(originalScale * popScale, scaleDuration).SetEase(scaleEase))
                .Append(winScreenPanel.transform.DOScale(originalScale, scaleDuration * 0.2f).SetEase(scaleEase))
                .SetUpdate(true);
        }
    }


    private void ReloadLastRoom()
    {
        Time.timeScale = 1f;
        string sceneToLoad = PlayerPrefs.GetString("LastRoom", lastRoomSceneName);
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
            if (PlayerHealth.instance != null)
            {
                PlayerHealth.instance.currentHealth = PlayerHealth.instance.maxHealth;
                PlayerHealth.instance.UpdateHealthBar();
            }
        }
        else
        {
            Debug.LogWarning("[UIManager] No last room stored! Loading main menu.");
            QuitToMainMenu();
        }
    }

    private void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(ResumeGame);
        if (quitToMainMenuButton != null)
            quitToMainMenuButton.onClick.RemoveListener(QuitToMainMenu);
        if (quitGameButton != null)
            quitGameButton.onClick.RemoveListener(QuitGame);
        if (reloadLastRoomButton != null)
            reloadLastRoomButton.onClick.RemoveListener(ReloadLastRoom);
        if (loseQuitToMainMenuButton != null)
            loseQuitToMainMenuButton.onClick.RemoveListener(QuitToMainMenu);
        if (winQuitToMainMenuButton != null)
            winQuitToMainMenuButton.onClick.RemoveListener(QuitToMainMenu);

        // Clean up event subscriptions
        if (PlayerHealth.instance != null)
        {
            PlayerHealth.instance.OnPlayerDeath -= ShowLoseScreen;
        }

        // Clean up boss death subscriptions
        EnemyHealth[] enemies = FindObjectsOfType<EnemyHealth>();
        foreach (var enemy in enemies)
        {
            if (enemy.isBoss)
            {
                enemy.OnBossDeath.RemoveListener(ShowWinScreen);
            }
        }
    }
}
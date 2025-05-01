using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Pause Menu UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitToMainMenuButton;
    [SerializeField] private Button quitGameButton;
    [SerializeField] private SceneField mainMenuSceneName;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float scaleDuration = 0.3f;
    [SerializeField] private float popScale = 1.1f;
    [SerializeField] private Ease fadeEase = Ease.InOutQuad;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    private bool isPaused = false;
    private CanvasGroup pauseMenuCanvasGroup;
    private Vector3 originalScale;

    private void Awake()
    {
        // Initialize CanvasGroup for fading
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
       
    }

    private void Update()
    {
        // Toggle pause menu with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    private void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Pause game time

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            pauseMenuCanvasGroup.alpha = 0f;
            pauseMenuPanel.transform.localScale = originalScale * 0.8f;

            // Animate fade in and scale up
            Sequence showSequence = DOTween.Sequence();
            showSequence.Append(pauseMenuCanvasGroup.DOFade(1f, fadeDuration).SetEase(fadeEase))
                        .Join(pauseMenuPanel.transform.DOScale(originalScale * popScale, scaleDuration).SetEase(scaleEase))
                        .Append(pauseMenuPanel.transform.DOScale(originalScale, scaleDuration * 0.2f).SetEase(scaleEase))
                        .SetUpdate(true); // Ignore time scale
        }
    }

    private void ResumeGame()
    {
        isPaused = false;

        if (pauseMenuPanel != null)
        {
            // Animate fade out and scale down
            Sequence hideSequence = DOTween.Sequence();
            hideSequence.Append(pauseMenuCanvasGroup.DOFade(0f, fadeDuration).SetEase(fadeEase))
                        .Join(pauseMenuPanel.transform.DOScale(originalScale * 0.8f, scaleDuration).SetEase(scaleEase))
                        .OnComplete(() =>
                        {
                            pauseMenuPanel.SetActive(false);
                            Time.timeScale = 1f; // Resume game time
                        })
                        .SetUpdate(true); // Ignore time scale
        }
        else
        {
            Time.timeScale = 1f; // Fallback to resume time
        }
    }

    private void QuitToMainMenu()
    {
        Time.timeScale = 1f; // Ensure time is normal for main menu
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
    }
}
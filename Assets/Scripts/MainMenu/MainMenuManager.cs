using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu")]
    Animator anim;

    public GameObject levelSelectionCanvas;
    public GameObject mainMenuCanvas;
    public GameObject confirmMenuCanvas;
    public GameObject settingsCanvas;

    public TextMeshProUGUI gameName;

    public Button startButton;
    public Button settingsButton;
    public Button quitButton;
    public Button newGameButton;
    public Button loadGameButton;
    public Button yesConfirmation;
    public Button noConfirmation;
    public Button backButton;
    public Button tapToStartButton;
    public Button settingsBackButton;

    // MAIN MENU
    public UnityEvent onStartButtonPressed;
    public UnityEvent onSettingsMenu;
    public UnityEvent onConfirmationMenu;

    // GAME MODE SELECTION
    public UnityEvent onNewGame;
    public UnityEvent onLoadGame;
    public UnityEvent onBackToMainMenu;

    // CONFIRMATION MENU
    public UnityEvent onConfirmation_Yes;
    public UnityEvent onConfirmation_No;

    // SETTINGS MENU
    public UnityEvent onSettingsBack;

    [Header("Navigation Indicator")]
    public RectTransform indicator;
    public float indicatorOffset = -3.4f;

    public List<Button> menuButtons = new List<Button>();
    private int selectedIndex = 0;

    [Header("Scenes To Load")]
    [SerializeField] private SceneField Level1;
    [SerializeField] private SceneField _mainMenuScene;

    [Header("Animations")]
    public List<Animator> externalAnimators;

    [Header("Settings")]
    public Slider volumeSlider;
    public Slider brightnessSlider;

    private bool startButtonPressed = false;
    private bool isReturningToMainMenu = false;
    private List<AsyncOperation> _scenesToLoad = new List<AsyncOperation>();

    private void Awake()
    {
        Time.timeScale = 1f;
        anim = GetComponent<Animator>();

        // Initialize canvases
        mainMenuCanvas.SetActive(true);
        confirmMenuCanvas.SetActive(false);
        levelSelectionCanvas.SetActive(false);
        if (settingsCanvas != null)
            settingsCanvas.SetActive(false);

        // MAIN MENU
        menuButtons.Add(startButton);
        menuButtons.Add(settingsButton);
        menuButtons.Add(quitButton);

        AssignButtonListeners();
        UpdateHoverListeners();

        MoveIndicator(menuButtons[selectedIndex]);

        // Initialize settings
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            volumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }
        if (brightnessSlider != null)
        {
            brightnessSlider.value = PlayerPrefs.GetFloat("Brightness", 1f);
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
        }
    }

    private void Update()
    {
        HandleMenuNavigation();

        if (indicator != null && menuButtons.Count > 0)
        {
            Vector3 targetPosition = new Vector3(indicator.position.x, menuButtons[selectedIndex].transform.position.y - indicatorOffset, indicator.position.z);
            indicator.position = Vector3.Lerp(indicator.position, targetPosition, Time.deltaTime * 10f);
        }
    }

    private void HandleMenuNavigation()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex = (selectedIndex - 1 + menuButtons.Count) % menuButtons.Count;
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex = (selectedIndex + 1) % menuButtons.Count;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            menuButtons[selectedIndex].onClick.Invoke();
        }
    }

    private void AssignButtonListeners()
    {
        // MAIN MENU
        startButton.onClick.AddListener(() => { MoveIndicator(startButton); OnStartButtonPressed(); });
        settingsButton.onClick.AddListener(() => { MoveIndicator(settingsButton); OnSettingsMenuPressed(); });
        quitButton.onClick.AddListener(() => { MoveIndicator(quitButton); OnConfirmationMenu(); });

        // GAME MODE SELECTION
        newGameButton.onClick.AddListener(() => { MoveIndicator(newGameButton); OnNewGamePressed(); });
        loadGameButton.onClick.AddListener(() => { MoveIndicator(loadGameButton); OnLoadGamePressed(); });
        backButton.onClick.AddListener(() => { MoveIndicator(backButton); OnBackToMainMenu(); });

        // CONFIRMATION
        yesConfirmation.onClick.AddListener(() => { MoveIndicator(yesConfirmation); OnConfirmationYes(); });
        noConfirmation.onClick.AddListener(() => { MoveIndicator(noConfirmation); OnConfirmationNo(); });

        // SETTINGS
        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(() => { MoveIndicator(settingsBackButton); OnSettingsBackPressed(); });

        // TAP TO START
        tapToStartButton.onClick.RemoveAllListeners();
        tapToStartButton.onClick.AddListener(() => { OnBackToMainMenu(); });
    }

    private void UpdateHoverListeners()
    {
        foreach (var button in menuButtons)
        {
            if (button == null) continue;

            EventTrigger trigger = button.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entry.callback.AddListener((data) => { OnButtonHover(button); });

            trigger.triggers.Add(entry);
        }

        // Add hover listeners for sub-menu buttons
        AddHoverListener(newGameButton);
        AddHoverListener(loadGameButton);
        AddHoverListener(backButton);
        AddHoverListener(yesConfirmation);
        AddHoverListener(noConfirmation);
        AddHoverListener(settingsBackButton);
        AddHoverListener(tapToStartButton);
    }

    private void AddHoverListener(Button button)
    {
        if (button == null) return;

        EventTrigger trigger = button.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entry.callback.AddListener((data) => { OnButtonHover(button); });

        trigger.triggers.Add(entry);
    }

    private void OnButtonHover(Button hoveredButton)
    {
        int index = menuButtons.IndexOf(hoveredButton);
        if (index >= 0)
            selectedIndex = index;
    }

    public void OnStartButtonPressed()
    {
        if (startButtonPressed) return;

        startButtonPressed = true;
        onStartButtonPressed?.Invoke();

        menuButtons.Clear();
        anim.SetTrigger("isStart");

        mainMenuCanvas.SetActive(false);
        levelSelectionCanvas.SetActive(true);

        // GAME MODE SELECTION
        menuButtons.Add(newGameButton);
        menuButtons.Add(loadGameButton);
        menuButtons.Add(backButton);

        selectedIndex = 0;
        startButtonPressed = false;
        MoveIndicator(menuButtons[selectedIndex]);
        UpdateHoverListeners();

        TriggerExternalAnimators("isStart");
    }

    public void OnSettingsMenuPressed()
    {
        onSettingsMenu?.Invoke();

        mainMenuCanvas.SetActive(false);
        settingsCanvas.SetActive(true);

        menuButtons.Clear();
        if (settingsBackButton != null)
            menuButtons.Add(settingsBackButton);

        selectedIndex = 0;
        MoveIndicator(menuButtons[selectedIndex]);
        UpdateHoverListeners();

        TriggerExternalAnimators("isSettings");
    }

    public void OnConfirmationMenu()
    {
        onConfirmationMenu?.Invoke();
        mainMenuCanvas.SetActive(false);
        confirmMenuCanvas.SetActive(true);

        menuButtons.Clear();
        menuButtons.Add(yesConfirmation);
        menuButtons.Add(noConfirmation);

        selectedIndex = 0;
        MoveIndicator(menuButtons[selectedIndex]);
        UpdateHoverListeners();

        TriggerExternalAnimators("isConfirm");
    }

    public void OnNewGamePressed()
    {
        onNewGame?.Invoke();
        StartCoroutine(LoadScene(Level1));
    }

    public void OnLoadGamePressed()
    {
        onLoadGame?.Invoke();
        Debug.Log("Load Game pressed. Implement save/load system to load saved game state.");
        StartCoroutine(LoadScene(Level1));
    }

    public void OnBackToMainMenu()
    {
        if (isReturningToMainMenu) return;

        isReturningToMainMenu = true;
        onBackToMainMenu?.Invoke();

        anim.SetTrigger("isBack");

        levelSelectionCanvas.SetActive(false);
        confirmMenuCanvas.SetActive(false);
        settingsCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);

        menuButtons.Clear();
        menuButtons.Add(startButton);
        menuButtons.Add(settingsButton);
        menuButtons.Add(quitButton);

        selectedIndex = 0;
        MoveIndicator(menuButtons[selectedIndex]);
        UpdateHoverListeners();

        TriggerExternalAnimators("isBack");

        isReturningToMainMenu = false;
    }

    public void OnConfirmationYes()
    {
        onConfirmation_Yes?.Invoke();
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void OnConfirmationNo()
    {
        onConfirmation_No?.Invoke();
        OnBackToMainMenu();
    }

    public void OnSettingsBackPressed()
    {
        onSettingsBack?.Invoke();
        OnBackToMainMenu();
    }

    private void MoveIndicator(Button selectedButton)
    {
        if (indicator != null && selectedButton != null)
        {
            indicator.position = new Vector3(indicator.position.x, selectedButton.transform.position.y - indicatorOffset, indicator.position.z);
        }
    }

    private void TriggerExternalAnimators(string trigger)
    {
        foreach (var animator in externalAnimators)
        {
            if (animator != null)
                animator.SetTrigger(trigger);
        }
    }

    private IEnumerator LoadScene(SceneField scene)
    {
        if (scene == null)
        {
            Debug.LogError("Scene to load is null!");
            yield break;
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene.SceneName);
        _scenesToLoad.Add(asyncLoad);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        _scenesToLoad.Remove(asyncLoad);
    }

    private void SetMasterVolume(float volume)
    {
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.SetMasterVolume(volume);
        }
        else
        {
            Debug.LogWarning("AudioManager not found in scene!");
        }

        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    private void SetBrightness(float brightness)
    {
        PlayerPrefs.SetFloat("Brightness", brightness);
        PlayerPrefs.Save();
        Debug.Log($"Brightness set to {brightness}. Implement brightness adjustment logic.");
    }
}
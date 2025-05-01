using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu")]
    Animator anim;

    public GameObject levelSelectionCanvas;
    public GameObject mainMenuCanvas;
    public GameObject confirmMenuCanvas;

    public TextMeshProUGUI gameName;

    public Button startButton;
    public Button quitButton;
    public Button newGameButton;
    public Button loadGameButton;
    public Button yesConfirmation;
    public Button noConfirmation;
    public Button backButton;
    public Button tapToStartButton;

    // MAIN MENU
    public UnityEvent onStartButtonPressed;
    public UnityEvent onConfirmationMenu;

    // GAME MODE SELECTION
    public UnityEvent onNewGame;
    public UnityEvent onLoadGame;
    public UnityEvent onBackToMainMenu;

    // CONFIRMATION MENU
    public UnityEvent onConfirmation_Yes;
    public UnityEvent onConfirmation_No;

    public List<Button> menuButtons = new List<Button>();
    private int selectedIndex = 0;

    [Header("Scenes To Load")]
    [SerializeField] private SceneField Level1;
    [SerializeField] private SceneField _mainMenuScene;

    [Header("Animations")]
    public List<Animator> externalAnimators;
    [SerializeField] private Image transitionPanel; // Full-screen panel for scene transition
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float buttonHoverSlide = 10f; // Pixels to slide up on hover
    [SerializeField] private Ease slideEase = Ease.InOutCubic;

    private bool startButtonPressed = false;
    private bool isReturningToMainMenu = false;
    private List<AsyncOperation> _scenesToLoad = new List<AsyncOperation>();
    private CanvasGroup mainMenuCanvasGroup;
    private CanvasGroup levelSelectionCanvasGroup;
    private CanvasGroup confirmMenuCanvasGroup;
    private Vector2 mainMenuOriginalPos;
    private Vector2 levelSelectionOriginalPos;
    private Vector2 confirmMenuOriginalPos;
    private Vector2 transitionPanelOriginalPos;

    private void Awake()
    {
        Time.timeScale = 1f;
        anim = GetComponent<Animator>();

        // Initialize CanvasGroups and store original positions
        InitializeCanvas(mainMenuCanvas, ref mainMenuCanvasGroup, ref mainMenuOriginalPos);
        InitializeCanvas(levelSelectionCanvas, ref levelSelectionCanvasGroup, ref levelSelectionOriginalPos);
        InitializeCanvas(confirmMenuCanvas, ref confirmMenuCanvasGroup, ref confirmMenuOriginalPos);

        // Initialize canvases
        mainMenuCanvas.SetActive(true);
        mainMenuCanvasGroup.alpha = 1f;
        mainMenuCanvas.GetComponent<RectTransform>().anchoredPosition = mainMenuOriginalPos;
        confirmMenuCanvas.SetActive(false);
        confirmMenuCanvasGroup.alpha = 0f;
        levelSelectionCanvas.SetActive(false);
        levelSelectionCanvasGroup.alpha = 0f;

        // Initialize transition panel
        if (transitionPanel != null)
        {
            transitionPanelOriginalPos = transitionPanel.GetComponent<RectTransform>().anchoredPosition;
            transitionPanel.color = new Color(transitionPanel.color.r, transitionPanel.color.g, transitionPanel.color.b, 1f);
            transitionPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -Screen.height);
        }
        else
        {
            Debug.LogWarning("TransitionPanel not assigned in MainMenuManager. Scene transitions will not animate.", this);
        }

        // MAIN MENU
        menuButtons.Add(startButton);
        menuButtons.Add(quitButton);

        AssignButtonListeners();
        UpdateHoverListeners();
    }

    private void InitializeCanvas(GameObject canvas, ref CanvasGroup canvasGroup, ref Vector2 originalPos)
    {
        if (canvas != null)
        {
            canvasGroup = canvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = canvas.AddComponent<CanvasGroup>();
            }
            originalPos = canvas.GetComponent<RectTransform>().anchoredPosition;
        }
        else
        {
            Debug.LogError($"{canvas} not assigned in MainMenuManager.", this);
        }
    }

    private void Update()
    {
        HandleMenuNavigation();
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
        startButton.onClick.AddListener(() => OnStartButtonPressed());
        quitButton.onClick.AddListener(() => OnConfirmationMenu());

        // GAME MODE SELECTION
        newGameButton.onClick.AddListener(() => OnNewGamePressed());
        loadGameButton.onClick.AddListener(() => OnLoadGamePressed());
        backButton.onClick.AddListener(() => OnBackToMainMenu());

        // CONFIRMATION
        yesConfirmation.onClick.AddListener(() => OnConfirmationYes());
        noConfirmation.onClick.AddListener(() => OnConfirmationNo());

        // TAP TO START
        tapToStartButton.onClick.RemoveAllListeners();
        tapToStartButton.onClick.AddListener(() => OnBackToMainMenu());
    }

    private void UpdateHoverListeners()
    {
        foreach (var button in menuButtons)
        {
            if (button == null) continue;

            EventTrigger trigger = button.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener((data) => { OnButtonHover(button); OnButtonHoverEnter(button); });

            EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener((data) => { OnButtonHoverExit(button); });

            trigger.triggers.Add(enterEntry);
            trigger.triggers.Add(exitEntry);
        }

        // Add hover listeners for sub-menu buttons
        AddHoverListener(newGameButton);
        AddHoverListener(loadGameButton);
        AddHoverListener(backButton);
        AddHoverListener(yesConfirmation);
        AddHoverListener(noConfirmation);
        AddHoverListener(tapToStartButton);
    }

    private void AddHoverListener(Button button)
    {
        if (button == null) return;

        EventTrigger trigger = button.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => { OnButtonHover(button); OnButtonHoverEnter(button); });

        EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => { OnButtonHoverExit(button); });

        trigger.triggers.Add(enterEntry);
        trigger.triggers.Add(exitEntry);
    }

    private void OnButtonHover(Button hoveredButton)
    {
        int index = menuButtons.IndexOf(hoveredButton);
        if (index >= 0)
            selectedIndex = index;
    }

    private void OnButtonHoverEnter(Button button)
    {
        if (button != null)
        {
            RectTransform rect = button.GetComponent<RectTransform>();
            Vector2 originalPos = rect.anchoredPosition;
            rect.DOAnchorPosY(originalPos.y + buttonHoverSlide, slideDuration).SetEase(slideEase);
        }
    }

    private void OnButtonHoverExit(Button button)
    {
        if (button != null)
        {
            RectTransform rect = button.GetComponent<RectTransform>();
            Vector2 originalPos = rect.anchoredPosition;
            rect.DOAnchorPosY(originalPos.y - buttonHoverSlide, slideDuration).SetEase(slideEase);
        }
    }

    public void OnStartButtonPressed()
    {
        if (startButtonPressed) return;

        startButtonPressed = true;
        onStartButtonPressed?.Invoke();

        anim.SetTrigger("isStart");

        TransitionCanvas(mainMenuCanvas, mainMenuOriginalPos, levelSelectionCanvas, levelSelectionOriginalPos, () =>
        {
            mainMenuCanvas.SetActive(false);
            levelSelectionCanvas.SetActive(true);
            menuButtons.Clear();
            menuButtons.Add(newGameButton);
            menuButtons.Add(loadGameButton);
            menuButtons.Add(backButton);
            selectedIndex = 0;
            startButtonPressed = false;
            UpdateHoverListeners();
        });

        TriggerExternalAnimators("isStart");
    }

    public void OnConfirmationMenu()
    {
        onConfirmationMenu?.Invoke();

        TransitionCanvas(mainMenuCanvas, mainMenuOriginalPos, confirmMenuCanvas, confirmMenuOriginalPos, () =>
        {
            mainMenuCanvas.SetActive(false);
            confirmMenuCanvas.SetActive(true);
            menuButtons.Clear();
            menuButtons.Add(yesConfirmation);
            menuButtons.Add(noConfirmation);
            selectedIndex = 0;
            UpdateHoverListeners();
        });

        TriggerExternalAnimators("isConfirm");
    }

    public void OnNewGamePressed()
    {
        onNewGame?.Invoke();
        StartCoroutine(TransitionAndLoadScene(Level1));
    }

    public void OnLoadGamePressed()
    {
        onLoadGame?.Invoke();
        Debug.Log("Load Game pressed. Implement save/load system to load saved game state.");
        StartCoroutine(TransitionAndLoadScene(Level1));
    }

    public void OnBackToMainMenu()
    {
        if (isReturningToMainMenu) return;

        isReturningToMainMenu = true;
        onBackToMainMenu?.Invoke();

        anim.SetTrigger("isBack");

        GameObject fromCanvas = levelSelectionCanvas.activeSelf ? levelSelectionCanvas :
                               confirmMenuCanvas.activeSelf ? confirmMenuCanvas : null;
        Vector2 fromOriginalPos = levelSelectionCanvas.activeSelf ? levelSelectionOriginalPos :
                                 confirmMenuCanvas.activeSelf ? confirmMenuOriginalPos : Vector2.zero;

        TransitionCanvas(fromCanvas, fromOriginalPos, mainMenuCanvas, mainMenuOriginalPos, () =>
        {
            levelSelectionCanvas.SetActive(false);
            confirmMenuCanvas.SetActive(false);
            mainMenuCanvas.SetActive(true);
            menuButtons.Clear();
            menuButtons.Add(startButton);
            menuButtons.Add(quitButton);
            selectedIndex = 0;
            UpdateHoverListeners();
            isReturningToMainMenu = false;
        });

        TriggerExternalAnimators("isBack");
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

    private void TransitionCanvas(GameObject fromCanvas, Vector2 fromOriginalPos, GameObject toCanvas, Vector2 toOriginalPos, System.Action onComplete)
    {
        if (fromCanvas != null && toCanvas != null)
        {
            RectTransform fromRect = fromCanvas.GetComponent<RectTransform>();
            RectTransform toRect = toCanvas.GetComponent<RectTransform>();
            float slideDistance = Screen.width; // Slide full screen width

            // Start with toCanvas off-screen to the right
            toRect.anchoredPosition = toOriginalPos + new Vector2(slideDistance, 0);

            Sequence transitionSequence = DOTween.Sequence();
            transitionSequence.Append(fromRect.DOAnchorPosX(fromOriginalPos.x - slideDistance, slideDuration).SetEase(slideEase))
                             .Join(fromCanvas.GetComponent<CanvasGroup>().DOFade(0f, slideDuration).SetEase(slideEase))
                             .Append(toRect.DOAnchorPosX(toOriginalPos.x, slideDuration).SetEase(slideEase))
                             .Join(toCanvas.GetComponent<CanvasGroup>().DOFade(1f, slideDuration).SetEase(slideEase))
                             .OnComplete(() => onComplete?.Invoke());
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    private IEnumerator TransitionAndLoadScene(SceneField scene)
    {
        if (scene == null)
        {
            Debug.LogError("Scene to load is null!");
            yield break;
        }

        if (transitionPanel != null)
        {
            RectTransform panelRect = transitionPanel.GetComponent<RectTransform>();
            panelRect.anchoredPosition = new Vector2(0, -Screen.height);
            yield return panelRect.DOAnchorPosY(0, slideDuration).SetEase(slideEase).WaitForCompletion();
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene.SceneName);
        _scenesToLoad.Add(asyncLoad);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        _scenesToLoad.Remove(asyncLoad);
    }

    private void TriggerExternalAnimators(string trigger)
    {
        foreach (var animator in externalAnimators)
        {
            if (animator != null)
                animator.SetTrigger(trigger);
        }
    }
}
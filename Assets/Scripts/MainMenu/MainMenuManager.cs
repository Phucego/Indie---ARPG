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
    
    public TextMeshProUGUI gameName;

    public Button startButton;
    public Button settingsButton;
    public Button quitButton;
    public Button newGameButton;
    public Button loadGameButton;
    public Button yesConfirmation;
    public Button noConfirmation;
    public Button backButton;
    
    //MAIN MENU
    public UnityEvent onStartButtonPressed;
    public UnityEvent onSettingsMenu; 
    public UnityEvent onConfirmationMenu; 

    //GAME MODE SELECTION
    public UnityEvent onNewGame; 
    public UnityEvent onLoadGame; 
    public UnityEvent onBackToMainMenu; 

    //CONFIRMATION MENU
    public UnityEvent onConfirmation_Yes; 
    public UnityEvent onConfirmation_No; 

    [Header("Navigation Indicator")]
    public RectTransform indicator;
    public float indicatorOffset = -3.4f;

    public List<Button> menuButtons = new List<Button>();
    private int selectedIndex = 0;

    [Header("Scenes To Load")]
    [SerializeField] private SceneField Level1;
    [SerializeField] private SceneField _mainMenuScene;
    
    [Header("Animations")]
    public List<Animator> externalAnimators; // List of Animators on different GameObjects
    
    private bool startButtonPressed = false; 
    private List<AsyncOperation> _scenesToLoad = new List<AsyncOperation>();

    private void Awake()
    {
        Time.timeScale = 1f;
        anim = GetComponent<Animator>();

        confirmMenuCanvas.SetActive(false);
        levelSelectionCanvas.SetActive(false);


        //MAIN MENU
        menuButtons.Add(startButton);
        menuButtons.Add(settingsButton);
        menuButtons.Add(quitButton);

        AssignButtonListeners();
        AssignHoverListeners();

        MoveIndicator(menuButtons[selectedIndex]);
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
        //MAIN MENU
        startButton.onClick.AddListener(() => { MoveIndicator(startButton); OnStartButtonPressed(); });
        settingsButton.onClick.AddListener(() => MoveIndicator(settingsButton));
        quitButton.onClick.AddListener(() => { MoveIndicator(quitButton); OnConfirmationMenu(); });

        //GAME MODE SELECTION
        newGameButton.onClick.AddListener(() => { MoveIndicator(newGameButton); });
        loadGameButton.onClick.AddListener(() => { MoveIndicator(loadGameButton); });
        backButton.onClick.AddListener(() => { MoveIndicator(backButton); });

        //CONFIRMATION
        yesConfirmation.onClick.AddListener(() => { MoveIndicator(yesConfirmation); });
        noConfirmation.onClick.AddListener(() => { MoveIndicator(noConfirmation); });
    }

    private void AssignHoverListeners()
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
    }

    private void OnButtonHover(Button hoveredButton)
    {
        selectedIndex = menuButtons.IndexOf(hoveredButton);
    }

    public void OnStartButtonPressed()
    {
        if (startButtonPressed) return; // Prevent duplicate execution

        startButtonPressed = true; 
        onStartButtonPressed?.Invoke(); 

        //Clear the main menu buttons
        menuButtons.Clear();
        anim.SetTrigger("isStart");

        levelSelectionCanvas.SetActive(true);

        //GAME MODE SELECTION
        menuButtons.Add(newGameButton);
        menuButtons.Add(loadGameButton);
        menuButtons.Add(backButton);

        MoveIndicator(menuButtons[selectedIndex]);
    }

    public void OnConfirmationMenu()
    {
        onConfirmationMenu?.Invoke();
        confirmMenuCanvas.SetActive(true);
        
        menuButtons.Clear();
        menuButtons.Add(yesConfirmation);
        menuButtons.Add(noConfirmation);

        MoveIndicator(menuButtons[selectedIndex]);
    }

    private void MoveIndicator(Button selectedButton)
    {
        if (indicator != null)
        {
            indicator.position = new Vector3(indicator.position.x, selectedButton.transform.position.y - indicatorOffset, indicator.position.z);
        }
    }
}
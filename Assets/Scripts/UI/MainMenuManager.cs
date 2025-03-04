using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu")]
    Animator anim;

    public GameObject levelSelectionCanvas;
    public GameObject mainMenuCanvas;
    public TextMeshProUGUI gameName;

    public Button startButton;
    public Button settingsButton;
    public Button quitButton;
    /*public Button backButton;
    public Button Quit_Yes;
    public Button Quit_No;
    public Button tutLevel;*/

    public GameObject confirmMenuCanvas;

    [Header("Navigation Indicator")]
    public RectTransform indicator; // UI indicator reference
    public float indicatorOffset = -3.4f; // Offset for the indicator position

    public List<Button> menuButtons = new List<Button>(); // List of menu buttons
    private int selectedIndex = 0; // Current button index

    [Header("Scenes To Load")]
    [SerializeField] private SceneField Level1;
    [SerializeField] private SceneField _mainMenuScene;

    private List<AsyncOperation> _scenesToLoad = new List<AsyncOperation>();

    private void Awake()
    {
        Time.timeScale = 1f;
        anim = GetComponent<Animator>();

        confirmMenuCanvas.SetActive(false);
        levelSelectionCanvas.SetActive(false);

        // Add menu buttons to the list in order
        menuButtons.Add(startButton);
        menuButtons.Add(settingsButton);
        menuButtons.Add(quitButton);

        AssignButtonListeners();
        AssignHoverListeners();

        MoveIndicator(menuButtons[selectedIndex]); // Set default indicator position
    }

    private void Update()
    {
        HandleMenuNavigation();
    
        // Smooth move
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
            menuButtons[selectedIndex].onClick.Invoke(); // Select the highlighted button
        }
    }

    private void AssignButtonListeners()
    {
        startButton.onClick.AddListener(() => { MoveIndicator(startButton); MainMenuOut(); });
        settingsButton.onClick.AddListener(() => MoveIndicator(settingsButton));
        quitButton.onClick.AddListener(() => { MoveIndicator(quitButton); QuitGame(); });

        /*backButton.onClick.AddListener(() => { MoveIndicator(backButton); MainMenuIn(); });
        Quit_Yes.onClick.AddListener(OnConfirmQuit);
        Quit_No.onClick.AddListener(OnConfirmBack);
        tutLevel.onClick.AddListener(() => { MoveIndicator(tutLevel); OnStartLevel(); });*/
    }

    private void AssignHoverListeners()
    {
        foreach (var button in menuButtons)
        {
            if (button == null) continue;

            // Ensure the button has an EventTrigger component
            EventTrigger trigger = button.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = button.gameObject.AddComponent<EventTrigger>();
            }

            // Clear any previous triggers to avoid duplicates
            trigger.triggers.Clear();

            // Create a new PointerEnter event
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entry.callback.AddListener((data) => { OnButtonHover(button); });

            trigger.triggers.Add(entry);
        }
    }



    private void OnButtonHover(Button hoveredButton)
    {
        
        selectedIndex = menuButtons.IndexOf(hoveredButton);
    }

    public void MainMenuOut()
    {
        anim.SetBool("fromMenu", true);
        levelSelectionCanvas.SetActive(true);

        menuButtons.Clear();
        /*menuButtons.Add(tutLevel);
        menuButtons.Add(backButton);*/
        AssignHoverListeners();
        selectedIndex = 0;
        MoveIndicator(menuButtons[selectedIndex]);
    }

    public void MainMenuIn()
    {
        anim.SetBool("fromMenu", false);
        mainMenuCanvas.SetActive(true);
        levelSelectionCanvas.SetActive(false);

        menuButtons.Clear();
        menuButtons.Add(startButton);
        menuButtons.Add(settingsButton);
        menuButtons.Add(quitButton);
        AssignHoverListeners();
        selectedIndex = 0;
        MoveIndicator(menuButtons[selectedIndex]);
    }

    void OnStartLevel()
    {
        levelSelectionCanvas.SetActive(false);
        StartCoroutine(StartLevelTransition());
    }

    #region Confirmation Menu
    private void OnConfirmQuit()
    {
        Application.Quit();
    }

    private void OnConfirmBack()
    {
        anim.SetBool("isConfirmationMenu", false);
    }
    #endregion

    public void QuitGame()
    {
        anim.SetBool("isConfirmationMenu", true);
    }

    IEnumerator StartLevelTransition()
    {
        anim.SetTrigger("isStart");
        yield return new WaitForSeconds(1);
    }
    private void MoveIndicator(Button selectedButton)
    {
        if (indicator != null)
        {
            indicator.position = new Vector3(indicator.position.x, selectedButton.transform.position.y - indicatorOffset, indicator.position.z);
        }
    }
}
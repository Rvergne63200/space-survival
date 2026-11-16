using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Pause menu (Escape in game, see ParentUI): resume, back to the main menu, quit.
// The panel (UIDocument with PauseMenu.uxml, editable in the UI Builder) is registered as an interface:
// Escape closes it like the other interfaces. The game is paused (timeScale 0) while it is open.
public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private string mainMenuScene = "0_MainMenu";

    private bool paused;

    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void Update()
    {
        // Also closed by InterfaceManager.CloseInterfaces (Escape)
        if (paused && !IsOpen)
        {
            Resume();
        }
    }

    public void Open()
    {
        if (panel == null || IsOpen) return;

        // The UIDocument builds its elements when it is enabled
        panel.SetActive(true);
        InterfaceManager.GetInstance().SetInterface(panel, true);

        VisualElement root = panel.GetComponent<UIDocument>().rootVisualElement;
        BindButton(root, "resume-button", Close);
        BindButton(root, "main-menu-button", ReturnToMainMenu);
        BindButton(root, "quit-button", Quit);

        Time.timeScale = 0f;
        paused = true;
    }

    public void Close()
    {
        if (panel == null) return;

        panel.SetActive(false);
        InterfaceManager.GetInstance().SetInterface(panel, false);
        Resume();
    }

    private void Resume()
    {
        Time.timeScale = 1f;
        paused = false;
    }

    private void BindButton(VisualElement root, string name, System.Action action)
    {
        Button button = root.Q<Button>(name);

        if (button == null)
        {
            Debug.LogWarning("Pause menu: no button named \"" + name + "\" in the UXML.");
            return;
        }

        button.clicked += action;
    }

    private void ReturnToMainMenu()
    {
        Close();

        // The interfaces of the game (inventory, storage...) are closed before leaving the world
        InterfaceManager.GetInstance().CloseInterfaces();

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.BeforeSceneChange();
        }

        SceneManager.LoadScene(mainMenuScene, LoadSceneMode.Single);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

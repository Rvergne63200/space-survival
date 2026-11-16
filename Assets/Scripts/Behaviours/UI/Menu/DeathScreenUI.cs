using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Death screen (PlayerManager.KillPlayer): red filter, "Respawn" (available after respawnDelay seconds) and "Main menu".
// The panel (UIDocument with DeathScreen.uxml, editable in the UI Builder) is not an interface: Escape does not close it.
public class DeathScreenUI : MonoBehaviour
{
    public static DeathScreenUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private float respawnDelay = 25f;
    [SerializeField] private string mainMenuScene = "0_MainMenu";

    private Button respawnButton;
    private string respawnText;
    private float remainingDelay;

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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // Never kept from a scene to another
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Close();
    }

    public void Open()
    {
        if (panel == null || IsOpen) return;

        // The UIDocument builds its elements when it is enabled
        panel.SetActive(true);

        // Free cursor, no player control
        ModeManager.Instance.EnableMode(PlayingMode.Menu);

        VisualElement root = panel.GetComponent<UIDocument>().rootVisualElement;

        respawnButton = root.Q<Button>("respawn-button");

        if (respawnButton != null)
        {
            respawnText = respawnButton.text;
            respawnButton.clicked += Respawn;
        }
        else
        {
            Debug.LogWarning("Death screen: no button named \"respawn-button\" in the UXML.");
        }

        Button mainMenuButton = root.Q<Button>("main-menu-button");

        if (mainMenuButton != null)
        {
            mainMenuButton.clicked += ReturnToMainMenu;
        }
        else
        {
            Debug.LogWarning("Death screen: no button named \"main-menu-button\" in the UXML.");
        }

        remainingDelay = respawnDelay;
        UpdateRespawnButton();
    }

    private void Update()
    {
        if (!IsOpen || remainingDelay <= 0f) return;

        remainingDelay -= Time.unscaledDeltaTime;
        UpdateRespawnButton();
    }

    private void UpdateRespawnButton()
    {
        if (respawnButton == null) return;

        bool available = remainingDelay <= 0f;

        respawnButton.SetEnabled(available);
        respawnButton.text = available ? respawnText : respawnText + " (" + Mathf.CeilToInt(remainingDelay) + ")";
    }

    private void Close()
    {
        if (!IsOpen) return;

        panel.SetActive(false);
        ModeManager.Instance.DisableMode(PlayingMode.Menu);
    }

    private void Respawn()
    {
        if (remainingDelay > 0f) return;

        Close();

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.Respawn();
        }
    }

    private void ReturnToMainMenu()
    {
        Close();

        // The game is saved with the player respawned (not dead with empty stats)
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.Respawn();
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.BeforeSceneChange();
        }

        SceneManager.LoadScene(mainMenuScene, LoadSceneMode.Single);
    }
}

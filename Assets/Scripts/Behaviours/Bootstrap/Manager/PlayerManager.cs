using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private CameraManager cameraManager;


    public GameObject prefab;


    [SerializeField] private PlayerInventory inventory;
    public PlayerInventory Inventory => inventory;

    [SerializeField] private PlayerStats stats;
    public PlayerStats Stats => stats;


    private GameObject currentPlayer;
    public GameObject Player => currentPlayer;

    public Transform ViewSource => cameraManager.Camera.transform;

    public UnityEvent<PlayerContext> ev_updatePlayerContext;
    public UnityEvent<GameObject> ev_spawnPlayer;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // True when the player was placed at its saved position (not on the spawn point)
    public bool SpawnedFromSave { get; private set; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Searched in all the loaded scenes: when a scene is played without the Bootstrap, the Bootstrap is loaded after it
        PlayerSpawnPoint spawnPoint = FindAnyObjectByType<PlayerSpawnPoint>();

        if (spawnPoint == null)
        {
            // No player in the scenes without spawn point (main menu)
            DespawnPlayer();
            return;
        }

        // Set before spawning: the ev_spawnPlayer listeners read it (ChunkManager)
        if (SaveManager.Instance != null && SaveManager.Instance.TryGetPlayerPose(out Vector3 position, out Quaternion rotation))
        {
            SpawnedFromSave = true;
            SpawnPlayer(position, rotation);
        }
        else
        {
            SpawnedFromSave = false;
            SpawnPlayer(spawnPoint.transform);
        }
    }

    public void DespawnPlayer()
    {
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
            currentPlayer = null;
        }

        // The scene has its own camera and UI (main menu)
        SetPlayerViewVisible(false);
        CameraManager.Instance?.SetTarget(null);
    }

    private void SetPlayerViewVisible(bool visible)
    {
        if (CameraManager.Instance != null && CameraManager.Instance.Camera != null)
        {
            CameraManager.Instance.Camera.SetActive(visible);
        }

        UIManager.Instance?.SetHudVisible(visible);
    }

    public void SpawnPlayer(Transform spawnPoint)
    {
        SpawnPlayer(spawnPoint.position, spawnPoint.rotation);
    }

    public void SpawnPlayer(Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            Debug.LogError("Player prefab is empty.");
            return;
        }

        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }

        currentPlayer = Instantiate(
            prefab,
            position,
            rotation
        );

        ev_spawnPlayer.Invoke(currentPlayer);

        PlayerContext playerContext = currentPlayer.GetComponent<PlayerContext>();

        playerContext.Initialize(this);

        ev_updatePlayerContext.Invoke(playerContext);

        SetPlayerViewVisible(true);
        CameraManager.Instance?.SetTarget(currentPlayer.transform);
    }
}

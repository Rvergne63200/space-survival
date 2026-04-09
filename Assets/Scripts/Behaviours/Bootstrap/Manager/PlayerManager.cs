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

    public Pickuper Pickuper => currentPlayer.GetComponent<Pickuper>();


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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SpawnPlayer(transform);
    }

    public void SpawnPlayer(Transform spawnPoint)
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
            spawnPoint.position,
            spawnPoint.rotation
        );

        ev_spawnPlayer.Invoke(currentPlayer);

        PlayerContext playerContext = currentPlayer.GetComponent<PlayerContext>();

        playerContext.Initialize(this);

        ev_updatePlayerContext.Invoke(playerContext);

        CameraManager.Instance?.SetTarget(currentPlayer.transform);
    }
}

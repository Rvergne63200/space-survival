using System.Collections.Generic;
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

    // Left where the player dies, with its inventory (must also be in SaveManager.runtimePrefabs)
    [SerializeField] private GameObject deathBagPrefab;


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

    // Between the death and the respawn (death screen): the stats are frozen
    public bool IsDead { get; private set; }

    // Health at 0 (PlayerStats): the inventory stays in a bag where the player died, then the death screen is shown
    public void KillPlayer()
    {
        if (currentPlayer == null || IsDead) return;

        IsDead = true;

        DropInventoryInBag(currentPlayer.transform.position);

        // Inventory, storage... (the bag already took the item held at the cursor)
        InterfaceManager.GetInstance().CloseInterfaces();

        if (DeathScreenUI.Instance != null)
        {
            DeathScreenUI.Instance.Open();
        }
        else
        {
            Respawn();
        }
    }

    // The player appears again on the spawn point with full stats (death screen)
    public void Respawn()
    {
        IsDead = false;
        stats.RefillAll();

        PlayerSpawnPoint spawnPoint = FindAnyObjectByType<PlayerSpawnPoint>();

        if (spawnPoint == null)
        {
            DespawnPlayer();
            return;
        }

        SpawnedFromSave = false;
        SpawnPlayer(spawnPoint.transform);
    }

    private void DropInventoryInBag(Vector3 deathPosition)
    {
        List<ItemSlot<Item>> slots = new List<ItemSlot<Item>>();
        slots.AddRange(inventory.Content.Slots);
        slots.AddRange(inventory.ToolBar.Slots);
        slots.Add(inventory.draggingSlot);

        bool hasItems = slots.Exists(slot => slot != null && !slot.IsEmpty());

        if (!hasItems) return;

        if (deathBagPrefab == null)
        {
            Debug.LogError("PlayerManager: no death bag prefab, the inventory is kept.");
            return;
        }

        // On the ground under the player (its own layer is ignored)
        Vector3 position = deathPosition;
        int layerMask = ~(1 << currentPlayer.layer);

        if (Physics.Raycast(deathPosition + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 50f, layerMask, QueryTriggerInteraction.Ignore))
        {
            position = hit.point;
        }

        GameObject bag = Instantiate(deathBagPrefab, position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));

        // Created during the game: the SaveManager instantiates it again when the world loads
        bag.GetComponent<SaveableObject>().SetRuntime(SaveManager.RuntimePrefabKeyPrefix + deathBagPrefab.name);

        ItemStorage storage = bag.GetComponent<ItemStorage>();

        foreach (ItemSlot<Item> slot in slots)
        {
            if (slot != null && !slot.IsEmpty())
            {
                storage.Add(slot.Item, slot.Quantity);
                slot.Clear();
            }
        }

        inventory.Content.ev_Update.Invoke();
        inventory.ToolBar.ev_Update.Invoke();
    }

    public void DespawnPlayer()
    {
        IsDead = false;

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
        IsDead = false;

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

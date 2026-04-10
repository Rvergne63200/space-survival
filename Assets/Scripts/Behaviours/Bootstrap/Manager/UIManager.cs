using UnityEngine;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private PlayerManager playerManager;

    public GameObject prefab;


    private GameObject currentUI;
    public GameObject UI => currentUI;

    public PlayerInventory PlayerInventory => playerManager.Inventory;
    public PlayerStats PlayerStats => playerManager.Stats;


    public PlayerContext PlayerContext => playerManager.Player?.GetComponent<PlayerContext>();
    public UnityEvent<PlayerContext> ev_updatePlayerContext => playerManager.ev_updatePlayerContext;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SpawnUI();
    }

    public void SpawnUI()
    {
        if (prefab == null)
        {
            return;
        }

        if (currentUI != null)
        {
            Destroy(currentUI);
        }

        currentUI = Instantiate(
            prefab,
            Vector3.zero,
            Quaternion.identity,
            transform.parent
        );

        currentUI.GetComponent<ParentUI>().Initialize(this);
        playerManager.ev_spawnPlayer.AddListener(OnSpawnPlayer);
    }

    public void OnSpawnPlayer(GameObject player)
    {
        UpdatePickuper(player);
        UpdateSearcher(player);
    }

    public void UpdatePickuper(GameObject player)
    {
        player.GetComponent<Pickuper>().ev_pickup.AddListener(currentUI.GetComponent<ParentUI>().PickupInfoGroupUI.OnPickupItems);
    }

    public void UpdateSearcher(GameObject player)
    {
        player.GetComponent<Searcher>().ev_UpdateSearchStorage.AddListener(currentUI.GetComponent<ParentUI>().SearchedItemStorageUI.SetItemStorage);
    }
}

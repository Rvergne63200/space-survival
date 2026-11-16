using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// Saves the world (player, saveable objects, terrain edits) and restores it when the world is loaded.
// One JSON file per game in Application.persistentDataPath/Saves, chosen in the main menu (new game or load).
// Saved when the application quits, before a scene change and periodically while in the world.
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [SerializeField] private string savesFolderName = "Saves";
    [SerializeField] private float autosaveInterval = 120f;

    // Every item of the game, to find them by id (inventory, chests, pickups). See "Refresh item list" in the component menu
    [SerializeField] private List<ItemData> items = new List<ItemData>();

    // Prefabs the player can build, found by name for the built objects
    [SerializeField] private List<GameObject> buildPrefabs = new List<GameObject>();

    public const string ItemPrefabKeyPrefix = "item:";
    public const string BuildPrefabKeyPrefix = "build:";

    private const string EditorSaveName = "Partie éditeur";

    private SaveData save;
    private string currentSlotId;
    private readonly Dictionary<string, ObjectSaveData> savedObjects = new Dictionary<string, ObjectSaveData>();
    private readonly HashSet<string> destroyedObjects = new HashSet<string>();
    private readonly Dictionary<string, SaveableObject> liveObjects = new Dictionary<string, SaveableObject>();
    private readonly Dictionary<int, ItemData> itemsById = new Dictionary<int, ItemData>();

    // Inventory and stats of a new game (the PlayerManager persists between the games of a session)
    private PlayerSaveData defaultPlayer;

    private ChunkManager restoredWorld;
    private bool changingScene;
    private bool quitting;
    private float autosaveTimer;

    public bool HasSave => save != null;
    public int WorldSeed => save != null ? save.worldSeed : 0;
    public IReadOnlyList<TerrainEditSaveData> SavedTerrainEdits => save != null ? save.terrainEdits : new List<TerrainEditSaveData>();

    // The world is the scene with a ChunkManager
    private bool WorldLoaded => ChunkManager.Instance != null;

    private string SavesFolder => Path.Combine(Application.persistentDataPath, savesFolderName);

    private string GetSlotPath(string slotId) => Path.Combine(SavesFolder, slotId + ".json");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        foreach (ItemData item in items)
        {
            if (item != null)
            {
                itemsById[item.Id] = item;
            }
        }

        MigrateSingleSave();
        SaveableObject.RegisterWaitingObjects(this);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // The PlayerManager is in the Bootstrap too: its inventory and stats are still the ones of a new game
        if (PlayerManager.Instance != null)
        {
            defaultPlayer = CapturePlayerState(PlayerManager.Instance);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Update()
    {
        if (!WorldLoaded || autosaveInterval <= 0f) return;

        autosaveTimer += Time.unscaledDeltaTime;

        if (autosaveTimer >= autosaveInterval)
        {
            autosaveTimer = 0f;
            Save();
        }
    }

    private void OnApplicationQuit()
    {
        Save();
        quitting = true;
    }

    // Called before loading another scene: the objects of the current scene are about to be destroyed
    public void BeforeSceneChange()
    {
        Save();
        changingScene = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        changingScene = false;
        autosaveTimer = 0f;

        // When a scene is played without the Bootstrap, the world is loaded before the managers: the Bootstrap
        // loaded event then restores it. The check avoids restoring the same world twice
        if (!WorldLoaded || restoredWorld == ChunkManager.Instance) return;

        restoredWorld = ChunkManager.Instance;
        RestoreWorld();
    }

    // ---------- Games (main menu) ----------

    public List<SaveSlotInfo> ListSaves()
    {
        List<SaveSlotInfo> saves = new List<SaveSlotInfo>();

        if (!Directory.Exists(SavesFolder)) return saves;

        foreach (string path in Directory.GetFiles(SavesFolder, "*.json"))
        {
            SaveData data = ReadFile(path);

            if (data == null) continue;

            DateTime.TryParse(data.lastSaveTime, null, DateTimeStyles.RoundtripKind, out DateTime lastSaveTime);

            saves.Add(new SaveSlotInfo
            {
                id = Path.GetFileNameWithoutExtension(path),
                name = string.IsNullOrEmpty(data.saveName) ? Path.GetFileNameWithoutExtension(path) : data.saveName,
                lastSaveTime = lastSaveTime,
                seed = data.worldSeed
            });
        }

        // Most recent first
        saves.Sort((a, b) => b.lastSaveTime.CompareTo(a.lastSaveTime));
        return saves;
    }

    public bool SaveNameExists(string saveName)
    {
        foreach (SaveSlotInfo info in ListSaves())
        {
            if (string.Equals(info.name, saveName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    // Creates the save of a new game and makes it the current one. The world must then be loaded
    public void NewGame(string saveName, int seed)
    {
        ClearState();

        currentSlotId = Guid.NewGuid().ToString("N");
        save = new SaveData { saveName = saveName, worldSeed = seed, lastSaveTime = DateTime.Now.ToString("o") };

        WriteFile(GetSlotPath(currentSlotId), save);
    }

    // Makes the save the current one. The world must then be loaded
    public bool LoadGame(string slotId)
    {
        ClearState();

        SaveData data = ReadFile(GetSlotPath(slotId));

        if (data == null) return false;

        currentSlotId = slotId;
        save = data;

        foreach (ObjectSaveData objectData in save.objects)
        {
            savedObjects[objectData.id] = objectData;
        }

        foreach (string id in save.destroyedObjects)
        {
            destroyedObjects.Add(id);
        }

        return true;
    }

    public void DeleteSave(string slotId)
    {
        string path = GetSlotPath(slotId);

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        if (slotId == currentSlotId)
        {
            ClearState();
        }
    }

    // A number is used as is, any other text is converted to a number
    public static int ParseSeed(string text)
    {
        text = text?.Trim() ?? "";

        if (int.TryParse(text, out int seed))
        {
            return seed;
        }

        // FNV-1a: unlike string.GetHashCode, always the same number for the same text
        unchecked
        {
            int hash = (int)2166136261;

            foreach (char character in text)
            {
                hash = (hash ^ character) * 16777619;
            }

            return hash;
        }
    }

    public static int RandomSeed()
    {
        return new System.Random().Next(0, int.MaxValue);
    }

    private void ClearState()
    {
        save = null;
        currentSlotId = null;
        savedObjects.Clear();
        destroyedObjects.Clear();
    }

    // ---------- Saveable objects ----------

    public ItemData GetItem(int id)
    {
        if (itemsById.TryGetValue(id, out ItemData item))
        {
            return item;
        }

        Debug.LogWarning("Save: unknown item id " + id + ". Add it to the SaveManager item list (Refresh item list).");
        return null;
    }

    public bool TryGetPlayerPose(out Vector3 position, out Quaternion rotation)
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;

        if (save == null || save.player == null) return false;

        position = save.player.position;
        rotation = save.player.rotation;
        return true;
    }

    public IEnumerable<SaveableObject> LiveObjects => liveObjects.Values;

    public bool IsDestroyed(string id)
    {
        return destroyedObjects.Contains(id);
    }

    // Called by SaveableObject.Start. Applies the saved state, or destroys the object if it was removed in the save
    public bool Register(SaveableObject saveable)
    {
        if (destroyedObjects.Contains(saveable.Id))
        {
            Destroy(saveable.gameObject);
            return false;
        }

        if (liveObjects.TryGetValue(saveable.Id, out SaveableObject other) && other != null && other != saveable)
        {
            Debug.LogWarning("Save: two objects have the id \"" + saveable.Id + "\" (" + other.name + " and " + saveable.name + "). Only the first one is saved.", saveable);
            return false;
        }

        liveObjects[saveable.Id] = saveable;

        if (savedObjects.TryGetValue(saveable.Id, out ObjectSaveData data))
        {
            saveable.Restore(data);
        }

        return true;
    }

    public void Unregister(SaveableObject saveable)
    {
        if (liveObjects.TryGetValue(saveable.Id, out SaveableObject live) && live == saveable)
        {
            liveObjects.Remove(saveable.Id);
        }

        // Unloaded with its scene (its state was saved just before): it still exists in the world
        if (changingScene || quitting || !saveable.gameObject.scene.isLoaded) return;

        // Removed during the game (picked up, destroyed...)
        savedObjects.Remove(saveable.Id);

        if (!saveable.IsRuntime)
        {
            destroyedObjects.Add(saveable.Id);
        }
    }

    // Before destroying a part of the world that will be created again later (far chunk): keeps the state of its
    // saveable objects, which are recreated with it, instead of noting them as removed
    public void UnloadObjects(GameObject root)
    {
        foreach (SaveableObject saveable in root.GetComponentsInChildren<SaveableObject>(true))
        {
            if (!saveable.IsRegistered) continue;

            if (liveObjects.TryGetValue(saveable.Id, out SaveableObject live) && live == saveable)
            {
                savedObjects[saveable.Id] = saveable.Capture();
                liveObjects.Remove(saveable.Id);
            }

            saveable.MarkUnloaded();
        }
    }

    // ---------- Save / restore ----------

    public void Save()
    {
        if (!WorldLoaded) return;

        // World played directly in the editor, without choosing a game in the main menu
        if (currentSlotId == null)
        {
            currentSlotId = Guid.NewGuid().ToString("N");
            save = new SaveData { saveName = EditorSaveName + " " + DateTime.Now.ToString("g") };
        }

        PlayerManager playerManager = PlayerManager.Instance;

        SaveData data = new SaveData
        {
            saveName = save.saveName,
            lastSaveTime = DateTime.Now.ToString("o"),
            worldSeed = ChunkManager.Instance.Seed,
            player = CapturePlayer(playerManager)
        };

        // Objects not loaded right now (in chunks not generated this session) keep their last saved state
        foreach (SaveableObject saveable in liveObjects.Values)
        {
            if (saveable != null)
            {
                savedObjects[saveable.Id] = saveable.Capture();
            }
        }

        data.objects.AddRange(savedObjects.Values);
        data.destroyedObjects.AddRange(destroyedObjects);
        data.terrainEdits.AddRange(ChunkManager.Instance.TerrainEdits);

        save = data;
        WriteFile(GetSlotPath(currentSlotId), data);
    }

    private PlayerSaveData CapturePlayer(PlayerManager playerManager)
    {
        if (playerManager == null || playerManager.Player == null)
        {
            // No player to save: keep the previous player data
            return save?.player;
        }

        PlayerSaveData data = CapturePlayerState(playerManager);
        data.position = playerManager.Player.transform.position;
        data.rotation = playerManager.Player.transform.rotation;

        CameraMovement cameraMovement = GetCameraMovement();

        if (cameraMovement != null)
        {
            data.cameraPitch = cameraMovement.Pitch;
            data.cameraYaw = cameraMovement.Yaw;
        }

        return data;
    }

    // Stats and inventory, which are on the PlayerManager
    private PlayerSaveData CapturePlayerState(PlayerManager playerManager)
    {
        return new PlayerSaveData
        {
            stats = playerManager.Stats.Capture(),
            inventory = playerManager.Inventory.Content.CaptureSlots(),
            toolBar = playerManager.Inventory.ToolBar.CaptureSlots(),
            handedIndex = playerManager.Inventory.HandedIndex
        };
    }

    private void RestoreWorld()
    {
        // A new game starts with the default inventory and stats, even after another game in the same session
        PlayerSaveData player = save != null && save.player != null ? save.player : defaultPlayer;
        PlayerManager playerManager = PlayerManager.Instance;

        if (player != null && playerManager != null)
        {
            playerManager.Stats.Restore(player.stats);
            playerManager.Inventory.Content.RestoreSlots(player.inventory, GetItem);
            playerManager.Inventory.ToolBar.RestoreSlots(player.toolBar, GetItem);
            playerManager.Inventory.SetHandedIndex(player.handedIndex);

            CameraMovement cameraMovement = GetCameraMovement();

            if (cameraMovement != null)
            {
                cameraMovement.SetRotation(player.cameraPitch, player.cameraYaw);
            }
        }

        // Scene and generated objects restore themselves when they register. Runtime objects must be created again
        foreach (ObjectSaveData data in savedObjects.Values)
        {
            if (!string.IsNullOrEmpty(data.prefabKey) && !liveObjects.ContainsKey(data.id))
            {
                InstantiateRuntimeObject(data);
            }
        }
    }

    private void InstantiateRuntimeObject(ObjectSaveData data)
    {
        GameObject prefab = GetRuntimePrefab(data);

        if (prefab == null)
        {
            Debug.LogWarning("Save: cannot recreate \"" + data.id + "\", unknown prefab key \"" + data.prefabKey + "\".");
            return;
        }

        GameObject instance = Instantiate(prefab, data.position, data.rotation);

        SaveableObject saveable = instance.GetComponent<SaveableObject>();
        if (saveable == null)
        {
            saveable = instance.AddComponent<SaveableObject>();
        }

        // The saved state (components) is applied when it registers, at Start
        saveable.SetRuntime(data.prefabKey, data.id);

        if (data.prefabKey.StartsWith(BuildPrefabKeyPrefix))
        {
            BuildObject buildObject = instance.GetComponent<BuildObject>();
            if (buildObject != null)
            {
                buildObject.Put();
            }
        }

        // Do not fall through the terrain while its chunk is generated
        Rigidbody body = instance.GetComponent<Rigidbody>();
        if (body != null && ChunkManager.Instance != null)
        {
            ChunkManager.Instance.HoldUntilGenerated(body);
        }
    }

    private GameObject GetRuntimePrefab(ObjectSaveData data)
    {
        if (data.prefabKey.StartsWith(ItemPrefabKeyPrefix))
        {
            if (int.TryParse(data.prefabKey.Substring(ItemPrefabKeyPrefix.Length), out int itemId))
            {
                return GetItem(itemId)?.Model;
            }

            return null;
        }

        if (data.prefabKey.StartsWith(BuildPrefabKeyPrefix))
        {
            string prefabName = data.prefabKey.Substring(BuildPrefabKeyPrefix.Length);

            foreach (GameObject prefab in buildPrefabs)
            {
                if (prefab != null && prefab.name == prefabName)
                {
                    return prefab;
                }
            }
        }

        return null;
    }

    private CameraMovement GetCameraMovement()
    {
        if (CameraManager.Instance == null || CameraManager.Instance.Camera == null) return null;

        return CameraManager.Instance.Camera.GetComponent<CameraMovement>();
    }

    // ---------- Files ----------

    // Save of the first version (a single save.json): becomes one of the saves of the list
    private void MigrateSingleSave()
    {
        string oldPath = Path.Combine(Application.persistentDataPath, "save.json");

        if (!File.Exists(oldPath)) return;

        SaveData data = ReadFile(oldPath);

        if (data != null)
        {
            if (string.IsNullOrEmpty(data.saveName))
            {
                data.saveName = "Partie sauvegardée";
            }

            WriteFile(GetSlotPath(Guid.NewGuid().ToString("N")), data);
        }

        File.Delete(oldPath);
    }

    private SaveData ReadFile(string path)
    {
        if (!File.Exists(path)) return null;

        try
        {
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
        }
        catch (Exception exception)
        {
            Debug.LogError("Save: cannot read " + path + ": " + exception.Message);
            return null;
        }
    }

    private void WriteFile(string path, SaveData data)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // Write to a temporary file first, so that a crash while writing does not corrupt the save
            string temporaryPath = path + ".tmp";
            File.WriteAllText(temporaryPath, JsonUtility.ToJson(data));

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.Move(temporaryPath, path);
        }
        catch (Exception exception)
        {
            Debug.LogError("Save: cannot write " + path + ": " + exception.Message);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Refresh item list")]
    private void RefreshItemList()
    {
        items.Clear();

        foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:ItemData"))
        {
            ItemData item = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));

            if (item != null)
            {
                items.Add(item);
            }
        }

        UnityEditor.EditorUtility.SetDirty(this);
    }

    [ContextMenu("Open saves folder")]
    private void OpenSavesFolder()
    {
        Directory.CreateDirectory(SavesFolder);
        UnityEditor.EditorUtility.RevealInFinder(SavesFolder);
    }
#endif
}

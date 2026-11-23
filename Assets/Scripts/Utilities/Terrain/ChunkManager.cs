using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance { get; private set; }

    [SerializeField] GameObject chunkPrefab;
    [SerializeField] int chunkSize = 64;
    [SerializeField] int generationDistance = 2;
    // Deactivated chunks farther than generationDistance + destroyMargin are destroyed (see DestroyFarChunks)
    [SerializeField] int destroyMargin = 2;
    Vector2Int? lastPlayerChunk;
    // Seed of a new world: a saved world keeps the seed it was created with
    [SerializeField] int seed = 0;
    int worldSeed;
    public int Seed => worldSeed;
    [SerializeField] Transform[] toReplaceConfig = new Transform[0];
    // Shape of the terrain (relief and ground), tuned with Tools > Terrain Preview. Empty = Resources/Terrain/TerrainShape
    [SerializeField] TerrainShapeSettings shapeSettings;

    Transform playerTransform;
    // Built from the settings and the world seed: gives the generated height of any point, loaded or not
    TerrainShape shape;

    // Terrain flattenings (constructions): saved, and applied again to the chunks when they are generated
    readonly List<TerrainEditSaveData> terrainEdits = new List<TerrainEditSaveData>();
    public IReadOnlyList<TerrainEditSaveData> TerrainEdits => terrainEdits;

    // Chunks whose generation is finished
    readonly HashSet<Vector2Int> readyChunks = new HashSet<Vector2Int>();

    // Bodies kept kinematic until the chunk under them is generated (saved positions), with their previous isKinematic
    readonly List<KeyValuePair<Rigidbody, bool>> heldBodies = new List<KeyValuePair<Rigidbody, bool>>();

    Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>();
    Dictionary<Vector2Int, Terrain> terrains = new Dictionary<Vector2Int, Terrain>();

    HashSet<Vector2Int> loadedChunks = new HashSet<Vector2Int>();
    HashSet<Vector2Int> generatedChunks = new HashSet<Vector2Int>();

    public HashSet<Transform> ToReplace = new HashSet<Transform>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        // Setup objects
        SaveManager saveManager = SaveManager.Instance;
        worldSeed = saveManager != null && saveManager.HasSave ? saveManager.WorldSeed : seed;

        if (saveManager != null)
        {
            terrainEdits.AddRange(saveManager.SavedTerrainEdits);
        }

        if (shapeSettings == null)
        {
            shapeSettings = TerrainShapeSettings.LoadGameSettings();
        }
        shape = new TerrainShape(shapeSettings, unchecked((uint)worldSeed));

        foreach (Transform t in toReplaceConfig)
        {
            ToReplace.Add(t);
        }

        // The player is spawned by the PlayerManager on scene load (before Start), and may be spawned again later
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.ev_spawnPlayer.AddListener(OnSpawnPlayer);

            if (PlayerManager.Instance.Player != null)
            {
                OnSpawnPlayer(PlayerManager.Instance.Player);
            }
        }
        else
        {
            // Scene played without the Bootstrap: the player is placed in the scene
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            OnSpawnPlayer(player);
        }

        // Load chunks around player
        if (playerTransform != null)
        {
            LoadNeededChunks(new HashSet<Vector2Int>());
        }
    }

    private void OnDestroy()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.ev_spawnPlayer.RemoveListener(OnSpawnPlayer);
        }
    }

    private void OnSpawnPlayer(GameObject player)
    {
        if (playerTransform != null)
        {
            ToReplace.Remove(playerTransform);
        }

        playerTransform = player != null ? player.transform : null;

        if (playerTransform == null) return;

        // A saved position is kept (it can be on a capsule floor, above the terrain): only wait for the terrain.
        // Otherwise (spawn point), the player is placed on the terrain surface
        if (PlayerManager.Instance != null && PlayerManager.Instance.SpawnedFromSave)
        {
            HoldUntilGenerated(playerTransform.GetComponent<Rigidbody>());
        }
        else
        {
            ToReplace.Add(playerTransform);

            // Respawn (after death) on a chunk already generated: ToReplace only applies when a chunk is generated
            Vector2Int coord = GetChunkPositionFor(playerTransform.position);

            if (readyChunks.Contains(coord) && terrains.TryGetValue(coord, out Terrain terrain) && terrain != null)
            {
                ReplaceObject(terrain.terrainData, terrain.transform.position, playerTransform);
            }
        }
    }

    // Keeps the body kinematic until the chunk under it is generated, so that it does not fall through the terrain
    public void HoldUntilGenerated(Rigidbody body)
    {
        if (body == null || readyChunks.Contains(GetChunkPositionFor(body.position))) return;

        heldBodies.Add(new KeyValuePair<Rigidbody, bool>(body, body.isKinematic));
        body.isKinematic = true;
    }

    private void OnChunkReady(Vector2Int coord)
    {
        readyChunks.Add(coord);

        for (int i = heldBodies.Count - 1; i >= 0; i--)
        {
            Rigidbody body = heldBodies[i].Key;

            if (body == null)
            {
                heldBodies.RemoveAt(i);
                continue;
            }

            if (GetChunkPositionFor(body.position) == coord)
            {
                body.isKinematic = heldBodies[i].Value;
                heldBodies.RemoveAt(i);
            }
        }
    }

    // Called by TerrainGenerator when the heights of a chunk are generated, before ev_OnTerminateGeneration
    public void ApplyTerrainEdits(Terrain terrain)
    {
        foreach (TerrainEditSaveData edit in terrainEdits)
        {
            ApplyTerrainEdit(terrain, edit);
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        Vector2Int playerChunk = GetChunkPositionFor(playerTransform.position);

        // The needed chunks only change when the player enters another chunk
        if (lastPlayerChunk == playerChunk) return;

        lastPlayerChunk = playerChunk;

        HashSet<Vector2Int> neededChunks = new HashSet<Vector2Int>();

        LoadNeededChunks(neededChunks);
        UnloadNotNeededChunks(neededChunks);
        DestroyFarChunks(playerChunk);
    }

    // Chunks beyond generationDistance + destroyMargin are destroyed to free the memory (the closer ones are only
    // deactivated, so that going back and forth near a border does not generate them again). A destroyed chunk is
    // generated again identically when the player comes back: the terrain comes from the seed, the terrain edits are
    // replayed and its saveable objects keep their state in the SaveManager
    void DestroyFarChunks(Vector2Int playerChunk)
    {
        int maxDistance = generationDistance + Mathf.Max(0, destroyMargin);
        List<Vector2Int> farChunks = new List<Vector2Int>();

        foreach (Vector2Int coord in chunks.Keys)
        {
            if (Mathf.Max(Mathf.Abs(coord.x - playerChunk.x), Mathf.Abs(coord.y - playerChunk.y)) > maxDistance)
            {
                farChunks.Add(coord);
            }
        }

        foreach (Vector2Int coord in farChunks)
        {
            DestroyChunk(coord);
        }
    }

    void DestroyChunk(Vector2Int coord)
    {
        GameObject chunk = chunks[coord];

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.UnloadObjects(chunk);
        }

        // The TerrainData is a copy made by TerrainGenerator.Initialize: it is not destroyed with the GameObject
        if (terrains.TryGetValue(coord, out Terrain terrain) && terrain != null)
        {
            Destroy(terrain.terrainData);
        }

        Destroy(chunk);

        chunks.Remove(coord);
        terrains.Remove(coord);
        readyChunks.Remove(coord);
        generatedChunks.Remove(coord);

        // The objects outside the chunk hierarchy (dropped items...) stay loaded: keep them from falling while the
        // terrain under them is missing
        if (SaveManager.Instance != null)
        {
            foreach (SaveableObject saveable in SaveManager.Instance.LiveObjects)
            {
                if (saveable != null && GetChunkPositionFor(saveable.transform.position) == coord && saveable.TryGetComponent(out Rigidbody body))
                {
                    HoldUntilGenerated(body);
                }
            }
        }
    }

    // Height the terrain is generated with at a world position (m), loaded or not. The flattenings are not included
    public float GeneratedHeightAt(float x, float z)
    {
        return shape != null ? shape.Sample(x, z).height : 0f;
    }

    public Vector2Int GetChunkPositionFor(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / chunkSize),
            Mathf.FloorToInt(position.z / chunkSize)
        );
    }

    // ---------- Flattening under the constructions (TerrainFlattening) ----------

    // Saved and applied to the loaded chunks; replayed on the chunks generated later
    public void AddTerrainEdit(TerrainEditSaveData edit)
    {
        terrainEdits.Add(edit);

        foreach (Terrain terrain in GetChunksInRadius(edit.position, TerrainFlattening.Reach(edit)))
        {
            ApplyTerrainEdit(terrain, edit);
        }
    }

    // Inside the footprint (disc or tilted rectangle) the terrain is set to the target height (dug or filled), around it
    // it joins the natural terrain over "margin" meters. Only the samples within reach are read and written
    private void ApplyTerrainEdit(Terrain chunk, TerrainEditSaveData edit)
    {
        if (edit.shape == TerrainEditSaveData.Legacy) return;

        TerrainData data = chunk.terrainData;
        Vector3 chunkPos = chunk.transform.position;
        int resolution = data.heightmapResolution;
        float step = data.size.x / (resolution - 1);
        float reach = TerrainFlattening.Reach(edit);

        int xMin = Mathf.Clamp(Mathf.FloorToInt((edit.position.x - reach - chunkPos.x) / step), 0, resolution - 1);
        int xMax = Mathf.Clamp(Mathf.CeilToInt((edit.position.x + reach - chunkPos.x) / step), 0, resolution - 1);
        int zMin = Mathf.Clamp(Mathf.FloorToInt((edit.position.z - reach - chunkPos.z) / step), 0, resolution - 1);
        int zMax = Mathf.Clamp(Mathf.CeilToInt((edit.position.z + reach - chunkPos.z) / step), 0, resolution - 1);

        if (xMin >= xMax || zMin >= zMax) return;

        int width = xMax - xMin + 1;
        int depth = zMax - zMin + 1;
        float[,] heights = data.GetHeights(xMin, zMin, width, depth);
        bool changed = false;

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 world = new Vector3(chunkPos.x + (xMin + x) * step, 0f, chunkPos.z + (zMin + z) * step);

                if (!TerrainFlattening.Evaluate(edit, world, out float target, out float weight)) continue;

                float normalizedTarget = (target - chunkPos.y) / data.size.y;
                heights[z, x] = Mathf.Lerp(heights[z, x], normalizedTarget, weight);
                changed = true;
            }
        }

        if (changed) data.SetHeights(xMin, zMin, heights);
    }

    public List<Terrain> GetChunksInRadius(Vector3 position, float radius)
    {
        List<Terrain> chunks = new List<Terrain>();

        int chunkRadius = Mathf.CeilToInt(radius / chunkSize);

        Vector2Int centerChunkPos = GetChunkPositionFor(position);
        
        for(int x = -chunkRadius; x <= chunkRadius; x++)
        {
            for(int z = -chunkRadius; z <= chunkRadius; z++)
            {
                Vector2Int coord = new Vector2Int(
                    centerChunkPos.x + x,
                    centerChunkPos.y + z
                );

                if (!this.chunks.ContainsKey(coord)) continue;

                GameObject chunk = this.chunks[coord];
                if (!chunk.activeSelf) continue;

                Terrain chunkTerrain = chunk.GetComponent<Terrain>();

                if(chunkTerrain != null)
                {
                    chunks.Add(chunkTerrain);
                }
            }
        }

        return chunks;
    }

    void ReplaceObject(TerrainData terrainData, Vector3 chunkPosition, Transform objectToReplace)
    {
        Vector3 localPosition = objectToReplace.position - chunkPosition;
        float height = chunkPosition.y + terrainData.GetInterpolatedHeight(
            localPosition.x / terrainData.size.x,
            localPosition.z / terrainData.size.z
        );

        Vector3 position = new Vector3(objectToReplace.position.x, height + 0.7f, objectToReplace.position.z);

        // An interpolated rigidbody ignores direct transform changes, so it must be moved through the rigidbody
        Rigidbody rigidbody = objectToReplace.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.position = position;
            rigidbody.linearVelocity = Vector3.zero;
        }

        objectToReplace.position = position;
    }

    void LoadNeededChunks(HashSet<Vector2Int> neededChunks)
    {
        Vector2Int playerChunk = GetChunkPositionFor(playerTransform.position);

        for (int x = -generationDistance; x <= generationDistance; x++)
        {
            for (int z = -generationDistance; z <= generationDistance; z++)
            {
                Vector2Int coord = new Vector2Int(
                    playerChunk.x + x,
                    playerChunk.y + z
                );

                neededChunks.Add(coord);

                if (!loadedChunks.Contains(coord))
                {
                    LoadChunk(coord);
                    generatedChunks.Add(coord);
                }
            }
        }
    }

    void UnloadNotNeededChunks(HashSet<Vector2Int> neededChunks)
    {
        foreach (var chunk in loadedChunks)
        {
            if (!neededChunks.Contains(chunk))
            {
                UnloadChunk(chunk);
            }
        }

        loadedChunks = neededChunks;
    }


    void LoadChunk(Vector2Int coord)
    {
        if(chunks.ContainsKey(coord))
        {
            GameObject chunk = chunks[coord];

            if(!chunk.activeSelf)
            {
                chunk.SetActive(true);
            }

            return;
        }

        Vector3 position = new Vector3(
            coord.x * chunkSize,
            0,
            coord.y * chunkSize
        );

        GameObject newChunk = Instantiate(chunkPrefab, position, Quaternion.identity);
        TerrainGenerator chunkTerrainGenerator = newChunk.GetComponent<TerrainGenerator>();


        foreach(Transform objectToReplace in ToReplace)
        {
            if (GetChunkPositionFor(objectToReplace.position) == coord)
            {
                UnityAction<TerrainData> callback = null;

                callback = (TerrainData terrainData) =>
                {
                    ReplaceObject(terrainData, position, objectToReplace);
                    chunkTerrainGenerator.ev_OnTerminateGeneration.RemoveListener(callback);
                };

                chunkTerrainGenerator.ev_OnTerminateGeneration.AddListener(callback);
            }
        }

        // After the objects to replace (listeners are called in order): a held body is released on the final terrain
        chunkTerrainGenerator.ev_OnTerminateGeneration.AddListener(terrainData => OnChunkReady(coord));

        chunkTerrainGenerator.Initialize(coord, shape, chunkSize);

        // Neighbors are managed in LateUpdate: Unity's auto connection must not override them
        Terrain terrain = newChunk.GetComponent<Terrain>();
        terrain.allowAutoConnect = false;

        chunks.Add(coord, newChunk);
        terrains.Add(coord, terrain);
    }

    void UnloadChunk(Vector2Int coord)
    {
        if (!chunks.ContainsKey(coord)) return;

        GameObject chunk = chunks[coord];

        chunk.SetActive(false);
    }

    // The neighbors let the terrain LODs match at the borders (no cracks between chunks seen from far away).
    // They are checked every frame because Unity may reset them (terrain connectivity is rebuilt when a terrain is
    // enabled or disabled): SetNeighbors is only called when they are wrong.
    void LateUpdate()
    {
        foreach (Vector2Int coord in loadedChunks)
        {
            Terrain terrain = GetActiveTerrain(coord);

            if (terrain == null) continue;

            // Chunk coordinates are (x, z): "top" is +z
            Terrain left = GetActiveTerrain(coord + Vector2Int.left);
            Terrain top = GetActiveTerrain(coord + Vector2Int.up);
            Terrain right = GetActiveTerrain(coord + Vector2Int.right);
            Terrain bottom = GetActiveTerrain(coord + Vector2Int.down);

            if (terrain.leftNeighbor != left || terrain.topNeighbor != top || terrain.rightNeighbor != right || terrain.bottomNeighbor != bottom)
            {
                terrain.SetNeighbors(left, top, right, bottom);
            }
        }
    }

    Terrain GetActiveTerrain(Vector2Int coord)
    {
        if (!terrains.TryGetValue(coord, out Terrain terrain) || !terrain.gameObject.activeSelf) return null;

        return terrain;
    }


    void OnDrawGizmos()
    {
        if (playerTransform == null) return;

        Vector2Int playerChunk = new Vector2Int(
            Mathf.FloorToInt(playerTransform.position.x / chunkSize),
            Mathf.FloorToInt(playerTransform.position.z / chunkSize)
        );

        int debugRadius = generationDistance + 100; // un peu plus large pour voir autour

        for (int x = -debugRadius; x <= debugRadius; x++)
        {
            for (int z = -debugRadius; z <= debugRadius; z++)
            {
                Vector2Int coord = new Vector2Int(
                    playerChunk.x + x,
                    playerChunk.y + z
                );

                bool isLoaded = loadedChunks != null && loadedChunks.Contains(coord);
                bool isGenerated = generatedChunks != null && generatedChunks.Contains(coord);

 
                if (!isGenerated) continue;

                Vector3 worldPos = new Vector3(
                    coord.x * chunkSize,
                    0,
                    coord.y * chunkSize
                );

                Gizmos.color = isLoaded ? Color.green : Color.red;

                Gizmos.DrawWireCube(
                    worldPos + new Vector3(chunkSize / 2f, 0, chunkSize / 2f),
                    new Vector3(chunkSize, 1, chunkSize)
                );
            }
        }
    }
}

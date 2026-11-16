using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance { get; private set; }

    [SerializeField] GameObject chunkPrefab;
    [SerializeField] int chunkSize = 64;
    [SerializeField] int generationDistance = 2;
    [SerializeField] int seed = 0;
    public int Seed => seed;
    [SerializeField] Transform[] toReplaceConfig = new Transform[0];

    Transform playerTransform;
    Vector2 perlinOffset;

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
        Random.InitState(seed);

        perlinOffset = new Vector2(Random.value * 999999, Random.value * 999999);

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

        if (playerTransform != null)
        {
            ToReplace.Add(playerTransform);
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        HashSet<Vector2Int> neededChunks = new HashSet<Vector2Int>();

        LoadNeededChunks(neededChunks);
        UnloadNotNeededChunks(neededChunks);
    }

    public Vector2Int GetChunkPositionFor(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / chunkSize),
            Mathf.FloorToInt(position.z / chunkSize)
        );
    }

    public void FlattenTerrainAt(Vector3 worldPos, float radius, float targetHeight)
    {
        List<Terrain> terrains = GetChunksInRadius(worldPos, radius);

        foreach (Terrain terrain in terrains)
        {
            FlattenOnChunk(terrain, worldPos, radius, targetHeight);
        }
    }

    private void FlattenOnChunk(Terrain chunk, Vector3 worldPos, float flatRadius, float targetHeight)
    {
        TerrainData data = chunk.terrainData;
        Vector3 chunkPos = chunk.transform.position;

        int resolution = data.heightmapResolution;

        float[,] heights = data.GetHeights(0, 0, resolution, resolution);

        // Conversion monde => heightmap
        float normalizedTargetHeight = (targetHeight - chunkPos.y) / data.size.y;

        // Idéalement expose ça en paramètre
        float toleranceWorld = 4f;
        float tolerance = toleranceWorld / data.size.y;

        float radius = flatRadius * 1.3f;

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float worldX = chunkPos.x + (x / (float)(resolution - 1)) * data.size.x;
                float worldZ = chunkPos.z + (z / (float)(resolution - 1)) * data.size.z;

                float dist = Vector2.Distance(
                    new Vector2(worldX, worldZ),
                    new Vector2(worldPos.x, worldPos.z)
                );

                if (dist > radius) continue;

                // -------------------------
                // 1. Falloff radial (plateau + pente)
                // -------------------------
                float falloff;

                if (dist <= flatRadius)
                {
                    falloff = 1f;
                }
                else
                {
                    float t = (dist - flatRadius) / (radius - flatRadius);
                    t = Mathf.Pow(t, 2f); // douceur de la pente
                    falloff = Mathf.Lerp(1f, 0f, t);
                }

                // -------------------------
                // 2. Falloff vertical (tolérance douce)
                // -------------------------
                float currentHeight = heights[z, x];

                float delta = currentHeight - (normalizedTargetHeight - tolerance);

                float toleranceFactor = Mathf.Clamp01(delta / tolerance);

                // adoucissement
                toleranceFactor = Mathf.Pow(toleranceFactor, 2f);
                // alternative :
                // toleranceFactor = Mathf.SmoothStep(0f, 1f, toleranceFactor);

                // -------------------------
                // 3. Combinaison des deux
                // -------------------------
                float finalFalloff = falloff * toleranceFactor;

                heights[z, x] = Mathf.Lerp(currentHeight, normalizedTargetHeight, finalFalloff);
            }
        }

        data.SetHeights(0, 0, heights);
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


        chunkTerrainGenerator.Initialize(coord, perlinOffset, chunkSize);

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

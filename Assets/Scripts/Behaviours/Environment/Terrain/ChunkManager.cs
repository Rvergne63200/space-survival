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
    [SerializeField] Transform[] toReplaceConfig = new Transform[0];

    Transform playerTransform;
    Vector2 perlinOffset;

    Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>();

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
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        ToReplace.Add(playerTransform);
        foreach (Transform t in toReplaceConfig)
        {
            ToReplace.Add(t);
        }

        // Load chunks around player
        LoadNeededChunks(new HashSet<Vector2Int>());
    }

    void Update()
    {
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

        targetHeight = (targetHeight - chunkPos.y) / data.size.y;

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

                float radius = flatRadius * 1.3f;

                if (dist > radius) continue;

                float falloff;

                if(dist <= flatRadius)
                {
                    falloff = 1f;
                }
                else
                {
                    float t = (dist - flatRadius) / (radius - flatRadius);

                    t = Mathf.Pow(t, 2f);

                    falloff = Mathf.Lerp(1f, 0f, t);
                }

                heights[z, x] = Mathf.Lerp(heights[z, x], targetHeight, falloff);
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
        float height = terrainData.GetHeight((int)(chunkPosition.x - objectToReplace.position.x), (int)(chunkPosition.z - objectToReplace.position.z));
        objectToReplace.position = new Vector3(objectToReplace.position.x, height + 0.7f, objectToReplace.position.z);
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


        chunks.Add(coord, newChunk);
    }

    void UnloadChunk(Vector2Int coord)
    {
        if (!chunks.ContainsKey(coord)) return;

        GameObject chunk = chunks[coord];

        chunk.SetActive(false);
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

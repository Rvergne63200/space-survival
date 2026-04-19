using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChunkManager : MonoBehaviour
{
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

    Vector2Int GetChunkFor(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / chunkSize),
            Mathf.FloorToInt(position.z / chunkSize)
        );
    }

    void ReplaceObject(TerrainData terrainData, Vector3 chunkPosition, Transform objectToReplace)
    {
        float height = terrainData.GetHeight((int)(chunkPosition.x - objectToReplace.position.x), (int)(chunkPosition.z - objectToReplace.position.z));
        objectToReplace.position = new Vector3(objectToReplace.position.x, height + 0.7f, objectToReplace.position.z);
    }

    void LoadNeededChunks(HashSet<Vector2Int> neededChunks)
    {
        Vector2Int playerChunk = GetChunkFor(playerTransform.position);

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
            if (GetChunkFor(objectToReplace.position) == coord)
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

    /*
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
    */
}

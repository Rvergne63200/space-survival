using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private int chunkSize = 64;
    [SerializeField] private int generationDistance = 2;

    private HashSet<Vector2Int> loadedChunks = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> generatedChunks = new HashSet<Vector2Int>();

    private Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>(); 

    private Transform playerTransform = null;

    private Vector2 perlinOffset;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        perlinOffset = new Vector2(0, 0); //new Vector2(Random.Range(0, 99999.99f), Random.Range(0, 99999.99f));
    }

    void Update()
    {
        Vector2Int playerChunk = new Vector2Int(
            Mathf.FloorToInt(playerTransform.position.x / chunkSize), 
            Mathf.FloorToInt(playerTransform.position.z / chunkSize)
        );

        HashSet<Vector2Int> neededChunks = new HashSet<Vector2Int>();

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

        foreach (var chunk in loadedChunks)
        {
            if (!neededChunks.Contains(chunk))
            {
                UnloadChunk(chunk);
            }
        }

        loadedChunks = neededChunks;
    }


    private void LoadChunk(Vector2Int coord)
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

        chunkTerrainGenerator.Initialize(coord, perlinOffset, chunkSize);

        chunks.Add(coord, newChunk);
    }

    private void UnloadChunk(Vector2Int coord)
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

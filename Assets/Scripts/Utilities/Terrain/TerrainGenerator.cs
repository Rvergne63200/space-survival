using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Generates the terrain of a chunk (on the Chunk prefab, created by ChunkManager): computes its heights from the
/// TerrainShape in the background, applies them, replays the saved flattenings, then raises ev_OnTerminateGeneration.
/// </summary>
public class TerrainGenerator : MonoBehaviour
{
    public UnityEvent<TerrainData> ev_OnTerminateGeneration;

    [Tooltip("Size of the chunk (m), set by ChunkManager")]
    public int size = 64;
    [Tooltip("Height range of the terrain (m): the generated heights must stay between 0 and this value")]
    public float maxHeight = 1024;

    // Ground zones: each terrain layer of the TerrainData is a ground zone (Ground_Rust, Ground_Ochre...). The zones
    // are painted from a world-space noise (one noise per layer, the strongest wins with soft transitions), so they
    // are continuous between the chunks and the same at every generation.
    [Header("Ground zones")]
    [Tooltip("Typical size of a ground zone (m)")]
    [SerializeField] private float zoneSize = 400f;
    [Tooltip("Sharpness of the transitions between zones (higher = narrower transitions)")]
    [SerializeField] private float zoneSharpness = 8f;
    [Tooltip("Weight of each zone, in the order of the terrain layers (missing = 1). Higher = more frequent")]
    [SerializeField] private float[] zoneWeights = { 1.25f, 1f, 0.92f, 0.85f };
    [Tooltip("Texels per side of the zone map of a chunk (power of two, the zones are large)")]
    [SerializeField] private int zoneResolution = 64;

    private Terrain terrain;
    private TerrainShape shape;
    // World position (X, Z) of the south-west corner of the chunk
    private Vector2 origin;

    public void Initialize(Vector2Int coord, TerrainShape shape, int size)
    {
        terrain = GetComponent<Terrain>();
        terrain.terrainData = Instantiate(terrain.terrainData);
        terrain.GetComponent<TerrainCollider>().terrainData = terrain.terrainData;

        // Used by the "Terrain/World UV Standard" shader to position the textures from the world position
        Shader.SetGlobalVector("_WorldUVTerrainSize", new Vector4(size, size, 0f, 0f));

        this.size = size;
        this.shape = shape;
        origin = new Vector2(coord.x * size, coord.y * size);

        Generate();
    }

    async void Generate()
    {
        TerrainData data = terrain.terrainData;

        data.heightmapResolution = size + 1;
        data.size = new Vector3(size, maxHeight, size);

        int layerCount = data.terrainLayers.Length;
        if (layerCount > 1)
        {
            data.alphamapResolution = zoneResolution;
        }
        int zoneRes = data.alphamapResolution;

        float[,] heights = await Task.Run(() => GenerateHeights());
        float[,,] zones = layerCount > 1 ? await Task.Run(() => GenerateZones(layerCount, zoneRes)) : null;

        if (this == null) return;

        data.SetHeights(0, 0, heights);
        if (zones != null)
        {
            data.SetAlphamaps(0, 0, zones);
        }

        // Saved terrain flattenings (constructions), before the objects are placed on the terrain
        if (ChunkManager.Instance != null)
        {
            ChunkManager.Instance.ApplyTerrainEdits(terrain);
        }

        ev_OnTerminateGeneration.Invoke(data);
    }

    private float[,] GenerateHeights()
    {
        float[,] heights = new float[size + 1, size + 1];

        for (int x = 0; x <= size; x++)
        {
            for (int y = 0; y <= size; y++)
            {
                heights[y, x] = Mathf.Clamp01(shape.Sample(origin.x + x, origin.y + y).height / maxHeight);
            }
        }

        return heights;
    }

    // Weights of the terrain layers (zone map) of the chunk. Texel (x, y) is at x / (res - 1) of the chunk, like
    // the alphamap of Unity, so the edges of two neighbour chunks get the same values.
    private float[,,] GenerateZones(int layerCount, int res)
    {
        float[,,] map = new float[res, res, layerCount];
        float[] w = new float[layerCount];
        float step = size / (float)(res - 1);

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float worldX = origin.x + x * step;
                float worldY = origin.y + y * step;
                float total = 0f;
                for (int k = 0; k < layerCount; k++)
                {
                    w[k] = Mathf.Pow(ZoneNoise(worldX, worldY, k) * ZoneWeight(k), zoneSharpness);
                    total += w[k];
                }
                for (int k = 0; k < layerCount; k++)
                {
                    map[y, x, k] = w[k] / total;
                }
            }
        }

        return map;
    }

    // Noise of the zone of a layer (0..1): its own large noise, plus a smaller one for less regular edges
    private float ZoneNoise(float worldX, float worldY, int layer)
    {
        float x = (worldX + 3000f + 1733f * layer) / zoneSize;
        float y = (worldY + 5000f + 2311f * layer) / zoneSize;
        return Mathf.PerlinNoise(x, y) * 0.85f + Mathf.PerlinNoise(x * 2.3f, y * 2.3f) * 0.15f;
    }

    private float ZoneWeight(int layer)
    {
        return zoneWeights != null && layer < zoneWeights.Length ? zoneWeights[layer] : 1f;
    }
}

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class TerrainGenerator : MonoBehaviour
{
    public UnityEvent<TerrainData> ev_OnTerminateGeneration;

    public int size = 64;
    public float height = 50f;

    public float perlinX = 1f;
    public float perlinY = 1f;

    public float perlinScale = 25f;
    public float maxHeight = 1024;

    public Vector2 perlinOffset = new Vector2(0, 0);

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

    public void Initialize(Vector2 position, Vector2 perlinOffset, int size)
    {
        terrain = GetComponent<Terrain>();
        terrain.terrainData = Instantiate(terrain.terrainData);
        terrain.GetComponent<TerrainCollider>().terrainData = terrain.terrainData;

        // Used by the "Terrain/World UV Standard" shader to position the textures from the world position
        Shader.SetGlobalVector("_WorldUVTerrainSize", new Vector4(size, size, 0f, 0f));

        this.size = size;
        this.perlinOffset = new Vector2(
            position.x * size,
            position.y * size
        ) + perlinOffset;

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
                heights[y, x] = HeightAt(perlinOffset.x + x, perlinOffset.y + y) / maxHeight;
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
                float worldX = perlinOffset.x + x * step;
                float worldY = perlinOffset.y + y * step;
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

    // Generated height (m) at a point of the noise: the world position plus the offset of the world (ChunkManager).
    // Also used by ChunkManager.GeneratedHeightAt for the areas that are not loaded
    public float HeightAt(float worldX, float worldY)
    {
        float xCoord = worldX / perlinX;
        float yCoord = worldY / perlinY;

        float baseHeigth = height;
        float localVariation = Mathf.PerlinNoise(xCoord * 0.01f, yCoord * 0.01f) * perlinScale;
        float globalVariation = Mathf.PerlinNoise(xCoord * 0.005f, yCoord * 0.005f) * 3 * perlinScale;

        float hollowVariation = Mathf.PerlinNoise(xCoord * 0.001f, yCoord * 0.001f);
        hollowVariation = Mathf.Clamp(hollowVariation - 0.7f, 0f, 1f);
        hollowVariation *= 35 * perlinScale;

        float mountainsVariation = Mathf.PerlinNoise((xCoord + 1024/perlinX) * 0.001f, (yCoord + 1024/perlinY) * 0.001f);
        mountainsVariation = Mathf.Clamp(mountainsVariation - 0.5f, 0f, 1f);
        mountainsVariation *= 40 * Mathf.Exp(mountainsVariation) * perlinScale;

        return baseHeigth + localVariation + globalVariation + mountainsVariation - hollowVariation;
    }
}

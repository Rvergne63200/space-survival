using System.Collections;
using System.ComponentModel;
using System.Drawing;
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

    private Terrain terrain;

    public void Initialize(Vector2 position, Vector2 perlinOffset, int size)
    {
        terrain = GetComponent<Terrain>();
        terrain.terrainData = Instantiate(terrain.terrainData);
        terrain.GetComponent<TerrainCollider>().terrainData = terrain.terrainData;

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

        float[,] heights = await Task.Run(() => GenerateHeights());

        if (this == null) return;

        data.SetHeights(0, 0, heights);

        ev_OnTerminateGeneration.Invoke(data);
    }

    private float[,] GenerateHeights()
    {
        float[,] heights = new float[size + 1, size + 1];

        for (int x = 0; x <= size; x++)
        {
            for (int y = 0; y <= size; y++)
            {
                float worldX = perlinOffset.x + x;
                float worldY = perlinOffset.y + y;

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

                heights[y, x] = (baseHeigth + localVariation + globalVariation + mountainsVariation - hollowVariation) / maxHeight; 
            }
        }

        return heights;
    }
}

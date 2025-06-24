using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public Vector2 size = new Vector2(0,0);
    public float height = 50f;

    public float perlinX = 1f;
    public float perlinY = 1f;

    public float perlinScale = 25f;
    public float maxHeight = 1024;

    public Vector2 perlinOffset = new Vector2(256.12f, 546584.4f);


    private Terrain terrain;

    private void Start()
    {
        perlinOffset = new Vector2(Random.Range(0, 99999.99f), Random.Range(0, 99999.99f));

        terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrainData(terrain.terrainData);
    }

    private TerrainData GenerateTerrainData(TerrainData terrainData)
    {
        terrainData.heightmapResolution = (int)size.x + 1;
        terrainData.size = new Vector3(size.x, maxHeight, size.y);
        terrainData.SetHeights(0, 0, GenerateHeights(perlinX, perlinY, perlinOffset.x, perlinOffset.y));
        return terrainData;
    }

    private float[,] GenerateHeights(float perlinX, float perlinY, float offsetX, float offsetY)
    {
        float[,] heights = new float[(int)size.x, (int)size.y];

        for(int x = 0; x < size.x; x++)
        {
            for(int y = 0; y < size.y; y++)
            {
                float xCoord = ((float)x + offsetX) / perlinX;
                float yCoord = ((float)y + offsetY) / perlinY;

                float baseHeigth = height;
                float localVariation = Mathf.PerlinNoise(xCoord * 0.01f, yCoord * 0.01f) * perlinScale;
                float globalVariation = Mathf.PerlinNoise(xCoord * 0.005f, yCoord * 0.005f) * 3 * perlinScale;

                float hollowVariation = Mathf.PerlinNoise(xCoord * 0.001f, yCoord * 0.001f);
                hollowVariation = Mathf.Clamp(hollowVariation - 0.7f, 0f, 1f);
                hollowVariation *= 35 * perlinScale;

                float mountainsVariation = Mathf.PerlinNoise((xCoord + 1024/perlinX) * 0.001f, (yCoord + 1024/perlinY) * 0.001f);
                mountainsVariation = Mathf.Clamp(mountainsVariation - 0.5f, 0f, 1f);
                mountainsVariation *= 40 * Mathf.Exp(mountainsVariation) * perlinScale;

                heights[x, y] = (baseHeigth + localVariation + globalVariation + mountainsVariation - hollowVariation) / maxHeight; 
            }
        }

        return heights;
    }

    private void Update()
    {
        //terrain.terrainData = GenerateTerrainData(terrain.terrainData);
    }
}

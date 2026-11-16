using UnityEngine;

public class EnvironmentSpawner : MonoBehaviour
{
    public GameObject prefab;

    // Called by TerrainGenerator.ev_OnTerminateGeneration (wired in the Chunk prefab), once the heights are generated
    public void Spawn(TerrainData terrainData)
    {
        Vector3 size = terrainData.size;

        // Seeded by the world seed and the chunk position: a chunk always gets the same objects, whatever the loading order
        int chunkX = Mathf.RoundToInt(transform.position.x);
        int chunkZ = Mathf.RoundToInt(transform.position.z);
        int worldSeed = ChunkManager.Instance != null ? ChunkManager.Instance.Seed : 0;
        System.Random random = new System.Random(unchecked(worldSeed * 73856093 ^ chunkX * 19349663 ^ chunkZ * 83492791));

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.z; y++)
            {
                // 0.1 % chance per square meter
                if (random.NextDouble() >= 0.001)
                {
                    continue;
                }

                float height = terrainData.GetHeight(x, y);

                GameObject spawned = Instantiate(prefab, new Vector3(transform.position.x + x, height + 0.2f, transform.position.z + y), RandomRotation(random), transform);
                spawned.transform.localScale = Vector3.one * Mathf.Lerp(1.20f, 1.80f, (float)random.NextDouble()) * 0.1f;
            }
        }
    }

    // Uniform random rotation (same distribution as UnityEngine.Random.rotation)
    private static Quaternion RandomRotation(System.Random random)
    {
        double u1 = random.NextDouble();
        double u2 = random.NextDouble() * 2.0 * System.Math.PI;
        double u3 = random.NextDouble() * 2.0 * System.Math.PI;

        double a = System.Math.Sqrt(1.0 - u1);
        double b = System.Math.Sqrt(u1);

        return new Quaternion((float)(a * System.Math.Sin(u2)), (float)(a * System.Math.Cos(u2)), (float)(b * System.Math.Sin(u3)), (float)(b * System.Math.Cos(u3)));
    }
}

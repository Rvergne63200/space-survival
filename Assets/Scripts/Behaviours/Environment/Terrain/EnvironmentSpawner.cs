using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentSpawner : MonoBehaviour
{
    public GameObject prefab;

    public void Start()
    {
        Spawn(gameObject.GetComponent<Terrain>().terrainData);
    }

    public void Spawn(TerrainData terrainData)
    {
        Vector3 size = terrainData.size;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.z; y++)
            {
                float height = terrainData.GetHeight(x, y);
                float random = Random.Range(0f, 1000f);

                if (random >= 999f)
                {
                    GameObject spawned = Instantiate(prefab, new Vector3(transform.position.x + x, height + 0.2f, transform.position.z + y), Random.rotation, transform);
                    spawned.transform.localScale = Vector3.one * Random.Range(1.80f, 1.20f) * 0.1f;
                }
            }
        }
    }
}

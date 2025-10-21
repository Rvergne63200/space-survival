using UnityEngine;

public class TerrainRepositioner : MonoBehaviour
{
    public ObjectPlacement[] placements;

    public void Start()
    {
        TerrainData data = gameObject.GetComponent<Terrain>().terrainData;

        ReplaceObjects(data);
        GameObject.FindGameObjectWithTag("Player").transform.position = new Vector3(1024, data.GetHeight(1024, 1024) + 5, 1024);
    }

    public void ReplaceObjects(TerrainData terrainData)
    {
        foreach (ObjectPlacement placement in placements)
        {
            Vector2 position = placement.position;
            float terrainHeight = terrainData.GetHeight((int) position.x, (int) position.y);
            placement.gameObject.transform.position = new Vector3(placement.position.x, placement.height + terrainHeight, placement.position.y);
        }
    }
}

/// <summary>Height of the terrain at a point and what its ground is made of, used for the ground layers.</summary>
public struct TerrainSample
{
    public float height;    // metres
    public float sand;      // 0..1 dunes and sand basins
    public float sediment;  // 0..1 dry lake floors and river beds
    public float rock;      // 0..1 cliffs, canyon walls, crater rims and ejecta
}

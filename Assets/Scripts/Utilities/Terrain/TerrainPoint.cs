/// <summary>
/// A terrain point while the landscape features are applied to it in turn (TerrainShape): its position, the height
/// being built, and the masks that the earlier features leave for the later ones (a dune is not on a plateau…).
/// </summary>
public struct TerrainPoint
{
    // World position (metres)
    public float x, z;
    // Position distorted at the large scales (TerrainRegions), so that the big shapes are never straight
    public float warpedX, warpedZ;
    // Large scale region value, about -1..1 : low = sand basins, high = plateaus
    public float region;
    // Height being built (metres)
    public float height;

    // Masks 0..1 left by the features
    public float plateau;   // on a plateau (0 in the canyons)
    public float basin;     // in a sand basin
    public float sand;      // ground of sand (dunes)
    public float sediment;  // ground of sediment (dry lake floors, river beds)
    public float rock;      // bare rock (cliffs, canyon walls, crater rims)

    public TerrainSample ToSample() => new TerrainSample { height = height, sand = sand, sediment = sediment, rock = rock };
}

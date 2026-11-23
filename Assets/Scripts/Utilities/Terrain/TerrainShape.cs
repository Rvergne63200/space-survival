/// <summary>
/// Shape of the planet surface: the landscape features applied in turn to a point. A pure function of the world
/// position, the seed and the settings (thread-safe, deterministic), so every chunk, the map and the areas that are
/// not loaded agree without saving anything.
/// </summary>
public class TerrainShape
{
    private readonly ITerrainFeature[] _features;

    public TerrainShape(TerrainShapeSettings settings, uint seed)
    {
        // The order matters : each feature reads the masks left by the previous ones
        _features = new ITerrainFeature[]
        {
            new TerrainRegions(settings.Regions, seed),
            new TerrainPlains(settings.Plains, seed),
            new TerrainPlateaus(settings.Plateaus, seed),
            new TerrainDunes(settings.Dunes, seed),
            new TerrainDryLakes(settings.DryLakes, settings.Plains.BaseHeight, seed),
            new TerrainDryRivers(settings.DryRivers, seed),
            new TerrainCraters(settings.Craters, seed),
        };
    }

    public TerrainSample Sample(float x, float z)
    {
        var p = new TerrainPoint { x = x, z = z };
        foreach (ITerrainFeature feature in _features)
            feature.Apply(ref p);
        return p.ToSample();
    }
}

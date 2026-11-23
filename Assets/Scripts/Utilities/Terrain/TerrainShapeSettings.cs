using UnityEngine;

/// <summary>
/// Settings of the shape of the planet surface (TerrainShape), one section per landscape feature.
/// Distances and heights in metres. Tune them with the previews of Tools > Terrain Preview.
/// The settings of the game are the asset Resources/Terrain/TerrainShape (loaded by ChunkManager).
/// </summary>
[CreateAssetMenu(menuName = "Environment/Terrain Shape Settings")]
public class TerrainShapeSettings : ScriptableObject
{
    // Path of the settings of the game in Resources
    public const string ResourcePath = "Terrain/TerrainShape";

    /// <summary>The settings of the game, or the default settings (with a warning) if the asset is missing.</summary>
    public static TerrainShapeSettings LoadGameSettings()
    {
        var settings = Resources.Load<TerrainShapeSettings>(ResourcePath);
        if (settings != null) return settings;

        Debug.LogWarning($"TerrainShapeSettings: Resources/{ResourcePath} not found, the default settings are used");
        return CreateInstance<TerrainShapeSettings>();
    }

    [SerializeField] private RegionsSettings _regions = new RegionsSettings();
    [SerializeField] private PlainsSettings _plains = new PlainsSettings();
    [SerializeField] private PlateausSettings _plateaus = new PlateausSettings();
    [SerializeField] private DunesSettings _dunes = new DunesSettings();
    [SerializeField] private DryLakesSettings _dryLakes = new DryLakesSettings();
    [SerializeField] private DryRiversSettings _dryRivers = new DryRiversSettings();
    [SerializeField] private CratersSettings _craters = new CratersSettings();

    public RegionsSettings Regions => _regions;
    public PlainsSettings Plains => _plains;
    public PlateausSettings Plateaus => _plateaus;
    public DunesSettings Dunes => _dunes;
    public DryLakesSettings DryLakes => _dryLakes;
    public DryRiversSettings DryRivers => _dryRivers;
    public CratersSettings Craters => _craters;
}

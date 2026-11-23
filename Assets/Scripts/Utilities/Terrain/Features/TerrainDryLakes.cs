using System;
using UnityEngine;

[Serializable]
public class DryLakesSettings
{
    [Tooltip("Size of the lake basins, metres")]
    [SerializeField] private float _size = 1300f;
    [Tooltip("Lake noise value above which there is a lake (0..1, higher = fewer lakes)")]
    [SerializeField] private float _threshold = 0.66f;
    [Tooltip("Depth of the flat lake floors under the plains, metres")]
    [SerializeField] private float _depth = 4f;

    public float Size => _size;
    public float Threshold => _threshold;
    public float Depth => _depth;
}

/// <summary>
/// Dry lakes in the plains (not on the plateaus nor in the dunes): flat floors of sediment under the level of the
/// plains. Marks the ground as sediment.
/// </summary>
public class TerrainDryLakes : ITerrainFeature
{
    private readonly DryLakesSettings _s;
    private readonly float _floorHeight;
    private readonly uint _seed;

    // plainsHeight : height of the plains, the lake floors are under it
    public TerrainDryLakes(DryLakesSettings settings, float plainsHeight, uint seed)
    {
        _s = settings;
        _floorHeight = plainsHeight - settings.Depth;
        _seed = seed;
    }

    public void Apply(ref TerrainPoint p)
    {
        float n = 0.5f + 0.5f * TerrainNoise.Fbm(p.x / _s.Size + 7.7f, p.z / _s.Size - 3.3f, _seed + 9, 3);
        float lake = TerrainNoise.SmoothStep01(_s.Threshold, _s.Threshold + 0.05f, n);
        lake *= (1f - p.plateau) * (1f - p.sand);
        p.height = Mathf.Lerp(p.height, _floorHeight, lake);
        p.sediment = Mathf.Max(p.sediment, lake);
    }
}

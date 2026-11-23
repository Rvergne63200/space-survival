using System;
using UnityEngine;

[Serializable]
public class DryRiversSettings
{
    [Tooltip("Size of the river network, metres")]
    [SerializeField] private float _size = 1400f;
    [Tooltip("Width of the river beds (0..1 of the ridged noise, smaller = narrower)")]
    [SerializeField] private float _width = 0.035f;
    [Tooltip("Depth of the river beds, metres")]
    [SerializeField] private float _depth = 5f;
    [Tooltip("Size of the areas that have rivers, metres")]
    [SerializeField] private float _areaSize = 2600f;

    public float Size => _size;
    public float Width => _width;
    public float Depth => _depth;
    public float AreaSize => _areaSize;
}

/// <summary>
/// Old dry river beds in some areas of the plains: meandering lines (ridged noise of a distorted position) dug with
/// steep banks and a wide bed. Marks the ground as sediment.
/// </summary>
public class TerrainDryRivers : ITerrainFeature
{
    private readonly DryRiversSettings _s;
    private readonly uint _seed;

    public TerrainDryRivers(DryRiversSettings settings, uint seed)
    {
        _s = settings;
        _seed = seed;
    }

    public void Apply(ref TerrainPoint p)
    {
        // meanders : a strong distortion at the scale of a bend
        float rx = p.x + 200f * TerrainNoise.Fbm(p.x / 300f, p.z / 300f, _seed + 15, 3);
        float rz = p.z + 200f * TerrainNoise.Fbm(p.x / 300f + 5.1f, p.z / 300f + 8.4f, _seed + 16, 3);
        float line = TerrainNoise.Ridged(rx / _s.Size, rz / _s.Size, _seed + 8, 3);
        float bed = TerrainNoise.SmoothStep01(1f - _s.Width, 1f - _s.Width * 0.3f, line);

        float area = TerrainNoise.SmoothStep01(0f, 0.25f,
            TerrainNoise.Fbm(p.x / _s.AreaSize, p.z / _s.AreaSize, _seed + 17, 2));
        bed *= area * (1f - p.plateau) * (1f - p.sand * 0.8f);

        p.height -= _s.Depth * Mathf.Sqrt(bed);
        p.sediment = Mathf.Max(p.sediment, bed);
    }
}

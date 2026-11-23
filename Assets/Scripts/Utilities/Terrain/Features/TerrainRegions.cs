using System;
using UnityEngine;

[Serializable]
public class RegionsSettings
{
    [Tooltip("Size of the regions (highlands, plains, sand basins), metres")]
    [SerializeField] private float _size = 3000f;
    [Tooltip("Distortion of the large shapes, metres (0 = straight and regular shapes)")]
    [SerializeField] private float _warpStrength = 300f;
    [Tooltip("Size of the distortion, metres")]
    [SerializeField] private float _warpSize = 900f;

    public float Size => _size;
    public float WarpStrength => _warpStrength;
    public float WarpSize => _warpSize;
}

/// <summary>
/// First feature of the terrain: the large scale map of the regions (low = sand basins, middle = plains,
/// high = plateaus) that the other features read, and the distorted position they use for their big shapes.
/// Does not change the height.
/// </summary>
public class TerrainRegions : ITerrainFeature
{
    private readonly RegionsSettings _s;
    private readonly uint _seed;

    public TerrainRegions(RegionsSettings settings, uint seed)
    {
        _s = settings;
        _seed = seed;
    }

    public void Apply(ref TerrainPoint p)
    {
        float wx = p.x / _s.WarpSize, wz = p.z / _s.WarpSize;
        p.warpedX = p.x + _s.WarpStrength * TerrainNoise.Fbm(wx, wz, _seed + 1, 3);
        p.warpedZ = p.z + _s.WarpStrength * TerrainNoise.Fbm(wx + 31.7f, wz - 12.3f, _seed + 2, 3);
        p.region = TerrainNoise.Fbm(p.warpedX / _s.Size, p.warpedZ / _s.Size, _seed + 3, 3);
    }
}

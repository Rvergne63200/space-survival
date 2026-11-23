using System;
using UnityEngine;

[Serializable]
public class DunesSettings
{
    [Tooltip("Region value below which the ground sinks into sand basins (-1..1, higher = more basins)")]
    [SerializeField] private float _basinThreshold = -0.12f;
    [Tooltip("Depth of the sand basins under the plains, metres")]
    [SerializeField] private float _basinDepth = 16f;
    [Tooltip("Distance between two dune crests, metres")]
    [SerializeField] private float _wavelength = 70f;
    [Tooltip("Height of the dunes, metres")]
    [SerializeField] private float _height = 10f;
    [Tooltip("Direction of the dominant wind, degrees (0 = +X)")]
    [SerializeField] private float _windAngle = 30f;

    public float BasinThreshold => _basinThreshold;
    public float BasinDepth => _basinDepth;
    public float Wavelength => Mathf.Max(1f, _wavelength);
    public float Height => _height;
    public float WindAngle => _windAngle;
}

/// <summary>
/// Sand basins in the low regions, covered with dunes: crests across the dominant wind, with a gentle windward side
/// and a steep lee side. Leaves the basin mask and marks the ground as sand.
/// </summary>
public class TerrainDunes : ITerrainFeature
{
    private readonly DunesSettings _s;
    private readonly uint _seed;
    private readonly float _windCos, _windSin;

    public TerrainDunes(DunesSettings settings, uint seed)
    {
        _s = settings;
        _seed = seed;
        float a = settings.WindAngle * Mathf.Deg2Rad;
        _windCos = Mathf.Cos(a);
        _windSin = Mathf.Sin(a);
    }

    public void Apply(ref TerrainPoint p)
    {
        float basin = TerrainNoise.SmoothStep01(_s.BasinThreshold + 0.05f, _s.BasinThreshold - 0.3f, p.region);
        basin *= 1f - p.plateau;
        p.height -= _s.BasinDepth * basin;
        p.basin = basin;
        p.sand = basin;

        // Along the wind, in wavelengths, bent by a noise so that the crests are not straight
        float along = (p.x * _windCos + p.z * _windSin) / _s.Wavelength;
        float across = -p.x * _windSin + p.z * _windCos;
        float u = along + 0.6f * TerrainNoise.Fbm(p.x / 160f, p.z / 160f, _seed + 13, 3);
        float f = u - Mathf.Floor(u);
        const float crest = 0.78f;                                            // windward side 78 %, lee side 22 %
        float profile = f < crest
            ? TerrainNoise.SmoothStep01(0f, crest, f)
            : 1f - TerrainNoise.SmoothStep01(crest, 1f, f);
        // the crests are higher or lower along their length
        float amplitude = 0.5f + 0.5f * TerrainNoise.Fbm(across / 140f, along * 0.3f, _seed + 14, 2);
        p.height += _s.Height * basin * amplitude * profile;
    }
}

using System;
using UnityEngine;

[Serializable]
public class PlateausSettings
{
    [Tooltip("Region value above which the ground rises into plateaus (-1..1, lower = more plateaus)")]
    [SerializeField] private float _threshold = 0.1f;
    [Tooltip("Height of the plateaus above the plains, metres")]
    [SerializeField] private float _height = 60f;
    [Tooltip("Number of rock layers (terraces) in the cliffs")]
    [SerializeField] private int _strata = 4;
    [Tooltip("Steepness of the cliffs (region width of the edge, smaller = steeper)")]
    [SerializeField] private float _edgeWidth = 0.15f;
    [Tooltip("Size of the canyon network cut in the plateaus, metres")]
    [SerializeField] private float _canyonSize = 1200f;
    [Tooltip("Width of the canyons (0..1 of the ridged noise, smaller = narrower)")]
    [SerializeField] private float _canyonWidth = 0.08f;

    public float Threshold => _threshold;
    public float Height => _height;
    public int Strata => Mathf.Max(1, _strata);
    public float EdgeWidth => Mathf.Max(0.001f, _edgeWidth);
    public float CanyonSize => _canyonSize;
    public float CanyonWidth => _canyonWidth;
}

/// <summary>
/// Plateaus and mesas in the high regions: steep cliffs with a jagged outline, stratified in terraces, and cut by
/// canyons down to the plains. Leaves the plateau mask (for the dunes, lakes, rivers) and marks the cliffs as rock.
/// </summary>
public class TerrainPlateaus : ITerrainFeature
{
    private readonly PlateausSettings _s;
    private readonly uint _seed;

    public TerrainPlateaus(PlateausSettings settings, uint seed)
    {
        _s = settings;
        _seed = seed;
    }

    public void Apply(ref TerrainPoint p)
    {
        // Jagged outline : the edge noise moves the cliff back and forth
        float edgeNoise = 0.05f * TerrainNoise.Fbm(p.x / 220f, p.z / 220f, _seed + 4, 3);
        float plateau = TerrainNoise.SmoothStep01(_s.Threshold, _s.Threshold + _s.EdgeWidth, p.region + edgeNoise);

        plateau *= 1f - Canyon(p);

        // Terraces : the rise is split in steps, each step a flat-ish ledge then a steep wall
        float strata = _s.Strata;
        float t = plateau * strata;
        float step = Mathf.Floor(t);
        float terrace = (step + TerrainNoise.SmoothStep01(0.35f, 1f, t - step)) / strata;
        terrace = Mathf.Lerp(terrace, plateau, 0.3f);                       // ledges not perfectly flat
        p.height += _s.Height * terrace * (1f + 0.15f * TerrainNoise.Fbm(p.x / 400f, p.z / 400f, _seed + 12, 2));

        float cliff = plateau * (1f - plateau) * 4f;                         // 1 in the middle of the edge
        p.rock = Mathf.Max(p.rock, Mathf.Clamp01(cliff * 1.5f));
        p.plateau = plateau;
    }

    // 1 at the bottom of a canyon : ridged lines of a distorted noise
    private float Canyon(in TerrainPoint p)
    {
        float cx = p.warpedX + 120f * TerrainNoise.Fbm(p.x / 250f, p.z / 250f, _seed + 11, 2);
        float line = TerrainNoise.Ridged(cx / _s.CanyonSize, p.warpedZ / _s.CanyonSize, _seed + 7, 2);
        return TerrainNoise.SmoothStep01(1f - 2.5f * _s.CanyonWidth, 1f - _s.CanyonWidth * 0.35f, line);
    }
}

using System;
using UnityEngine;

[Serializable]
public class PlainsSettings
{
    [Tooltip("Height of the plains, metres")]
    [SerializeField] private float _baseHeight = 120f;
    [Tooltip("Amplitude of the gentle undulation of the plains, metres")]
    [SerializeField] private float _relief = 10f;
    [Tooltip("Size of the undulation, metres")]
    [SerializeField] private float _reliefSize = 350f;
    [Tooltip("Height of the groups of rounded hills, metres")]
    [SerializeField] private float _hillHeight = 30f;
    [Tooltip("Size of a hill, metres")]
    [SerializeField] private float _hillSize = 180f;
    [Tooltip("Size of the areas with hills, metres")]
    [SerializeField] private float _hillAreaSize = 1100f;

    public float BaseHeight => _baseHeight;
    public float Relief => _relief;
    public float ReliefSize => _reliefSize;
    public float HillHeight => _hillHeight;
    public float HillSize => _hillSize;
    public float HillAreaSize => _hillAreaSize;
}

/// <summary>
/// Base of the terrain: plains with a gentle undulation, a fine roughness, and groups of rounded hills here and there.
/// Sets the height that the other features change.
/// </summary>
public class TerrainPlains : ITerrainFeature
{
    private readonly PlainsSettings _s;
    private readonly uint _seed;

    public TerrainPlains(PlainsSettings settings, uint seed)
    {
        _s = settings;
        _seed = seed;
    }

    public void Apply(ref TerrainPoint p)
    {
        float h = _s.BaseHeight
            + _s.Relief * TerrainNoise.Fbm(p.x / _s.ReliefSize, p.z / _s.ReliefSize, _seed + 5, 4)
            + 1.2f * TerrainNoise.Fbm(p.x / 60f, p.z / 60f, _seed + 6, 3);

        float area = TerrainNoise.SmoothStep01(0.05f, 0.5f,
            TerrainNoise.Fbm(p.x / _s.HillAreaSize, p.z / _s.HillAreaSize, _seed + 18, 3));
        float hills = 0.5f + 0.5f * TerrainNoise.Fbm(p.x / _s.HillSize, p.z / _s.HillSize, _seed + 19, 4);
        h += _s.HillHeight * area * hills * hills;

        p.height = h;
    }
}

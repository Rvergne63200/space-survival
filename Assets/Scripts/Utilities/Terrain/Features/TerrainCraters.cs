using System;
using UnityEngine;

[Serializable]
public class CraterScale
{
    [Tooltip("Size of a cell, metres (one crater at most per cell)")]
    [SerializeField] private float _cellSize = 260f;
    [Tooltip("Probability of a crater in a cell (0..1)")]
    [SerializeField] private float _chance = 0.3f;

    public CraterScale() { }
    public CraterScale(float cellSize, float chance)
    {
        _cellSize = cellSize;
        _chance = chance;
    }

    public float CellSize => Mathf.Max(1f, _cellSize);
    public float Chance => _chance;
}

[Serializable]
public class CratersSettings
{
    [Tooltip("Crater scales, from the largest to the smallest")]
    [SerializeField] private CraterScale[] _scales =
    {
        new CraterScale(900f, 0.35f), new CraterScale(260f, 0.3f), new CraterScale(70f, 0.15f),
    };
    [Tooltip("Largest crater radius, as a fraction of the cell")]
    [SerializeField] private float _maxRadius = 0.16f;

    public CraterScale[] Scales => _scales;
    public float MaxRadius => _maxRadius;
}

/// <summary>
/// Meteorite craters at several scales. The world is cut in cells; each cell may hold one crater whose position, size
/// and erosion come from the seed. Profile: bowl with a flat floor, raised rim, ejecta around. Rims are rock.
/// </summary>
public class TerrainCraters : ITerrainFeature
{
    private readonly CratersSettings _s;
    private readonly uint _seed;

    public TerrainCraters(CratersSettings settings, uint seed)
    {
        _s = settings;
        _seed = seed;
    }

    public void Apply(ref TerrainPoint p)
    {
        for (int k = 0; k < _s.Scales.Length; k++)
        {
            CraterScale scale = _s.Scales[k];
            uint seed = _seed + 100u + (uint)k * 7919u;
            int cx = Mathf.FloorToInt(p.x / scale.CellSize), cz = Mathf.FloorToInt(p.z / scale.CellSize);
            // a crater (and its ejecta) can overlap the neighbour cells
            for (int i = cx - 1; i <= cx + 1; i++)
                for (int j = cz - 1; j <= cz + 1; j++)
                    ApplyCrater(ref p, i, j, scale, seed);
        }
    }

    private void ApplyCrater(ref TerrainPoint p, int i, int j, CraterScale scale, uint seed)
    {
        if (TerrainNoise.Hash01(i, j, seed) > scale.Chance) return;

        float cell = scale.CellSize;
        float px = (i + 0.2f + 0.6f * TerrainNoise.Hash01(i, j, seed + 1)) * cell;
        float pz = (j + 0.2f + 0.6f * TerrainNoise.Hash01(i, j, seed + 2)) * cell;
        float size01 = TerrainNoise.Hash01(i, j, seed + 3);
        float radius = cell * _s.MaxRadius * (0.35f + 0.65f * size01 * size01);    // many small, few large
        float fresh = 0.4f + 0.6f * TerrainNoise.Hash01(i, j, seed + 4);           // old craters are eroded

        float dx = p.x - px, dz = p.z - pz;
        float d = Mathf.Sqrt(dx * dx + dz * dz) / radius;                          // 1 on the rim
        if (d > 2.5f) return;
        // slightly irregular outline
        d *= 1f + 0.06f * TerrainNoise.Gradient(Mathf.Atan2(dz, dx) * 2f + i * 3.1f, j * 1.7f, seed + 5);

        float depth = 0.22f * radius * fresh, rim = 0.06f * radius * fresh;
        float h;
        if (d < 1f)
        {
            const float floorRadius = 0.45f;                                       // flat floor in the middle
            float bowl = TerrainNoise.SmoothStep01(floorRadius, 1f, d);
            h = -depth * (1f - bowl) + rim * Mathf.Pow(d, 6f);
        }
        else
        {
            h = rim * Mathf.Exp(-(d - 1f) * (d - 1f) * 6f);                       // raised rim and ejecta
        }
        p.height += h;
        p.rock = Mathf.Max(p.rock, Mathf.Exp(-(d - 1f) * (d - 1f) * 18f) * fresh);
    }
}

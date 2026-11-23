using UnityEngine;

/// <summary>
/// Deterministic noise for the terrain generation: gradient noise, fBm and ridged noise, seeded by an integer hash
/// (no shared table), so it is thread-safe and gives the same value for the same world position and seed.
/// All functions take world-like coordinates already divided by the feature size.
/// </summary>
public static class TerrainNoise
{
    /// <summary>Integer hash of a lattice point and a seed.</summary>
    public static uint Hash(int x, int y, uint seed)
    {
        uint h = seed * 0x9E3779B9u;
        h ^= (uint)x * 0x27D4EB2Du;
        h = (h ^ (h >> 15)) * 0x85EBCA6Bu;
        h ^= (uint)y * 0x165667B1u;
        h = (h ^ (h >> 13)) * 0xC2B2AE35u;
        return h ^ (h >> 16);
    }

    /// <summary>Hash of a lattice point mapped to 0..1.</summary>
    public static float Hash01(int x, int y, uint seed)
    {
        return (Hash(x, y, seed) & 0xFFFFFFu) / 16777215f;
    }

    static float Quintic(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);

    // 16 gradient directions, picked by the hash (no trigonometry per sample). Read-only, so thread-safe
    const int GradientCount = 16;
    static readonly float[] GradX = new float[GradientCount], GradY = new float[GradientCount];

    static TerrainNoise()
    {
        for (int i = 0; i < GradientCount; i++)
        {
            float a = (i + 0.5f) * 6.2831853f / GradientCount;
            GradX[i] = Mathf.Cos(a);
            GradY[i] = Mathf.Sin(a);
        }
    }

    static float GradDot(int ix, int iy, float dx, float dy, uint seed)
    {
        uint g = Hash(ix, iy, seed) & (GradientCount - 1);
        return GradX[g] * dx + GradY[g] * dy;
    }

    /// <summary>Gradient noise, about -1..1.</summary>
    public static float Gradient(float x, float y, uint seed)
    {
        int ix = Mathf.FloorToInt(x), iy = Mathf.FloorToInt(y);
        float fx = x - ix, fy = y - iy;
        float u = Quintic(fx), v = Quintic(fy);
        float a = GradDot(ix, iy, fx, fy, seed);
        float b = GradDot(ix + 1, iy, fx - 1f, fy, seed);
        float c = GradDot(ix, iy + 1, fx, fy - 1f, seed);
        float d = GradDot(ix + 1, iy + 1, fx - 1f, fy - 1f, seed);
        return 1.41f * Mathf.Lerp(Mathf.Lerp(a, b, u), Mathf.Lerp(c, d, u), v);
    }

    /// <summary>Fractal sum of gradient noise, about -1..1.</summary>
    public static float Fbm(float x, float y, uint seed, int octaves, float gain = 0.5f)
    {
        float sum = 0f, amp = 1f, norm = 0f;
        for (int i = 0; i < octaves; i++)
        {
            sum += amp * Gradient(x, y, seed + (uint)i * 131u);
            norm += amp;
            amp *= gain;
            // rotation + scale between the octaves, so their lattices do not align
            float nx = 1.6f * x - 1.2f * y, ny = 1.2f * x + 1.6f * y;
            x = nx + 17.3f; y = ny - 9.1f;
        }
        return sum / norm;
    }

    /// <summary>Ridged noise, 0..1, 1 on the ridges (thin lines where the fBm crosses zero).</summary>
    public static float Ridged(float x, float y, uint seed, int octaves)
    {
        return 1f - Mathf.Abs(Fbm(x, y, seed, octaves));
    }

    public static float SmoothStep01(float a, float b, float x)
    {
        float t = Mathf.Clamp01((x - a) / (b - a));
        return t * t * (3f - 2f * t);
    }
}

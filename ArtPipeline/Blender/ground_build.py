# Ground_<Zone> : tileable terrain textures, one per ground zone (painted by TerrainGenerator). Ground_Rust is the
# rust rock of the ore deposits (dp_build.py) ; the others are variants of it (DA palette, 04 - Couleur et lumiere).
# One tile = TILE metres. Every noise frequency is a whole number of cycles per tile and the lattice wraps,
# so the texture repeats without seam. Output : Assets/Terrain/Textures/Ground_<Zone>_BaseColor.png (RGB)
# and Ground_<Zone>_Normal.png ; the TerrainLayer Assets/Terrain/Ground_<Zone>.terrainlayer uses them (tile = TILE).
import bpy, os
import numpy as np

PROJ = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival"
TILE = 16.0          # metres per tile (= tileSize of the TerrainLayer)
RES = 2048
PATCH = 2.0          # size of the colour patches, relative to the deposit rock (user : x2, 2026-11-23)
NUANCE = 0.3         # strength of the patches : 1 = deposit rock, small = almost uniform ground (user, 2026-11-23)

def cyc(per_m):
    """Whole number of cycles per tile for a frequency in cycles per metre."""
    return max(1, int(round(per_m * TILE)))

def srgb(h): return np.array([int(h[i:i + 2], 16) / 255 for i in (1, 3, 5)])
ROCK, ROCK_L, ROCK_D = srgb("#b5562d"), srgb("#c98b5a"), srgb("#7e3a20")
# zone : (base, darker nuance, lighter nuance, noise seed). Order = order of the terrain layers.
GROUNDS = {
    "Rust":     (ROCK, ROCK_D, ROCK_L, 11),                                          # rust plains (main)
    "Ochre":    (srgb("#bf7445"), srgb("#9c5733"), srgb("#d29a68"), 21),             # ochre sand
    "Dust":     (srgb("#7d4a36"), srgb("#4f3b37"), srgb("#9a5a3c"), 31),             # dark rust, basalt dust
    "Sediment": (srgb("#cfae86"), srgb("#b08a64"), srgb("#e3d3b8"), 41),             # pale sediments (old lakes)
}

def sstep(a, b, x):
    t = np.clip((x - a) / (b - a), 0.0, 1.0)
    return t * t * (3 - 2 * t)

def lerp(a, b, t):
    t = np.asarray(t)
    if t.ndim == 2: t = t[..., None]
    return a + (b - a) * t

class TileNoise:
    """Value noise whose lattice wraps : fbm(cycles) repeats exactly once per tile."""
    def __init__(s, seed): s.t = np.random.default_rng(seed).random((2048, 2048))   # >= highest period
    def v(s, x, y, period):
        xi = np.floor(x).astype(np.int64); yi = np.floor(y).astype(np.int64)
        fx = x - xi; fy = y - yi
        fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy)
        x0, x1 = xi % period, (xi + 1) % period; y0, y1 = yi % period, (yi + 1) % period
        t = s.t
        a = t[x0, y0]; b = t[x1, y0]; c = t[x0, y1]; d = t[x1, y1]
        return a + (b - a) * fx + (c - a) * fy + (a - b - c + d) * fx * fy
    def fbm(s, U, V, cycles, octv=4, pers=0.5):
        """U, V in tiles (0..1). cycles : whole number of lattice cells per tile."""
        tot = 0.0; amp = 1.0; norm = 0.0; c = cycles
        for i in range(octv):
            tot = tot + amp * s.v(U * c + i * 0.37, V * c + i * 0.61, c)
            norm += amp; amp *= pers; c *= 2
        return tot / norm

def save_png(name, rgb, path, noncolor):
    img = bpy.data.images.get(name)
    if img: bpy.data.images.remove(img)
    h, w = rgb.shape[:2]
    img = bpy.data.images.new(name, w, h, alpha=False)
    img.colorspace_settings.name = "Non-Color" if noncolor else "sRGB"
    img.pixels.foreach_set(np.dstack([rgb, np.ones((h, w))]).astype(np.float32).ravel())
    img.filepath_raw = path; img.file_format = "PNG"; img.save()
    bpy.data.images.remove(img)

def build(zone="Rust"):
    base, dk, lt, seed = GROUNDS[zone]
    folder = f"{PROJ}/Assets/Terrain/Textures"; os.makedirs(folder, exist_ok=True)
    g = (np.arange(RES) + 0.5) / RES
    U, V = np.meshgrid(g, g)
    n, n2 = TileNoise(seed), TileNoise(seed + 1)
    # same layers as the deposit rock : large dark / light patches, ochre blotches, grain
    # frequencies of dp_build.py (cycles per metre) : the patches divided by PATCH, the grain and relief unchanged
    # almost uniform rust ground : soft nuances around ROCK (smooth ramps, no hard thresholds), fine grain
    dark = n.fbm(U, V, cyc(0.5 / PATCH), 5); light = n2.fbm(U, V, cyc(1.4 / PATCH), 5)
    col = lerp(base, dk, NUANCE * 0.6 * sstep(0.35, 0.8, 1 - dark))
    col = lerp(col, lt, NUANCE * 0.5 * sstep(0.45, 0.85, light))
    col = col * (0.96 + 0.08 * n2.fbm(U, V, cyc(18.0), 2))[..., None]
    hf = 0.0035 * n.fbm(U, V, cyc(9.0), 4) + 0.006 * n2.fbm(U, V, cyc(2.5), 3)
    hf = hf + 0.003 * sstep(0.62, 0.8, n.fbm(U + 0.5, V, cyc(4.0), 3))
    px = TILE / RES
    # wrapped derivatives (np.roll) : the normal map tiles too
    gx = (np.roll(hf, -1, 1) - np.roll(hf, 1, 1)) / (2 * px)
    gy = (np.roll(hf, -1, 0) - np.roll(hf, 1, 0)) / (2 * px)
    nl = np.sqrt(gx * gx + gy * gy + 1)
    nrm = np.dstack([(-gx / nl) * 0.5 + 0.5, (-gy / nl) * 0.5 + 0.5, (1 / nl) * 0.5 + 0.5])
    p = f"Ground_{zone}"
    save_png(p + "_BaseColor", np.clip(col, 0, 1), f"{folder}/{p}_BaseColor.png", False)
    save_png(p + "_Normal", nrm, f"{folder}/{p}_Normal.png", True)
    return p

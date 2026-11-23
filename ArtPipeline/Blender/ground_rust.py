# Ground_Rust : tileable terrain texture, the rust rock painting of the ore deposits (dp_build.py).
# One tile = TILE metres. Every noise frequency is a whole number of cycles per tile and the lattice wraps,
# so the texture repeats without seam. Output : Assets/Terrain/Textures/Ground_Rust_BaseColor.png (RGB)
# and Ground_Rust_Normal.png ; the TerrainLayer Assets/Terrain/Ground_Rust.terrainlayer uses them (tile 8 m).
import bpy, os
import numpy as np

PROJ = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival"
TILE = 8.0
RES = 2048

def srgb(h): return np.array([int(h[i:i + 2], 16) / 255 for i in (1, 3, 5)])
ROCK, ROCK_L, ROCK_D = srgb("#b5562d"), srgb("#c98b5a"), srgb("#7e3a20")

def sstep(a, b, x):
    t = np.clip((x - a) / (b - a), 0.0, 1.0)
    return t * t * (3 - 2 * t)

def lerp(a, b, t):
    t = np.asarray(t)
    if t.ndim == 2: t = t[..., None]
    return a + (b - a) * t

class TileNoise:
    """Value noise whose lattice wraps : fbm(cycles) repeats exactly once per tile."""
    def __init__(s, seed): s.t = np.random.default_rng(seed).random((1024, 1024))
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

def build():
    folder = f"{PROJ}/Assets/Terrain/Textures"; os.makedirs(folder, exist_ok=True)
    g = (np.arange(RES) + 0.5) / RES
    U, V = np.meshgrid(g, g)
    n, n2 = TileNoise(11), TileNoise(12)
    # same layers as the deposit rock : large dark / light patches, ochre blotches, grain
    # (frequencies of dp_build.py per metre x 8 m, rounded to whole cycles)
    col = lerp(ROCK_D, ROCK, sstep(0.28, 0.72, n.fbm(U, V, 4, 4)))
    col = lerp(col, ROCK_L, 0.55 * sstep(0.55, 0.82, n2.fbm(U, V, 11, 4)))
    col = col * (0.92 + 0.16 * n2.fbm(U, V, 144, 2))[..., None]
    hf = 0.0035 * n.fbm(U, V, 72, 4) + 0.006 * n2.fbm(U, V, 20, 3)
    hf = hf + 0.003 * sstep(0.62, 0.8, n.fbm(U + 0.5, V, 32, 3))
    px = TILE / RES
    # wrapped derivatives (np.roll) : the normal map tiles too
    gx = (np.roll(hf, -1, 1) - np.roll(hf, 1, 1)) / (2 * px)
    gy = (np.roll(hf, -1, 0) - np.roll(hf, 1, 0)) / (2 * px)
    nl = np.sqrt(gx * gx + gy * gy + 1)
    nrm = np.dstack([(-gx / nl) * 0.5 + 0.5, (-gy / nl) * 0.5 + 0.5, (1 / nl) * 0.5 + 0.5])
    save_png("Ground_Rust_BaseColor", np.clip(col, 0, 1), f"{folder}/Ground_Rust_BaseColor.png", False)
    save_png("Ground_Rust_Normal", nrm, f"{folder}/Ground_Rust_Normal.png", True)
    return {"folder": folder}

# Deposit_<Ore> : ore deposit, a rust rock slab (~6 m) slightly heaved by the ore below, split in a star of
# deep fissures. The ore veins follow the fissures and dive into the dark : the deposit goes on underground.
# The Drill (platform 3.6 m) is placed on the flat centre, its head going down into the central hole.
# Deposit_<Ore> > Body (single textured material M_Deposit_<Ore>)
#               > DrillPoint (empty at the centre of the plateau, +Z = up)
# Textures (unwrapped UVs, painted texel by texel from the baked 3D position) : Deposit_<Ore>_BaseColor / _Normal / _MetallicSmoothness .png,
# painted procedurally with numpy from the same fields as the geometry.
import bpy, bmesh, math
import numpy as np
from mathutils import Matrix, Vector

PROJ = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival"
S = 3.2            # half size of the mapped square (m), UV = (xy + S) / 2S
TEX = 2048         # texture size (3 mm per pixel)
GRID = 0.02        # mesh sampling before decimation (m)
PLATEAU = 0.30     # height of the flat centre (drill platform r = 1.8 m)
DECIMATE = 0.10

def srgb(h): return np.array([int(h[i:i + 2], 16) / 255 for i in (1, 3, 5)])

ROCK, ROCK_L, ROCK_D = srgb("#b5562d"), srgb("#c98b5a"), srgb("#7e3a20")
DEEP = srgb("#060608")
ORES = {
    "Coal":   dict(seed=11, vein=srgb("#18171b"), vein2=srgb("#34353b"), glint=srgb("#7d889a"), halo=srgb("#5a4234"),
                   vein_s=0.16, glint_s=0.75, glint_m=0.0, glint_k=0.10),
    "Iron":   dict(seed=23, vein=srgb("#2e2a2c"), vein2=srgb("#57504f"), glint=srgb("#a9adb3"), halo=srgb("#6b3424"),
                   vein_s=0.35, glint_s=0.70, glint_m=0.85, glint_k=0.12),
    "Copper": dict(seed=37, vein=srgb("#1f5e4b"), vein2=srgb("#3f9a80"), glint=srgb("#c47a3e"), halo=srgb("#4f5a3e"),
                   vein_s=0.30, glint_s=0.75, glint_m=0.90, glint_k=0.10),
}

def sstep(a, b, x):
    t = np.clip((x - a) / (b - a), 0.0, 1.0)
    return t * t * (3 - 2 * t)

def lerp(a, b, t):
    t = np.asarray(t)
    if max(np.ndim(a), np.ndim(b)) in (1, 3) and t.ndim in (0, 2): t = t[..., None]
    return a + (b - a) * t

# ------------------------------------------------------------------ noise
class Noise:
    def __init__(s, seed): s.t = np.random.default_rng(seed).random((256, 256))
    def v(s, x, y):
        xi = np.floor(x).astype(np.int64); yi = np.floor(y).astype(np.int64)
        fx = x - xi; fy = y - yi
        fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy)
        t = s.t
        a = t[xi & 255, yi & 255]; b = t[(xi + 1) & 255, yi & 255]
        c = t[xi & 255, (yi + 1) & 255]; d = t[(xi + 1) & 255, (yi + 1) & 255]
        return a + (b - a) * fx + (c - a) * fy + (a - b - c + d) * fx * fy
    def fbm(s, x, y, freq, octv=4, pers=0.5):
        tot = 0.0; amp = 1.0; norm = 0.0
        for i in range(octv):
            tot = tot + amp * s.v(x * freq + i * 17.31, y * freq - i * 9.13)
            norm += amp; amp *= pers; freq *= 2.03
        return tot / norm

# ------------------------------------------------------------------ fissures
def polyline(rng, segs, x, y, ang, length, w0, w1, d0, d1, step=0.1, wig=0.14, prog=(0.0, 1.0)):
    """prog : progress along the fissure system at both ends (0 at the centre, 1 at the tips)."""
    n = max(2, int(length / step)); pts = [(x, y)]
    for _ in range(n):
        ang += rng.normal(0, 0.45) if rng.random() < 0.22 else rng.normal(0, wig * 0.3)   # straight runs, sharp kinks
        x += math.cos(ang) * step; y += math.sin(ang) * step; pts.append((x, y))
    for i in range(n):
        t0, t1 = i / n, (i + 1) / n
        segs.append((*pts[i], *pts[i + 1], w0 + (w1 - w0) * t0 ** 0.8, w0 + (w1 - w0) * t1 ** 0.8,
                     d0 + (d1 - d0) * t0, d0 + (d1 - d0) * t1,
                     prog[0] + (prog[1] - prog[0]) * t0, prog[0] + (prog[1] - prog[0]) * t1))
    return pts

def make_cracks(seed):
    """Main fissures (geometry) and hairline cracks (texture only). The hairlines are not random :
    each fissure ends in a hairline that carries on its path, and short cracks start from the lips."""
    rng = np.random.default_rng(seed)
    main, hair = [], []
    def extend(pts, w):           # hairline carrying on past the tip of a fissure
        (x0, y0), (x1, y1) = pts[-2], pts[-1]
        polyline(rng, hair, x1, y1, math.atan2(y1 - y0, x1 - x0), rng.uniform(0.3, 0.8), w, 0.005, 0.004, 0.001, step=0.05)
    def lip_cracks(pts, w0, w1, count):   # short cracks opening from the lips, roughly across the fissure
        for _ in range(count):
            t = rng.uniform(0.1, 0.8); j = int(t * (len(pts) - 2))
            (x0, y0), (x1, y1) = pts[j], pts[j + 1]
            ang = math.atan2(y1 - y0, x1 - x0); side = rng.choice([-1, 1])
            w = w0 + (w1 - w0) * t ** 0.8
            nx, ny = math.cos(ang + side * math.pi / 2), math.sin(ang + side * math.pi / 2)
            polyline(rng, hair, x0 + nx * w * 0.9, y0 + ny * w * 0.9, ang + side * rng.uniform(1.0, 1.4),
                     rng.uniform(0.12, 0.4) * (0.5 + w / w0), 0.012, 0.005, 0.003, 0.001, step=0.04)
    k = 6; off = rng.uniform(0, 2 * math.pi)
    for i in range(k):
        a = off + 2 * math.pi * i / k + rng.uniform(-0.25, 0.25)
        L = rng.uniform(2.4, 2.95)
        big = i % 2 == 0                                      # three major fissures, three minor ones
        w0 = rng.uniform(0.15, 0.19) if big else rng.uniform(0.08, 0.11)
        pts = polyline(rng, main, 0.2 * math.cos(a), 0.2 * math.sin(a), a, L if big else L * 0.75, w0, 0.025, 0.30, 0.04)
        extend(pts, 0.016)
        lip_cracks(pts, w0, 0.025, 4 if big else 2)
        for _ in range(int(rng.integers(1, 3))):
            t = rng.uniform(0.3, 0.7); j = int(t * (len(pts) - 2))
            (bx, by), (cx, cy) = pts[j], pts[j + 1]
            ba = math.atan2(cy - by, cx - bx) + rng.choice([-1, 1]) * rng.uniform(0.5, 0.9)
            wp = w0 + (0.025 - w0) * t ** 0.8
            bp = polyline(rng, main, bx, by, ba, rng.uniform(0.5, 1.1), wp * 0.6, 0.02, 0.30 * (1 - t) * 0.6, 0.02, prog=(t, 1.0))
            extend(bp, 0.012)
    return main, hair

def seg_fields(xs, ys, segs, dnoise, halo=True):
    """Max over segments of : a = inward coordinate (1 at the axis, 0 at the lip), carve depth,
    vein band and halo band (wider soft zones along the fissure)."""
    X, Y = np.meshgrid(xs, ys)          # arrays [iy, ix]
    A = np.zeros_like(X); C = np.zeros_like(X); V = np.zeros_like(X); H = np.zeros_like(X); T = np.zeros_like(X)
    h = xs[1] - xs[0]
    for (x0, y0, x1, y1, w0, w1, d0, d1, p0, p1) in segs:
        m = (max(w0, w1) * 6 + 0.3) / 0.7 if halo else max(w0, w1) / 0.7   # d is scaled by dnoise >= 0.73
        i0 = max(0, int((min(x0, x1) - m - xs[0]) / h)); i1 = min(len(xs), int((max(x0, x1) + m - xs[0]) / h) + 2)
        j0 = max(0, int((min(y0, y1) - m - ys[0]) / h)); j1 = min(len(ys), int((max(y0, y1) + m - ys[0]) / h) + 2)
        if i0 >= i1 or j0 >= j1: continue
        x = X[j0:j1, i0:i1]; y = Y[j0:j1, i0:i1]
        dx, dy = x1 - x0, y1 - y0; L2 = dx * dx + dy * dy
        t = np.clip(((x - x0) * dx + (y - y0) * dy) / L2, 0, 1)
        d = np.hypot(x - (x0 + t * dx), y - (y0 + t * dy)) * dnoise[j0:j1, i0:i1]
        w = w0 + (w1 - w0) * t; D = d0 + (d1 - d0) * t
        a = np.clip(1 - d / w, 0, 1)
        sl = (slice(j0, j1), slice(i0, i1))
        A[sl] = np.maximum(A[sl], a)
        C[sl] = np.maximum(C[sl], D * np.sqrt(sstep(0.0, 1.0, a)))      # V profile, steep walls
        if halo:
            vs = np.clip(1 - d / (w * 3.2 + 0.05), 0, 1)
            T[sl] = np.where(vs > V[sl], p0 + (p1 - p0) * t, T[sl])      # progress of the closest fissure
            V[sl] = np.maximum(V[sl], vs)
            H[sl] = np.maximum(H[sl], np.clip(1 - d / (w * 6 + 0.3), 0, 1))
    return X, Y, A, C, V, H, T

def rim_radius(n, th):
    return 2.9 + 0.32 * (n.fbm(np.cos(th) * 2.2 + 5, np.sin(th) * 2.2 + 5, 1.0, 4) - 0.5) * 2

def fields(xs, ys, ore, fine):
    """All the fields on a grid. fine=True : texture resolution (painting + fine relief)."""
    n = Noise(ore["seed"]); n2 = Noise(ore["seed"] + 1)
    X0, Y0 = np.meshgrid(xs, ys)
    dnoise = 1 + 0.40 * (n2.fbm(X0, Y0, 3.0, 3) - 0.5) + 0.14 * (n.fbm(X0, Y0, 9.0, 2) - 0.5)   # broken lips
    main, hair = make_cracks(ore["seed"])
    X, Y, A, C, V, H, T = seg_fields(xs, ys, main, dnoise)
    r = np.hypot(X, Y); th = np.arctan2(Y, X); Rn = rim_radius(n, th)
    # central hole where the drill head goes down
    rN = r * (1 + 0.3 * (n.fbm(X, Y, 2.5, 3) - 0.5))
    ha = np.clip(1 - rN / 0.36, 0, 1)
    A = np.maximum(A, ha); C = np.maximum(C, 0.34 * sstep(0.0, 0.5, ha))
    V = np.maximum(V, np.clip(1 - rN / 0.75, 0, 1)); H = np.maximum(H, np.clip(1 - rN / 1.2, 0, 1))
    # base slab : flat plateau, soft shoulder, then a skirt that goes under the ground
    s = sstep(1.9, Rn, r)
    hb = PLATEAU - 0.025 * (r / 1.9) ** 2
    hb = hb + (-0.04 - hb) * s
    amp = 0.012 + 0.05 * sstep(1.7, 2.5, r)
    hb = hb + amp * (n.fbm(X, Y, 0.9, 4) - 0.5) * 2
    hb = np.where(r > Rn, -0.04 - 0.30 * sstep(Rn, Rn + 0.12, r), hb)
    fade = 1 - sstep(Rn - 0.25, Rn, r)
    # fissure bottoms stay above the ground (z = 0), with a smooth min : rounded bottom, no crease
    cap = np.maximum(hb - 0.015, 0); C = C * fade; k = 0.05
    hk = np.clip(k - np.abs(C - cap), 0, None) / k
    C = np.maximum(np.minimum(C, cap) - hk * hk * k * 0.25, 0)
    A = A * fade; V = V * fade
    height = hb - C
    out = dict(X=X, Y=Y, r=r, Rn=Rn, A=A, height=height)
    if not fine: return out
    HA = seg_fields(xs, ys, hair, dnoise, halo=False)[2]
    # fissure footprint grown by ~2.5 cm : the lip of the mesh (blurred, decimated) always falls inside it
    lip = C > 0.002
    for _ in range(5):
        p = np.pad(lip, 1)
        lip = lip | p[:-2, 1:-1] | p[2:, 1:-1] | p[1:-1, :-2] | p[1:-1, 2:]
    out.update(hb=hb, V=V, H=H, HA=HA * fade, fade=fade, T=T, C=C, lip=lip.astype(np.float64))
    return out

# ------------------------------------------------------------------ painting, texel by texel from its 3D position
def paint(P, g, xs, ore):
    """P : [h, w, 3] world position of each texel (baked). g : grid fields. The noise coordinates mix z in,
    so the walls get their own detail instead of a stretched copy of the top."""
    x, y, z = P[..., 0], P[..., 1], P[..., 2]
    smp = lambda k: bilinear(g[k], xs, x, y)
    V, H, HA, fade, Rn = smp("V"), smp("H"), smp("HA"), smp("fade"), smp("Rn")
    depth, lip = smp("C"), smp("lip")             # carve depth (smooth field, not the decimated mesh)
    r = np.hypot(x, y)
    u = x + 0.8 * z; v = y - 0.8 * z
    n = Noise(ore["seed"]); n2 = Noise(ore["seed"] + 1)
    # rust rock
    rock = lerp(ROCK_D, ROCK, sstep(0.28, 0.72, n.fbm(u, v, 0.55, 4)))
    rock = lerp(rock, ROCK_L, 0.55 * sstep(0.55, 0.82, n2.fbm(u, v, 1.4, 4)))
    rock = rock * (0.92 + 0.16 * n2.fbm(u, v, 18.0, 2))[..., None]
    rock = lerp(rock, ore["halo"], 0.12 * sstep(0.0, 1.0, H))
    # ore, with the same kind of nuances as the ground : large patches, medium blotches, grain
    oc = lerp(ore["vein"], ore["vein2"], sstep(0.25, 0.75, n.fbm(u + 20, v, 2.0, 4)))
    oc = lerp(oc, ore["halo"], 0.40 * sstep(0.55, 0.80, n2.fbm(u, v + 20, 3.5, 4)))
    oc = oc * (0.75 + 0.50 * sstep(0.2, 0.8, n.fbm(u - 9, v, 6.0, 3)))[..., None]
    oc = oc * (0.90 + 0.20 * n2.fbm(u + 5, v, 18.0, 2))[..., None]
    gl = sstep(1 - ore["glint_k"], 1 - ore["glint_k"] + 0.03, n2.fbm(u, v, 30.0, 2))
    oc = lerp(oc, ore["glint"], 0.6 * gl)
    # coverage : a broken edge (several noise scales) that thins out in clumps; always ore on the lip and walls
    e = V + 0.30 * (n2.fbm(u, v, 1.6, 5) - 0.5) + 0.22 * (n.fbm(u, v, 6.0, 3) - 0.5) + 0.08 * (n2.fbm(u, v, 20.0, 2) - 0.5)
    cov = sstep(0.585, 0.60, e)                                                   # crisp, irregular edge
    clumps = sstep(0.62, 0.65, n.fbm(u + 50, v, 9.0, 3)) * sstep(0.44, 0.585, e)  # patches thinning out
    cov = np.maximum(np.maximum(cov, clumps), sstep(0.3, 0.7, lip))
    deep = sstep(0.05, 0.20, depth)
    ore_c = lerp(oc, DEEP, deep)
    # near the edge the ore is partly covered by rock dust (grainy), the transition is in the material, not a blur
    dust = (1 - sstep(0.60, 0.75, e)) * sstep(0.45, 0.70, n2.fbm(u, v, 25.0, 2)) * 0.65 * (1 - sstep(0.0, 0.01, depth))
    ore_c = lerp(ore_c, rock, dust)
    col = lerp(rock, ore_c, cov)
    col = col * (1 - 0.15 * sstep(0.1, 0.8, HA))[..., None]
    col = lerp(col, ROCK * 0.85 + ROCK_L * 0.15, 0.7 * sstep(Rn - 0.5, Rn, r))      # dust on the rim
    om = cov * (1 - deep)
    smooth = (0.12 + (ore["vein_s"] - 0.12) * om) * (1 - 0.7 * deep)
    smooth = smooth + (ore["glint_s"] - smooth) * gl * om
    metal = ore["glint_m"] * gl * om
    hf = 0.0035 * n.fbm(u, v, 9.0, 4) + 0.006 * n2.fbm(u, v, 2.5, 3)
    hf = hf + 0.003 * sstep(0.62, 0.8, n.fbm(u + 40, v, 4.0, 3)) * (1 - cov) - 0.002 * om * n.fbm(u, v, 6.0, 3)
    return np.clip(col, 0, 1), np.clip(smooth, 0, 1), metal, hf

# ------------------------------------------------------------------ images
def save_png(name, rgba, path, noncolor):
    img = bpy.data.images.get(name)
    if img: bpy.data.images.remove(img)
    h, w = rgba.shape[:2]
    img = bpy.data.images.new(name, w, h, alpha=True)
    img.colorspace_settings.name = "Non-Color" if noncolor else "sRGB"
    img.pixels.foreach_set(rgba.astype(np.float32).ravel())
    img.filepath_raw = path; img.file_format = "PNG"; img.save()
    img.alpha_mode = "CHANNEL_PACKED"
    return img

def bake_positions(ob):
    """Bakes the world position of every texel (Cycles, emission), encoded as (P + 4) / 8."""
    sc = bpy.context.scene; eng = sc.render.engine
    sc.render.engine = "CYCLES"; sc.cycles.samples = 1
    img = bpy.data.images.new("_PosBake", TEX, TEX, alpha=True, float_buffer=True)
    img.colorspace_settings.name = "Non-Color"
    m = bpy.data.materials.new("_PosMat"); m.use_nodes = True; nt = m.node_tree; nt.nodes.clear()
    out = nt.nodes.new("ShaderNodeOutputMaterial"); em = nt.nodes.new("ShaderNodeEmission")
    geo = nt.nodes.new("ShaderNodeNewGeometry")
    add = nt.nodes.new("ShaderNodeVectorMath"); add.operation = "ADD"; add.inputs[1].default_value = (4, 4, 4)
    scl = nt.nodes.new("ShaderNodeVectorMath"); scl.operation = "SCALE"; scl.inputs["Scale"].default_value = 0.125
    nt.links.new(geo.outputs["Position"], add.inputs[0]); nt.links.new(add.outputs[0], scl.inputs[0])
    nt.links.new(scl.outputs[0], em.inputs["Color"]); nt.links.new(em.outputs[0], out.inputs["Surface"])
    tex = nt.nodes.new("ShaderNodeTexImage"); tex.image = img; nt.nodes.active = tex
    old = ob.data.materials[0]; ob.data.materials[0] = m
    for o in bpy.context.selected_objects: o.select_set(False)
    ob.select_set(True); bpy.context.view_layer.objects.active = ob
    bpy.ops.object.bake(type="EMIT", margin=16, margin_type="EXTEND", use_clear=True)
    px = np.empty(TEX * TEX * 4, np.float32); img.pixels.foreach_get(px)
    ob.data.materials[0] = old; sc.render.engine = eng
    bpy.data.images.remove(img); bpy.data.materials.remove(m)
    return px.reshape(TEX, TEX, 4)[..., :3].astype(np.float64) * 8 - 4

def build_textures(model, ore, folder, body):
    xs = -S + (np.arange(TEX) + 0.5) * (2 * S / TEX)
    g = fields(xs, xs, ore, fine=True)
    P = bake_positions(body)
    col, smooth, metal, hf = paint(P, g, xs, ore)
    one = np.ones(col.shape[:2])
    # tangent space normal map : height derivatives along u and v, per metre of surface
    Pv, Pu = np.gradient(P, axis=(0, 1))
    su = np.maximum(np.linalg.norm(Pu, axis=2), 1e-5); sv = np.maximum(np.linalg.norm(Pv, axis=2), 1e-5)
    hv, hu = np.gradient(hf)
    gx = np.clip(hu / su, -2, 2); gy = np.clip(hv / sv, -2, 2)
    nl = np.sqrt(gx * gx + gy * gy + 1)
    nrm = np.dstack([(-gx / nl) * 0.5 + 0.5, (-gy / nl) * 0.5 + 0.5, (1 / nl) * 0.5 + 0.5, one])
    return (save_png(model + "_BaseColor", np.dstack([col, one]), f"{folder}/{model}_BaseColor.png", False),
            save_png(model + "_Normal", nrm, f"{folder}/{model}_Normal.png", True),
            save_png(model + "_MetallicSmoothness", np.dstack([metal, metal, metal, smooth]),
                     f"{folder}/{model}_MetallicSmoothness.png", True))

def make_material(model, imgs):
    name = "M_" + model
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True; nt = m.node_tree; nt.nodes.clear()
    out = nt.nodes.new("ShaderNodeOutputMaterial"); bs = nt.nodes.new("ShaderNodeBsdfPrincipled")
    tb, tn, tm = (nt.nodes.new("ShaderNodeTexImage") for _ in range(3))
    tb.image, tn.image, tm.image = imgs
    nm = nt.nodes.new("ShaderNodeNormalMap"); inv = nt.nodes.new("ShaderNodeMath"); inv.operation = "SUBTRACT"
    inv.inputs[0].default_value = 1.0
    nt.links.new(tb.outputs["Color"], bs.inputs["Base Color"])
    nt.links.new(tn.outputs["Color"], nm.inputs["Color"]); nt.links.new(nm.outputs["Normal"], bs.inputs["Normal"])
    nt.links.new(tm.outputs["Color"], bs.inputs["Metallic"])
    nt.links.new(tm.outputs["Alpha"], inv.inputs[1]); nt.links.new(inv.outputs[0], bs.inputs["Roughness"])
    nt.links.new(bs.outputs[0], out.inputs["Surface"])
    m.diffuse_color = (0.45, 0.12, 0.05, 1)
    return m

# ------------------------------------------------------------------ mesh
def blur(h, passes):
    for _ in range(passes):
        p = np.pad(h, 1, mode="edge")
        h = (4 * h + p[:-2, 1:-1] + p[2:, 1:-1] + p[1:-1, :-2] + p[1:-1, 2:]) / 8
    return h

def bilinear(field, xs, x, y):
    """Field [iy, ix] on the grid xs (both axes), sampled at the points x, y."""
    h = xs[1] - xs[0]
    fx = np.clip((x - xs[0]) / h, 0, len(xs) - 1.001); fy = np.clip((y - xs[0]) / h, 0, len(xs) - 1.001)
    i = fx.astype(np.int64); j = fy.astype(np.int64); u = fx - i; v = fy - j
    return (field[j, i] * (1 - u) * (1 - v) + field[j, i + 1] * u * (1 - v)
            + field[j + 1, i] * (1 - u) * v + field[j + 1, i + 1] * u * v)

def build_mesh(model, ore, mat, col):
    xs = np.arange(-S, S + 1e-6, GRID)
    f = fields(xs, xs, ore, fine=False)
    f["height"] = blur(f["height"], 3)                  # blur : no staircase on the lips crossing the grid
    inside = f["r"] < f["Rn"] + 0.12
    ny, nx = inside.shape
    vid = -np.ones(inside.shape, dtype=np.int64); vid[inside] = np.arange(inside.sum())
    jj, ii = np.nonzero(inside)
    co = np.stack([xs[ii], xs[jj], f["height"][jj, ii]], 1)
    q = np.stack([vid[:-1, :-1], vid[:-1, 1:], vid[1:, 1:], vid[1:, :-1]], -1).reshape(-1, 4)
    q = q[(q >= 0).all(1)]
    me = bpy.data.meshes.new("Body"); me.from_pydata(co.tolist(), [], q.tolist()); me.update()
    ob = bpy.data.objects.new("Body", me); col.objects.link(ob)
    me.materials.append(mat)
    # detail kept in the fissures : the decimation is driven by a vertex group (1 = keep)
    gy, gx = np.gradient(f["height"], GRID)
    keep = np.clip(np.maximum(f["A"] * 3, np.hypot(gx, gy) * 1.5), 0, 1)
    vg = ob.vertex_groups.new(name="Detail")
    kv = keep[jj, ii]
    for v in range(len(kv)):
        if kv[v] > 0.01: vg.add([v], float(kv[v]), "REPLACE")
    dec = ob.modifiers.new("Dec", "DECIMATE"); dec.ratio = DECIMATE
    dec.vertex_group = "Detail"; dec.invert_vertex_group = True; dec.vertex_group_factor = 6.0
    bpy.context.view_layer.objects.active = ob
    for o in bpy.context.selected_objects: o.select_set(False)
    ob.select_set(True)
    bpy.ops.object.modifier_apply(modifier="Dec")
    ob.vertex_groups.clear()
    me = ob.data
    # keep the decimated vertices, but rebuild the triangles with a 2D Delaunay triangulation : the slab is a
    # heightfield, so this can never fold (the collapse left flipped faces, black holes in the bake) and it
    # gives even triangles (no slivers, no shadow stripes)
    from mathutils import geometry as mgeo
    vco = np.zeros(len(me.vertices) * 3); me.vertices.foreach_get("co", vco); vco = vco.reshape(-1, 3)
    # + a sparse regular sprinkle (25 cm) : caps the triangle size on the flat parts (no sag under the dome)
    from mathutils.kdtree import KDTree
    kd = KDTree(len(vco))
    for k, p in enumerate(vco): kd.insert(p, k)
    kd.balance()
    g = np.arange(-S, S, 0.25) + 0.125
    gx, gy = [a.ravel() for a in np.meshgrid(g, g)]
    keepg = bilinear((f["r"] < f["Rn"] - 0.05).astype(np.float64), xs, gx, gy) > 0.99
    gx, gy = gx[keepg], gy[keepg]
    gz = bilinear(f["height"], xs, gx, gy)
    extra = [(a, b, c) for a, b, c in zip(gx, gy, gz) if kd.find(Vector((a, b, c)))[2] > 0.08]
    if extra: vco = np.vstack([vco, np.array(extra)])
    vout, _, fout, vmap, _, _ = mgeo.delaunay_2d_cdt([Vector(p[:2]) for p in vco], [], [], 0, 1e-7)
    src = [o[0] if o else 0 for o in vmap]
    pts = np.array([(v.x, v.y, vco[src[k], 2]) for k, v in enumerate(vout)])
    tri = np.array([t for t in fout if len(t) == 3])
    cen = pts[tri].mean(1)
    insidef = bilinear((f["r"] < f["Rn"] + 0.12).astype(np.float64), xs, cen[:, 0], cen[:, 1]) > 0.99
    tri = tri[insidef]
    used = np.unique(tri); remap = -np.ones(len(pts), np.int64); remap[used] = np.arange(len(used))
    me.clear_geometry()
    me.from_pydata(pts[used].tolist(), [], remap[tri].tolist()); me.update()
    for p in me.polygons:                      # counter-clockwise from above
        if p.normal.z < 0: p.flip()
    me.update()
    # unwrap (one island, the slab is a disc) : the walls get as many texels as the top, no stretching
    me.uv_layers.new(name="UVMap")
    bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.select_all(action="SELECT")
    try: bpy.ops.uv.unwrap(method="MINIMUM_STRETCH", margin=0.005)
    except Exception: bpy.ops.uv.unwrap(method="ANGLE_BASED", margin=0.005)
    bpy.ops.object.mode_set(mode="OBJECT")
    me = ob.data
    uvs = np.zeros(len(me.loops) * 2); me.uv_layers[0].data.foreach_get("uv", uvs); uvs = uvs.reshape(-1, 2)
    flip = 0                                     # keep the UVs unmirrored (tangent space of the normal map)
    for p in me.polygons[:400]:
        l = list(p.loop_indices)
        a = uvs[l[1]] - uvs[l[0]]; b = uvs[l[2]] - uvs[l[0]]
        flip += 1 if (a[0] * b[1] - a[1] * b[0]) * p.normal.z < 0 else -1
    if flip > 0:
        uvs[:, 0] = 1 - uvs[:, 0]; me.uv_layers[0].data.foreach_set("uv", uvs.ravel())
    co = np.zeros(len(me.vertices) * 3); me.vertices.foreach_get("co", co); co = co.reshape(-1, 3)
    # normals from the smoothed height field, not from the faces : no visible facets
    hs = blur(f["height"], 3)
    gy, gx = np.gradient(hs, GRID)
    nx_ = -bilinear(gx, xs, co[:, 0], co[:, 1]); ny_ = -bilinear(gy, xs, co[:, 0], co[:, 1])
    nrm = np.stack([nx_, ny_, np.ones_like(nx_)], 1); nrm /= np.linalg.norm(nrm, axis=1)[:, None]
    me.shade_smooth()
    me.normals_split_custom_set_from_vertices(nrm.tolist())
    return ob

def build(kind):
    ore = ORES[kind]; model = "Deposit_" + kind
    folder = f"{PROJ}/Assets/Resources/Models/{model}"
    import os; os.makedirs(folder, exist_ok=True)
    col = bpy.data.collections.get("Models")
    if col is None:
        col = bpy.data.collections.new("Models"); bpy.context.scene.collection.children.link(col)
    for o in list(bpy.data.objects):
        if o.name in (model, "Body", "DrillPoint") or (o.parent and o.parent.name == model):
            bpy.data.objects.remove(o)
    tmp = bpy.data.materials.get("M_" + model) or bpy.data.materials.new("M_" + model)
    root = bpy.data.objects.new(model, None); col.objects.link(root)
    body = build_mesh(model, ore, tmp, col); body.parent = root
    imgs = build_textures(model, ore, folder, body)
    make_material(model, imgs)
    dp = bpy.data.objects.new("DrillPoint", None); col.objects.link(dp)
    dp.empty_display_type = "SINGLE_ARROW"; dp.location = (0, 0, PLATEAU); dp.parent = root
    return dict(model=model, tris=sum(len(p.vertices) - 2 for p in body.data.polygons), verts=len(body.data.vertices))

# Deposit_<Ore>, version 3 : the ore is ROCK IN PLACE, embedded in the slab (user's request, 2026-11-23) :
#  - a massive ore seam cut by the fissures : below a depth that grows towards the tips, the fissures are filled
#    by the seam, whose top is broken in ledges ; above it the walls are rust rock ;
#  - a few big angular ore masses half caught in the walls, sticking out into the fissures (and slightly above
#    the slab outside the drill platform) ;
#  - rare small chips at the foot of the walls.
# Deposit_<Ore> > Body (rock, baked from a 1 cm high-poly that includes the ore for the contact shadows)
#               > Ore  (seam + masses + chips, faceted, material M_Deposit_<Ore>_Ore with a tileable texture)
#               > DrillPoint
import bpy, bmesh, math, os
import numpy as np
from mathutils import Vector, Matrix

SP = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival/ArtPipeline/Blender"
exec(open(SP + r"/dp_hd.py", encoding="utf8").read())

DEEP = srgb("#2a140c")            # bottom of the walls : dark rust
DEPTH_RAMP = (0.015, 0.20)
ORE_TILE = 0.5                     # ore texture tile (m)
ORE_TEX = 1024

class TileNoise:
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
        tot = 0.0; amp = 1.0; norm = 0.0; c = cycles
        for i in range(octv):
            tot = tot + amp * s.v(U * c + i * 0.37, V * c + i * 0.61, c)
            norm += amp; amp *= pers; c *= 2
        return tot / norm

# ------------------------------------------------------------------ fields shared by rock and ore
def seam_fields(ore, xs):
    f = fields(xs, xs, ore, fine=True)
    X, Y, T = f["X"], f["Y"], f["T"]
    n = Noise(ore["seed"] + 60); n2 = Noise(ore["seed"] + 61)
    ref = blur(f["hb"], 3)                                         # slab surface without the fissures
    # seam top depth : shallow near the centre, dipping towards the tips (then below the fissure bottoms)
    st = 0.13 + 0.05 * (n.fbm(X, Y, 1.2, 3) - 0.5) + 0.25 * sstep(0.35, 1.0, T)
    # broken top : ledges (terraces with short steep risers) and a little roughness
    q = n2.fbm(X, Y, 3.0, 4) * 7.0
    terr = 0.022 * (np.floor(q) + sstep(0.72, 1.0, q - np.floor(q)))
    top = ref - st + terr - 0.022 * 3.5 + 0.004 * n.fbm(X, Y, 15.0, 2)
    return f, ref, top

def rock_hp(ore):
    xs = np.arange(-S, S + 1e-6, HP_GRID)
    f = fields(xs, xs, ore, fine=True)
    X, Y = f["X"], f["Y"]
    n = Noise(ore["seed"] + 20); n2 = Noise(ore["seed"] + 21)
    ridge = (1 - np.abs(2 * n.fbm(X, Y, 5.0, 4) - 1)) ** 3
    pits = sstep(0.68, 0.74, n2.fbm(X + 13, Y, 14.0, 3))
    h = blur(f["height"], 2) + 0.008 * ridge + 0.003 * n2.fbm(X, Y, 20.0, 2) - 0.004 * pits
    z = np.zeros_like(X)
    return xs, f, h, {"ors": z, "lips": z, "keepw": z, "hbz": blur(f["hb"], 2)}

# ------------------------------------------------------------------ ore geometry
def box_uv(faces, uvl):
    for fc in faces:
        fc.normal_update()
        a = max(range(3), key=lambda i: abs(fc.normal[i]))
        i, j = [(1, 2), (0, 2), (0, 1)][a]
        for l in fc.loops:
            l[uvl].uv = (l.vert.co[i] / ORE_TILE, l.vert.co[j] / ORE_TILE)

def chunk(bm, uvl, c, size, rng, sub, cuts=(3, 6), dcut=(0.4, 0.75), flat=(0.55, 0.85)):
    """Angular chunk : jittered icosphere flattened by random cut planes (fracture faces)."""
    res = bmesh.ops.create_icosphere(bm, subdivisions=sub, radius=1.0)
    vs = res["verts"]
    for v in vs: v.co *= 1 + 0.2 * (rng.random() - 0.5)
    for _ in range(int(rng.integers(*cuts))):
        nrm = Vector(rng.normal(size=3)).normalized(); d = rng.uniform(*dcut)
        for v in vs:
            p = v.co.dot(nrm)
            if p > d: v.co -= nrm * (p - d)
    rot = Matrix.Rotation(rng.uniform(0, 6.3), 3, Vector(rng.normal(size=3)).normalized())
    sc = Vector((size, size * rng.uniform(0.7, 1.0), size * rng.uniform(*flat)))
    for v in vs: v.co = c + rot @ Vector((v.co.x * sc.x, v.co.y * sc.y, v.co.z * sc.z))
    box_uv({f for v in vs for f in v.link_faces}, uvl)

def build_ore_geo(ore, col):
    rng = np.random.default_rng(ore["seed"] + 70)
    xs = np.arange(-S, S + 1e-6, 0.01)
    f, ref, top = seam_fields(ore, xs)
    C, A, T, r = f["C"], f["A"], f["T"], f["r"]
    rock = blur(f["height"], 3)                                     # game slab surface
    # 1) seam : heightfield sheet where it stands above the fissure floor ; its edges are hidden in the rock
    region = (top > rock - 0.04) & (C > 0.005) & (f["fade"] > 0.3)
    p = np.pad(region, 2)
    for _ in range(2):
        p = p | np.roll(p, 1, 0) | np.roll(p, -1, 0) | np.roll(p, 1, 1) | np.roll(p, -1, 1)
    region = p[2:-2, 2:-2] & (f["r"] < f["Rn"])
    vid = -np.ones(region.shape, dtype=np.int64); vid[region] = np.arange(region.sum())
    jj, ii = np.nonzero(region)
    co = np.stack([xs[ii], xs[jj], np.minimum(top, ref - 0.012)[jj, ii]], 1)   # never above the slab surface
    q = np.stack([vid[:-1, :-1], vid[:-1, 1:], vid[1:, 1:], vid[1:, :-1]], -1).reshape(-1, 4)
    q = q[(q >= 0).all(1)]
    me = bpy.data.meshes.new("_Seam"); me.from_pydata(co.tolist(), [], q.tolist()); me.update()
    seam = bpy.data.objects.new("_Seam", me); col.objects.link(seam)
    dec = seam.modifiers.new("Dec", "DECIMATE"); dec.ratio = 0.06
    for o in bpy.context.selected_objects: o.select_set(False)
    seam.select_set(True); bpy.context.view_layer.objects.active = seam
    bpy.ops.object.modifier_apply(modifier="Dec")
    bm = bmesh.new(); bm.from_mesh(seam.data)
    bmesh.ops.triangulate(bm, faces=bm.faces[:])
    for _ in range(2): bmesh.ops.beautify_fill(bm, faces=bm.faces[:], edges=bm.edges[:])
    uvl = bm.loops.layers.uv.new("UVMap")
    box_uv(bm.faces[:], uvl)
    # 2) masses : big chunks half caught in the walls, along the fissures near the centre
    cand = np.argwhere((A > 0.15) & (A < 0.45) & (C > 0.06) & (T < 0.55) & (f["fade"] > 0.5))
    rng.shuffle(cand)
    placed = []
    for j, i in cand:
        if len(placed) >= 9: break
        x, y = xs[i], xs[j]
        if any((x - a) ** 2 + (y - b) ** 2 < 0.55 ** 2 for a, b in placed): continue
        size = rng.uniform(0.16, 0.30)
        depth_c = rng.uniform(0.10, 0.18)
        zc = ref[j, i] - depth_c
        lim = ref[j, i] + (0.07 if r[j, i] > 1.9 else -0.015)        # under the drill platform : stays below
        size = min(size, (lim - zc) / 0.8)
        if size < 0.1: continue
        chunk(bm, uvl, Vector((x, y, zc)), size, rng, 2, cuts=(5, 8), dcut=(0.35, 0.7), flat=(0.6, 0.9))
        placed.append((x, y))
    # 3) chips : rare small pieces at the foot of the walls
    cand = np.argwhere((C > 0.06) & (A > 0.35) & (T < 0.8))
    for k in rng.choice(len(cand), min(45, len(cand)), replace=False):
        j, i = cand[k]
        size = rng.uniform(0.015, 0.045)
        zc = max(rock[j, i], min(top[j, i], ref[j, i] - 0.012)) + size * 0.25
        chunk(bm, uvl, Vector((xs[i], xs[j], zc)), size, rng, 1 if size > 0.03 else 0)
    me = bpy.data.meshes.new("Ore"); bm.to_mesh(me); bm.free()
    bpy.data.objects.remove(seam)
    me.shade_smooth(); me.set_sharp_from_angle(angle=math.radians(32))
    ob = bpy.data.objects.new("Ore", me); col.objects.link(ob)
    return ob, len(placed)

# ------------------------------------------------------------------ ore texture (tileable, no cracks)
def ore_textures(ore, model, folder):
    g = (np.arange(ORE_TEX) + 0.5) / ORE_TEX
    U, V = np.meshgrid(g, g)
    n, n2 = TileNoise(ore["seed"] + 80), TileNoise(ore["seed"] + 81)
    oc = lerp(ore["vein"], ore["vein2"], sstep(0.3, 0.7, n.fbm(U, V, 3, 4)))
    oc = lerp(oc, ore["halo"], 0.3 * sstep(0.6, 0.8, n2.fbm(U, V, 5, 4)))
    oc = oc * (0.78 + 0.22 * n2.fbm(U, V, 40, 2))[..., None]
    gl = sstep(1 - ore["glint_k"], 1 - ore["glint_k"] + 0.03, n.fbm(U + 0.3, V, 48, 2))
    oc = lerp(oc, ore["glint"], 0.45 * gl)
    smooth = np.clip(ore["vein_s"] + 0.12 + 0.14 * (n.fbm(U, V + 0.4, 6, 3) - 0.5) + 0.2 * gl, 0, 1)
    metal = ore["glint_m"] * gl
    hf = 0.0015 * n2.fbm(U, V, 24, 3)
    px = ORE_TILE / ORE_TEX
    gx = (np.roll(hf, -1, 1) - np.roll(hf, 1, 1)) / (2 * px); gy = (np.roll(hf, -1, 0) - np.roll(hf, 1, 0)) / (2 * px)
    nl = np.sqrt(gx * gx + gy * gy + 1); one = np.ones_like(U)
    nrm = np.dstack([(-gx / nl) * 0.5 + 0.5, (-gy / nl) * 0.5 + 0.5, (1 / nl) * 0.5 + 0.5, one])
    p = model + "_Ore"
    def save(name, arr, nc):
        img = bpy.data.images.get(name)
        if img: bpy.data.images.remove(img)
        img = bpy.data.images.new(name, ORE_TEX, ORE_TEX, alpha=True)
        img.colorspace_settings.name = "Non-Color" if nc else "sRGB"
        img.pixels.foreach_set(arr.astype(np.float32).ravel())
        img.filepath_raw = f"{folder}/{name}.png"; img.file_format = "PNG"; img.save()
        img.alpha_mode = "CHANNEL_PACKED"
        return img
    return (save(p + "_BaseColor", np.dstack([np.clip(oc, 0, 1), one]), False),
            save(p + "_Normal", nrm, True),
            save(p + "_MetallicSmoothness", np.dstack([metal, metal, metal, smooth]), True))

def make_named_material(name, imgs):
    m = make_material(name, imgs)          # creates "M_" + name
    return m

# ------------------------------------------------------------------ build
def build_v3(kind, samples=12):
    ore = ORES[kind]; model = "Deposit_" + kind
    folder = f"{PROJ}/Assets/Resources/Models/{model}"
    col = bpy.data.collections.get("Models")
    hpcol = bpy.data.collections.get("_HighPoly") or bpy.data.collections.new("_HighPoly")
    if hpcol.name not in bpy.context.scene.collection.children: bpy.context.scene.collection.children.link(hpcol)
    hpcol.hide_render = False
    for o in list(bpy.data.objects):
        if o.name in (model, "Body", "Ore", "DrillPoint", HP_NAME, "_HP_Rubble", "_HP_Ore") or (o.parent and o.parent.name == model):
            bpy.data.objects.remove(o)
    mat = bpy.data.materials.get("M_" + model) or bpy.data.materials.new("M_" + model)
    root = bpy.data.objects.new(model, None); col.objects.link(root)
    lo = build_mesh(model, ore, mat, col); lo.parent = root
    oreob, masses = build_ore_geo(ore, col); oreob.parent = root
    omat = make_named_material(model + "_Ore", ore_textures(ore, model, folder))
    oreob.data.materials.clear(); oreob.data.materials.append(omat)
    dp = bpy.data.objects.new("DrillPoint", None); col.objects.link(dp)
    dp.empty_display_type = "SINGLE_ARROW"; dp.location = (0, 0, PLATEAU); dp.parent = root
    # rock high-poly, with a copy of the ore inside it (contact shadows where the ore meets the rock)
    xs, f, h, attrs = rock_hp(ore)
    hp = grid_mesh(HP_NAME, xs, h, f["r"] < f["Rn"] + 0.12, attrs, hpcol)
    bm = bmesh.new(); bm.from_mesh(hp.data); bm.from_mesh(oreob.data); bm.to_mesh(hp.data); bm.free()
    nv = len(hp.data.vertices)
    for k in ("ors", "lips", "keepw"):
        hp.data.attributes[k].data.foreach_set("value", np.zeros(nv, np.float32))
    co = np.zeros(nv * 3); hp.data.vertices.foreach_get("co", co); co = co.reshape(-1, 3)
    hbz = np.zeros(nv, np.float32); hp.data.attributes["hbz"].data.foreach_get("value", hbz)
    no = len(oreob.data.vertices)
    hbz[-no:] = co[-no:, 2]                                         # the ore copy : no depth darkening
    hp.data.attributes["hbz"].data.foreach_set("value", hbz)
    hp.data.materials.clear(); hp.data.materials.append(hp_material(ore))
    sc = bpy.context.scene; eng = sc.render.engine; sc.render.engine = "CYCLES"
    try: sc.cycles.device = "GPU"
    except Exception: pass
    sc.cycles.samples = samples
    i_col = new_img("_bk_col", False); bake(lo, [hp], "DIFFUSE", i_col, pass_filter={"COLOR"})
    sc.cycles.samples = 4
    i_nrm = new_img("_bk_nrm", True); bake(lo, [hp], "NORMAL", i_nrm, normal_space="TANGENT")
    i_rgh = new_img("_bk_rgh", True); bake(lo, [hp], "ROUGHNESS", i_rgh)
    sc.render.engine = eng
    pc, pn, pr = pixels(i_col), pixels(i_nrm), pixels(i_rgh)
    one = np.ones((TEX, TEX)); zero = 0 * one
    imgs = (save_png(model + "_BaseColor", np.dstack([pc[..., :3], one]), f"{folder}/{model}_BaseColor.png", False),
            save_png(model + "_Normal", np.dstack([pn[..., :3], one]), f"{folder}/{model}_Normal.png", True),
            save_png(model + "_MetallicSmoothness", np.dstack([zero, zero, zero, 1 - pr[..., 0]]),
                     f"{folder}/{model}_MetallicSmoothness.png", True))
    make_material(model, imgs)
    hpcol.hide_render = True
    tris = lambda o: sum(len(p.vertices) - 2 for p in o.data.polygons)
    return dict(model=model, body_tris=tris(lo), ore_tris=tris(oreob), masses=masses)

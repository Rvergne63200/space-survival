# Deposit_<Ore>, high definition pipeline : a detailed high-poly (1 cm heightfield with real relief : ore crust,
# rock ridges, rubble in the fissures), a Cycles node material (rock and ore ramps, cavity dirt from ambient
# occlusion, lighter edges from the bevel node, darkness with depth), baked onto the game mesh of dp_build.py :
# BaseColor (albedo with the cavity dirt), Normal (tangent) and MetallicSmoothness (from the roughness).
import bpy, bmesh, math
import numpy as np
from mathutils import Vector, noise as mnoise

SP = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival/ArtPipeline/Blender"
exec(open(SP + r"/dp_build.py", encoding="utf8").read())

HP_GRID = 0.01
HP_NAME = "_HP_Body"
DEPTH_RAMP = (0.03, 0.16)   # depth (m) where the walls start / finish fading to DEEP

def hp_fields(ore):
    xs = np.arange(-S, S + 1e-6, HP_GRID)
    f = fields(xs, xs, ore, fine=True)
    X, Y, C, V, hb = f["X"], f["Y"], f["C"], f["V"], f["hb"]
    n = Noise(ore["seed"] + 20); n2 = Noise(ore["seed"] + 21)
    # ore coverage : crisp, broken edge, clumps thinning out, always ore on the lip and in the walls
    e = V + 0.30 * (n2.fbm(X, Y, 1.6, 5) - 0.5) + 0.22 * (n.fbm(X, Y, 6.0, 3) - 0.5) + 0.08 * (n2.fbm(X, Y, 20.0, 2) - 0.5)
    # the ore thins out towards the tips of the fissures : fewer outcrops, then the walls turn back to rock
    # in patches (broken by noise, not a gradient)
    T = f["T"]
    e = e - 0.35 * sstep(0.3, 1.0, T)
    cov = sstep(0.585, 0.60, e)
    clumps = sstep(0.62, 0.65, n.fbm(X + 50, Y, 9.0, 3)) * sstep(0.44, 0.585, e)
    # lip : smooth field (a boolean mask on the 1 cm grid gives a sawtooth outline once interpolated)
    lip = blur(sstep(0.0, 0.004, C), 10) * 3.0
    keep = T + 0.35 * (n2.fbm(X - 30, Y, 4.0, 4) - 0.5) + 0.12 * (n.fbm(X, Y - 30, 14.0, 2) - 0.5)
    walls = np.clip(lip, 0, 1) * (1 - sstep(0.66, 0.70, keep))
    orem = np.maximum(np.maximum(cov, clumps), walls) * sstep(0.0, 0.3, f["fade"])
    wall = sstep(0.0, 0.01, C)
    # relief : ore crust standing slightly proud of the rock, ridged rock, rougher ore
    ridge = (1 - np.abs(2 * n.fbm(X, Y, 5.0, 4) - 1)) ** 3
    pits = sstep(0.68, 0.74, n2.fbm(X + 13, Y, 14.0, 3))
    rockd = 0.008 * ridge + 0.003 * n2.fbm(X, Y, 20.0, 2) - 0.004 * pits
    crust = (0.006 + 0.006 * n.fbm(X - 7, Y, 8.0, 3)) * (1 - wall)
    cracky = (1 - np.abs(2 * n2.fbm(X, Y, 12.0, 3) - 1)) ** 2
    h = blur(f["height"], 2) + (1 - orem) * rockd + orem * (crust + 0.004 * cracky)
    # attributes for the shader. The depth is NOT stored per vertex : on a steep wall two neighbouring vertices
    # are centimetres apart in height and the interpolation draws teeth. The shader computes it per pixel as
    # hbz - z (smooth reference surface minus the real height).
    fadef = sstep(0.0, 0.3, f["fade"])
    attrs = {"ors": np.maximum(cov, clumps) * fadef, "lips": np.clip(lip, 0, 1) * fadef,
             "keepw": 1 - sstep(0.66, 0.70, keep), "hbz": blur(hb, 2)}
    return xs, f, h, orem, C, attrs

def grid_mesh(name, xs, h, inside, attrs, col):
    vid = -np.ones(inside.shape, dtype=np.int64); vid[inside] = np.arange(inside.sum())
    jj, ii = np.nonzero(inside)
    co = np.stack([xs[ii], xs[jj], h[jj, ii]], 1)
    q = np.stack([vid[:-1, :-1], vid[:-1, 1:], vid[1:, 1:], vid[1:, :-1]], -1).reshape(-1, 4)
    q = q[(q >= 0).all(1)]
    me = bpy.data.meshes.new(name)
    me.vertices.add(len(co)); me.vertices.foreach_set("co", co.ravel())
    me.loops.add(q.size); me.loops.foreach_set("vertex_index", q.ravel())
    me.polygons.add(len(q)); me.polygons.foreach_set("loop_start", np.arange(0, q.size, 4))
    me.update(calc_edges=True)
    for k, a in attrs.items():
        at = me.attributes.new(k, "FLOAT", "POINT"); at.data.foreach_set("value", a[jj, ii].astype(np.float32))
    me.shade_smooth()
    ob = bpy.data.objects.new(name, me); col.objects.link(ob)
    return ob

def rubble(ore, xs, f, h, C, col, count=180):
    """Ore pebbles fallen at the bottom of the fissures and around the central hole."""
    rng = np.random.default_rng(ore["seed"] + 30)
    cand = np.argwhere((C > 0.05) & (f["A"] > 0.55))
    bm = bmesh.new()
    for k in rng.choice(len(cand), min(count, len(cand)), replace=False):
        j, i = cand[k]
        r = rng.uniform(0.012, 0.035)
        res = bmesh.ops.create_icosphere(bm, subdivisions=2, radius=r)
        sx, sy, sz = rng.uniform(0.7, 1.3), rng.uniform(0.7, 1.3), rng.uniform(0.45, 0.8)
        off = Vector(rng.uniform(-50, 50, 3))
        c = Vector((xs[i], xs[j], h[j, i] + r * sz * 0.35))
        for v in res["verts"]:
            p = v.co.copy()
            d = 1 + 0.35 * mnoise.noise(p * (6 / r * 0.02) + off)
            v.co = c + Vector((p.x * sx, p.y * sy, p.z * sz)) * d
    me = bpy.data.meshes.new("_HP_Rubble"); bm.to_mesh(me); bm.free()
    nv = len(me.vertices)
    co = np.zeros(nv * 3); me.vertices.foreach_get("co", co); co = co.reshape(-1, 3)
    for k, v in (("ors", np.ones(nv)), ("lips", np.zeros(nv)), ("keepw", np.ones(nv)),
                 ("hbz", co[:, 2] + bilinear(C, xs, co[:, 0], co[:, 1]))):      # hbz - z = depth of the fissure there
        me.attributes.new(k, "FLOAT", "POINT").data.foreach_set("value", v.astype(np.float32))
    ob = bpy.data.objects.new("_HP_Rubble", me); col.objects.link(ob)
    return ob

# ------------------------------------------------------------------ node material of the high-poly
def hp_material(ore):
    m = bpy.data.materials.get("_HP_Mat") or bpy.data.materials.new("_HP_Mat")
    m.use_nodes = True; nt = m.node_tree; nt.nodes.clear(); L = nt.links.new
    N = lambda t: nt.nodes.new(t)
    def rgb(c): return (*[float(x) ** 2.2 for x in c], 1.0)       # sRGB -> linear
    def ramp(fac, stops):
        r = N("ShaderNodeValToRGB"); cr = r.color_ramp
        while len(cr.elements) > 1: cr.elements.remove(cr.elements[-1])
        cr.elements[0].position = stops[0][0]; cr.elements[0].color = stops[0][1]
        for p, c in stops[1:]: e = cr.elements.new(p); e.color = c
        L(fac, r.inputs[0]); return r.outputs[0]
    def noise(scale, detail, rough, dist=0.0, w=0.0):
        t = N("ShaderNodeTexNoise"); t.noise_dimensions = "3D"
        t.inputs["Scale"].default_value = scale; t.inputs["Detail"].default_value = detail
        t.inputs["Roughness"].default_value = rough; t.inputs["Distortion"].default_value = dist
        L(tc.outputs["Object"], t.inputs["Vector"]); return t.outputs["Fac"]
    def mix(a, b, fac, mode="MIX"):
        x = N("ShaderNodeMix"); x.data_type = "RGBA"; x.blend_type = mode
        if isinstance(fac, float): x.inputs[0].default_value = fac
        else: L(fac, x.inputs[0])
        for s, v in ((6, a), (7, b)):
            if isinstance(v, tuple): x.inputs[s].default_value = v
            else: L(v, x.inputs[s])
        return x.outputs[2]
    def math(op, a, b):
        x = N("ShaderNodeMath"); x.operation = op
        for s, v in ((0, a), (1, b)):
            if isinstance(v, float): x.inputs[s].default_value = v
            else: L(v, x.inputs[s])
        return x.outputs[0]
    tc = N("ShaderNodeTexCoord")
    attr = lambda name: (lambda a: (setattr(a, "attribute_name", name), a.outputs["Fac"])[1])(N("ShaderNodeAttribute"))
    sep = N("ShaderNodeSeparateXYZ"); L(tc.outputs["Object"], sep.inputs[0])
    depth = math("SUBTRACT", attr("hbz"), sep.outputs["Z"])                       # per pixel, exact on the walls
    wallpix = math("MINIMUM", math("MAXIMUM", math("MULTIPLY", math("SUBTRACT", depth, 0.012), 125.0), 0.0), 1.0)
    ore_a = math("MAXIMUM", attr("ors"), math("MULTIPLY", math("MAXIMUM", attr("lips"), wallpix), attr("keepw")))
    # crisp, finely broken ore edge (the attribute itself is interpolated every centimetre)
    ore_f = ramp(math("ADD", ore_a, math("MULTIPLY", math("SUBTRACT", noise(16.0, 4, 0.55), 0.5), 0.22)),
                 [(0.47, (0, 0, 0, 1)), (0.53, (1, 1, 1, 1))])
    ao = N("ShaderNodeAmbientOcclusion"); ao.inputs["Distance"].default_value = 0.06; ao.samples = 16; ao.only_local = True
    cav = ao.outputs["AO"]
    ao2 = N("ShaderNodeAmbientOcclusion"); ao2.inputs["Distance"].default_value = 0.3; ao2.samples = 16; ao2.only_local = True
    deepao = ao2.outputs["AO"]
    bev = N("ShaderNodeBevel"); bev.inputs["Radius"].default_value = 0.008; bev.samples = 8
    geo = N("ShaderNodeNewGeometry")
    dot = N("ShaderNodeVectorMath"); dot.operation = "DOT_PRODUCT"
    L(bev.outputs[0], dot.inputs[0]); L(geo.outputs["Normal"], dot.inputs[1])
    edge = math("MULTIPLY", math("SUBTRACT", 1.0, dot.outputs["Value"]), 6.0)      # 0 flat, ~1 on sharp edges
    edge = math("MINIMUM", edge, 1.0)
    # rock : large patches, ochre blotches, grain, dirt in the cavities, lighter worn edges
    rock = ramp(noise(0.8, 6, 0.55, 0.3), [(0.30, rgb(ROCK_D)), (0.52, rgb(ROCK)), (0.72, rgb(ROCK_L))])
    rock = mix(rock, rgb(ROCK_L), math("MULTIPLY", ramp(noise(3.0, 5, 0.6), [(0.55, (0, 0, 0, 1)), (0.75, (1, 1, 1, 1))]), 0.45))
    rock = mix(rock, ramp(noise(45.0, 3, 0.7), [(0.2, (0.85, 0.85, 0.85, 1)), (0.8, (1.12, 1.12, 1.12, 1))]), 1.0, "MULTIPLY")
    rock = mix(rock, ramp(cav, [(0.0, rgb(srgb("#5a2614"))), (0.8, (1, 1, 1, 1))]), 1.0, "MULTIPLY")
    rock = mix(rock, rgb(ROCK_L * 1.1), math("MULTIPLY", edge, 0.5))
    # ore : dark and mid tones, brownish patches, thin crisp fractures, cavity darker, edges catch the light
    oc = ramp(noise(2.0, 6, 0.55, 0.2), [(0.35, rgb(ore["vein"])), (0.65, rgb(ore["vein2"]))])
    oc = mix(oc, rgb(ore["halo"]), math("MULTIPLY", ramp(noise(3.5, 4, 0.6), [(0.58, (0, 0, 0, 1)), (0.78, (1, 1, 1, 1))]), 0.4))
    vor = N("ShaderNodeTexVoronoi"); vor.feature = "DISTANCE_TO_EDGE"; vor.inputs["Scale"].default_value = 22.0
    vor.inputs["Randomness"].default_value = 0.9; L(tc.outputs["Object"], vor.inputs["Vector"])
    oc = mix(oc, ramp(vor.outputs["Distance"], [(0.0, (0.35, 0.35, 0.35, 1)), (0.035, (1, 1, 1, 1))]), 1.0, "MULTIPLY")
    oc = mix(oc, ramp(cav, [(0.0, (0.25, 0.25, 0.25, 1)), (0.8, (1, 1, 1, 1))]), 1.0, "MULTIPLY")
    oc = mix(oc, ramp(deepao, [(0.2, (0.15, 0.15, 0.15, 1)), (0.9, (1, 1, 1, 1))]), 1.0, "MULTIPLY")
    oc = mix(oc, rgb(ore["glint"]), math("MULTIPLY", edge, 0.2))
    # depth : the walls fade to black
    col = mix(rock, oc, ore_f)
    col = mix(col, rgb(DEEP), ramp(depth, [(DEPTH_RAMP[0], (0, 0, 0, 1)), (DEPTH_RAMP[1], (1, 1, 1, 1))]))
    bs = N("ShaderNodeBsdfPrincipled"); out = N("ShaderNodeOutputMaterial")
    L(col, bs.inputs["Base Color"])
    rough = math("ADD", math("MULTIPLY", ore_f, -(0.88 - (1 - ore["vein_s"]))), 0.88)
    rough = math("ADD", rough, math("MULTIPLY", math("SUBTRACT", noise(6.0, 3, 0.5), 0.5), 0.15))
    L(rough, bs.inputs["Roughness"])
    L(bs.outputs[0], out.inputs["Surface"])
    return m

# ------------------------------------------------------------------ bake
def bake(lo, hps, kind_, img, **kw):
    mat = bpy.data.materials.get("_BakeTarget") or bpy.data.materials.new("_BakeTarget")
    mat.use_nodes = True; nt = mat.node_tree; nt.nodes.clear()
    t = nt.nodes.new("ShaderNodeTexImage"); t.image = img; nt.nodes.active = t
    old = lo.data.materials[0]; lo.data.materials[0] = mat
    for o in bpy.context.view_layer.objects: o.select_set(False)
    for o in hps: o.select_set(True)
    lo.select_set(True); bpy.context.view_layer.objects.active = lo
    bpy.ops.object.bake(type=kind_, use_selected_to_active=True, cage_extrusion=0.08, max_ray_distance=0.2,
                        margin=16, margin_type="EXTEND", use_clear=True, **kw)
    lo.data.materials[0] = old

def new_img(name, noncolor, float_=False):
    img = bpy.data.images.get(name)
    if img: bpy.data.images.remove(img)
    img = bpy.data.images.new(name, TEX, TEX, alpha=True, float_buffer=float_)
    img.colorspace_settings.name = "Non-Color" if noncolor else "sRGB"
    return img

def pixels(img):
    px = np.empty(TEX * TEX * 4, np.float32); img.pixels.foreach_get(px); return px.reshape(TEX, TEX, 4)

def build_hd(kind, samples=16):
    ore = ORES[kind]; model = "Deposit_" + kind
    folder = f"{PROJ}/Assets/Resources/Models/{model}"
    col = bpy.data.collections.get("Models")
    hpcol = bpy.data.collections.get("_HighPoly") or bpy.data.collections.new("_HighPoly")
    if hpcol.name not in bpy.context.scene.collection.children: bpy.context.scene.collection.children.link(hpcol)
    hpcol.hide_render = False
    for o in list(bpy.data.objects):
        if o.name in (model, "Body", "DrillPoint", HP_NAME, "_HP_Rubble") or (o.parent and o.parent.name == model):
            bpy.data.objects.remove(o)
    tmp = bpy.data.materials.get("M_" + model) or bpy.data.materials.new("M_" + model)
    root = bpy.data.objects.new(model, None); col.objects.link(root)
    lo = build_mesh(model, ore, tmp, col); lo.parent = root
    dp = bpy.data.objects.new("DrillPoint", None); col.objects.link(dp)
    dp.empty_display_type = "SINGLE_ARROW"; dp.location = (0, 0, PLATEAU); dp.parent = root
    # high-poly
    xs, f, h, orem, C, attrs = hp_fields(ore)
    inside = f["r"] < f["Rn"] + 0.12
    hp = grid_mesh(HP_NAME, xs, h, inside, attrs, hpcol)
    rb = rubble(ore, xs, f, h, C, hpcol)
    m = hp_material(ore)
    for o in (hp, rb): o.data.materials.clear(); o.data.materials.append(m)
    sc = bpy.context.scene; eng = sc.render.engine; sc.render.engine = "CYCLES"
    try: sc.cycles.device = "GPU"
    except Exception: pass
    sc.cycles.samples = samples
    i_col = new_img("_bk_col", False); bake(lo, [hp, rb], "DIFFUSE", i_col, pass_filter={"COLOR"})
    sc.cycles.samples = 4
    i_nrm = new_img("_bk_nrm", True); bake(lo, [hp, rb], "NORMAL", i_nrm, normal_space="TANGENT")
    i_rgh = new_img("_bk_rgh", True); bake(lo, [hp, rb], "ROUGHNESS", i_rgh)
    sc.render.engine = eng
    pc, pn, pr = pixels(i_col), pixels(i_nrm), pixels(i_rgh)
    one = np.ones((TEX, TEX))
    smooth = 1 - pr[..., 0]
    imgs = (save_png(model + "_BaseColor", np.dstack([pc[..., :3], one]), f"{folder}/{model}_BaseColor.png", False),
            save_png(model + "_Normal", np.dstack([pn[..., :3], one]), f"{folder}/{model}_Normal.png", True),
            save_png(model + "_MetallicSmoothness", np.dstack([0 * one, 0 * one, 0 * one, smooth]),
                     f"{folder}/{model}_MetallicSmoothness.png", True))
    make_material(model, imgs)
    hpcol.hide_render = True
    return dict(model=model, hp_verts=len(hp.data.vertices), lo_tris=sum(len(p.vertices) - 2 for p in lo.data.polygons))

# Storage kit "family A" (chamfered white caissons, graphite corner posts) — three SEPARATE models, one .blend each.
#   Crate_Small  0.80 x 0.60 x 0.55   > Body, Lid (rot X, hinge at the back)          clips Lid_Open / Lid_Close
#   Crate_Large  1.60 x 1.00 x 0.90   > Body, Lid_L / Lid_R (rot Y, butterfly)        clips Lid_Open / Lid_Close
#   Cabinet      1.00 x 0.60 x 2.00   > Body, Door_L / Door_R (rot Z, front hinges)   clips Doors_Open / Doors_Close
# Hollow and empty inside (graphite). Crates are stackable : the upper crate's root goes on the empty "StackPoint".
# build(model) clears the scene and builds one model.
import bpy, bmesh, math
from mathutils import Matrix, Vector
SP = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival/ArtPipeline/Blender"
exec(open(SP + r"\al_build.py", encoding="utf8").read())

def T(*v): return Matrix.Translation(Vector(v))
RY90 = Matrix.Rotation(math.pi / 2, 4, 'Y')       # local +Z -> +X
RX90 = Matrix.Rotation(math.pi / 2, 4, 'X')       # local +Z -> -Y

CRATES = {   # w, d, h (top of the posts = stacking plane), hz (top of the body, under the lid)
    "Crate_Small": dict(w=0.80, d=0.60, h=0.55, hz=0.45, dbl=False),
    "Crate_Large": dict(w=1.60, d=1.00, h=0.90, hz=0.78, dbl=True),
}
POST = 0.13                  # corner post size
HINGE_OFF = 0.02             # lid hinge axis 2 cm outside the lid edge : fixed knuckles never cut into the lid
PL_Z0 = -0.005               # plinth bottom (slightly sunk) ; stack point = h - PL_Z0
WALL = 0.04
LID_OPEN_S = math.radians(-105)
LID_OPEN_L = math.radians(110)
CAB = dict(w=1.00, d=0.60, h=2.00)
DOOR_OPEN = math.radians(100)

def cham(a0, a1, b0, b1, c):
    return [(a0 + c, b0), (a1 - c, b0), (a1, b0 + c), (a1, b1 - c), (a1 - c, b1), (a0 + c, b1), (a0, b1 - c), (a0, b0 + c)]

def solid(bm, poly, z0, z1, m_side, m_bot=None, m_top=None, eb=EB):
    """extrude a CCW (x, y) polygon (may be non convex) between z0 and z1, edge-broken."""
    tmp = bmesh.new()
    a = [tmp.verts.new((x, y, z0)) for x, y in poly]; b = [tmp.verts.new((x, y, z1)) for x, y in poly]
    n = len(poly)
    for i in range(n):
        j = (i + 1) % n
        f = tmp.faces.new((a[i], a[j], b[j], b[i])); f.material_index = m_side[i] if isinstance(m_side, list) else m_side
    tmp.faces.new(a[::-1]).material_index = m_side if m_bot is None else m_bot
    tmp.faces.new(b).material_index = m_side if m_top is None else m_top
    bmesh.ops.recalc_face_normals(tmp, faces=tmp.faces)
    _bevel_tmp(tmp, eb)
    _copy_into(bm, tmp, None); tmp.free()

def hollow_z(bm, outer, inner, z0, zf, z1, mo=WHITE, mi=DARK, mrim=DARK, mfloor=DARK):
    """open-top tray : outer polygon z0..z1, cavity (inner polygon) from the floor zf up to z1."""
    tmp = bmesh.new()
    O0 = [tmp.verts.new((x, y, z0)) for x, y in outer]; O1 = [tmp.verts.new((x, y, z1)) for x, y in outer]
    I1 = [tmp.verts.new((x, y, z1)) for x, y in inner]; I0 = [tmp.verts.new((x, y, zf)) for x, y in inner]
    n = len(outer)
    tmp.faces.new(O0[::-1]).material_index = mo
    for i in range(n):
        j = (i + 1) % n
        tmp.faces.new((O0[i], O0[j], O1[j], O1[i])).material_index = mo
        tmp.faces.new((O1[i], O1[j], I1[j], I1[i])).material_index = mrim
        tmp.faces.new((I1[i], I1[j], I0[j], I0[i])).material_index = mi
    tmp.faces.new(I0).material_index = mfloor
    bmesh.ops.recalc_face_normals(tmp, faces=tmp.faces)
    _bevel_tmp(tmp, EB)
    _copy_into(bm, tmp, None); tmp.free()

# ================================================================== crates
def crate_body(c):
    w, d, h, hz = c["w"], c["d"], c["h"], c["hz"]
    x0, x1, y0, y1 = -w / 2, w / 2, -d / 2, d / 2
    bm = bmesh.new()
    cbox(bm, (w + 0.04, d + 0.04, 0.08 - PL_Z0), (0, 0, (0.08 + PL_Z0) / 2), DARK, 0.03)          # plinth
    hollow_z(bm, cham(x0, x1, y0, y1, 0.08), cham(x0 + WALL, x1 - WALL, y0 + WALL, y1 - WALL, 0.05), 0.08 - 0.01, 0.12, hz)
    for sx in (-1, 1):                                                                            # corner posts (stop under the lid)
        for sy in (-1, 1):
            cx, cy = sx * (w / 2 - 0.05), sy * (d / 2 - 0.05)
            pt = hz - 0.008                                                                       # below the rim : no shared plane
            cbox(bm, (POST, POST, pt - 0.06), (cx, cy, (pt + 0.06) / 2), DARK, 0.03)
            add_screw(bm, (cx, cy + sy * 0.066, 0.25 * h), (0, sy, 0), r=0.012, mat=METAL)       # on the post's outer face
    # front : neon strip in a graphite channel ; sides : graphite inset panels
    ln = w - 2 * POST - 0.08
    cbox(bm, (ln + 0.04, 0.012, 0.06), (0, y0 - 0.004, hz - 0.13), DARK, 0.01, 0.002)
    pill(bm, (ln, 0.014, 0.025), (0, y0 - 0.011, hz - 0.13), NEON)
    for sx in (-1, 1):
        cbox(bm, (0.012, d - 2 * POST - 0.06, hz - 0.30), (sx * (w / 2 + 0.002), 0, 0.08 + (hz - 0.08) / 2 - 0.02), DARK, 0.02, 0.002)
    # fixed hinge knuckles on the lid axis (HINGE_OFF behind the lid edge), alternating with the lid's knuckles
    # (5 mm gaps). Each sits on an L bracket : a foot under the rim, an upright behind the lid edge.
    za = h - 0.018
    if not c["dbl"]:
        ya = d / 2 + 0.005 + HINGE_OFF
        for sx in (-1, 1):
            for off in (-0.115, 0.115):
                x = sx * 0.16 + off
                oct_cyl(bm, 0.018, -0.04, 0.04, DARK, T(x, ya, za) @ RY90, ch=0.004)
                cbox(bm, (0.08, 0.018, za - (hz - 0.06)), (x, ya, (za + hz - 0.06) / 2), DARK, 0.006)
                y_in = d / 2 - 0.004
                cbox(bm, (0.08, ya + 0.006 - y_in, 0.05), (x, (ya + 0.006 + y_in) / 2, hz - 0.035), DARK, 0.006)   # foot stops inside the upright
    else:
        for sx in (-1, 1):
            xa = sx * (w / 2 + 0.005 + HINGE_OFF)
            for sy in (-1, 1):
                for off in (-0.125, 0.125):
                    y = sy * (d / 2 - 0.2) + off
                    oct_cyl(bm, 0.018, -0.04, 0.04, DARK, T(xa, y, za) @ RX90, ch=0.004)
                    cbox(bm, (0.018, 0.08, za - (hz - 0.06)), (xa, y, (za + hz - 0.06) / 2), DARK, 0.006)
                    x_in = sx * (w / 2 - 0.004)
                    cbox(bm, (abs(xa + sx * 0.006 - x_in), 0.08, 0.05), ((xa + sx * 0.006 + x_in) / 2, y, hz - 0.035), DARK, 0.006)
    return bm

def lid_panel(bm, x0, x1, y0, y1, z0, z1, c):
    cbox(bm, (x1 - x0, y1 - y0, z1 - z0), ((x0 + x1) / 2, (y0 + y1) / 2, (z0 + z1) / 2), WHITE, 0.035)
    cbox(bm, (x1 - x0 - 0.20, y1 - y0 - 0.20, 0.008), ((x0 + x1) / 2, (y0 + y1) / 2, z1 + 0.002), DARK, 0.03, 0.002)
    # stacking studs above the corner posts that lie under this panel
    for sx in (-1, 1):
        for sy in (-1, 1):
            cx, cy = sx * (c["w"] / 2 - 0.05), sy * (c["d"] / 2 - 0.05)
            if x0 < cx < x1:
                add_revolve(bm, [(0, z1), (0.032, z1), (0.024, z1 + 0.014), (0, z1 + 0.014)], [DARK] * 3, 8,
                            T(cx, cy, 0) @ Matrix.Rotation(math.pi / 8, 4, 'Z'))

def crate_lids(c):
    """returns [(name, bm, origin)] built in world coords. The lid covers the whole top (posts stop under it)."""
    w, d, h, hz = c["w"], c["d"], c["h"], c["hz"]
    xi = w / 2 + 0.005                            # lid half span : 5 mm past the tray
    y0, y1 = -d / 2 - 0.005, d / 2 + 0.005
    z0, z1 = hz + 0.003, h
    out = []
    if not c["dbl"]:
        bm = bmesh.new()
        lid_panel(bm, -xi, xi, y0, y1, z0, z1, c)
        cbox(bm, (0.16, 0.03, 0.05), (0, y0 - 0.012, (z0 + z1) / 2), HAZARD, 0.012)               # handle
        ya = y1 + HINGE_OFF
        for sx in (-1, 1):
            oct_cyl(bm, 0.018, -0.07, 0.07, DARK, T(sx * 0.16, ya, h - 0.018) @ RY90, ch=0.004)
            cbox(bm, (0.12, HINGE_OFF + 0.02, 0.026), (sx * 0.16, y1 + HINGE_OFF / 2 - 0.005, h - 0.018), DARK, 0.006)   # strap
        out.append(("Lid", bm, (0, ya, h - 0.018)))
    else:
        for side, name in ((-1, "Lid_L"), (1, "Lid_R")):
            bm = bmesh.new()
            a, b = (-xi, -0.004) if side < 0 else (0.004, xi)
            lid_panel(bm, a, b, y0, y1, z0, z1, c)
            hx = (a if side < 0 else b) + side * HINGE_OFF                                       # axis outside the lid edge
            hxh = b - 0.10 if side < 0 else a + 0.10                                             # handle near the meeting edge
            cbox(bm, (0.14, 0.03, 0.05), (hxh, y0 - 0.012, (z0 + z1) / 2), HAZARD, 0.012)
            for sy in (-1, 1):
                oct_cyl(bm, 0.018, -0.08, 0.08, DARK, T(hx, sy * (d / 2 - 0.2), h - 0.018) @ RX90, ch=0.004)
                cbox(bm, (HINGE_OFF + 0.02, 0.12, 0.026), (hx - side * (HINGE_OFF / 2 - 0.005), sy * (d / 2 - 0.2), h - 0.018), DARK, 0.006)  # strap
            out.append((name, bm, (hx, 0, h - 0.018)))
    return out

# ================================================================== cabinet
def cabinet_body():
    w, d, h = CAB["w"], CAB["d"], CAB["h"]
    x0, x1, y0, y1 = -w / 2, w / 2, -d / 2, d / 2
    t, cf, cb = 0.05, 0.03, 0.10                  # wall, front corner chamfer (< wall), back chamfer
    bm = bmesh.new()
    cbox(bm, (w + 0.04, d + 0.04, 0.085 - PL_Z0 + 0.0), (0, 0, (0.085 + PL_Z0) / 2), DARK, 0.03)      # plinth
    outer = [(x0 + cf, y0), (x1 - cf, y0), (x1, y0 + cf), (x1, y1 - cb), (x1 - cb, y1), (x0 + cb, y1), (x0, y1 - cb), (x0, y0 + cf)]
    solid(bm, outer, 0.08, 0.15, DARK)                                                             # bottom slab (cavity floor)
    U = [(x0 + cf, y0), (x0 + t, y0), (x0 + t, y1 - t), (x1 - t, y1 - t), (x1 - t, y0), (x1 - cf, y0), (x1, y0 + cf),
         (x1, y1 - cb), (x1 - cb, y1), (x0 + cb, y1), (x0, y1 - cb), (x0, y0 + cf)]
    m = [DARK, DARK, DARK, DARK, DARK, WHITE, WHITE, WHITE, WHITE, WHITE, WHITE, WHITE]               # cavity + wall fronts graphite
    solid(bm, U, 0.15, h - 0.08, m, DARK, DARK)
    solid(bm, outer, h - 0.08, h, WHITE, DARK, WHITE)                                              # top slab (cavity ceiling graphite)
    # graphite top band, side insets, back vents
    ob = [(x0 + cf - 0.0, y0 - 0.012), (x1 - cf, y0 - 0.012), (x1 + 0.012, y0 + cf), (x1 + 0.012, y1 - cb), (x1 - cb, y1 + 0.012),
          (x0 + cb, y1 + 0.012), (x0 - 0.012, y1 - cb), (x0 - 0.012, y0 + cf)]
    solid(bm, ob, h - 0.07, h - 0.02, DARK)
    for sx in (-1, 1):
        cbox(bm, (0.012, d - 0.26, 1.2), (sx * (w / 2 + 0.002), 0.01, 1.0), DARK, 0.03, 0.002)
        for k in range(6):
            cbox(bm, (0.02, d - 0.34, 0.02), (sx * (w / 2 + 0.012), 0.01, 0.62 + k * 0.15), METAL, 0.004, 0.0)
    return bm

def cabinet_doors():
    w, d, h = CAB["w"], CAB["d"], CAB["h"]
    x0, x1 = -w / 2, w / 2
    y0 = -d / 2 - 0.006                           # door back face 6 mm in front of the body (knuckle clears the front chamfer)
    zc, hd = 1.03, 1.74                           # door z 0.16 .. 1.90 : stays under the top band (1.93)
    out = []
    for side, name in ((-1, "Door_L"), (1, "Door_R")):
        bm = bmesh.new()
        a, b = (x0 + 0.01, -0.004) if side < 0 else (0.004, x1 - 0.01)
        cbox(bm, (b - a, 0.04, hd), ((a + b) / 2, y0 - 0.02, zc), WHITE, 0.035)
        cbox(bm, (b - a - 0.10, 0.008, hd - 0.40), ((a + b) / 2, y0 - 0.042, zc), DARK, 0.03, 0.002)
        hx = a if side < 0 else b                                                                    # hinge = outer edge
        ix = b if side < 0 else a                                                                    # meeting edge
        cbox(bm, (0.03, 0.03, 0.28), (ix + side * 0.05, y0 - 0.058, 1.05), HAZARD, 0.01)              # handle (near the meeting edge)
        if side < 0:
            pill(bm, (0.02, 0.012, 1.2), (a + 0.035, y0 - 0.046, zc), NEON)
        else:
            cbox(bm, (0.24, 0.008, 0.14), ((a + b) / 2 + 0.03, y0 - 0.047, 1.62), INNER, 0.015, 0.002)  # label screen
            pill(bm, (0.04, 0.012, 0.04), ((a + b) / 2 + 0.19, y0 - 0.048, 1.62), ORANGE)
        for z in (0.35, 1.0, 1.72):                                                                   # hinge knuckles
            oct_cyl(bm, 0.015, z - 0.05, z + 0.05, DARK, T(hx, y0, 0), ch=0.004)
        out.append((name, bm, (hx, y0, 0)))
    return out

# ================================================================== setup / animation
def clear():
    for ob in list(bpy.data.objects):
        if not ob.name.startswith("_"): bpy.data.objects.remove(ob)
    for a in list(bpy.data.actions): bpy.data.actions.remove(a)
    for me in list(bpy.data.meshes):
        if me.users == 0: bpy.data.meshes.remove(me)

CLIPS = {"Crate_Small": [("Lid_Open", 1, 24), ("Lid_Close", 36, 60)],
         "Crate_Large": [("Lid_Open", 1, 30), ("Lid_Close", 42, 72)],
         "Cabinet":     [("Doors_Open", 1, 30), ("Doors_Close", 42, 72)]}

def key(ob, path, idx, f, v, interp='BEZIER'):
    getattr(ob, path)[idx] = v
    ob.keyframe_insert(path, index=idx, frame=f)
    ad = ob.animation_data
    fc = ad.action.layers[0].strips[0].channelbag(ad.action_slot).fcurves.find(path, index=idx)
    for kp in fc.keyframe_points:
        if abs(kp.co[0] - f) < 1e-4: kp.interpolation = interp

def build(model):
    clear()
    sc = bpy.context.scene
    col = bpy.data.collections.get("Models") or bpy.data.collections.new("Models")
    if col.name not in sc.collection.children: sc.collection.children.link(col)
    mats = make_mats(model)
    root = bpy.data.objects.new(model, None); col.objects.link(root); root.empty_display_size = 0.5
    if model in CRATES:
        c = CRATES[model]
        finish2("Body", crate_body(c), col, root, mats)
        movers = [finish2(n, bm, col, root, mats, origin=o) for n, bm, o in crate_lids(c)]
        sp = bpy.data.objects.new("StackPoint", None); col.objects.link(sp)
        sp.parent = root; sp.location = (0, 0, c["h"] - PL_Z0); sp.empty_display_type = 'ARROWS'; sp.empty_display_size = 0.3
    else:
        finish2("Body", cabinet_body(), col, root, mats)
        movers = [finish2(n, bm, col, root, mats, origin=o) for n, bm, o in cabinet_doors()]
    # animation : one clip pair, all movers together
    (o_name, o0, o1), (c_name, c0, c1) = CLIPS[model]
    for ob in movers:
        if model == "Crate_Small": idx, ang = 0, LID_OPEN_S
        elif model == "Crate_Large": idx, ang = 1, (-LID_OPEN_L if ob.name == "Lid_L" else LID_OPEN_L)
        else: idx, ang = 2, (-DOOR_OPEN if ob.name == "Door_L" else DOOR_OPEN)
        for f, v in ((o0, 0.0), (o1, ang), (c0, ang), (c1, 0.0)): key(ob, "rotation_euler", idx, f, v)
    sc.frame_start, sc.frame_end = 1, c1; sc.render.fps = 24; sc.frame_set(1)
    bpy.context.view_layer.update()
    tri = lambda o: sum(len(p.vertices) - 2 for p in o.data.polygons)
    return {o.name: tri(o) for o in root.children_recursive if o.type == 'MESH'}

def collisions(model, step=1):
    from mathutils.bvhtree import BVHTree
    O = bpy.data.objects; sc = bpy.context.scene; dg = bpy.context.evaluated_depsgraph_get()
    def tree(ob):
        M = ob.matrix_world
        return BVHTree.FromPolygons([M @ v.co for v in ob.data.vertices], [p.vertices[:] for p in ob.data.polygons])
    movers = [o for o in O[model].children if o.type == 'MESH' and o.name != "Body"]
    hits = []
    for f in range(1, sc.frame_end + 1, step):
        sc.frame_set(f); dg.update()
        body = tree(O["Body"]); ts = [tree(m) for m in movers]
        for i, m in enumerate(movers):
            k = len(ts[i].overlap(body))
            if k: hits.append((m.name, "Body", f, k))
            for j in range(i + 1, len(movers)):
                k = len(ts[i].overlap(ts[j]))
                if k: hits.append((m.name, movers[j].name, f, k))
    sc.frame_set(1)
    return hits

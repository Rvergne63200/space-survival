# Control panel : holographic desk (pupitre) ~ 0.84 x 0.54 x 1.13 m (hologram top ~1.78 m).
# DA white / graphite / blue neon / orange lights ; vertical holographic screen above a rising projector bar.
# ControlPanel > Body (static : plinth, white desk, graphite side panels, rear housing with an open well, console)
#              > Projector (translates Z : emitter bar rising out of the well)   clips Power_On / Power_Off
#              > Hologram (static, _Hologram material, shown by script)      > Hologram_Screen (UV quad, future UI)
# No cable socket on the model : the connector is placed in Unity.
import bpy, bmesh, math
from mathutils import Matrix, Vector
SP = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival/ArtPipeline/Blender"
exec(open(SP + r"\al_build.py", encoding="utf8").read())
HOLO = 9

def T(*v): return Matrix.Translation(Vector(v))

def cham(a0, a1, b0, b1, c):
    return [(a0 + c, b0), (a1 - c, b0), (a1, b0 + c), (a1, b1 - c), (a1 - c, b1), (a0 + c, b1), (a0, b1 - c), (a0, b0 + c)]

def hollow_z(bm, outer, inner, z0, zf, z1, mo=WHITE, mi=DARK, mrim=DARK, mfloor=DARK):
    """open-top tray : outer polygon z0..z1, cavity (inner polygon) from the floor zf up to z1 (as st_build)."""
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

# ------------------------------------------------------------------ layout (m) ; front (player side) = -Y
BW = 0.40                                   # body half width
# desk section (y, z), CCW : back, rear step (under the housing), sloped desk, front lip, leaning front, kick
BODY_P = [(0.25, 0.04), (0.25, 0.92), (0.21, 0.96), (0.04, 0.96), (0.04, 1.04), (-0.30, 0.94),
          (-0.34, 0.90), (-0.34, 0.83), (-0.22, 0.56), (-0.22, 0.10), (-0.18, 0.04)]
DESK0, DESK1 = Vector((0, -0.30, 0.94)), Vector((0, 0.04, 1.04))
SLOPE = math.atan2(DESK1.z - DESK0.z, DESK1.y - DESK0.y)
# rear housing (open well holding the projector bar)
HX, HY0, HY1 = 0.41, 0.03, 0.26             # outer half width, y range
H_Z0, H_ZF, H_Z1 = 0.955, 0.97, 1.13        # bottom, well floor, rim
WX, WY0, WY1 = 0.31, 0.10, 0.20             # well half width, y range
# projector bar (origin = bottom centre at rest)
PJ = Vector((0, 0.15, 0.975))
PJ_H = 0.14
LIFT = 0.08
# hologram (vertical, tilted back)
LENS = Vector((0, 0.15, PJ.z + LIFT + PJ_H + 0.008))     # lens line top when deployed
HOLO_C = Vector((0, 0.19, 1.53))
HOLO_W, HOLO_H = 0.84, 0.48
HOLO_TILT = math.radians(-8)

def cp_mats():
    ms = make_mats("ControlPanel")
    m = bpy.data.materials.get("M_ControlPanel_Hologram") or bpy.data.materials.new("M_ControlPanel_Hologram")
    nt = m.node_tree; nt.nodes.clear()
    out = nt.nodes.new("ShaderNodeOutputMaterial"); mix = nt.nodes.new("ShaderNodeMixShader")
    tr = nt.nodes.new("ShaderNodeBsdfTransparent"); em = nt.nodes.new("ShaderNodeEmission")
    em.inputs["Color"].default_value = (0.05, 0.45, 1.0, 1); em.inputs["Strength"].default_value = 2.5
    mix.inputs[0].default_value = 0.6
    m.surface_render_method = "BLENDED"; m.use_backface_culling = False
    nt.links.new(tr.outputs[0], mix.inputs[1]); nt.links.new(em.outputs[0], mix.inputs[2])
    nt.links.new(mix.outputs[0], out.inputs["Surface"])
    m.diffuse_color = (0.15, 0.6, 1.0, 0.5)
    return ms + [m]

def on_desk(u, v, lift):
    """point on the sloped desk : u along x, v up the slope from the front edge, lift above the face."""
    R = Matrix.Rotation(SLOPE, 3, 'X')
    return DESK0 + Vector((u, 0, 0)) + R @ Vector((0, v, lift))

# ------------------------------------------------------------------ body
def build_body():
    bm = bmesh.new()
    R = Matrix.Rotation(SLOPE, 3, 'X')
    cbox(bm, (0.84, 0.52, 0.08), (0, 0.02, 0.02), DARK, 0.02)                        # plinth (sunk 2 cm)
    prism_x(bm, BODY_P, -BW, BW, WHITE, end_ch=0.03)                                  # white desk
    side = offset_poly(BODY_P, -0.075)                                                # graphite side panels
    for s in (-1, 1):
        x0, x1 = (BW - 0.005, BW + 0.010) if s > 0 else (-BW - 0.010, -BW + 0.005)
        prism_x(bm, side, x0, x1, DARK, end_ch=0.004, eb=0.002)
    # rear housing : white cap with a graphite well
    hollow_z(bm, cham(-HX, HX, HY0, HY1, 0.03), cham(-WX, WX, WY0, WY1, 0.01), H_Z0, H_ZF, H_Z1,
             mo=WHITE, mi=DARK, mrim=WHITE, mfloor=DARK)
    # front lip : neon line
    pill(bm, (0.70, 0.012, 0.025), (0, -0.344, 0.865), NEON)
    # console on the desk : graphite inset, glow touch strip, buttons, status light
    cbox(bm, (0.70, 0.22, 0.012), on_desk(0, 0.17, 0.004), DARK, 0.02, 0.002, rot=R)
    cbox(bm, (0.34, 0.13, 0.006), on_desk(-0.12, 0.17, 0.011), INNER, 0.008, 0.001, rot=R)
    for k, m in enumerate((WHITE, WHITE, ORANGE)):
        pill(bm, (0.045, 0.04, 0.02), on_desk(0.19 + (k % 2) * 0.07 if k < 2 else 0.225, 0.13 + (k // 2) * 0.09, 0.012), m, rot=R)
    pill(bm, (0.12, 0.03, 0.02), on_desk(-0.12, 0.035, 0.006), STATUS, rot=R)
    # kick face : graphite panel ; back : graphite panel + metal vents
    cbox(bm, (0.60, 0.012, 0.36), (0, -0.222, 0.32), DARK, 0.02, 0.002)
    cbox(bm, (0.60, 0.012, 0.44), (0, 0.252, 0.46), DARK, 0.02, 0.002)
    for k in range(6):
        cbox(bm, (0.50, 0.02, 0.022), (0, 0.258, 0.30 + k * 0.065), METAL, 0.004, 0.0)
    return bm

def build_projector():
    bm = bmesh.new()       # built at rest in world coords ; origin = PJ
    z0 = PJ.z
    cbox(bm, (0.56, 0.07, 0.10), (0, PJ.y, z0 + 0.05), METAL, 0.01)                      # stem
    cbox(bm, (0.60, 0.08, 0.045), (0, PJ.y, z0 + PJ_H - 0.0225), DARK, 0.012)            # emitter head
    pill(bm, (0.52, 0.02, 0.012), (0, PJ.y, z0 + PJ_H + 0.002), NEON)                    # lens line
    for s in (-1, 1):
        cbox(bm, (0.03, 0.084, 0.03), (s * 0.27, PJ.y, z0 + PJ_H - 0.01), WHITE, 0.006)   # end caps
    return bm

def build_hologram(col, root, holo_mat):
    for n in ("Hologram", "Hologram_Screen"):
        o = bpy.data.objects.get(n)
        if o: bpy.data.objects.remove(o, do_unlink=True)
    R = Matrix.Rotation(HOLO_TILT, 3, 'X')
    L0 = LENS
    def sp(u, v): return HOLO_C + R @ Vector((u, 0, v))
    bm = bmesh.new()
    def sbox(size, u, v): add_box(bm, size, sp(u, v), 0, rot=R)
    for v in (-HOLO_H / 2, HOLO_H / 2): sbox((HOLO_W, 0.004, 0.008), 0, v)
    for u in (-HOLO_W / 2, HOLO_W / 2): sbox((0.008, 0.004, HOLO_H), u, 0)
    for u in (-HOLO_W / 2 + 0.03, HOLO_W / 2 - 0.03):            # corner ticks
        for v in (-HOLO_H / 2 + 0.03, HOLO_H / 2 - 0.03):
            sbox((0.03, 0.004, 0.004), u, v)
    b0 = [bm.verts.new(L0 + Vector((x, y, 0))) for x, y in ((-0.25, -0.008), (0.25, -0.008), (0.25, 0.008), (-0.25, 0.008))]
    b1 = [bm.verts.new(sp(u, -HOLO_H / 2) + R @ Vector((0, dy, 0))) for u, dy in ((-HOLO_W / 2, -0.01), (HOLO_W / 2, -0.01), (HOLO_W / 2, 0.01), (-HOLO_W / 2, 0.01))]
    for i in range(4):
        j = (i + 1) % 4
        bm.faces.new((b0[i], b0[j], b1[j], b1[i]))
    bmesh.ops.translate(bm, vec=-L0, verts=bm.verts)
    me = bpy.data.meshes.new("Hologram"); bm.to_mesh(me); bm.free(); me.materials.append(holo_mat)
    holo = bpy.data.objects.new("Hologram", me); col.objects.link(holo); holo.parent = root; holo.location = L0
    sw, sh = HOLO_W - 0.012, HOLO_H - 0.012
    me = bpy.data.meshes.new("Hologram_Screen")
    me.from_pydata([(-sw / 2, 0, -sh / 2), (sw / 2, 0, -sh / 2), (sw / 2, 0, sh / 2), (-sw / 2, 0, sh / 2)], [], [(0, 1, 2, 3)])
    uv = me.uv_layers.new(name="UVMap")
    for li, (u, v) in zip(range(4), ((0, 0), (1, 0), (1, 1), (0, 1))): uv.data[li].uv = (u, v)
    me.materials.append(holo_mat)
    scr = bpy.data.objects.new("Hologram_Screen", me); col.objects.link(scr)
    scr.parent = holo; scr.location = HOLO_C - L0; scr.rotation_euler = (HOLO_TILT, 0, 0)
    return holo

def tri_ngons(ob):
    """triangulate the n-gons (concave caps) : Unity discards the concave ones as 'self-intersecting'."""
    bm = bmesh.new(); bm.from_mesh(ob.data)
    ng = [f for f in bm.faces if len(f.verts) > 4]
    if ng: bmesh.ops.triangulate(bm, faces=ng, quad_method='BEAUTY', ngon_method='BEAUTY')
    bm.to_mesh(ob.data); bm.free(); ob.data.update()
    return len(ng)

# ------------------------------------------------------------------ setup
def clear():
    for ob in list(bpy.data.objects):
        if not ob.name.startswith("_"): bpy.data.objects.remove(ob)
    for a in list(bpy.data.actions): bpy.data.actions.remove(a)
    for me in list(bpy.data.meshes):
        if me.users == 0: bpy.data.meshes.remove(me)

def setup():
    clear()
    sc = bpy.context.scene
    col = bpy.data.collections.get("Models") or bpy.data.collections.new("Models")
    if col.name not in sc.collection.children: sc.collection.children.link(col)
    mats = cp_mats()
    root = bpy.data.objects.new("ControlPanel", None); col.objects.link(root); root.empty_display_size = 0.5
    finish2("Body", build_body(), col, root, mats[:HOLO])
    finish2("Projector", build_projector(), col, root, mats[:HOLO], origin=tuple(PJ))
    build_hologram(col, root, mats[HOLO])
    for n in ("Body", "Projector"): tri_ngons(bpy.data.objects[n])
    bpy.context.view_layer.update()
    tri = lambda o: sum(len(p.vertices) - 2 for p in o.data.polygons)
    return {o.name: tri(o) for o in root.children_recursive if o.type == 'MESH'}

# ------------------------------------------------------------------ animation
CLIPS = [("Power_On", 1, 24), ("Power_Off", 36, 60)]
def key(ob, path, idx, f, v, interp='BEZIER'):
    getattr(ob, path)[idx] = v
    ob.keyframe_insert(path, index=idx, frame=f)
    ad = ob.animation_data
    fc = ad.action.layers[0].strips[0].channelbag(ad.action_slot).fcurves.find(path, index=idx)
    for kp in fc.keyframe_points:
        if abs(kp.co[0] - f) < 1e-4: kp.interpolation = interp

HOLO_OFF = 0.001        # hologram scale when off (folded into the lens, invisible)
def animate():
    """Power_On : the bar rises (1-16) then the hologram unfolds from the lens (16-24).
    Power_Off : the hologram folds back into the lens (36-44) then the bar goes down (44-60)."""
    pj, holo = bpy.data.objects["Projector"], bpy.data.objects["Hologram"]
    for ob in (pj, holo):
        if ob.animation_data: ob.animation_data_clear()
    z0, z1 = PJ.z, PJ.z + LIFT
    for f, v in ((1, z0), (16, z1), (44, z1), (60, z0)): key(pj, "location", 2, f, v)
    for i in range(3):
        for f, v in ((1, HOLO_OFF), (16, HOLO_OFF), (24, 1.0), (36, 1.0), (44, HOLO_OFF), (60, HOLO_OFF)):
            key(holo, "scale", i, f, v)
    sc = bpy.context.scene; sc.frame_start, sc.frame_end = 1, 60; sc.render.fps = 24; sc.frame_set(1)
    return CLIPS

# ------------------------------------------------------------------ checks
def collisions(step=1):
    from mathutils.bvhtree import BVHTree
    O = bpy.data.objects; sc = bpy.context.scene; dg = bpy.context.evaluated_depsgraph_get()
    def tree(ob):
        M = ob.matrix_world
        return BVHTree.FromPolygons([M @ v.co for v in ob.data.vertices], [p.vertices[:] for p in ob.data.polygons])
    body = tree(O["Body"]); hits = []
    for f in range(1, 61, step):
        sc.frame_set(f); dg.update()
        k = len(tree(O["Projector"]).overlap(body))
        if k: hits.append(("Projector", f, k))
    sc.frame_set(1)
    return hits

# Coal generator "Furnace Block" (~2.3 x 1.5 x 3.1 m) — DA white / graphite / blue neon / orange lights.
# CoalGenerator > Body (static)
#               > Hatch          (hopper lid, rot X, hinge at the back)        clips Hatch_Open / Hatch_Close
#               > Lever          (power lever on the front, rot X)             clips Power_On / Power_Off
#               > Fan            (cooling fan on the right flank, rot X)        clip  Fan_Spin (loop)
#               > Chimney_Shaft  (origin at its bottom, mesh = SHAFT_L m along +Z ; scaled in Z by script)
#               > Chimney_Head   (origin at the top of the shaft ; moved in Z by script) > Smoke (empty, +Z = up)
#               > Chimney_Collar (dome crossing collar ; placed / hidden by script)
import bpy, bmesh, math
from mathutils import Matrix, Vector
SP = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival/ArtPipeline/Blender"
exec(open(SP + r"\al_build.py", encoding="utf8").read())
FIRE, GLASS = 9, 10

def T(*v): return Matrix.Translation(Vector(v))
RX90 = Matrix.Rotation(math.pi / 2, 4, 'X')      # local +Z -> -Y (front)
RY90 = Matrix.Rotation(math.pi / 2, 4, 'Y')      # local +Z -> +X (right)

# ------------------------------------------------------------------ layout (m)
W, D = 1.10, 0.70                    # body half width (x) / half depth (y)
Z0, ZT, CH = 0.12, 1.55, 0.35        # body bottom (sunk in the plinth), top, top chamfers
YF = -D                              # front face
HX, HY = -0.35, -0.10                # hopper centre
HINGE = (HX, HY + 0.48, 2.02)        # hatch hinge axis (along X)
OPEN = math.radians(-105)            # hatch open (leans 15 deg past vertical, toward the back)
LEVER = (0.10, -0.775, 0.68)         # lever pivot (axis X)
L_OFF, L_ON = math.radians(25), math.radians(155)
FAN = (1.14, 0.05, 0.68)             # fan centre (axis X)
CX, CY = 0.55, 0.32                 # chimney axis
ZB = 1.95                            # chimney base top = shaft origin
SHAFT_L = 0.85                       # default shaft length (head origin = ZB + SHAFT_L)
COLLAR_Z = 2.35                      # preview position of the collar

def coal_mats():
    ms = make_mats("CoalGenerator")
    f = bpy.data.materials.get("M_CoalGenerator_Fire") or bpy.data.materials.new("M_CoalGenerator_Fire")
    b = next(x for x in f.node_tree.nodes if x.type == 'BSDF_PRINCIPLED')
    b.inputs["Base Color"].default_value = (1.0, 0.30, 0.04, 1); b.inputs["Roughness"].default_value = 0.5
    b.inputs["Emission Color"].default_value = (1.0, 0.22, 0.02, 1); b.inputs["Emission Strength"].default_value = 4.0
    f.diffuse_color = (1.0, 0.30, 0.04, 1)
    g = bpy.data.materials.get("M_CoalGenerator_Glass") or bpy.data.materials.new("M_CoalGenerator_Glass")
    b = next(x for x in g.node_tree.nodes if x.type == 'BSDF_PRINCIPLED')
    b.inputs["Base Color"].default_value = (0.25, 0.6, 0.8, 1); b.inputs["Roughness"].default_value = 0.08
    b.inputs["Alpha"].default_value = 0.35; g.surface_render_method = 'BLENDED'; g.use_backface_culling = False
    g.diffuse_color = (0.25, 0.6, 0.8, 0.35)
    return ms + [f, g]

def ring(bm, r0, r1, z0, z1, mat, M=Matrix(), sides=12):
    """faceted flat ring (closed square profile), facets aligned with oct_cyl(sides)."""
    add_revolve(bm, [(r0, z0), (r1, z0), (r1, z1), (r0, z1)], [mat] * 4, sides,
                M @ Matrix.Rotation(math.pi / sides, 4, 'Z'), closed=True)

# ------------------------------------------------------------------ body
def build_hopper(bm):
    tmp = bmesh.new()
    def loop(h, z): return [tmp.verts.new((HX + sx * h, HY + sy * h, z)) for sx, sy in ((-1, -1), (1, -1), (1, 1), (-1, 1))]
    B, Tt, Ti, Bi = loop(0.30, 1.50), loop(0.44, 1.95), loop(0.405, 1.95), loop(0.315, 1.66)
    tmp.faces.new(B[::-1]).material_index = WHITE
    for i in range(4):
        j = (i + 1) % 4
        tmp.faces.new((B[i], B[j], Tt[j], Tt[i])).material_index = WHITE
        tmp.faces.new((Tt[i], Tt[j], Ti[j], Ti[i])).material_index = DARK
        tmp.faces.new((Ti[i], Ti[j], Bi[j], Bi[i])).material_index = DARK
    tmp.faces.new(Bi).material_index = METAL
    bmesh.ops.recalc_face_normals(tmp, faces=tmp.faces)
    _bevel_tmp(tmp, EB)
    _copy_into(bm, tmp, None); tmp.free()
    for k in range(4):                                                    # feed grate on the floor
        cbox(bm, (0.56, 0.025, 0.012), (HX, HY - 0.2 + k * 0.133, 1.667), METAL, 0.003, 0.0)
    # graphite rim frame (no shared planes: side bars lower and shorter)
    for s in (-1, 1):
        cbox(bm, (0.94, 0.065, 0.06), (HX, HY + s * 0.4375, 1.96), DARK, 0.015)
        cbox(bm, (0.065, 0.80, 0.05), (HX + s * 0.4375, HY, 1.955), DARK, 0.015)
    # fixed hinge knuckles + brackets (lid knuckle sits between them, 3 mm gaps)
    hx, hy, hz = HINGE
    for s in (-1, 1):
        oct_cyl(bm, 0.03, 0.153, 0.33, DARK, T(hx, hy, hz) @ Matrix.Rotation(s * math.pi / 2, 4, 'Y'), ch=0.008)
        cbox(bm, (0.15, 0.05, 0.04), (hx + s * 0.24, hy, 1.98), DARK, 0.012)

def build_chimney_base(bm):
    M = T(CX, CY, 0)
    oct_cyl(bm, 0.30, 1.20, 1.66, DARK, M, ch=0.03, sides=12)            # pedestal, sunk in the body top / chamfer
    oct_cyl(bm, 0.25, 1.64, ZB, WHITE, M, ch=0.02, sides=12)
    for k in range(6):                                                    # cooling fins
        a = k * math.pi / 3 + math.pi / 6
        d = Vector((math.cos(a), math.sin(a), 0))
        cbox(bm, (0.10, 0.03, 0.21), Vector((CX, CY, 1.775)) + d * 0.285, DARK, 0.012,
             rot=Matrix.Rotation(a, 3, 'Z'))
    ring(bm, 0.235, 0.262, 1.885, 1.91, DARK, M)                          # graphite band under the shaft

def build_body():
    bm = bmesh.new()
    cbox(bm, (2.34, 1.54, 0.18), (0, 0, 0.05), DARK, 0.05)                # plinth (sunk: z -0.04)
    cbox(bm, (2.22, 1.42, 0.16), (0, 0, 0.22), DARK, 0.02)                # graphite base band
    prism_y(bm, prof6(W, Z0, ZT, CH), -D, D, WHITE, end_ch=0.04)          # main caisson
    # neon belt (front + flanks)
    pill(bm, (2.0, 0.02, 0.03), (0, YF - 0.009, 1.10), NEON)
    for s in (-1, 1):
        pill(bm, (0.02, 1.30, 0.03), (s * (W + 0.009), 0, 1.10), NEON)
    # ---- front : furnace door with porthole (fire behind glass)
    dx, dz = -0.44, 0.69
    cbox(bm, (0.76, 0.03, 0.66), (dx, YF - 0.0125, dz), DARK, 0.07)
    Mp = T(dx, YF - 0.0275, dz) @ RX90                                    # local z = outward (-Y)
    oct_cyl(bm, 0.24, 0.0, 0.015, METAL, Mp, ch=0.004, sides=12)
    oct_cyl(bm, 0.185, 0.015, 0.02, FIRE, Mp, ch=0.0, sides=12)
    oct_cyl(bm, 0.20, 0.025, 0.03, GLASS, Mp, ch=0.0, sides=12)           # closed thin prism, 5 mm in front of the fire
    ring(bm, 0.195, 0.24, 0.015, 0.0345, DARK, Mp)                        # retaining ring over the glass edge
    for zz in (0.50, 0.88):                                               # hinges (left) + latch (right, hazard)
        cbox(bm, (0.05, 0.04, 0.10), (dx - 0.395, YF - 0.03, zz), DARK, 0.012)
    cbox(bm, (0.04, 0.035, 0.16), (dx + 0.41, YF - 0.035, dz), HAZARD, 0.01)
    for sx in (-1, 1):
        for sz in (-1, 1):
            add_screw(bm, (dx + sx * 0.33, YF - 0.0275, dz + sz * 0.28), (0, -1, 0), r=0.014, mat=METAL)
    # ---- front : lever base plate + ears (the lever is a separate object)
    lx, ly, lz = LEVER
    cbox(bm, (0.16, 0.03, 0.26), (lx, YF - 0.015, lz - 0.02), DARK, 0.03)
    for s in (-1, 1):
        cbox(bm, (0.025, 0.06, 0.09), (lx + s * 0.0655, ly + 0.005, lz), DARK, 0.008)
    # ---- front : console (screen, keys, status light)
    cx_ = 0.58
    cbox(bm, (0.60, 0.03, 0.46), (cx_, YF - 0.015, 0.75), DARK, 0.05)
    cbox(bm, (0.48, 0.012, 0.20), (cx_, YF - 0.034, 0.855), INNER, 0.015, 0.002)
    pill(bm, (0.22, 0.02, 0.035), (cx_ - 0.10, YF - 0.035, 0.66), STATUS)
    for k, m in enumerate((WHITE, WHITE, ORANGE)):
        pill(bm, (0.05, 0.02, 0.04), (cx_ + 0.07 + k * 0.065, YF - 0.035, 0.66), m)
    cbox(bm, (0.40, 0.012, 0.012), (cx_, YF - 0.034, 0.585), METAL, 0.003, 0.0)   # id strip
    # ---- right flank : fan shroud, back plate, guard (the fan is a separate object)
    fx, fy, fz = FAN
    Mf = T(W, fy, fz) @ RY90                                              # local z = outward (+X)
    oct_cyl(bm, 0.31, -0.008, 0.005, DARK, Mf, ch=0.0, sides=12)
    add_revolve(bm, [(0.30, -0.01), (0.345, -0.01), (0.345, 0.06), (0.325, 0.085), (0.30, 0.085)], [DARK] * 5, 12,
                Mf @ Matrix.Rotation(math.pi / 12, 4, 'Z'), closed=True)
    for k in range(5):
        dz_ = -0.22 + k * 0.11
        cbox(bm, (0.008, 2 * math.sqrt(0.315 ** 2 - dz_ ** 2), 0.012), (W + 0.076, fy, fz + dz_), METAL, 0.002, 0.0)
    for yy in (-0.52, 0.52):
        for zz in (0.45, 0.95):
            add_screw(bm, (W, yy, zz), (1, 0, 0), r=0.016, mat=METAL)
    # ---- left flank : vents
    cbox(bm, (0.03, 0.90, 0.50), (-W - 0.012, 0, 0.68), DARK, 0.05)
    for k in range(6):
        cbox(bm, (0.02, 0.80, 0.022), (-W - 0.032, 0, 0.49 + k * 0.076), METAL, 0.004, 0.0)
    for yy in (-0.52, 0.52):
        for zz in (0.45, 0.95):
            add_screw(bm, (-W, yy, zz), (-1, 0, 0), r=0.016, mat=METAL)
    # ---- back : service panel
    cbox(bm, (1.20, 0.03, 0.50), (0, D + 0.012, 0.70), DARK, 0.05)
    for k in range(6):
        cbox(bm, (1.0, 0.02, 0.022), (0, D + 0.032, 0.51 + k * 0.076), METAL, 0.004, 0.0)
    # ---- top : hopper + chimney base
    build_hopper(bm)
    build_chimney_base(bm)
    return bm

# ------------------------------------------------------------------ moving parts
def build_hatch():
    bm = bmesh.new()       # origin = hinge axis ; lid extends toward -Y, bottom 2 cm under the axis
    cbox(bm, (0.96, 0.935, 0.045), (0, -0.5025, 0.0025), WHITE, 0.02)     # plate starts 3.5 cm in front of the axis
    cbox(bm, (0.70, 0.66, 0.012), (0, -0.51, 0.029), DARK, 0.03, 0.003)
    for s in (-1, 1):
        cbox(bm, (0.04, 0.50, 0.012), (s * 0.20, -0.51, 0.039), METAL, 0.004, 0.0)
    cbox(bm, (0.30, 0.05, 0.04), (0, -0.985, 0.0), HAZARD, 0.012)          # handle (front edge)
    oct_cyl(bm, 0.03, -0.15, 0.15, DARK, RY90, ch=0.008)                   # knuckle
    cbox(bm, (0.26, 0.05, 0.03), (0, -0.035, 0.0), DARK, 0.008)            # strap knuckle -> plate
    return bm

def build_lever():
    bm = bmesh.new()       # origin = pivot ; arm along +Z at angle 0
    oct_cyl(bm, 0.04, -0.05, 0.05, DARK, RY90, ch=0.01)
    cbox(bm, (0.04, 0.03, 0.20), (0, 0, 0.11), HAZARD, 0.01)
    pill(bm, (0.09, 0.045, 0.05), (0, 0, 0.225), DARK)
    return bm

def build_fan():
    bm = bmesh.new()       # origin = fan centre ; axis X
    oct_cyl(bm, 0.07, -0.025, 0.025, METAL, RY90, ch=0.008)
    for k in range(7):
        a = 2 * math.pi * k / 7
        R = Matrix.Rotation(a, 3, 'X')
        cbox(bm, (0.012, 0.09, 0.19), R @ Vector((0, 0, 0.17)), DARK, 0.0, 0.003,
             rot=R @ Matrix.Rotation(math.radians(25), 3, 'Z'))
    return bm

def build_shaft():
    bm = bmesh.new()       # origin = bottom ; scaled in Z by script (keep it free of round details)
    oct_cyl(bm, 0.19, -0.03, SHAFT_L, WHITE, ch=0.0, sides=12)
    cbox(bm, (0.05, 0.02, SHAFT_L - 0.10), (0, -0.19, SHAFT_L / 2), DARK, 0.008)
    return bm

def build_head():
    bm = bmesh.new()       # origin = top of the shaft
    oct_cyl(bm, 0.23, -0.04, 0.04, DARK, ch=0.01, sides=12)
    add_revolve(bm, [(0, 0.04), (0.20, 0.04), (0.25, 0.16), (0.25, 0.24), (0, 0.24)], [WHITE] * 4, 12,
                Matrix.Rotation(math.pi / 12, 4, 'Z'))
    ring(bm, 0.235, 0.258, 0.19, 0.215, NEON)
    ring(bm, 0.17, 0.262, 0.235, 0.29, DARK)
    oct_cyl(bm, 0.172, 0.236, 0.245, DARK, ch=0.0, sides=12)
    for k, off in enumerate((-0.07, 0.0, 0.07)):                           # exhaust grille
        cbox(bm, (2 * math.sqrt(0.168 ** 2 - off ** 2), 0.02, 0.012), (0, off, 0.251), METAL, 0.003, 0.0)
    pill(bm, (0.05, 0.05, 0.035), (0.0, 0.215, 0.305), ORANGE)             # beacon (back side)
    return bm

SHAFT_R = 0.19                       # shaft vertex radius (12 sides)
COLLAR_R = SHAFT_R + 0.10            # collar radius when flat : 10 cm past the shaft
def build_collar():
    # origin = centre ; SOLID graphite disc (the shaft goes through it visually : no hole when it tilts).
    # The script tilts it to the dome normal and stretches it in local X (ellipse r/cos + 10 cm) : no screws, plain chamfer.
    bm = bmesh.new()
    oct_cyl(bm, COLLAR_R, -0.03, 0.03, DARK, ch=0.015, sides=16)
    return bm

def tilt_collar(deg, z=None):
    """preview of what CoalGeneratorAnimator does : tilt around local Y (slope along X) and stretch X."""
    c = bpy.data.objects["Chimney_Collar"]
    t = math.radians(deg)
    c.rotation_euler = (0, t, 0)
    c.scale = ((SHAFT_R / max(math.cos(t), 0.0175) + 0.10) / COLLAR_R, 1, 1)
    if z is not None: c.location.z = z

# ------------------------------------------------------------------ setup
def setup():
    for ob in list(bpy.data.objects):
        if not ob.name.startswith("_"): bpy.data.objects.remove(ob)
    for a in list(bpy.data.actions): bpy.data.actions.remove(a)
    for me in list(bpy.data.meshes):
        if me.users == 0: bpy.data.meshes.remove(me)
    sc = bpy.context.scene
    col = bpy.data.collections.get("Models") or bpy.data.collections.new("Models")
    if col.name not in sc.collection.children: sc.collection.children.link(col)
    mats = coal_mats()
    root = bpy.data.objects.new("CoalGenerator", None); col.objects.link(root); root.empty_display_size = 0.5
    finish2("Body", build_body(), col, root, mats)
    finish2("Hatch", build_hatch(), col, root, mats); bpy.data.objects["Hatch"].location = HINGE
    finish2("Lever", build_lever(), col, root, mats); bpy.data.objects["Lever"].location = LEVER
    finish2("Fan", build_fan(), col, root, mats); bpy.data.objects["Fan"].location = FAN
    finish2("Chimney_Shaft", build_shaft(), col, root, mats).location = (CX, CY, ZB)
    head = finish2("Chimney_Head", build_head(), col, root, mats); head.location = (CX, CY, ZB + SHAFT_L)
    finish2("Chimney_Collar", build_collar(), col, root, mats).location = (CX, CY, COLLAR_Z)
    smoke = bpy.data.objects.new("Smoke", None); col.objects.link(smoke)
    smoke.parent = head; smoke.location = (0, 0, 0.30); smoke.empty_display_type = 'SINGLE_ARROW'; smoke.empty_display_size = 0.4
    bpy.data.objects["Lever"].rotation_euler = (L_OFF, 0, 0)
    bpy.context.view_layer.update()
    tri = lambda o: sum(len(p.vertices) - 2 for p in o.data.polygons)
    return {o.name: tri(o) for o in root.children_recursive if o.type == 'MESH'}

# ------------------------------------------------------------------ animation
CLIPS = [("Hatch_Open", 1, 24), ("Hatch_Close", 36, 60), ("Power_On", 72, 84), ("Power_Off", 96, 108), ("Fan_Spin", 120, 168)]
def key(ob, path, idx, f, v, interp='BEZIER'):
    getattr(ob, path)[idx] = v
    ob.keyframe_insert(path, index=idx, frame=f)
    ad = ob.animation_data
    fc = ad.action.layers[0].strips[0].channelbag(ad.action_slot).fcurves.find(path, index=idx)
    for kp in fc.keyframe_points:
        if abs(kp.co[0] - f) < 1e-4: kp.interpolation = interp

def animate():
    O = bpy.data.objects
    hatch, lever, fan = O["Hatch"], O["Lever"], O["Fan"]
    for ob in (hatch, lever, fan):
        if ob.animation_data: ob.animation_data_clear()
    # Hatch_Open / Hatch_Close
    for f, v in ((1, 0.0), (24, OPEN), (36, OPEN), (60, 0.0)): key(hatch, "rotation_euler", 0, f, v)
    # Power_On / Power_Off : lever flips down through the front
    for f, v in ((1, L_OFF), (72, L_OFF), (84, L_ON), (96, L_ON), (108, L_OFF)): key(lever, "rotation_euler", 0, f, v)
    # Fan_Spin : one turn in 2 s, linear loop
    key(fan, "rotation_euler", 0, 1, 0.0); key(fan, "rotation_euler", 0, 120, 0.0, 'LINEAR'); key(fan, "rotation_euler", 0, 168, 2 * math.pi, 'LINEAR')
    sc = bpy.context.scene; sc.frame_start, sc.frame_end = 1, 168; sc.render.fps = 24; sc.frame_set(1)
    return CLIPS

# ------------------------------------------------------------------ checks
def collisions(step=2):
    from mathutils.bvhtree import BVHTree
    O = bpy.data.objects; sc = bpy.context.scene; dg = bpy.context.evaluated_depsgraph_get()
    def tree(ob):
        M = ob.matrix_world
        return BVHTree.FromPolygons([M @ v.co for v in ob.data.vertices], [p.vertices[:] for p in ob.data.polygons])
    pairs = [("Hatch", "Body", 1, 60), ("Hatch", "Chimney_Shaft", 1, 60), ("Hatch", "Chimney_Head", 1, 60),
             ("Hatch", "Chimney_Collar", 1, 60), ("Lever", "Body", 72, 108), ("Fan", "Body", 120, 168)]
    hits = []
    for a, b, f0, f1 in pairs:
        for f in list(range(f0, f1 + 1, step)) + [f1]:
            sc.frame_set(f); dg.update()
            n = len(tree(O[a]).overlap(tree(O[b])))
            if n: hits.append((a, b, f, n))
    sc.frame_set(1)
    return hits

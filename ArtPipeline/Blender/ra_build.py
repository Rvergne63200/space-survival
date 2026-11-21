# Relay antenna v2 (DA: white / graphite / blue neon / orange lights, wide chamfers)
# Hierarchy: RelayAntenna > Base > Turret (rot Z) > Arm (rot X) > Dish ; Turret > Mast_Mid (z) > Mast_Top (z)
import bpy, bmesh, math
from mathutils import Matrix, Vector
SP = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival/ArtPipeline/Blender"
exec(open(SP + r"\al_build.py", encoding="utf8").read())     # cbox, oct_cyl, beam, pill, finish2, make_mats, prism_x...

HINGE = Vector((0, 0, 0.88))          # arm pivot (X axis)
TUR_Z = 0.32                          # turret pivot height (top of base)
ARM_L = 0.62
MAST = Vector((0.27, 0.44, 0))        # mast axis (x, y) on the turret's back-right bracket
MAST_Z0, MAST_Z1 = 0.42, 1.18         # outer tube
MID_TRAVEL, TOP_TRAVEL = 0.52, 0.48
ARM_DEPLOYED, ARM_FOLDED = math.radians(-20), math.radians(80)

def Rx(a): return Matrix.Rotation(a, 4, 'X')
def T(*v): return Matrix.Translation(Vector(v))

# ------------------------------------------------------------------ base (static)
def build_base():
    bm = bmesh.new()
    oct_cyl(bm, 0.46, 0.0, 0.10, DARK, ch=0.03)                               # foot plate
    oct_cyl(bm, 0.40, 0.10, 0.26, WHITE, ch=0.05)                             # body
    oct_cyl(bm, 0.36, 0.26, TUR_Z, DARK, ch=0.03)                             # bearing ring
    add_revolve(bm, [(0.405, 0.165), (0.412, 0.165), (0.412, 0.19), (0.405, 0.19)], [NEON] * 4, 8,
                Matrix.Rotation(math.pi / 8, 4, 'Z'), closed=True)          # neon band
    for k in range(4):                                                        # stabiliser legs + pads
        a = math.pi / 4 + k * math.pi / 2
        d = Vector((math.cos(a), math.sin(a), 0))
        beam(bm, d * 0.32 + Vector((0, 0, 0.16)), d * 0.74 + Vector((0, 0, 0.05)), 0.09, 0.08, WHITE, 0.025)
        oct_cyl(bm, 0.09, 0.0, 0.05, DARK, T(*(d * 0.76)), ch=0.015)
        add_screw(bm, d * 0.76 + Vector((0, 0, 0.05)), (0, 0, 1), r=0.02, mat=METAL)
    for k in range(3):                                                        # orange status lights (front)
        pill(bm, (0.035, 0.02, 0.035), (-0.06 + k * 0.06, -0.395, 0.215), ORANGE)
    cbox(bm, (0.22, 0.03, 0.08), (0, -0.39, 0.13), DARK, 0.015, 0.003)       # access plate
    return bm

# ------------------------------------------------------------------ turret (rotates around Z), origin at its pivot
def build_turret():
    bm = bmesh.new()
    z0 = 0.0                                                                  # local z (world TUR_Z)
    oct_cyl(bm, 0.34, z0, z0 + 0.06, METAL, ch=0.015)
    # chamfered housing
    prof = [(-0.26, 0.06), (0.26, 0.06), (0.26, 0.30), (0.18, 0.40), (-0.18, 0.40), (-0.26, 0.30)]   # (y, z)
    prism_x(bm, prof, -0.22, 0.22, WHITE, end_ch=0.05)
    cbox(bm, (0.03, 0.3, 0.16), (0.225, 0.0, 0.2), DARK, 0.03, 0.004)
    cbox(bm, (0.03, 0.3, 0.16), (-0.225, 0.0, 0.2), DARK, 0.03, 0.004)
    for sx in (-1, 1):
        pill(bm, (0.02, 0.22, 0.025), (sx * 0.24, 0.0, 0.25), NEON)
        for k in range(3):
            cbox(bm, (0.02, 0.2, 0.012), (sx * 0.238, 0.0, 0.14 + k * 0.035), METAL, 0.003, 0.0)
    # yoke cheeks holding the arm hinge
    hz = HINGE.z - TUR_Z
    for sx in (-1, 1):
        prof_y = [(-0.10, 0.36), (0.10, 0.36), (0.10, hz + 0.02), (0.05, hz + 0.09), (-0.05, hz + 0.09), (-0.10, hz + 0.02)]
        prism_x(bm, prof_y, sx * 0.105 - 0.025, sx * 0.105 + 0.025, DARK, end_ch=0.01)
        oct_cyl(bm, 0.05, 0.0, 0.03, METAL, T(sx * 0.13, 0, hz) @ Matrix.Rotation(sx * math.pi / 2, 4, 'Y'), ch=0.008)
    # mast bracket + outer tube + status light
    mx, my = MAST.x, MAST.y
    beam(bm, (0.12, 0.2, 0.22), (mx, my, 0.22), 0.08, 0.08, DARK, 0.02)
    oct_cyl(bm, 0.07, MAST_Z0 - TUR_Z - 0.14, MAST_Z0 - TUR_Z - 0.06, DARK, T(mx, my, 0), ch=0.015)
    oct_cyl(bm, 0.05, MAST_Z0 - TUR_Z - 0.10, MAST_Z1 - TUR_Z, WHITE, T(mx, my, 0), ch=0.01)
    oct_cyl(bm, 0.058, MAST_Z1 - TUR_Z - 0.03, MAST_Z1 - TUR_Z + 0.005, DARK, T(mx, my, 0), ch=0.008)
    pill(bm, (0.06, 0.02, 0.03), (0, 0.262, 0.33), ORANGE)
    return bm

# ------------------------------------------------------------------ arm (rotates around X), origin at the hinge, built along +Z
def build_arm():
    bm = bmesh.new()
    oct_cyl(bm, 0.075, -0.075, 0.075, DARK, Matrix.Rotation(math.pi / 2, 4, 'Y'), ch=0.015)   # hinge hub (axis X)
    cbox(bm, (0.13, 0.11, ARM_L), (0, 0, ARM_L / 2 + 0.02), WHITE, 0.035)
    cbox(bm, (0.135, 0.06, 0.32), (0, 0.03, ARM_L / 2 + 0.04), DARK, 0.02, 0.003)            # back spine
    pill(bm, (0.02, 0.02, 0.34), (0, -0.058, ARM_L / 2 + 0.04), NEON)                         # front neon line
    cbox(bm, (0.2, 0.12, 0.08), (0, 0, ARM_L), DARK, 0.03)                                    # head
    oct_cyl(bm, 0.05, -0.12, 0.12, METAL, T(0, 0, ARM_L) @ Matrix.Rotation(math.pi / 2, 4, 'Y'), ch=0.01)
    return bm

# ------------------------------------------------------------------ dish (static child of the arm), faceted parabola
DISH_R, DISH_F = 0.40, 0.36
def build_dish():
    bm = bmesh.new()
    S = 12
    rs = [0.0, 0.08, 0.17, 0.26, 0.34, DISH_R]
    zf = lambda r: r * r / (4 * DISH_F)
    front = [(r, zf(r)) for r in rs]
    back = [(r, zf(r) - 0.03) for r in reversed(rs)]
    prof = front + [(DISH_R + 0.03, zf(DISH_R) + 0.01), (DISH_R + 0.04, zf(DISH_R) - 0.02), (DISH_R + 0.02, zf(DISH_R) - 0.045)] + back
    mats = [WHITE] * (len(front) - 1) + [DARK, DARK, DARK, DARK] + [DARK] * (len(back) - 1)
    add_revolve(bm, prof, mats, S, Matrix.Rotation(math.pi / S, 4, 'Z'))
    # back mount + ribs
    oct_cyl(bm, 0.11, -0.12, -0.02, DARK, ch=0.02)
    for k in range(6):
        a = 2 * math.pi * k / 6 + math.pi / 12
        d = Vector((math.cos(a), math.sin(a), 0))
        beam(bm, d * 0.1 + Vector((0, 0, -0.04)), d * 0.33 + Vector((0, 0, zf(0.33) - 0.045)), 0.03, 0.03, METAL, 0.006)
    # feed : 3 struts to the receiver at the focus
    for k in range(3):
        a = 2 * math.pi * k / 3 + math.pi / 2
        p = Vector((math.cos(a) * 0.36, math.sin(a) * 0.36, zf(0.36)))
        beam(bm, p, Vector((0, 0, DISH_F - 0.05)), 0.022, 0.022, DARK, 0.005)
    oct_cyl(bm, 0.05, DISH_F - 0.08, DISH_F - 0.01, DARK, ch=0.012)
    oct_cyl(bm, 0.035, DISH_F - 0.01, DISH_F + 0.015, NEON, ch=0.008)
    oct_cyl(bm, 0.04, zf(0) - 0.005, zf(0) + 0.03, METAL, ch=0.01)
    return bm
# dish pose in arm space : axis +Z(dish) -> -Y(arm), back mount on the arm head
DISH_IN_ARM = T(0, -0.10, ARM_L) @ Rx(math.pi / 2)

# ------------------------------------------------------------------ telescopic mast
def build_mast_mid():
    bm = bmesh.new()       # origin = mast axis at the outer tube top (retracted)
    oct_cyl(bm, 0.036, -0.62, 0.04, WHITE, ch=0.008)
    oct_cyl(bm, 0.042, 0.01, 0.05, DARK, ch=0.008)
    return bm
def build_mast_top():
    bm = bmesh.new()       # origin = top of Mast_Mid
    oct_cyl(bm, 0.022, -0.56, 0.06, METAL, ch=0.005)
    oct_cyl(bm, 0.03, 0.06, 0.10, DARK, ch=0.006)
    oct_cyl(bm, 0.022, 0.10, 0.16, ORANGE, ch=0.008)                          # beacon
    oct_cyl(bm, 0.006, 0.16, 0.38, DARK, ch=0.0, sides=6)                     # whip
    for z in (0.22, 0.30):
        oct_cyl(bm, 0.014, z, z + 0.012, NEON, ch=0.0, sides=6)
    return bm

def setup():
    for ob in list(bpy.data.objects):
        if not ob.name.startswith("_"): bpy.data.objects.remove(ob)
    for a in list(bpy.data.actions): bpy.data.actions.remove(a)
    for me in list(bpy.data.meshes):
        if me.users == 0: bpy.data.meshes.remove(me)
    sc = bpy.context.scene
    col = bpy.data.collections.get("Models") or bpy.data.collections.new("Models")
    if col.name not in sc.collection.children: sc.collection.children.link(col)
    mats = make_mats("RelayAntenna")
    root = bpy.data.objects.new("RelayAntenna", None); col.objects.link(root); root.empty_display_size = 0.5
    base = finish2("Base", build_base(), col, root, mats)
    tur = finish2("Turret", build_turret(), col, base, mats); tur.location = (0, 0, TUR_Z)
    arm = finish2("Arm", build_arm(), col, tur, mats); arm.location = (0, 0, HINGE.z - TUR_Z)
    bm = build_dish(); bmesh.ops.transform(bm, matrix=DISH_IN_ARM, verts=bm.verts)
    finish2("Dish", bm, col, arm, mats)
    mid = finish2("Mast_Mid", build_mast_mid(), col, tur, mats); mid.location = (MAST.x, MAST.y, MAST_Z1 - TUR_Z)
    top = finish2("Mast_Top", build_mast_top(), col, mid, mats); top.location = (0, 0, 0.0)
    arm.rotation_euler = (ARM_FOLDED, 0, 0)
    bpy.context.view_layer.update()
    tri = lambda o: sum(len(p.vertices) - 2 for p in o.data.polygons)
    return {o.name: tri(o) for o in root.children_recursive if o.type == 'MESH'}

# ------------------------------------------------------------------ animation (single timeline, split + filtered in Unity)
CLIPS = [("Dish_Deploy", 1, 37), ("Dish_Undeploy", 50, 86), ("Antenna_Extend", 100, 124),
         ("Antenna_Retract", 130, 154), ("Rotate_Loop", 170, 266)]
def key(ob, path, idx, f, v, interp='BEZIER'):
    getattr(ob, path)[idx] = v
    ob.keyframe_insert(path, index=idx, frame=f)
    ad = ob.animation_data
    fc = ad.action.layers[0].strips[0].channelbag(ad.action_slot).fcurves.find(path, index=idx)
    for kp in fc.keyframe_points:
        if abs(kp.co[0] - f) < 1e-4: kp.interpolation = interp

def animate():
    O = bpy.data.objects
    arm, tur, mid, top = O["Arm"], O["Turret"], O["Mast_Mid"], O["Mast_Top"]
    for ob in (arm, tur, mid, top):
        if ob.animation_data: ob.animation_data_clear()
    mz = MAST_Z1 - TUR_Z
    def arm_k(f, a): key(arm, "rotation_euler", 0, f, a)
    def mast_k(f, m, t): key(mid, "location", 2, f, mz + m * MID_TRAVEL); key(top, "location", 2, f, t * TOP_TRAVEL)
    # stowed rest pose on frame 1 (folded, retracted, heading 0)
    # Dish_Deploy : unlock pause, rise, small overshoot then settle
    arm_k(1, ARM_FOLDED); arm_k(5, ARM_FOLDED); arm_k(31, ARM_DEPLOYED - math.radians(4)); arm_k(37, ARM_DEPLOYED)
    # Dish_Undeploy : slow start, lands softly on the stop
    arm_k(50, ARM_DEPLOYED); arm_k(54, ARM_DEPLOYED - math.radians(2)); arm_k(84, ARM_FOLDED + math.radians(1)); arm_k(86, ARM_FOLDED)
    # Antenna : tubes one after the other
    mast_k(1, 0, 0); mast_k(100, 0, 0); mast_k(112, 1, 0); mast_k(124, 1, 1)
    mast_k(130, 1, 1); mast_k(142, 1, 0); mast_k(154, 0, 0)
    key(tur, "rotation_euler", 2, 1, 0.0); key(tur, "rotation_euler", 2, 170, 0.0, 'LINEAR')
    key(tur, "rotation_euler", 2, 266, 2 * math.pi, 'LINEAR')
    sc = bpy.context.scene; sc.frame_start, sc.frame_end = 1, 266; sc.render.fps = 24; sc.frame_set(1)
    return CLIPS

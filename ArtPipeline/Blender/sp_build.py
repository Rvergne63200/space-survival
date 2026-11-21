# Ground solar panel (DA: white / graphite / blue neon / orange lights) — 3 hinged sections, accordion fold + tilt.
# Hierarchy: SolarPanel > Base (cabinet + mast) > Panel_Tilt (rot X, centre section) > Wing_L / Wing_R (rot Y)
import bpy, bmesh, math
from mathutils import Matrix, Vector
SP = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival/ArtPipeline/Blender"
exec(open(SP + r"\al_build.py", encoding="utf8").read())
CELL = 9

PIVOT_Z = 1.00                 # tilt axis (X) height
PW, PL, PT = 0.90, 1.30, 0.06  # section width (x), length (y), thickness
Z0 = 0.08                      # section bottom above the tilt axis
GAP = 0.01                     # between sections
HZ_L = Z0 + 0.100              # left hinge axis height (wing lands on the centre)
HZ_R = Z0 + 0.145              # right hinge axis height (wing lands on the left wing)
TILT = math.radians(30)
XH = {-1: -(PW / 2 + GAP / 2), 1: PW / 2 + GAP / 2 + 0.05}   # hinge axes x ; right one pushed out to clear the folded left wing
HZ = {-1: HZ_L, 1: HZ_R}

def T(*v): return Matrix.Translation(Vector(v))

def solar_mats():
    ms = make_mats("SolarPanel")
    m = bpy.data.materials.get("M_SolarPanel_Cell") or bpy.data.materials.new("M_SolarPanel_Cell")
    b = next(x for x in m.node_tree.nodes if x.type == 'BSDF_PRINCIPLED')
    b.inputs["Base Color"].default_value = (0.03, 0.08, 0.22, 1); b.inputs["Roughness"].default_value = 0.15
    b.inputs["Metallic"].default_value = 0.3; m.diffuse_color = (0.03, 0.08, 0.22, 1)
    return ms + [m]

def section(bm, x0, x1, z0):
    """one panel section: white chamfered frame, 3x4 blue cells on top, graphite underside ribs."""
    cx, w = (x0 + x1) / 2, x1 - x0
    cbox(bm, (w, PL, PT), (cx, 0, z0 + PT / 2), WHITE, 0.02)
    cbox(bm, (w - 0.06, PL - 0.06, 0.012), (cx, 0, z0 + PT - 0.004), DARK, 0.004, 0.0)       # cell bed
    nx, ny = 3, 4
    cw, cl = (w - 0.10) / nx, (PL - 0.10) / ny
    for i in range(nx):
        for j in range(ny):
            cbox(bm, (cw - 0.014, cl - 0.014, 0.006), (x0 + 0.05 + cw * (i + 0.5), -PL / 2 + 0.05 + cl * (j + 0.5), z0 + PT + 0.003),
                 CELL, 0.003, 0.0)
    for y in (-0.4, 0.0, 0.4):                                                                  # underside ribs
        cbox(bm, (w - 0.08, 0.04, 0.02), (cx, y, z0 - 0.009), DARK, 0.006, 0.0)
    pill(bm, (w - 0.12, 0.018, 0.012), (cx, -PL / 2 - 0.004, z0 + PT / 2), NEON)              # front edge neon

def build_base():
    bm = bmesh.new()
    # battery cabinet
    prof = [(-0.36, 0.06), (0.36, 0.06), (0.36, 0.36), (0.30, 0.44), (-0.30, 0.44), (-0.36, 0.36)]   # (y, z)
    prism_x(bm, prof, -0.46, 0.46, WHITE, end_ch=0.06)
    cbox(bm, (1.0, 0.8, 0.07), (0, 0, 0.035), DARK, 0.03)
    cbox(bm, (0.5, 0.03, 0.2), (0.1, -0.36, 0.23), DARK, 0.04, 0.004)                       # front panel
    for k in range(4):                                                                       # charge gauge
        pill(bm, (0.08, 0.02, 0.03), (-0.05 + k * 0.1, -0.375, 0.27), NEON if k < 3 else DARK)
    for k in range(2):
        pill(bm, (0.03, 0.02, 0.03), (-0.05 + k * 0.08, -0.375, 0.18), ORANGE)
    for sx in (-1, 1):
        cbox(bm, (0.03, 0.5, 0.2), (sx * 0.46, 0, 0.24), DARK, 0.04, 0.004)
        for k in range(4):
            cbox(bm, (0.02, 0.4, 0.014), (sx * 0.472, 0, 0.17 + k * 0.045), METAL, 0.003, 0.0)
    for sx in (-1, 1):                                                                        # stabiliser legs
        for sy in (-1, 1):
            d = Vector((sx * 0.8, sy * 0.6, 0)).normalized()
            beam(bm, d * 0.45 + Vector((0, 0, 0.12)), d * 0.95 + Vector((0, 0, 0.05)), 0.08, 0.07, WHITE, 0.02)
            oct_cyl(bm, 0.08, 0.0, 0.05, DARK, T(*(d * 0.97)), ch=0.015)
    # mast + yoke for the tilt axis
    oct_cyl(bm, 0.13, 0.44, 0.52, DARK, ch=0.02)
    oct_cyl(bm, 0.085, 0.52, PIVOT_Z - 0.11, WHITE, ch=0.015)          # ends inside the collar (no shared face)
    oct_cyl(bm, 0.10, PIVOT_Z - 0.14, PIVOT_Z - 0.10, DARK, ch=0.01)
    pill(bm, (0.02, 0.02, 0.22), (0, -0.086, 0.70), NEON)
    return bm

YAW_Z = PIVOT_Z - 0.10          # top of the static collar = turntable axis origin
def build_yaw():
    """rotating head (Z axis) : turntable + yoke carrying the tilt axis. Origin = top of the mast collar."""
    bm = bmesh.new()
    oct_cyl(bm, 0.115, 0.003, 0.035, METAL, ch=0.01, sides=12)                                 # turntable
    add_revolve(bm, [(0.117, 0.012), (0.121, 0.012), (0.121, 0.024), (0.117, 0.024)], [NEON] * 4, 12,
                Matrix.Rotation(math.pi / 12, 4, 'Z'), closed=True)                               # neon ring
    cbox(bm, (0.26, 0.16, 0.04), (0, 0, 0.05), DARK, 0.015)                                     # yoke base
    pz = PIVOT_Z - YAW_Z
    for sx in (-1, 1):
        prof_y = [(-0.07, 0.04), (0.07, 0.04), (0.07, pz + 0.02), (0.03, pz + 0.06), (-0.03, pz + 0.06), (-0.07, pz + 0.02)]
        prism_x(bm, prof_y, sx * 0.10 - 0.02, sx * 0.10 + 0.02, DARK, end_ch=0.008)
    pill(bm, (0.04, 0.02, 0.03), (0, -0.081, 0.05), ORANGE)                                     # heading marker (front)
    return bm

def build_tilt():
    bm = bmesh.new()      # origin = tilt axis
    oct_cyl(bm, 0.045, -0.08, 0.08, METAL, Matrix.Rotation(math.pi / 2, 4, 'Y'), ch=0.01)      # axle
    cbox(bm, (0.12, PL - 0.1, 0.06), (0, 0, Z0 - 0.04), DARK, 0.02)                            # spine
    cbox(bm, (0.12, 0.14, 0.09), (0, 0, 0.02), DARK, 0.03)                                     # hub on the axle
    section(bm, -PW / 2, PW / 2, Z0)
    for sx in (-1, 1):                                                                        # hinge knuckles
        xh, hz = XH[sx], HZ[sx]
        px = sx * (PW / 2 - 0.035)                                                           # post on the centre section
        ys = (-0.45, 0.45) if sx < 0 else (-(PL / 2 + 0.035), PL / 2 + 0.035)                  # right : at the ends, clear of the folded stack
        if sx > 0:
            for y in ys:
                cbox(bm, (0.10, 0.06, PT), (px - 0.03, y - math.copysign(0.02, y), Z0 + PT / 2), DARK, 0.01)
        for y in ys:
            cbox(bm, (0.04, 0.05, hz - (Z0 + PT) + 0.02), (px, y, (Z0 + PT + hz) / 2 + 0.005), DARK, 0.008)
            beam(bm, (px, y, hz), (xh, y, hz), 0.05, 0.04, DARK, 0.008)                        # arm above the wings
            oct_cyl(bm, 0.022, -0.08, 0.08, METAL, T(xh, y, hz) @ Matrix.Rotation(math.pi / 2, 4, 'X'), ch=0.005)
    pill(bm, (0.05, 0.05, 0.03), (0, PL / 2 - 0.08, Z0 - 0.07), ORANGE)                      # status light under the rear
    return bm

def build_wing(side):
    """origin = hinge axis ; side -1 = left, +1 = right."""
    bm = bmesh.new()
    hz, xh = HZ[side], XH[side]
    x_in, x_out = side * (PW / 2 + GAP), side * (PW / 2 + GAP + PW)
    section(bm, min(x_in, x_out), max(x_in, x_out), Z0)
    ys = (-0.36, 0.36) if side < 0 else (-(PL / 2 + 0.035), PL / 2 + 0.035)
    for y in ys:                                                                               # wing side of the hinge
        yw = max(-PL / 2 + 0.05, min(PL / 2 - 0.05, y))
        beam(bm, (x_in + side * 0.06, yw, Z0 + PT / 2), (xh, y, hz), 0.05, 0.05, DARK, 0.01)
        oct_cyl(bm, 0.024, -0.05, 0.05, METAL, T(xh, y, hz) @ Matrix.Rotation(math.pi / 2, 4, 'X'), ch=0.005)
    bmesh.ops.translate(bm, vec=-Vector((xh, 0, hz)), verts=bm.verts)
    return bm, Vector((xh, 0, hz))

def setup():
    for ob in list(bpy.data.objects):
        if not ob.name.startswith("_"): bpy.data.objects.remove(ob)
    for a in list(bpy.data.actions): bpy.data.actions.remove(a)
    for me in list(bpy.data.meshes):
        if me.users == 0: bpy.data.meshes.remove(me)
    sc = bpy.context.scene
    col = bpy.data.collections.get("Models") or bpy.data.collections.new("Models")
    if col.name not in sc.collection.children: sc.collection.children.link(col)
    mats = solar_mats()
    root = bpy.data.objects.new("SolarPanel", None); col.objects.link(root); root.empty_display_size = 0.5
    base = finish2("Base", build_base(), col, root, mats)
    yaw = finish2("Panel_Yaw", build_yaw(), col, base, mats); yaw.location = (0, 0, YAW_Z)
    tilt = finish2("Panel_Tilt", build_tilt(), col, yaw, mats); tilt.location = (0, 0, PIVOT_Z - YAW_Z)
    for side, name in ((-1, "Wing_L"), (1, "Wing_R")):
        bm, hinge = build_wing(side)
        w = finish2(name, bm, col, tilt, mats); w.location = hinge
    bpy.context.view_layer.update()
    tri = lambda o: sum(len(p.vertices) - 2 for p in o.data.polygons)
    return {o.name: tri(o) for o in root.children_recursive if o.type == 'MESH'}

# ------------------------------------------------------------------ animation (single timeline)
CLIPS = [("Panel_Deploy", 1, 72), ("Panel_Fold", 90, 162), ("Rotate_Loop", 180, 276)]
FOLD_L, FOLD_R = math.radians(180), math.radians(-180)
def key(ob, path, idx, f, v):
    getattr(ob, path)[idx] = v
    ob.keyframe_insert(path, index=idx, frame=f)

def animate():
    O = bpy.data.objects
    tilt, wl, wr = O["Panel_Tilt"], O["Wing_L"], O["Wing_R"]
    for ob in (tilt, wl, wr):
        if ob.animation_data: ob.animation_data_clear()
    def pose(f, t, l, r):
        key(tilt, "rotation_euler", 0, f, t); key(wl, "rotation_euler", 1, f, l); key(wr, "rotation_euler", 1, f, r)
    # Panel_Deploy : top wing (right) opens, then the left one, then the array tilts toward the front
    pose(1, 0, FOLD_L, FOLD_R); pose(6, 0, FOLD_L, FOLD_R)
    key(wr, "rotation_euler", 1, 28, 0.0); key(wl, "rotation_euler", 1, 24, FOLD_L)
    key(wl, "rotation_euler", 1, 48, 0.0); key(tilt, "rotation_euler", 0, 50, 0.0)
    pose(72, TILT, 0, 0)
    # Panel_Fold : reverse order
    pose(90, TILT, 0, 0); key(wl, "rotation_euler", 1, 108, 0.0); key(wr, "rotation_euler", 1, 108, 0.0)
    key(tilt, "rotation_euler", 0, 108, 0.0)
    key(wl, "rotation_euler", 1, 132, FOLD_L); key(wr, "rotation_euler", 1, 132, 0.0)
    pose(156, 0, FOLD_L, FOLD_R); pose(162, 0, FOLD_L, FOLD_R)
    # Rotate_Loop : the head turns 360 deg around the vertical axis (4 s), loops seamlessly
    yaw = O["Panel_Yaw"]
    if yaw.animation_data: yaw.animation_data_clear()
    key(yaw, "rotation_euler", 2, 1, 0.0); key(yaw, "rotation_euler", 2, 180, 0.0); key(yaw, "rotation_euler", 2, 276, 2 * math.pi)
    for kp in yaw.animation_data.action.layers[0].strips[0].channelbag(yaw.animation_data.action_slot).fcurves[0].keyframe_points:
        kp.interpolation = 'LINEAR'
    sc = bpy.context.scene; sc.frame_start, sc.frame_end = 1, 276; sc.render.fps = 24; sc.frame_set(1)
    return CLIPS

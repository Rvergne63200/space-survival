# Small wind turbine (~4 m) — DA white / graphite / blue neon / orange lights.
# WindTurbine > Base > Mast_Mid (z) > Mast_Top (z) > Nacelle (yaw z, script) > Rotor (spin y) > Blade_1..3 (fold x)
import bpy, bmesh, math
from mathutils import Matrix, Vector
SP = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival/ArtPipeline/Blender"
exec(open(SP + r"\al_build.py", encoding="utf8").read())

def T(*v): return Matrix.Translation(Vector(v))
RX90 = Matrix.Rotation(math.pi / 2, 4, 'X')      # local +Z -> -Y (forward)

OUT_Z0, OUT_Z1 = 0.46, 1.50     # outer mast tube
MID_L, TOP_L = 1.0, 0.95
MID_TRAVEL, TOP_TRAVEL = 0.80, 0.80
AXIS_Z = 0.20                   # rotor axis above the nacelle origin (yaw bearing)
ROTOR_Y = -0.36                 # rotor plane in front of the yaw axis
HINGE_R = 0.25                  # blade hinge radius
BLADE_L = 1.15
FOLD = math.radians(-90)        # blades folded back along the nacelle

def build_base():
    bm = bmesh.new()
    oct_cyl(bm, 0.48, 0.0, 0.08, DARK, ch=0.03)
    oct_cyl(bm, 0.42, 0.08, 0.36, WHITE, ch=0.06)
    oct_cyl(bm, 0.30, 0.36, OUT_Z0, DARK, ch=0.03)
    add_revolve(bm, [(0.425, 0.20), (0.432, 0.20), (0.432, 0.225), (0.425, 0.225)], [NEON] * 4, 8,
                Matrix.Rotation(math.pi / 8, 4, 'Z'), closed=True)
    for k in range(4):
        a = k * math.pi / 2 + math.pi / 4
        d = Vector((math.cos(a), math.sin(a), 0))
        beam(bm, d * 0.36 + Vector((0, 0, 0.20)), d * 1.02 + Vector((0, 0, 0.05)), 0.09, 0.08, WHITE, 0.025)
        oct_cyl(bm, 0.09, 0.0, 0.05, DARK, T(*(d * 1.04)), ch=0.015)
        add_screw(bm, d * 1.04 + Vector((0, 0, 0.05)), (0, 0, 1), r=0.02, mat=METAL)
        beam(bm, d * 0.10 + Vector((0, 0, OUT_Z0 + 0.32)), d * 0.62 + Vector((0, 0, 0.17)), 0.04, 0.04, DARK, 0.01)   # mast struts (tube -> leg)
    for k in range(3):
        pill(bm, (0.035, 0.02, 0.035), (-0.06 + k * 0.06, -0.41, 0.27), ORANGE)
    cbox(bm, (0.24, 0.03, 0.08), (0, -0.405, 0.15), DARK, 0.015, 0.003)
    # outer mast tube
    oct_cyl(bm, 0.13, OUT_Z0, OUT_Z0 + 0.08, DARK, ch=0.02)
    oct_cyl(bm, 0.11, OUT_Z0 + 0.06, OUT_Z1, WHITE, ch=0.015)
    oct_cyl(bm, 0.125, OUT_Z1 - 0.05, OUT_Z1 + 0.005, DARK, ch=0.01)
    pill(bm, (0.02, 0.02, 0.5), (0, -0.111, 1.0), NEON)
    return bm

def build_mast(r, L, collar_r):
    bm = bmesh.new()       # origin = top of the tube below (retracted)
    oct_cyl(bm, r, -L + 0.05, 0.03, WHITE, ch=0.008)
    oct_cyl(bm, collar_r, 0.0, 0.05, DARK, ch=0.008)
    return bm

def build_nacelle():
    bm = bmesh.new()       # origin = yaw bearing (top of Mast_Top)
    oct_cyl(bm, 0.11, 0.0, 0.05, METAL, ch=0.01, sides=12)
    add_revolve(bm, [(0.112, 0.015), (0.116, 0.015), (0.116, 0.032), (0.112, 0.032)], [NEON] * 4, 12,
                Matrix.Rotation(math.pi / 12, 4, 'Z'), closed=True)
    # chamfered body along Y (profile in x, z), extruded with prism_x after swapping axes
    prof = [(-0.13, AXIS_Z - 0.13), (0.13, AXIS_Z - 0.13), (0.15, AXIS_Z - 0.08), (0.15, AXIS_Z + 0.06),
            (0.10, AXIS_Z + 0.12), (-0.10, AXIS_Z + 0.12), (-0.15, AXIS_Z + 0.06), (-0.15, AXIS_Z - 0.08)]
    tmp = bmesh.new()
    prism_x(tmp, [(x, z) for x, z in prof], ROTOR_Y + 0.12, 0.62, WHITE, end_ch=0.05)    # built along X : rotate to Y
    bmesh.ops.transform(tmp, matrix=Matrix.Rotation(math.pi / 2, 4, 'Z'), verts=tmp.verts)   # X (extrusion) -> +Y (back)
    _copy_into(bm, tmp, None); tmp.free()
    cbox(bm, (0.05, 0.4, 0.12), (0.15, 0.2, AXIS_Z), DARK, 0.02, 0.004)
    cbox(bm, (0.05, 0.4, 0.12), (-0.15, 0.2, AXIS_Z), DARK, 0.02, 0.004)
    for sx in (-1, 1):
        pill(bm, (0.02, 0.32, 0.02), (sx * 0.172, 0.2, AXIS_Z + 0.03), NEON)
        for k in range(3):
            cbox(bm, (0.02, 0.3, 0.012), (sx * 0.17, 0.2, AXIS_Z - 0.04 + k * 0.03 - 0.03), METAL, 0.003, 0.0)
    oct_cyl(bm, 0.08, 0.004, 0.03, DARK, T(0, 0.62, AXIS_Z) @ RX90 @ Matrix.Rotation(math.pi, 4, 'X'), ch=0.01)  # rear cap (proud of the body)
    pill(bm, (0.04, 0.03, 0.03), (0, 0.63, AXIS_Z + 0.06), ORANGE)                         # rear beacon
    oct_cyl(bm, 0.07, 0.0, 0.10, METAL, T(0, ROTOR_Y + 0.12, AXIS_Z) @ RX90, ch=0.01)      # shaft housing
    return bm

def build_rotor():
    bm = bmesh.new()       # origin = rotor axis, rotor plane ; axis = Y (forward = -Y)
    prof = [(0.0, -0.40), (0.06, -0.38), (0.13, -0.30), (0.20, -0.16), (0.22, -0.06), (0.22, 0.06), (0.19, 0.10), (0.0, 0.10)]
    add_revolve(bm, prof, [WHITE, WHITE, WHITE, WHITE, DARK, DARK, DARK], 12, RX90 @ Matrix.Rotation(math.pi / 12, 4, 'Z') @ Matrix.Rotation(math.pi, 4, 'X'))
    add_revolve(bm, [(0.0, -0.405), (0.03, -0.405), (0.03, -0.39), (0.0, -0.39)], [NEON] * 3, 8, RX90 @ Matrix.Rotation(math.pi, 4, 'X'))
    for k in range(3):                                                                 # hinge lugs
        a = k * 2 * math.pi / 3
        d = Vector((math.sin(a), 0, math.cos(a)))
        c = d * (HINGE_R - 0.02)
        rot = Matrix.Rotation(-a, 3, 'Y')
        cbox(bm, (0.14, 0.08, 0.06), c, DARK, 0.015, rot=rot)
    return bm

def build_blade():
    bm = bmesh.new()       # origin = hinge ; blade along +Z, hinge axis = X
    oct_cyl(bm, 0.035, -0.08, 0.08, METAL, Matrix.Rotation(math.pi / 2, 4, 'Y'), ch=0.008)
    cbox(bm, (0.09, 0.05, 0.14), (0, 0, 0.07), DARK, 0.015)                       # root cuff
    # tapered, slightly cambered plank (8 stations)
    n = 8; prev = None
    for i in range(n + 1):
        t = i / n; z = 0.14 + (BLADE_L - 0.14) * t
        w = 0.17 * (1 - t) + 0.07 * t; th = 0.036 * (1 - t) + 0.016 * t
        sec = [(-w / 2, 0.0), (-w / 2 + 0.02, -th / 2), (w / 2 - 0.01, -th / 2 + 0.004), (w / 2, 0.0),
               (w / 2 - 0.01, th / 2), (-w / 2 + 0.02, th / 2 + 0.006)]
        ring = [bm.verts.new((x, y, z)) for x, y in sec]
        if prev:
            for k in range(len(sec)):
                l = (k + 1) % len(sec)
                f = bm.faces.new((prev[k], prev[l], ring[l], ring[k]))
                f.material_index = ORANGE if t > 0.92 else WHITE
        else:
            bm.faces.new(ring[::-1]).material_index = WHITE
        prev = ring
    bm.faces.new(prev).material_index = ORANGE
    pill(bm, (0.015, 0.012, 0.5), (-0.03, -0.022, 0.5), NEON)                      # leading-edge neon (front face)
    return bm

BLADE_ANG = [0, 2 * math.pi / 3, 4 * math.pi / 3]

def setup():
    for ob in list(bpy.data.objects):
        if not ob.name.startswith("_"): bpy.data.objects.remove(ob)
    for a in list(bpy.data.actions): bpy.data.actions.remove(a)
    for me in list(bpy.data.meshes):
        if me.users == 0: bpy.data.meshes.remove(me)
    sc = bpy.context.scene
    col = bpy.data.collections.get("Models") or bpy.data.collections.new("Models")
    if col.name not in sc.collection.children: sc.collection.children.link(col)
    mats = make_mats("WindTurbine")
    root = bpy.data.objects.new("WindTurbine", None); col.objects.link(root); root.empty_display_size = 0.5
    base = finish2("Base", build_base(), col, root, mats)
    mid = finish2("Mast_Mid", build_mast(0.085, MID_L, 0.10), col, base, mats); mid.location = (0, 0, OUT_Z1)
    top = finish2("Mast_Top", build_mast(0.062, TOP_L, 0.078), col, mid, mats); top.location = (0, 0, 0.05)
    nac = finish2("Nacelle", build_nacelle(), col, top, mats); nac.location = (0, 0, 0.05)
    rot = finish2("Rotor", build_rotor(), col, nac, mats); rot.location = (0, ROTOR_Y, AXIS_Z)
    for i, a in enumerate(BLADE_ANG):
        b = finish2(f"Blade_{i + 1}", build_blade(), col, rot, mats)
        b.location = (math.sin(a) * HINGE_R, 0, math.cos(a) * HINGE_R)
        b.rotation_euler = (FOLD, a, 0)
    bpy.context.view_layer.update()
    tri = lambda o: sum(len(p.vertices) - 2 for p in o.data.polygons)
    return {o.name: tri(o) for o in root.children_recursive if o.type == 'MESH'}

# ------------------------------------------------------------------ animation
CLIPS = [("Turbine_Deploy", 1, 96), ("Turbine_Fold", 110, 200), ("Rotor_Spin", 220, 268)]
def key(ob, path, idx, f, v, interp='BEZIER'):
    getattr(ob, path)[idx] = v
    ob.keyframe_insert(path, index=idx, frame=f)
    ad = ob.animation_data
    fc = ad.action.layers[0].strips[0].channelbag(ad.action_slot).fcurves.find(path, index=idx)
    for kp in fc.keyframe_points:
        if abs(kp.co[0] - f) < 1e-4: kp.interpolation = interp

def animate():
    O = bpy.data.objects
    mid, top, rot = O["Mast_Mid"], O["Mast_Top"], O["Rotor"]
    blades = [O[f"Blade_{i}"] for i in (1, 2, 3)]
    for ob in [mid, top, rot] + blades:
        if ob.animation_data: ob.animation_data_clear()
    def mast(f, m, t): key(mid, "location", 2, f, OUT_Z1 + m * MID_TRAVEL); key(top, "location", 2, f, 0.05 + t * TOP_TRAVEL)
    def fold(f, k):
        for b in blades: key(b, "rotation_euler", 0, f, FOLD * k)
    # Turbine_Deploy : mast out (mid then top), then blades open with a small overshoot
    mast(1, 0, 0); fold(1, 1); mast(6, 0, 0); mast(26, 1, 0); mast(48, 1, 1); fold(54, 1)
    fold(84, -0.06); fold(92, 0); mast(96, 1, 1); fold(96, 0)
    # Turbine_Fold : blades close, then mast in (top then mid)
    mast(110, 1, 1); fold(110, 0); fold(146, 1); mast(152, 1, 1); mast(174, 1, 0); mast(196, 0, 0); mast(200, 0, 0); fold(200, 1)
    # Rotor_Spin : one turn in 2 s, linear loop
    key(rot, "rotation_euler", 1, 1, 0.0); key(rot, "rotation_euler", 1, 220, 0.0, 'LINEAR'); key(rot, "rotation_euler", 1, 268, -2 * math.pi, 'LINEAR')
    sc = bpy.context.scene; sc.frame_start, sc.frame_end = 1, 268; sc.render.fps = 24; sc.frame_set(1)
    return CLIPS

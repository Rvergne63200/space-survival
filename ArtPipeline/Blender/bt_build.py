# Batteries : three static models of the same family (white shell, graphite plinth, 5-segment neon gauge, status light).
#   Battery_Wall    : wall pack 0.60 x 0.14 x 0.46 m, hung on the solid wall of the domes. Origin = wall contact point, at
#                     the vertical centre ; back against the wall at y = 0, front = -Y. 2.5 cm graphite bracket : the flat
#                     housing never cuts the 8 / 12 / 16 m walls (see wall_clearance()).
#   Battery_Cabinet : floor cabinet 0.70 x 0.50 x 1.10 m (under the solid wall height), indoor and outdoor.
#   Battery_Bank    : outdoor bank 1.60 x 0.90 x 1.20 m on graphite rails, cooling grille and orange beacon on top.
# Hierarchy : <Model> > Body (static) + Gauge_1 .. Gauge_5 (neon segments, bottom to top, lit by BatteryAnimator)
# The status light (*_Status) is in Body. Cable point : empty "Connector" on the connector boss.
import bpy, bmesh, math
from mathutils import Matrix, Vector
SP = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival/ArtPipeline/Blender"
exec(open(SP + r"\al_build.py", encoding="utf8").read())

MODELS = ("Battery_Wall", "Battery_Cabinet", "Battery_Bank")
SIDES = 12
RX = Matrix.Rotation(math.pi / 2, 4, 'Y')       # local Z -> +X (side connector)
RDOWN = Matrix.Rotation(math.pi, 4, 'X')        # local Z -> -Z (connector under the wall pack)

def small(bm, size, center, mat):
    """small rapported part : light chamfer + edge break scaled to its thinnest side."""
    t = min(size)
    cbox(bm, size, center, mat, min(0.004, t * 0.25), min(0.002, t * 0.12))

def boss(bm, M, r, l, ring_r, ring_l):
    """cable connector : graphite boss and metal ring, along local +Z of M (base at its origin)."""
    oct_cyl(bm, r, -0.005, l, DARK, M=M, ch=0.008, sides=SIDES)
    oct_cyl(bm, ring_r, l - 0.002, l + ring_l, METAL, M=M, ch=0.004, sides=SIDES)

def belt(bm, hw, hd, ch, d, z0, z1, mat=DARK):
    """band around a box shell of half size (hw, hd) and vertical chamfer ch : its footprint is the shell footprint
    offset by d (parallel faces, chamfers included), extruded from z0 to z1, edges broken."""
    foot = [(hw - ch, -hd), (hw, -hd + ch), (hw, hd - ch), (hw - ch, hd), (-hw + ch, hd), (-hw, hd - ch), (-hw, -hd + ch), (-hw + ch, -hd)]
    foot = offset_poly(foot, d)
    tmp = bmesh.new()
    a = [tmp.verts.new((x, y, z0)) for x, y in foot]
    b = [tmp.verts.new((x, y, z1)) for x, y in foot]
    n = len(foot)
    for i in range(n):
        j = (i + 1) % n
        tmp.faces.new((a[i], a[j], b[j], b[i]))
    tmp.faces.new(list(reversed(a))); tmp.faces.new(b)
    bmesh.ops.recalc_face_normals(tmp, faces=tmp.faces)
    _bevel_tmp(tmp, 0.004)
    _copy_into(bm, tmp, mat); tmp.free()

def louvers(bm, frame_size, center, n, axis_z=True):
    """vent grille on a -Y facing front : graphite frame and n metal slats proud of it."""
    fx, fy, fz = frame_size
    cx, cy, cz = center
    small(bm, frame_size, center, DARK)
    pitch = (fz - 0.06) / (n - 1)
    for i in range(n):
        small(bm, (fx - 0.04, fy, 0.016 if fz < 0.4 else 0.03), (cx, cy - 0.004, cz - (fz - 0.06) / 2 + i * pitch), METAL)

# ------------------------------------------------------------------ Battery_Wall
def wall_body():
    bm = bmesh.new()
    cbox(bm, (0.36, 0.030, 0.30), (0, -0.015, 0), DARK, 0.006, 0.002)            # wall bracket, back on the wall
    cbox(bm, (0.60, 0.14, 0.46), (0, -0.095, 0), WHITE, 0.03)                    # housing (front at y = -0.165)
    small(bm, (0.16, 0.012, 0.38), (0, -0.162, 0), DARK)                         # gauge panel, 3 mm proud
    small(bm, (0.04, 0.010, 0.018), (0, -0.170, 0.17), STATUS)                   # status light
    for x in (-0.19, 0.19):
        louvers(bm, (0.14, 0.010, 0.30), (x, -0.167, 0), 5)
    boss(bm, Matrix.Translation((0.20, -0.095, -0.23)) @ RDOWN, 0.035, 0.035, 0.022, 0.015)
    return bm, [(0.11, 0.010, 0.045, (0, -0.170, -0.13 + i * 0.065)) for i in range(5)], (0.20, -0.095, -0.28)

# ------------------------------------------------------------------ Battery_Cabinet
def cabinet_body():
    bm = bmesh.new()
    cbox(bm, (0.66, 0.46, 0.10), (0, 0, 0.03), DARK, 0.02)                       # plinth, 2 cm under the ground
    cbox(bm, (0.70, 0.50, 1.025), (0, 0, 0.5875), WHITE, 0.05)                   # shell (front at y = -0.25)
    small(bm, (0.36, 0.020, 0.78), (0, -0.243, 0.58), DARK)                      # gauge panel, 3 mm proud
    small(bm, (0.05, 0.012, 0.022), (0, -0.257, 0.91), STATUS)
    small(bm, (0.16, 0.006, 0.05), (0, -0.252, 0.16), PLATE)                     # module plate
    for s in (-1, 1):                                                            # side vents
        small(bm, (0.012, 0.30, 0.55), (s * 0.353, 0, 0.55), DARK)
        for i in range(7):
            small(bm, (0.010, 0.26, 0.022), (s * 0.360, 0, 0.33 + i * 0.073), METAL)
    boss(bm, Matrix.Translation((0, 0.12, 1.10)), 0.06, 0.045, 0.035, 0.02)       # connector on top, at the back
    small(bm, (0.12, 0.04, 0.004), (0, 0.01, 1.1005), HAZARD)                    # warning mark in front of it
    return bm, [(0.20, 0.012, 0.085, (0, -0.257, 0.30 + i * 0.13)) for i in range(5)], (0, 0.12, 1.165)

# ------------------------------------------------------------------ Battery_Bank
def bank_body():
    bm = bmesh.new()
    for y in (-0.32, 0.32):                                                      # rails, 2 cm under the ground
        cbox(bm, (1.56, 0.12, 0.10), (0, y, 0.03), DARK, 0.02)
    cbox(bm, (1.60, 0.90, 1.125), (0, 0, 0.6375), WHITE, 0.10)                   # shell (front at y = -0.45)
    belt(bm, 0.80, 0.45, 0.10, 0.01, 0.27, 0.33)                                 # graphite belt, 1 cm proud
    small(bm, (1.20, 0.006, 0.015), (0, -0.462, 0.30), NEON)                     # neon line on the belt
    small(bm, (0.40, 0.020, 0.68), (0, -0.453, 0.70), DARK)                      # gauge panel
    small(bm, (0.06, 0.012, 0.024), (0, -0.467, 1.00), STATUS)
    for x in (-0.48, 0.48):
        louvers(bm, (0.36, 0.012, 0.56), (x, -0.455, 0.70), 7)
    small(bm, (1.00, 0.50, 0.012), (0, 0, 1.204), DARK)                          # cooling grille on top
    for i in range(12):
        small(bm, (0.04, 0.44, 0.012), (-0.44 + i * 0.08, 0, 1.212), METAL)
    oct_cyl(bm, 0.04, 1.195, 1.23, DARK, M=Matrix.Translation((0.64, 0.28, 0)), ch=0.006, sides=SIDES)   # beacon
    oct_cyl(bm, 0.025, 1.228, 1.27, ORANGE, M=Matrix.Translation((0.64, 0.28, 0)), ch=0.008, sides=SIDES)
    boss(bm, Matrix.Translation((0.80, 0, 0.70)) @ RX, 0.06, 0.045, 0.035, 0.02)  # connector on the +X side
    small(bm, (0.004, 0.12, 0.03), (0.8005, 0, 0.82), HAZARD)
    return bm, [(0.24, 0.012, 0.08, (0, -0.467, 0.46 + i * 0.12)) for i in range(5)], (0.865, 0, 0.70)

BUILDERS = {"Battery_Wall": wall_body, "Battery_Cabinet": cabinet_body, "Battery_Bank": bank_body}

# ------------------------------------------------------------------ setup
def tri_ngons(ob):
    bm = bmesh.new(); bm.from_mesh(ob.data)
    ng = [f for f in bm.faces if len(f.verts) > 4]
    if ng: bmesh.ops.triangulate(bm, faces=ng, quad_method='BEAUTY', ngon_method='BEAUTY')
    bm.to_mesh(ob.data); bm.free(); ob.data.update()

def clear():
    for ob in list(bpy.data.objects):
        if not ob.name.startswith("_"): bpy.data.objects.remove(ob)
    for me in list(bpy.data.meshes):
        if me.users == 0: bpy.data.meshes.remove(me)

def build(model):
    clear()
    sc = bpy.context.scene
    col = bpy.data.collections.get("Models") or bpy.data.collections.new("Models")
    if col.name not in sc.collection.children: sc.collection.children.link(col)
    mats = make_mats(model)
    root = bpy.data.objects.new(model, None); col.objects.link(root); root.empty_display_size = 0.3
    bm, segs, conn = BUILDERS[model]()
    obs = [finish2("Body", bm, col, root, mats)]
    for i, (sx, sy, sz, c) in enumerate(segs):
        g = bmesh.new()
        pill(g, (sx, sy, sz), c, NEON)
        obs.append(finish2(f"Gauge_{i + 1}", g, col, root, mats, origin=c))
    for o in obs:
        tri_ngons(o)
        o.data.set_sharp_from_angle(angle=math.radians(20))
    e = bpy.data.objects.new("Connector", None); col.objects.link(e)
    e.parent = root; e.location = conn; e.empty_display_size = 0.05
    bpy.context.view_layer.update()
    return {o.name: sum(len(p.vertices) - 2 for p in o.data.polygons) for o in obs}

def wall_clearance():
    """smallest gap between the wall pack housing and the inner wall of each dome (pack centred on the wall)."""
    out = {}
    body = bpy.data.objects["Body"]
    for label, r_in in (("8m", 3.75), ("12m", 5.75), ("16m", 7.75)):
        worst = 1e9
        for v in body.data.vertices:
            p = body.matrix_world @ v.co
            if p.y > -0.0245: continue        # bracket : its back is on the wall plane, its corners go into the wall (hidden)
            worst = min(worst, r_in - math.hypot(p.x, p.y + r_in))
        out[label] = round(worst, 4)
    return out

# ------------------------------------------------------------------ preview
def preview_rig(target_z, dist, wall=False):
    sc = bpy.context.scene
    cam = bpy.data.objects.get("_Cam")
    if cam is None:
        cam = bpy.data.objects.new("_Cam", bpy.data.cameras.new("_Cam")); sc.collection.objects.link(cam)
    cam.rotation_mode = 'XYZ'
    sc.camera = cam
    sun = bpy.data.objects.get("_Sun")
    if sun is None:
        sun = bpy.data.objects.new("_Sun", bpy.data.lights.new("_Sun", 'SUN')); sc.collection.objects.link(sun)
    sun.data.energy = 3.0; sun.rotation_euler = (math.radians(50), 0, math.radians(-35))
    fill = bpy.data.objects.get("_Fill")
    if fill is None:
        fill = bpy.data.objects.new("_Fill", bpy.data.lights.new("_Fill", 'SUN')); sc.collection.objects.link(fill)
    fill.data.energy = 0.8; fill.rotation_euler = (math.radians(70), 0, math.radians(150))
    w = sc.world or bpy.data.worlds.new("World"); sc.world = w
    bg = next(n for n in w.node_tree.nodes if n.type == 'BACKGROUND')
    bg.inputs[0].default_value = (0.25, 0.12, 0.07, 1); bg.inputs[1].default_value = 0.6
    old = bpy.data.objects.get("_Wall") or bpy.data.objects.get("_Ground")
    if old: bpy.data.objects.remove(old)
    bm = bmesh.new()
    if wall:   # temporary dome wall (8 m dome : inner radius 3.75), never exported
        add_revolve(bm, [(3.75, 0.0), (3.75, 1.20), (3.95, 1.20), (3.95, 0.0)], [WHITE] * 4, 96,
                    Matrix.Translation((0, -3.75, 0)), closed=True)
        name = "_Wall"
    else:
        cbox(bm, (6, 6, 0.1), (0, 0, -0.05), DARK, 0, 0)
        name = "_Ground"
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    m = bpy.data.materials.get("_Floor") or bpy.data.materials.new("_Floor")
    m.diffuse_color = (0.35, 0.18, 0.1, 1)
    b = next(x for x in m.node_tree.nodes if x.type == 'BSDF_PRINCIPLED')
    b.inputs["Base Color"].default_value = (0.35, 0.18, 0.1, 1)
    me.materials.append(m)
    ob = bpy.data.objects.new(name, me); sc.collection.objects.link(ob)
    sc.render.engine = 'BLENDER_EEVEE'
    sc.render.resolution_x, sc.render.resolution_y = 1200, 900
    sc.view_settings.view_transform = 'AgX' if 'AgX' in [i.identifier for i in sc.view_settings.bl_rna.properties['view_transform'].enum_items] else 'Filmic'
    return cam

def aim(cam, az_deg, el_deg, dist, target):
    t = Vector(target)
    az, el = math.radians(az_deg), math.radians(el_deg)
    cam.location = t + Vector((math.sin(az) * math.cos(el), -math.cos(az) * math.cos(el), math.sin(el))) * dist
    cam.rotation_euler = (t - cam.location).to_track_quat('-Z', 'Y').to_euler()

def set_gauge(level):
    """preview only : lit segments neon, the others dark."""
    for i in range(1, 6):
        o = bpy.data.objects.get(f"Gauge_{i}")
        if o: o.hide_render = False; o.data.materials[0] = bpy.data.materials[f"M_{o.parent.name}_Neon" if i <= level else f"M_{o.parent.name}_Dark"]

def export(model):
    root = bpy.data.objects[model]
    fbx = rf"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival/Assets/Resources/Models/{model}/{model}.fbx"
    import os; os.makedirs(os.path.dirname(fbx), exist_ok=True)
    keep = root.matrix_world.copy(); root.matrix_world = Matrix()
    bpy.ops.object.select_all(action='DESELECT')
    for o in [root] + list(root.children_recursive): o.select_set(True)
    bpy.context.view_layer.objects.active = root
    bpy.ops.export_scene.fbx(filepath=fbx, use_selection=True, object_types={'EMPTY', 'MESH'},
        apply_scale_options='FBX_SCALE_UNITS', bake_space_transform=False, axis_forward='-Z', axis_up='Y',
        mesh_smooth_type='FACE', add_leaf_bones=False, path_mode='RELATIVE', bake_anim=False)
    root.matrix_world = keep
    return fbx

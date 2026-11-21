# Drill (derrick on a tripod, roller-cone head pushed by two hydraulic jacks) ~ D 3.6 x 4.45 m + side block.
# DA white / graphite / blue neon / orange lights ; holographic screen on the side block.
# Drill > Body (static : platform, housing, tripod, crown block, jack tubes, side block, console)
#       > Carriage (translates Z : drive block + motor + jack rods)        clips Drill_Lower / Drill_Raise
#           > Drill_Head (rotates Z : hex rod + collar + 3 roller cones)  clip  Drill_Spin (loop)
#       > Hologram (static, _Hologram material) > Hologram_Screen (UV quad, for a future UI texture)
#       > DrillDust (empty at the drilling point, +Z = up)
import bpy, bmesh, math
from mathutils import Matrix, Vector
SP = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival/ArtPipeline/Blender"
exec(open(SP + r"\al_build.py", encoding="utf8").read())
HOLO = 9

def T(*v): return Matrix.Translation(Vector(v))
RY90 = Matrix.Rotation(math.pi / 2, 4, 'Y')       # local +Z -> +X
ROT8 = Matrix.Rotation(math.pi / 8, 4, 'Z')

# ------------------------------------------------------------------ layout (m)
P_R0, P_R1 = 0.70, 1.80          # platform inner / outer vertex radius (z -0.04 .. 0.20)
H_R0, H_R1 = 0.76, 1.30          # housing ring inner / outer (z 0.19 .. 0.58)
HTOP = 0.58
LEG_ANG = (90, 210, 330)         # front (-Y) left open between two legs
FOOT_R, FOOT_Z = 1.50, 0.24
APEX = Vector((0, 0, 3.92))
CROWN = (3.85, 4.35)
JACK_X = 0.95                    # jack axis (x = +-0.95, y = 0), tubes on the housing
TUBE_TOP = 1.30
CB_UP, CB_DOWN = 2.00, 1.40      # carriage bottom : raised (rest) / lowered
TRAVEL = CB_UP - CB_DOWN
ROD_L = 0.80
TIP_UP = 0.58                    # roller cone tips at rest (lowered : TIP_UP - TRAVEL = -0.02)
BX0, BX1 = 1.90, 2.80            # side block x range
HOLO_C = Vector((2.35, -0.12, 1.52))   # hologram screen centre
HOLO_W, HOLO_H = 0.62, 0.36
HOLO_TILT = math.radians(-14)

def drill_mats():
    ms = make_mats("Drill")
    m = bpy.data.materials.get("M_Drill_Hologram") or bpy.data.materials.new("M_Drill_Hologram")
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

def ring8(bm, prof, mats):
    add_revolve(bm, prof, mats, 8, ROT8, closed=True)

def leg_dir(a): return Vector((math.cos(math.radians(a)), math.sin(math.radians(a)), 0))

# ------------------------------------------------------------------ body
def build_body():
    bm = bmesh.new()
    # platform (graphite, open in the middle) + white housing ring (graphite bore) + neon ring
    ring8(bm, [(P_R0, -0.04), (P_R1, -0.04), (P_R1, 0.15), (P_R1 - 0.06, 0.20), (P_R0, 0.20)], [DARK] * 5)
    ring8(bm, [(H_R0, 0.19), (H_R1, 0.19), (H_R1, 0.50), (H_R1 - 0.08, HTOP), (H_R0, HTOP)], [WHITE, WHITE, WHITE, WHITE, DARK])
    ring8(bm, [(H_R1 - 0.02, 0.40), (H_R1 + 0.012, 0.40), (H_R1 + 0.012, 0.43), (H_R1 - 0.02, 0.43)], [NEON] * 4)
    for k in range(8):                                                          # bolts on the platform
        a = math.pi / 8 + k * math.pi / 4 + math.pi / 8
        add_screw(bm, (1.56 * math.cos(a), 1.56 * math.sin(a), 0.20), (0, 0, 1), r=0.022, mat=METAL)
    # tripod : white legs, graphite feet + collars, crown block
    for a in LEG_ANG:
        d = leg_dir(a)
        foot = d * FOOT_R + Vector((0, 0, FOOT_Z))
        top = d * 0.22 + Vector((0, 0, APEX.z))
        beam(bm, foot, top, 0.16, 0.14, WHITE, 0.04)
        oct_cyl(bm, 0.15, 0.19, 0.30, DARK, T(*(d * FOOT_R)), ch=0.03)
        for t in (0.30, 0.62):
            p = foot.lerp(top, t)
            beam(bm, p - (top - foot).normalized() * 0.06, p + (top - foot).normalized() * 0.06, 0.19, 0.17, DARK, 0.03)
    cbox(bm, (0.80, 0.80, CROWN[1] - CROWN[0]), (0, 0, sum(CROWN) / 2), WHITE, 0.14)
    cbox(bm, (0.83, 0.83, 0.08), (0, 0, CROWN[0] + 0.06), DARK, 0.15, 0.004)
    oct_cyl(bm, 0.07, CROWN[1] - 0.02, CROWN[1] + 0.06, ORANGE, ch=0.02)     # beacon
    oct_cyl(bm, 0.18, CROWN[0] - 0.06, CROWN[0] + 0.01, DARK, ch=0.02)        # pulley housing under the crown
    # hydraulic jack tubes (open bore : the rods slide inside without touching)
    for s in (-1, 1):
        M = T(s * JACK_X, 0, 0)
        add_revolve(bm, [(0.05, HTOP - 0.03), (0.075, HTOP - 0.03), (0.075, TUBE_TOP - 0.02), (0.068, TUBE_TOP), (0.05, TUBE_TOP)],
                    [WHITE] * 5, 8, M @ ROT8, closed=True)
        ring8_M = M @ ROT8
        # collars : bore inside the tube wall (r 0.062), never on the tube bore plane
        add_revolve(bm, [(0.062, HTOP - 0.01), (0.11, HTOP - 0.01), (0.11, HTOP + 0.07), (0.062, HTOP + 0.07)], [DARK] * 4, 8, ring8_M, closed=True)
        add_revolve(bm, [(0.062, TUBE_TOP - 0.10), (0.085, TUBE_TOP - 0.10), (0.085, TUBE_TOP - 0.06), (0.062, TUBE_TOP - 0.06)], [DARK] * 4, 8, ring8_M, closed=True)
    # ---- side block : white caisson, graphite skirt, ore bin (back), sloped console (front)
    bx, bw = (BX0 + BX1) / 2, BX1 - BX0
    cbox(bm, (bw + 0.04, 1.14, 0.12), (bx, 0, 0.04), DARK, 0.03)
    cbox(bm, (bw, 1.10, 0.90), (bx, 0, 0.43), WHITE, 0.08)
    for k in range(6):                                                          # vents (+X face)
        cbox(bm, (0.02, 0.62, 0.022), (BX1 + 0.006, 0.05, 0.26 + k * 0.075), METAL, 0.004, 0.0)
    cbox(bm, (0.012, 0.70, 0.50), (BX1 - 0.002, 0.05, 0.445), DARK, 0.03, 0.003)
    # ore bin : rim + grate (back half of the top)
    cbox(bm, (bw - 0.10, 0.50, 0.06), (bx, 0.24, 0.88), DARK, 0.02)
    for k in range(6):
        cbox(bm, (bw - 0.20, 0.03, 0.012), (bx, 0.06 + k * 0.072, 0.913), METAL, 0.003, 0.0)
    # sloped console (front half)
    prism_x(bm, [(-0.50, 0.85), (-0.02, 0.85), (-0.02, 1.08), (-0.46, 0.96)], BX0 + 0.08, BX1 - 0.08, DARK, end_ch=0.02)
    slope = math.atan2(1.08 - 0.96, 0.44)
    R = Matrix.Rotation(slope, 3, 'X')
    def on_slope(u, v, lift):        # u along x, v along the slope (0 = front edge), lift above the face
        base = Vector((bx + u, -0.46, 0.96)) + R @ Vector((0, v, 0))
        return base + R @ Vector((0, 0, lift))
    cbox(bm, (0.50, 0.20, 0.012), on_slope(-0.05, 0.25, 0.006), INNER, 0.015, 0.002, rot=R)           # touch strip
    pill(bm, (0.16, 0.04, 0.02), on_slope(-0.05, 0.08, 0.008), STATUS, rot=R)                          # status light
    for k, m in enumerate((WHITE, WHITE, ORANGE)):
        pill(bm, (0.045, 0.04, 0.02), on_slope(0.24, 0.10 + k * 0.08, 0.008), m, rot=R)
    # holographic projector on top of the console
    MP = T(HOLO_C.x, -0.08, 1.08)
    oct_cyl(bm, 0.08, -0.01, 0.03, METAL, MP, ch=0.01)
    add_revolve(bm, [(0.055, 0.025), (0.072, 0.025), (0.072, 0.04), (0.055, 0.04)], [NEON] * 4, 8, MP @ ROT8, closed=True)
    oct_cyl(bm, 0.04, 0.03, 0.05, METAL, MP, ch=0.006)
    # conduit block -> housing
    M = T(H_R1 - 0.15, 0.25, 0.40) @ RY90
    oct_cyl(bm, 0.085, 0.0, BX0 - H_R1 + 0.17, DARK, M, ch=0.01)
    for z in (0.22, BX0 - H_R1 - 0.02):
        oct_cyl(bm, 0.10, z - 0.025, z + 0.025, METAL, M, ch=0.008)
    return bm

# ------------------------------------------------------------------ moving parts (built in world coords at rest)
def build_carriage():
    bm = bmesh.new()       # origin = carriage bottom centre
    z0 = CB_UP
    oct_cyl(bm, 0.55, z0, z0 + 0.60, DARK, ch=0.08)
    add_revolve(bm, [(0.535, z0 + 0.42), (0.556, z0 + 0.42), (0.556, z0 + 0.45), (0.535, z0 + 0.45)], [NEON] * 4, 8, ROT8, closed=True)
    oct_cyl(bm, 0.20, z0 - 0.06, z0 + 0.01, METAL, ch=0.02)                                    # gearbox output collar
    oct_cyl(bm, 0.28, z0 + 0.59, z0 + 0.88, WHITE, ch=0.05)                                    # motor
    oct_cyl(bm, 0.22, z0 + 0.87, z0 + 0.95, DARK, ch=0.02)
    for k in range(8):                                                                         # motor cooling ribs
        a = k * math.pi / 4 + math.pi / 8
        cbox(bm, (0.06, 0.025, 0.24), (0.29 * math.cos(a), 0.29 * math.sin(a), z0 + 0.73), DARK, 0.008,
             rot=Matrix.Rotation(a, 3, 'Z'))
    for s in (-1, 1):                                                                          # lugs + jack rods
        cbox(bm, (0.50, 0.16, 0.15), (s * 0.72, 0, z0 + 0.10), DARK, 0.04)
        oct_cyl(bm, 0.10, z0 - 0.03, z0 + 0.03, METAL, T(s * JACK_X, 0, 0), ch=0.01)
        oct_cyl(bm, 0.04, z0 - ROD_L, z0, METAL, T(s * JACK_X, 0, 0), ch=0.005)
    for s in (-1, 1):                                                                          # front panel detail
        cbox(bm, (0.16, 0.012, 0.24), (s * 0.13, -0.513, z0 + 0.24), METAL, 0.01, 0.002)
    return bm

def build_head():
    bm = bmesh.new()       # origin = axis at the carriage bottom ; spins around Z
    t = TIP_UP
    oct_cyl(bm, 0.10, t + 0.66, CB_UP + 0.02, METAL, ch=0.0, sides=6)                         # hex rod
    for z in (t + 0.95, CB_UP - 0.30):
        oct_cyl(bm, 0.13, z - 0.05, z + 0.05, DARK, ch=0.015, sides=6)                         # rod collars
    oct_cyl(bm, 0.24, t + 0.57, t + 0.67, METAL, ch=0.02, sides=12)
    oct_cyl(bm, 0.34, t + 0.28, t + 0.58, DARK, ch=0.05, sides=12)                             # bit body
    for k in range(6):                                                                         # junk slots (metal)
        a = k * math.pi / 3
        cbox(bm, (0.05, 0.02, 0.22), (0.334 * math.cos(a), 0.334 * math.sin(a), t + 0.43), METAL, 0.006,
             rot=Matrix.Rotation(a + math.pi / 2, 3, 'Z'))
    for k in range(3):                                                                         # roller cones, tilted out
        a = k * 2 * math.pi / 3
        M = T(0.15 * math.cos(a), 0.15 * math.sin(a), t) @ Matrix.Rotation(a, 4, 'Z') @ Matrix.Rotation(math.radians(-18), 4, 'Y')
        prof = [(0, 0.0), (0.035, 0.0), (0.07, 0.06), (0.10, 0.10), (0.13, 0.17), (0.155, 0.24), (0.17, 0.31), (0, 0.31)]
        add_revolve(bm, prof, [METAL, DARK, METAL, DARK, METAL, DARK, METAL], 8, M)
    return bm

def build_hologram(col, root, holo_mat):
    for n in ("Hologram", "Hologram_Screen"):
        o = bpy.data.objects.get(n)
        if o: bpy.data.objects.remove(o, do_unlink=True)
    R = Matrix.Rotation(HOLO_TILT, 3, 'X')
    L0 = Vector((HOLO_C.x, -0.08, 1.13))                         # projector lens
    def sp(u, v): return HOLO_C + R @ Vector((u, 0, v))
    bm = bmesh.new()
    def sbox(size, u, v): add_box(bm, size, sp(u, v), 0, rot=R)
    for v in (-HOLO_H / 2, HOLO_H / 2): sbox((HOLO_W, 0.004, 0.008), 0, v)
    for u in (-HOLO_W / 2, HOLO_W / 2): sbox((0.008, 0.004, HOLO_H), u, 0)
    for u in (-HOLO_W / 2 + 0.03, HOLO_W / 2 - 0.03):            # corner ticks
        for v in (-HOLO_H / 2 + 0.03, HOLO_H / 2 - 0.03):
            sbox((0.03, 0.004, 0.004), u, v)
    b0 = [bm.verts.new(L0 + Vector((x, y, 0))) for x, y in ((-0.03, -0.03), (0.03, -0.03), (0.03, 0.03), (-0.03, 0.03))]
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
    mats = drill_mats()
    root = bpy.data.objects.new("Drill", None); col.objects.link(root); root.empty_display_size = 1
    finish2("Body", build_body(), col, root, mats[:HOLO])
    car = finish2("Carriage", build_carriage(), col, root, mats[:HOLO], origin=(0, 0, CB_UP))
    head = finish2("Drill_Head", build_head(), col, car, mats[:HOLO], origin=(0, 0, CB_UP))
    head.location = (0, 0, 0)                                    # local to the carriage (same origin)
    build_hologram(col, root, mats[HOLO])
    dust = bpy.data.objects.new("DrillDust", None); col.objects.link(dust)
    dust.parent = root; dust.location = (0, 0, 0.02); dust.empty_display_type = 'SINGLE_ARROW'; dust.empty_display_size = 0.4
    bpy.context.view_layer.update()
    tri = lambda o: sum(len(p.vertices) - 2 for p in o.data.polygons)
    return {o.name: tri(o) for o in root.children_recursive if o.type == 'MESH'}

# ------------------------------------------------------------------ animation
CLIPS = [("Drill_Lower", 1, 48), ("Drill_Raise", 60, 108), ("Drill_Spin", 120, 168)]
def key(ob, path, idx, f, v, interp='BEZIER'):
    getattr(ob, path)[idx] = v
    ob.keyframe_insert(path, index=idx, frame=f)
    ad = ob.animation_data
    fc = ad.action.layers[0].strips[0].channelbag(ad.action_slot).fcurves.find(path, index=idx)
    for kp in fc.keyframe_points:
        if abs(kp.co[0] - f) < 1e-4: kp.interpolation = interp

def animate():
    O = bpy.data.objects
    car, head = O["Carriage"], O["Drill_Head"]
    for ob in (car, head):
        if ob.animation_data: ob.animation_data_clear()
    for f, v in ((1, CB_UP), (48, CB_DOWN), (60, CB_DOWN), (108, CB_UP)): key(car, "location", 2, f, v)
    key(head, "rotation_euler", 2, 1, 0.0); key(head, "rotation_euler", 2, 120, 0.0, 'LINEAR'); key(head, "rotation_euler", 2, 168, -2 * math.pi, 'LINEAR')
    sc = bpy.context.scene; sc.frame_start, sc.frame_end = 1, 168; sc.render.fps = 24; sc.frame_set(1)
    return CLIPS

# ------------------------------------------------------------------ checks
def collisions(step=1):
    """Carriage / Drill_Head against Body over the feed clips, and the head at the lowered pose while spinning."""
    from mathutils.bvhtree import BVHTree
    O = bpy.data.objects; sc = bpy.context.scene; dg = bpy.context.evaluated_depsgraph_get()
    def tree(ob):
        M = ob.matrix_world
        return BVHTree.FromPolygons([M @ v.co for v in ob.data.vertices], [p.vertices[:] for p in ob.data.polygons])
    body = tree(O["Body"]); hits = []
    for f in range(1, 109, step):
        sc.frame_set(f); dg.update()
        for n in ("Carriage", "Drill_Head"):
            k = len(tree(O[n]).overlap(body))
            if k: hits.append((n, f, k))
    car = O["Carriage"]                                          # lowered + spinning : pose by hand
    sc.frame_set(48); dg.update(); z_low = car.location.z
    for f in range(120, 169, 4):
        sc.frame_set(f); car.location.z = z_low; dg.update()
        k = len(tree(O["Drill_Head"]).overlap(body))
        if k: hits.append(("Drill_Head(spin)", f, k))
    sc.frame_set(1)
    return hits

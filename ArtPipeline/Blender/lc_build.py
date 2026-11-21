# Landing capsule (start of the game, first shelter) — octagonal cone ~ D 4.3 x 3.8 m on 4 legs.
# Walkable hollow interior (floor 0.72, empty : furnished later in Unity), door flush with the front facet (-Y)
# that folds out and down into a ramp.
# LandingCapsule > Body (static : shell, interior, legs, roof)
#                > Door (origin on the hinge axis, rot X)      clips Door_Open / Door_Close
import bpy, bmesh, math
from mathutils import Matrix, Vector
SP = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival/ArtPipeline/Blender"
exec(open(SP + r"\al_build.py", encoding="utf8").read())

S8, C8, T8 = math.sin(math.pi / 8), math.cos(math.pi / 8), math.tan(math.pi / 8)
ROT8 = Matrix.Rotation(math.pi / 8, 4, 'Z')          # vertices at 22.5 + k*45 deg -> a flat faces -Y

# ------------------------------------------------------------------ layout (m)
Z0, Z1 = 0.60, 3.30          # shell (walls) bottom / top
R0, R1 = 2.15, 1.50          # outer VERTEX radius at Z0 / Z1
TW = 0.08                    # wall thickness (horizontal, across a flat)
FL = 0.72                    # interior floor = door threshold
ZT = 2.67                    # top of the door opening
ZD = ZT - 0.075              # top of the door leaf : the top edge rises ~6 cm while swinging, the slot above
                             # is hidden from the inside by a lip the leaf never sweeps (it moves outward)
W2 = 0.56                    # half width of the door opening (player collider ~0.85 m wide)
ALPHA = math.atan(C8 * (R0 - R1) / (Z1 - Z0))      # facet tilt from vertical
KR = 0.035                   # hinge knuckle radius
FRONT = math.radians(270)

def ro(z): return R0 + (R1 - R0) * (z - Z0) / (Z1 - Z0)      # outer vertex radius
def ri(z): return ro(z) - TW / C8                           # inner vertex radius
def fo(z): return ro(z) * C8                                # outer flat distance

HINGE = Vector((0.0, -fo(FL) - 0.045, FL))                  # door axis (X), just outside the sill face

def nrm(a): return Vector((math.cos(a), math.sin(a), 0))
def tan_(a): return Vector((-math.sin(a), math.cos(a), 0))

def fpt(a, u, z, d):
    """point on the facet centred on angle a : u along the facet, z height, d = horizontal offset outside the
    outer surface (d < 0 : inside the wall / cabin). u = '+'/'-' : facet edge at that depth."""
    dist = fo(z) + d
    if u in ('+', '-'): u = (1 if u == '+' else -1) * dist * T8
    return nrm(a) * dist + tan_(a) * u + Vector((0, 0, z))

def hexa(bm, A, B, mo, mi, ms, eb=EB):
    """closed hexahedron from two quads (A outer, B inner), edge-broken."""
    tmp = bmesh.new()
    a = [tmp.verts.new(p) for p in A]; b = [tmp.verts.new(p) for p in B]
    tmp.faces.new(a).material_index = mo
    tmp.faces.new(b[::-1]).material_index = mi
    for i in range(4):
        j = (i + 1) % 4
        tmp.faces.new((a[j], a[i], b[i], b[j])).material_index = ms
    bmesh.ops.recalc_face_normals(tmp, faces=tmp.faces)
    _bevel_tmp(tmp, eb)
    _copy_into(bm, tmp, None); tmp.free()

def panel(bm, a, z0, z1, uL, uR, d_out, d_in, mo, mi=None, ms=DARK, eb=EB):
    """facet-aligned slab between depth d_out (outside) and -d_in, over z0..z1 and uL..uR."""
    mi = mo if mi is None else mi
    A = [fpt(a, uL, z0, d_out), fpt(a, uR, z0, d_out), fpt(a, uR, z1, d_out), fpt(a, uL, z1, d_out)]
    B = [fpt(a, uL, z0, -d_in), fpt(a, uR, z0, -d_in), fpt(a, uR, z1, -d_in), fpt(a, uL, z1, -d_in)]
    hexa(bm, A, B, mo, mi, ms, eb)

def facet_rot(a):
    """rotation : local X = along the facet, local -Y = outward facet normal, local Z = up the facet."""
    t = tan_(a); N = (nrm(a) * math.cos(ALPHA) + Vector((0, 0, math.sin(ALPHA))))
    up = N.cross(t)
    return Matrix((t, -N, up)).transposed()

def ring_slant(bm, z0, z1, d0, d1, mat):
    """octagonal ring hugging the slanted outer wall between depths d0 (inside) and d1 (outside)."""
    k = 1 / C8
    add_revolve(bm, [(ro(z0) + d0 * k, z0), (ro(z0) + d1 * k, z0), (ro(z1) + d1 * k, z1), (ro(z1) + d0 * k, z1)],
                [mat] * 4, 8, ROT8, closed=True)

# ------------------------------------------------------------------ body
def build_shell(bm):
    # 7 facets as one open strip (vertices 292.5 deg -> 247.5 deg, the long way round)
    tmp = bmesh.new()
    angs = [math.radians(292.5 + 45 * j) for j in range(8)]
    def ringv(r, z): return [tmp.verts.new((r * math.cos(a), r * math.sin(a), z)) for a in angs]
    O0, O1, I0, I1 = ringv(ro(Z0), Z0), ringv(ro(Z1), Z1), ringv(ri(Z0), Z0), ringv(ri(Z1), Z1)
    for j in range(7):
        k = j + 1
        tmp.faces.new((O0[j], O0[k], O1[k], O1[j])).material_index = WHITE
        tmp.faces.new((I0[k], I0[j], I1[j], I1[k])).material_index = WHITE
        tmp.faces.new((O0[k], O0[j], I0[j], I0[k])).material_index = DARK
        tmp.faces.new((O1[j], O1[k], I1[k], I1[j])).material_index = DARK
    tmp.faces.new((O0[0], O1[0], I1[0], I0[0])).material_index = DARK
    tmp.faces.new((O0[7], I0[7], I1[7], O1[7])).material_index = DARK
    bmesh.ops.recalc_face_normals(tmp, faces=tmp.faces)
    _bevel_tmp(tmp, EB)
    _copy_into(bm, tmp, None); tmp.free()
    # front facet around the door opening : sill, lintel, jambs (jamb faces graphite)
    panel(bm, FRONT, Z0, FL, '-', '+', 0.0, TW, WHITE)
    panel(bm, FRONT, ZT, Z1, '-', '+', 0.0, TW, WHITE)
    panel(bm, FRONT, FL, ZT, '-', -W2, 0.0, TW, WHITE)
    panel(bm, FRONT, FL, ZT, W2, '+', 0.0, TW, WHITE)

def build_door_frames(bm):
    # exterior frame (4 cm proud, no shared planes : sides run past the top piece)
    panel(bm, FRONT, FL - 0.02, ZT + 0.05, -W2 - 0.05, -W2, 0.04, 0.005, DARK)
    panel(bm, FRONT, FL - 0.02, ZT + 0.05, W2, W2 + 0.05, 0.04, 0.005, DARK)
    panel(bm, FRONT, ZT, ZT + 0.05, -W2, W2, 0.04, 0.005, DARK)
    # small visor (16 cm) + status light
    panel(bm, FRONT, ZT + 0.12, ZT + 0.19, -W2 - 0.10, W2 + 0.10, 0.16, 0.005, DARK)
    zc = ZT + 0.155
    pill(bm, (0.44, 0.02, 0.035), fpt(FRONT, 0.0, zc, 0.168), STATUS, rot=Matrix.Rotation(-ALPHA, 3, 'X'))
    # interior frame (3 cm into the cabin)
    din = TW + 0.03
    panel(bm, FRONT, FL, ZT + 0.06, -W2 - 0.06, -W2, -(TW - 0.005), din, DARK)
    panel(bm, FRONT, FL, ZT + 0.06, W2, W2 + 0.06, -(TW - 0.005), din, DARK)
    panel(bm, FRONT, ZD - 0.03, ZT + 0.06, -W2, W2, -TW, din, DARK)            # top piece = lip behind the leaf top
    # fixed hinge knuckles + brackets into the sill face
    hx, hy, hz = HINGE
    for s in (-1, 1):
        oct_cyl(bm, KR, 0.27, 0.47, DARK, Matrix.Translation(HINGE) @ Matrix.Rotation(s * math.pi / 2, 4, 'Y'), ch=0.008)
        cbox(bm, (0.18, 0.06, 0.07), (s * 0.37, hy + 0.025, hz - 0.045), DARK, 0.012)

def build_exterior(bm):
    # heat shield (graphite) + base band, top band, neon ring
    add_revolve(bm, [(0, 0.26), (1.72, 0.26), (1.82, 0.33), (2.20, 0.58), (2.20, 0.62), (0, 0.62)], [DARK] * 5, 8, ROT8)
    ring_slant(bm, Z0 + 0.005, 0.66, -0.02, 0.014, DARK)     # stays under the door knuckle
    ring_slant(bm, 3.12, Z1 - 0.005, -0.02, 0.014, DARK)
    ring_slant(bm, 3.00, 3.035, -0.02, 0.012, NEON)
    # vertical corner ribs (not on the two door corners)
    for k in (0, 1, 2, 3, 4, 7):
        a = math.pi / 8 + k * math.pi / 4
        rd, tg = nrm(a), tan_(a)
        def q(z):
            c = rd * ro(z) + Vector((0, 0, z))
            return [c + rd * 0.03 - tg * 0.035, c + rd * 0.03 + tg * 0.035, c - rd * 0.03 + tg * 0.035, c - rd * 0.03 - tg * 0.035]
        A, B = q(0.72), q(3.10)
        hexa(bm, A, B, DARK, DARK, DARK)
    # side facets : inset panels + vents (right / left / back)
    for a in (0.0, math.pi):
        panel(bm, a, 1.10, 2.35, -0.45, 0.45, 0.012, 0.005, DARK, eb=0.004)
        for k in range(7):
            panel(bm, a, 1.30 + k * 0.12, 1.33 + k * 0.12, -0.36, 0.36, 0.024, 0.0, METAL, ms=METAL, eb=0.003)
        for su in (-1, 1):
            for z in (1.18, 2.27):
                add_screw(bm, fpt(a, su * 0.40, z, 0.012), nrm(a) * math.cos(ALPHA) + Vector((0, 0, math.sin(ALPHA))), r=0.015, mat=METAL)
    b = math.pi / 2
    panel(bm, b, 1.0, 2.5, -0.40, 0.40, 0.012, 0.005, DARK, eb=0.004)
    panel(bm, b, 2.15, 2.40, -0.25, 0.25, 0.022, 0.0, INNER, ms=DARK, eb=0.003)          # back data screen
    # diagonal facets : RCS thruster blocks (top) + leg brackets (bottom) + legs
    for a in (math.radians(45), math.radians(135), math.radians(225), math.radians(315)):
        panel(bm, a, 2.58, 2.86, -0.20, 0.20, 0.12, 0.005, DARK, eb=0.006)
        n = nrm(a)
        for su in (-1, 1):
            p = fpt(a, su * 0.09, 2.72, 0.12)
            oct_cyl(bm, 0.035, 0.0, 0.06, METAL, Matrix.Translation(p) @ n.to_track_quat('Z', 'Y').to_matrix().to_4x4(), ch=0.008)
        panel(bm, a, 0.78, 1.25, -0.17, 0.17, 0.10, 0.005, DARK, eb=0.006)
        p0 = n * (fo(1.0) + 0.06) + Vector((0, 0, 1.0))
        p1 = n * 2.55 + Vector((0, 0, 0.08))
        beam(bm, p0, p1, 0.13, 0.11, WHITE, 0.03)
        beam(bm, n * 1.72 + Vector((0, 0, 0.34)), n * 2.36 + Vector((0, 0, 0.12)), 0.07, 0.07, DARK, 0.015)
        oct_cyl(bm, 0.075, 0.0, 0.42, METAL, Matrix.Translation(p0.lerp(p1, 0.55)) @ (p1 - p0).to_track_quat('Z', 'Y').to_matrix().to_4x4(), ch=0.01)
        oct_cyl(bm, 0.28, -0.05, 0.08, DARK, Matrix.Translation((n.x * 2.55, n.y * 2.55, 0)), ch=0.03)
    # roof : graphite chamfer, white parachute housing, beacon, antenna
    add_revolve(bm, [(0, 3.29), (1.53, 3.29), (1.16, 3.50), (0, 3.50)], [DARK] * 3, 8, ROT8)
    oct_cyl(bm, 0.90, 3.48, 3.68, WHITE, ch=0.05)
    oct_cyl(bm, 0.55, 3.66, 3.73, DARK, ch=0.015)
    pill(bm, (0.12, 0.12, 0.07), (0, 0, 3.755), ORANGE)
    oct_cyl(bm, 0.022, 3.72, 4.15, METAL, Matrix.Translation((0.35, 0.22, 0)), ch=0.005)
    oct_cyl(bm, 0.04, 4.15, 4.19, DARK, Matrix.Translation((0.35, 0.22, 0)), ch=0.008)

def build_interior(bm):
    rf = ri(FL) - 0.001
    oct_cyl(bm, rf, Z0, FL, METAL, ch=0.0)                                       # floor slab (edge meets the wall)
    add_revolve(bm, [(rf - 0.16, FL + 0.002), (rf - 0.01, FL + 0.002), (rf - 0.01, FL + 0.008), (rf - 0.16, FL + 0.008)],
                [DARK] * 4, 8, ROT8, closed=True)                                # graphite floor border
    for k in range(7):                                                           # floor grating strips
        cbox(bm, (2.2, 0.05, 0.008), (0, -0.9 + k * 0.3, FL + 0.004), DARK, 0.003, 0.0)
    # ceiling slab, central hatch disc, perimeter neon ring
    oct_cyl(bm, ri(3.18), 3.18, Z1 - 0.005, WHITE, ch=0.0)
    oct_cyl(bm, 0.50, 3.165, 3.18, DARK, ch=0.01)
    oct_cyl(bm, 0.36, 3.157, 3.166, METAL, ch=0.004)
    add_revolve(bm, [(ri(3.05) - 0.05, 3.03), (ri(3.05) + 0.01, 3.03), (ri(3.05) + 0.01, 3.06), (ri(3.05) - 0.05, 3.06)],
                [NEON] * 4, 8, ROT8, closed=True)
    # graphite ribs at the inner corners
    for k in range(8):
        a = math.pi / 8 + k * math.pi / 4
        rd, tg = nrm(a), tan_(a)
        def q(z):
            c = rd * ri(z) + Vector((0, 0, z))
            return [c + rd * 0.02 - tg * 0.03, c + rd * 0.02 + tg * 0.03, c - rd * 0.04 + tg * 0.03, c - rd * 0.04 - tg * 0.03]
        hexa(bm, q(FL + 0.008), q(3.02), DARK, DARK, DARK)

def build_body():
    bm = bmesh.new()
    build_shell(bm); build_door_frames(bm); build_exterior(bm); build_interior(bm)
    return bm

# ------------------------------------------------------------------ door (world coords, origin moved to HINGE by finish2)
def build_door():
    bm = bmesh.new()
    u = W2 - 0.005
    panel(bm, FRONT, FL + 0.005, ZD, -u, u, -0.003, TW - 0.003, WHITE)
    # outside : graphite inset, metal strips, hazard handle
    panel(bm, FRONT, 0.95, 2.35, -0.40, 0.40, 0.006, 0.004, DARK, eb=0.004)
    for z in (1.25, 1.65, 2.05):
        panel(bm, FRONT, z, z + 0.03, -0.32, 0.32, 0.016, 0.0, METAL, ms=METAL, eb=0.003)
    panel(bm, FRONT, 2.42, 2.50, -0.13, 0.13, 0.03, -0.0, HAZARD, eb=0.006)
    # inside (= walking face once open) : treads, side rails, hazard band at the far end
    din = -(TW - 0.005)
    z = FL + 0.15
    while z < ZD - 0.25:
        panel(bm, FRONT, z, z + 0.04, -0.47, 0.47, din, TW + 0.005, METAL, ms=METAL, eb=0.003)
        z += 0.20
    panel(bm, FRONT, ZD - 0.13, ZD - 0.045, -0.47, 0.47, din, TW + 0.005, HAZARD, eb=0.003)
    for uL, uR in ((-0.545, -0.495), (0.495, 0.545)):                          # side rails
        panel(bm, FRONT, FL + 0.03, ZD - 0.06, uL, uR, din, TW + 0.022, DARK, eb=0.004)
    # knuckle + strap
    oct_cyl(bm, KR, -0.24, 0.24, DARK, Matrix.Translation(HINGE) @ Matrix.Rotation(math.pi / 2, 4, 'Y'), ch=0.008)
    cbox(bm, (0.42, 0.06, 0.03), (0, HINGE.y + 0.03, FL + 0.021), DARK, 0.008)
    return bm

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
    mats = make_mats("LandingCapsule")
    root = bpy.data.objects.new("LandingCapsule", None); col.objects.link(root); root.empty_display_size = 1
    finish2("Body", build_body(), col, root, mats)
    finish2("Door", build_door(), col, root, mats, origin=tuple(HINGE))
    bpy.context.view_layer.update()
    tri = lambda o: sum(len(p.vertices) - 2 for p in o.data.polygons)
    return {o.name: tri(o) for o in root.children_recursive if o.type == 'MESH'}

def open_angle():
    """rotation (X) that lays the door on the ground : lowest door vertex at z = 0.01."""
    door = bpy.data.objects["Door"]
    vs = [v.co.copy() for v in door.data.vertices]
    def low(t):
        R = Matrix.Rotation(t, 3, 'X')
        return min((R @ v).z for v in vs) + HINGE.z
    lo, hi = math.radians(90), math.radians(170)
    for _ in range(40):
        mid = (lo + hi) / 2
        if low(mid) > 0.01: lo = mid
        else: hi = mid
    return (lo + hi) / 2

# ------------------------------------------------------------------ animation
CLIPS = [("Door_Open", 1, 48), ("Door_Close", 60, 108)]
def key(ob, path, idx, f, v, interp='BEZIER'):
    getattr(ob, path)[idx] = v
    ob.keyframe_insert(path, index=idx, frame=f)
    ad = ob.animation_data
    fc = ad.action.layers[0].strips[0].channelbag(ad.action_slot).fcurves.find(path, index=idx)
    for kp in fc.keyframe_points:
        if abs(kp.co[0] - f) < 1e-4: kp.interpolation = interp

def animate():
    door = bpy.data.objects["Door"]
    if door.animation_data: door.animation_data_clear()
    door.rotation_euler = (0, 0, 0)
    op = open_angle()
    for f, v in ((1, 0.0), (48, op), (60, op), (108, 0.0)): key(door, "rotation_euler", 0, f, v)
    sc = bpy.context.scene; sc.frame_start, sc.frame_end = 1, 108; sc.render.fps = 24; sc.frame_set(1)
    return {"clips": CLIPS, "open_deg": round(math.degrees(op), 2)}

# ------------------------------------------------------------------ checks
def collisions(step=1):
    from mathutils.bvhtree import BVHTree
    O = bpy.data.objects; sc = bpy.context.scene; dg = bpy.context.evaluated_depsgraph_get()
    def tree(ob):
        M = ob.matrix_world
        return BVHTree.FromPolygons([M @ v.co for v in ob.data.vertices], [p.vertices[:] for p in ob.data.polygons])
    hits = []
    body = tree(O["Body"])
    for f in range(1, 109, step):
        sc.frame_set(f); dg.update()
        n = len(tree(O["Door"]).overlap(body))
        if n: hits.append((f, n))
    sc.frame_set(1)
    return hits

# Wall screen : smooth curved TV hung on the inner wall of the domes ~ 1.10 x 0.62 m, 8 cm deep.
# DA white housing / graphite bezel / glow screen / blue neon line / status light.
# Origin = wall contact point, at the vertical centre of the screen ; front (viewer side) = -Y, back against the wall at y = 0.
# The curve (radius 3.6 m at the back) is a bit tighter than the smallest dome wall (inner radius 3.75 m) and the TV
# stands on a 2.5 cm graphite wall bracket : its edges never cut the 8 / 12 / 16 m walls.
# WallScreen > Body (static : housing, bezel, neon line, status light, power button, wall bracket)
#            > Screen (glow, UV 0..1 on the front face : future UI texture ; lit / dimmed by script)
import bpy, bmesh, math
from mathutils import Matrix, Vector
SP = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival/ArtPipeline/Blender"
exec(open(SP + r"\al_build.py", encoding="utf8").read())

STANDOFF = 0.025                 # wall bracket thickness
R_BACK = 3.60                    # curve radius of the housing back
R0 = R_BACK + STANDOFF           # curve centre at (0, -R0, 0) ; rho = R0 is the wall plane at x = 0
HW, HH = 0.55, 0.31              # housing half width (arc length on the back) / half height
SEGS = 48                        # smooth curve

def rho(d): return R0 - d        # radius from the curve centre for a depth d in front of the wall

def sweep(bm, prof, half, mats, segs=SEGS, end_ch=0.0, rho_ref=None):
    """closed (rho, z) profile swept on an arc of half length `half` (measured at rho_ref) around the vertical
    axis at (0, -R0) ; the end caps are chamfered by end_ch. mats : one per profile edge (caps take mats[0])."""
    rho_ref = rho_ref or max(p[0] for p in prof)
    a = half / rho_ref
    tmp = bmesh.new()
    rings = []
    for i in range(segs + 1):
        t = -a + 2 * a * i / segs
        s, c = math.sin(t), math.cos(t)
        rings.append([tmp.verts.new((r * s, -R0 + r * c, z)) for r, z in prof])
    n = len(prof)
    for i in range(segs):
        A, B = rings[i], rings[i + 1]
        for k in range(n):
            l = (k + 1) % n
            tmp.faces.new((A[k], B[k], B[l], A[l])).material_index = mats[k] if isinstance(mats, (list, tuple)) else mats
    m0 = mats[0] if isinstance(mats, (list, tuple)) else mats
    tmp.faces.new(rings[0][::-1]).material_index = m0
    tmp.faces.new(rings[-1]).material_index = m0
    bmesh.ops.recalc_face_normals(tmp, faces=tmp.faces)
    if end_ch > 0:
        tmp.faces.ensure_lookup_table()
        cap = [e for f in list(tmp.faces)[-2:] for e in f.edges]
        _bevel_tmp(tmp, end_ch, cap)
    _copy_into(bm, tmp, None); tmp.free()

def rect(d0, d1, z0, z1, cb, cf):
    """(rho, z) chamfered rectangle between depths d0 (back) and d1 (front) : cb back chamfer, cf front chamfer."""
    rb, rf = rho(d0), rho(d1)
    return [(rf, z0 + cf), (rf + cf, z0), (rb - cb, z0), (rb, z0 + cb), (rb, z1 - cb), (rb - cb, z1), (rf + cf, z1), (rf, z1 - cf)]

def arc_pt(x, d, z):
    """point on the arc at depth d, at (approximately) lateral position x."""
    r = rho(d); t = x / r
    return Vector((r * math.sin(t), -R0 + r * math.cos(t), z)), t

# depths (m in front of the wall)
D_BACK, D_FRONT = STANDOFF, 0.068        # housing
D_BZ0, D_BZ1 = 0.064, 0.072              # bezel : one graphite front plate, 4 mm proud of the housing
D_SC0, D_SC1 = 0.070, 0.076              # screen slab, 4 mm proud of the bezel
SC_X, SC_Z0, SC_Z1 = 0.515, -0.240, 0.255   # screen half width, bottom, top (wider bezel at the bottom)

def build_body():
    bm = bmesh.new()
    # housing (white), big chamfer on the back, small on the front
    sweep(bm, rect(D_BACK, D_FRONT, -HH, HH, 0.025, 0.006), HW, WHITE, end_ch=0.025)
    # bezel (graphite) : a single front plate, the screen stands on it (no seam at the corners)
    sweep(bm, rect(D_BZ0, D_BZ1, -0.300, 0.300, 0.002, 0.004), 0.545, DARK, end_ch=0.004)
    # neon line on the bottom bezel
    sweep(bm, rect(D_BZ1 - 0.001, D_BZ1 + 0.0025, -0.278, -0.258, 0.001, 0.0015), 0.30, NEON, segs=16, end_ch=0.002)
    # status light (script driven) and power button, bottom right
    for x, m, size in ((0.47, STATUS, (0.035, 0.012, 0.012)), (0.41, WHITE, (0.03, 0.012, 0.018))):
        p, t = arc_pt(x, D_BZ1, -0.268)
        pill(bm, size, p, m, rot=Matrix.Rotation(-t, 3, 'Z'))
    # wall bracket (graphite), its back on the wall plane, 5 mm into the housing
    cbox(bm, (0.36, STANDOFF + 0.005, 0.26), (0, -(STANDOFF + 0.005) / 2, 0), DARK, 0.006, 0.002)
    return bm

def build_screen():
    bm = bmesh.new()
    sweep(bm, [(rho(D_SC1), SC_Z0), (rho(D_SC0), SC_Z0), (rho(D_SC0), SC_Z1), (rho(D_SC1), SC_Z1)], SC_X, INNER, rho_ref=rho(D_SC1))
    return bm

def screen_uv(ob):
    """UV 0..1 on the front face (the faces looking at the curve centre), from the arc angle and the height."""
    me = ob.data
    uv = me.uv_layers.new(name="UVMap")
    a = SC_X / rho(D_SC1)
    for p in me.polygons:
        for li in p.loop_indices:
            v = me.vertices[me.loops[li].vertex_index].co
            t = math.atan2(v.x, v.y + R0)
            uv.data[li].uv = ((t + a) / (2 * a), (v.z - SC_Z0) / (SC_Z1 - SC_Z0))

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
    mats = make_mats("WallScreen")
    root = bpy.data.objects.new("WallScreen", None); col.objects.link(root); root.empty_display_size = 0.3
    finish2("Body", build_body(), col, root, mats)
    scr = finish2("Screen", build_screen(), col, root, mats)
    scr.data.set_sharp_from_angle(angle=math.radians(35))
    screen_uv(scr)
    for o in (bpy.data.objects["Body"], scr): tri_ngons(o)
    bpy.context.view_layer.update()
    tri = lambda o: sum(len(p.vertices) - 2 for p in o.data.polygons)
    return {o.name: tri(o) for o in root.children_recursive if o.type == 'MESH'}

def wall_clearance():
    """smallest gap between the housing back edges and the inner wall of each dome (TV centred on the wall)."""
    out = {}
    for label, r_in in (("8m", 3.75), ("12m", 5.75), ("16m", 7.75)):
        # wall circle centre at (0, -r_in), on the viewer side : the wall passes through the origin
        worst = 1e9
        for v in bpy.data.objects["Body"].data.vertices:
            p = bpy.data.objects["Body"].matrix_world @ v.co
            gap = r_in - math.hypot(p.x, p.y + r_in)        # > 0 : inside the dome (in front of the wall)
            worst = min(worst, gap)
        out[label] = round(worst, 4)
    return out

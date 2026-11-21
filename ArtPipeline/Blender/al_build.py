# Airlock + DomeConnector (same DA as the craft station / dome)
# Convention: OUTWARD = Blender -Y  (-> Unity +Z forward after FBX axis conversion), z = 0 ground.
import bpy, bmesh, math
from mathutils import Matrix, Vector
SP = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival/ArtPipeline/Blender"
exec(open(SP + r"\cs_v5.py", encoding="utf8").read())     # cbox, pill, prism_x, oct_cyl, beam, tube + cs_helpers
STATUS = 8

MAT_DEF = [  # index order = WHITE, DARK, METAL, NEON, ORANGE, INNER(Glow), PLATE, HAZARD, STATUS
    ("White", (0.82, 0.85, 0.88), None), ("Dark", (0.035, 0.045, 0.06), None),
    ("Metal", (0.16, 0.18, 0.22), None), ("Neon", (0.55, 0.92, 1.0), (0.05, 0.65, 1.0)),
    ("Orange", (1.0, 0.38, 0.03), (1.0, 0.25, 0.0)), ("Glow", (0.1, 0.4, 0.85), (0.01, 0.2, 0.55)),
    ("Plate", (0.55, 0.65, 0.75), None), ("Hazard", (0.95, 0.6, 0.04), None),
    ("Status", (1.0, 0.45, 0.05), (1.0, 0.3, 0.0)),
]
def make_mats(prefix):
    out = []
    for n, col, em in MAT_DEF:
        name = f"M_{prefix}_{n}"
        m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
        b = next(x for x in m.node_tree.nodes if x.type == 'BSDF_PRINCIPLED')
        b.inputs["Base Color"].default_value = (*col, 1); b.inputs["Roughness"].default_value = 0.5
        b.inputs["Emission Strength"].default_value = 3.0 if em else 0.0
        if em: b.inputs["Emission Color"].default_value = (*em, 1)
        m.diffuse_color = (*col, 1)
        out.append(m)
    return out

# ------------------------------------------------------------------ STANDARD FLANGE TEMPLATE (m)
FL = 0.215                                   # walking floor (dome floor 0.20 + 1.5 cm threshold)
def prof6(w, z0, z1, c):
    """CCW (x, z) section: flat bottom, vertical sides, wide 45 deg top chamfers."""
    return [(-w, z0), (w, z0), (w, z1 - c), (w - c, z1), (-(w - c), z1), (-w, z1 - c)]
PASS  = prof6(0.90, FL, 2.60, 0.40)          # clear passage (connector, tunnels)
SHELL = prof6(1.30, 0.00, 2.95, 0.55)        # outer skin of the standard section
DOOR  = prof6(0.70, FL, 2.45, 0.30)          # airlock door opening
BODY  = prof6(1.55, 0.00, 3.25, 0.80)        # airlock caisson
CHAMB = prof6(1.20, FL, 2.85, 0.55)          # airlock chamber interior

def offset_poly(pts, d):
    P = [Vector(p) for p in pts]; n = len(P); out = []
    for i in range(n):
        a, b, c = P[i - 1], P[i], P[(i + 1) % n]
        e1, e2 = (b - a).normalized(), (c - b).normalized()
        n1, n2 = Vector((e1.y, -e1.x)), Vector((e2.y, -e2.x))
        out.append(tuple(b + (n1 + n2) * (d / (1 + n1.dot(n2)))))
    return out

def V(x, y, z): return Vector((x, y, z))

def hollow_y(bm, outer, inner, y0, y1, mo=WHITE, mi=WHITE, mc=DARK):
    """closed 'frame' solid: outer section -> inner section, extruded y0..y1. mo/mi may be per-edge lists."""
    n = len(outer)
    O0 = [bm.verts.new(V(x, y0, z)) for x, z in outer]; O1 = [bm.verts.new(V(x, y1, z)) for x, z in outer]
    I0 = [bm.verts.new(V(x, y0, z)) for x, z in inner]; I1 = [bm.verts.new(V(x, y1, z)) for x, z in inner]
    pick = lambda m, i: m[i] if isinstance(m, (list, tuple)) else m
    for i in range(n):
        j = (i + 1) % n
        bm.faces.new((O0[i], O0[j], O1[j], O1[i])).material_index = pick(mo, i)
        bm.faces.new((I0[j], I0[i], I1[i], I1[j])).material_index = pick(mi, i)
        bm.faces.new((O0[j], O0[i], I0[i], I0[j])).material_index = mc
        bm.faces.new((O1[i], O1[j], I1[j], I1[i])).material_index = mc

def prism_y(bm, poly, y0, y1, mat, end_ch=0.02, eb=EB, cap_mat=None):
    tmp = bmesh.new()
    a = [tmp.verts.new((x, y0, z)) for x, z in poly]; b = [tmp.verts.new((x, y1, z)) for x, z in poly]
    n = len(poly)
    for i in range(n):
        j = (i + 1) % n
        tmp.faces.new((a[i], a[j], b[j], b[i]))
    tmp.faces.new(list(reversed(a))); tmp.faces.new(b)
    bmesh.ops.recalc_face_normals(tmp, faces=tmp.faces)
    cap = [e for e in tmp.edges if abs(e.verts[0].co.y - e.verts[1].co.y) < 1e-6]
    _bevel_tmp(tmp, end_ch, cap); _bevel_tmp(tmp, eb)
    _copy_into(bm, tmp, mat); tmp.free()

def fix_normals(bm):
    """every closed island gets outward normals (signed volume test)."""
    bm.faces.ensure_lookup_table(); seen = set()
    for f0 in bm.faces:
        if f0.index in seen: continue
        st, isl = [f0], []; seen.add(f0.index)
        while st:
            f = st.pop(); isl.append(f)
            for e in f.edges:
                for g in e.link_faces:
                    if g.index not in seen: seen.add(g.index); st.append(g)
        bmesh.ops.recalc_face_normals(bm, faces=isl)
        vol = sum(f.verts[0].co.dot(f.verts[i].co.cross(f.verts[i + 1].co)) for f in isl for i in range(1, len(f.verts) - 1))
        if vol < -1e-9: bmesh.ops.reverse_faces(bm, faces=isl)

def finish2(name, bm, col, parent, mats, origin=(0, 0, 0)):
    fix_normals(bm)
    bmesh.ops.translate(bm, vec=-Vector(origin), verts=bm.verts)
    old = bpy.data.meshes.get(name)
    if old and old.users == 0: bpy.data.meshes.remove(old)
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me); bm.free()
    for m in mats: me.materials.append(m)
    me.shade_smooth(); me.set_sharp_from_angle(angle=math.radians(35))
    ob = bpy.data.objects.new(name, me); col.objects.link(ob)
    ob.parent = parent; ob.matrix_parent_inverse = Matrix(); ob.location = origin
    # drop unused material slots
    used = sorted({p.material_index for p in ob.data.polygons})
    remap = {old: new for new, old in enumerate(used)}
    keep = [ob.data.materials[i] for i in used]
    new_idx = [remap[p.material_index] for p in ob.data.polygons]
    ob.data.materials.clear()
    for m in keep: ob.data.materials.append(m)
    ob.data.polygons.foreach_set("material_index", new_idx)   # clear() resets indices -> re-apply after
    ob.data.update()
    return ob

def rotY(): return Matrix.Rotation(math.pi / 2, 3, 'X')   # helper: local Z -> -Y? (unused)

# ================================================================== DOME CONNECTOR
# origin = point on the dome's outer wall (radius 6.0) at ground level; dome centre at (0, +6, 0).
DOME_C_Y = 6.0
def dome_surface_y(x, z):
    """y of the dome's outer skin (wall panels / ring beam / geodesic nodes) seen along -Y at (x, z)."""
    if z < 0.95: Rs = 6.07
    elif z < 1.20: Rs = 6.16
    else: Rs = math.sqrt(max(6.03 ** 2 - (z - 1.20) ** 2, 0.01))
    return DOME_C_Y - math.sqrt(max(Rs * Rs - x * x, 0.01))

def densify(poly, step=0.07):
    out = []; n = len(poly)
    for i in range(n):
        a, b = Vector(poly[i]), Vector(poly[(i + 1) % n])
        k = max(1, math.ceil((b - a).length / step))
        out += [tuple(a.lerp(b, t / k)) for t in range(k)]
    return out

FLANGE_Y = -0.55          # connector mating plane (airlock / tunnel origin goes here)
def build_connector():
    bm = bmesh.new()
    y_in, y_out = 1.0, FLANGE_Y + 0.12
    # main shell : white skin, metal floor, white inner walls
    hollow_y(bm, SHELL, PASS, y_out, y_in, mo=[DARK, WHITE, METAL, WHITE, METAL, WHITE],
             mi=[METAL, WHITE, WHITE, WHITE, WHITE, WHITE], mc=DARK)
    # standard flange (graphite) + bolts
    FLG = offset_poly(SHELL, 0.15)
    hollow_y(bm, FLG, PASS, FLANGE_Y, y_out, mo=DARK, mi=DARK, mc=DARK)
    for (x, z) in densify(offset_poly(SHELL, 0.075), 0.55):
        if z < 0.1: continue
        add_screw(bm, (x, y_out, z), (0, 1, 0), r=0.025, mat=METAL)
    # mid rib with neon line
    hollow_y(bm, offset_poly(SHELL, 0.05), offset_poly(SHELL, -0.02), -0.30, -0.20, mo=DARK, mi=DARK, mc=DARK)
    hollow_y(bm, offset_poly(SHELL, 0.03), offset_poly(SHELL, -0.02), -0.20, -0.17, mo=NEON, mi=NEON, mc=NEON)
    # gasket collar hugging the dome skin (hides the cut made in the dome by Unity)
    cin, cout = densify(offset_poly(SHELL, -0.02)), None
    cout = []
    base = offset_poly(SHELL, 0.24)
    n = len(SHELL)
    cin = []
    for i in range(n):   # same subdivision count per edge on both loops
        a, b = Vector(SHELL[i]), Vector(SHELL[(i + 1) % n])
        A, B = Vector(base[i]), Vector(base[(i + 1) % n])
        ai = Vector(offset_poly(SHELL, -0.02)[i]); bi = Vector(offset_poly(SHELL, -0.02)[(i + 1) % n])
        k = max(1, math.ceil((B - A).length / 0.07))
        for t in range(k):
            cin.append(tuple(ai.lerp(bi, t / k))); cout.append(tuple(A.lerp(B, t / k)))
    m = len(cin)
    rows = []
    for (x, z), (X, Z) in zip(cin, cout):
        z, Z = max(z, -0.25), max(Z, -0.25)
        ys_i, ys_o = dome_surface_y(x, z), dome_surface_y(X, Z)
        ys = min(ys_i, ys_o)
        rows.append((bm.verts.new(V(x, ys - 0.12, z)), bm.verts.new(V(X, ys_o - 0.05, Z)),
                     bm.verts.new(V(X, ys_o + 0.10, Z)), bm.verts.new(V(x, max(ys_i, ys_o) + 0.10, z))))
    for i in range(m):
        a, b = rows[i], rows[(i + 1) % m]
        for k in range(4):
            l = (k + 1) % 4
            f = bm.faces.new((a[k], b[k], b[l], a[l]))
            f.material_index = [DARK, WHITE, DARK, DARK][k]
    # interior portal (inside the dome) : graphite frame + neon outline
    hollow_y(bm, offset_poly(PASS, 0.20), offset_poly(PASS, -0.005), y_in - 0.10, y_in + 0.04, mo=DARK, mi=DARK, mc=DARK)
    hollow_y(bm, offset_poly(PASS, 0.12), offset_poly(PASS, 0.08), y_in + 0.04, y_in + 0.055, mo=NEON, mi=NEON, mc=NEON)
    # floor grating strips + threshold plates
    for k in range(7):
        y = y_out + 0.12 + k * ((y_in - y_out - 0.3) / 6)
        cbox(bm, (1.6, 0.05, 0.012), (0, y, FL + 0.004), DARK, 0.004, 0.0)
    # small orange status lamps beside the inner portal
    for sx in (-1, 1):
        pill(bm, (0.05, 0.03, 0.05), (sx * 1.02, y_in + 0.045, 1.9), ORANGE)
    return bm

# ================================================================== AIRLOCK
# origin = mating face (meets the connector flange), chamber extends along -Y (outward).
Y_INW = (-0.12, -0.45)        # inner door wall
Y_OUTW = (-3.15, -3.48)       # outer door wall
L_END = -3.62
DOOR_T = 0.07                 # leaf thickness
LEAF_TRAVEL = 0.80

def door_leaf_poly(side):
    """left (side=-1) or right (+1) half of DOOR, overlapping 1 cm in the middle and 5 cm into the jambs."""
    w, c = 0.70, 0.30
    if side < 0:
        return [(-(w + 0.05), FL - 0.005), (0.01, FL - 0.005), (0.01, 2.45 + 0.04), (-(w - c) + 0.0, 2.45 + 0.04), (-(w + 0.05), 2.15 - 0.0)]
    return [(-0.01, FL - 0.005), (w + 0.05, FL - 0.005), (w + 0.05, 2.15), ((w - c), 2.45 + 0.04), (-0.01, 2.45 + 0.04)]

def build_leaf(side, yc, outer_face_y_sign):
    bm = bmesh.new()
    poly = door_leaf_poly(side)
    prism_y(bm, poly, yc - DOOR_T / 2, yc + DOOR_T / 2, WHITE, end_ch=0.015)
    # detail on both faces: graphite inset panel + neon edge light near the meeting edge
    for s in (-1, 1):
        y = yc + s * (DOOR_T / 2 + 0.006)
        cbox(bm, (0.42, 0.012, 1.25), (side * 0.36, y, 1.05), DARK, 0.02, 0.003)
        for k in range(5):
            cbox(bm, (0.32, 0.016, 0.02), (side * 0.36, y + s * 0.004, 0.7 + k * 0.16), METAL, 0.004, 0.0)
        pill(bm, (0.03, 0.014, 1.5), (side * 0.06, y, 1.2), NEON)
        cbox(bm, (0.2, 0.012, 0.06), (side * 0.36, y, 1.95), HAZARD, 0.01, 0.002)
    return bm

def build_airlock_body():
    bm = bmesh.new()
    y0, y1 = 0.0, L_END
    # mating flange (matches the connector flange)
    FLG = offset_poly(SHELL, 0.15)
    hollow_y(bm, FLG, PASS, 0.0, -0.12, mo=DARK, mi=DARK, mc=DARK)
    for (x, z) in densify(offset_poly(SHELL, 0.075), 0.55):
        if z < 0.1: continue
        add_screw(bm, (x, -0.12, z), (0, -1, 0), r=0.025, mat=METAL)
    # inner door wall (connects passage -> chamber)
    hollow_y(bm, BODY, DOOR, Y_INW[0], Y_INW[1], mo=[DARK, WHITE, METAL, WHITE, METAL, WHITE], mi=METAL, mc=WHITE)
    # chamber walls
    hollow_y(bm, BODY, CHAMB, Y_INW[1], Y_OUTW[0], mo=[DARK, WHITE, METAL, WHITE, METAL, WHITE],
             mi=[METAL, WHITE, WHITE, WHITE, WHITE, WHITE], mc=WHITE)
    # outer door wall
    hollow_y(bm, BODY, DOOR, Y_OUTW[0], Y_OUTW[1], mo=[DARK, WHITE, METAL, WHITE, METAL, WHITE], mi=METAL, mc=WHITE)
    # graphite end frames + mid rib (wide chamfered bands around the caisson)
    for ya, yb, d in ((-0.11, -0.36, 0.07), (-1.72, -1.90, 0.04), (-3.26, Y_OUTW[1] - 0.01, 0.07)):   # +1 cm past the walls : no shared faces
        hollow_y(bm, offset_poly(BODY, d), offset_poly(BODY, -0.02), ya, yb, mo=DARK, mi=DARK, mc=DARK)
    hollow_y(bm, offset_poly(BODY, 0.045), offset_poly(BODY, -0.02), -1.90, -1.925, mo=NEON, mi=NEON, mc=NEON)
    # outer door portal (protruding, chamfered) + hazard sill
    hollow_y(bm, offset_poly(DOOR, 0.32), offset_poly(DOOR, -0.005), Y_OUTW[1], L_END, mo=[DARK, DARK, METAL, DARK, METAL, DARK], mi=METAL, mc=DARK)
    hollow_y(bm, offset_poly(DOOR, 0.16), offset_poly(DOOR, 0.12), L_END, L_END - 0.015, mo=NEON, mi=NEON, mc=NEON)
    cbox(bm, (1.5, 0.30, 0.04), (0, L_END + 0.02, FL - 0.005), HAZARD, 0.01, 0.003)
    for k in range(6):
        cbox(bm, (0.08, 0.31, 0.045), (-0.62 + k * 0.25, L_END + 0.02, FL - 0.003), DARK, 0.0, 0.0,
             rot=Matrix.Rotation(math.radians(35), 3, 'Y'))
    # base skirt (buried 0.3 m) + feet
    cbox(bm, (3.3, 3.5, 0.45), (0, -1.81, -0.075), DARK, 0.06)
    # side details: graphite inset panels, vertical neon, vents ; right side O2 tank
    for sx in (-1, 1):
        x = sx * 1.555
        cbox(bm, (0.03, 1.1, 1.3), (x, -1.05, 1.25), DARK, 0.06, 0.004)
        cbox(bm, (0.03, 1.1, 1.3), (x, -2.58, 1.25), DARK, 0.06, 0.004)
        pill(bm, (0.03, 0.05, 0.9), (x + sx * 0.012, -1.32, 1.25), NEON)
        for k in range(7):
            cbox(bm, (0.025, 0.5, 0.018), (x + sx * 0.012, -2.5, 0.86 + k * 0.11), METAL, 0.004, 0.0)
        for yy in (-0.75, -2.85):
            for zz in (0.45, 2.05):
                add_screw(bm, (x + sx * 0.015, yy, zz), (sx, 0, 0), r=0.02, mat=METAL)
    T = Matrix.Translation((1.83, -1.81, 0.55)) @ Matrix.Rotation(math.pi / 2, 4, 'X')
    oct_cyl(bm, 0.2, -0.85, 0.85, WHITE, T, ch=0.06)
    for zz in (-0.55, 0.55):
        oct_cyl(bm, 0.215, zz - 0.04, zz + 0.04, DARK, T, ch=0.01)
    for yy in (-1.2, -2.4):
        cbox(bm, (0.32, 0.08, 0.5), (1.68, yy, 0.42), DARK, 0.03)            # tank saddles
    tube(bm, [(1.83, -0.95, 0.55), (1.83, -0.75, 0.55), (1.6, -0.75, 0.9)], 0.035, METAL, DARK)
    # roof : air recycler cap (static housing ; the turbine is Fan_Roof)
    M = Matrix.Translation((0, -1.81, 3.25))
    oct_cyl(bm, 0.62, -0.05, 0.12, DARK, M, ch=0.04, sides=12)
    oct_cyl(bm, 0.50, 0.12, 0.16, METAL, M, ch=0.01, sides=12)
    for k in range(6):   # posts around the turbine
        a = k * math.pi / 3 + math.pi / 6
        beam(bm, M @ Vector((0.55 * math.cos(a), 0.55 * math.sin(a), 0.14)), M @ Vector((0.55 * math.cos(a), 0.55 * math.sin(a), 0.44)), 0.06, 0.06, DARK, 0.012)
    oct_cyl(bm, 0.58, 0.42, 0.50, DARK, M, ch=0.02, sides=12)
    oct_cyl(bm, 0.18, 0.50, 0.56, WHITE, M, ch=0.02, sides=12)
    oct_cyl(bm, 0.05, 0.56, 0.64, ORANGE, M, ch=0.01)                       # beacon
    # exterior control panel (right of the outer door) + status lamp above the door
    cbox(bm, (0.30, 0.06, 0.42), (1.27, L_END + 0.11, 1.3), DARK, 0.04, 0.004)
    cbox(bm, (0.2, 0.012, 0.1), (1.27, L_END + 0.075, 1.42), INNER, 0.01, 0.002)
    for r_ in range(2):
        for c_ in range(3):
            pill(bm, (0.045, 0.02, 0.035), (1.21 + c_ * 0.06, L_END + 0.075, 1.2 + r_ * 0.07), ORANGE if (r_, c_) == (0, 2) else WHITE)
    cbox(bm, (0.5, 0.05, 0.12), (0, Y_OUTW[1] - 0.02, 2.93), DARK, 0.03, 0.004)
    pill(bm, (0.40, 0.03, 0.06), (0, Y_OUTW[1] - 0.045, 2.93), STATUS)
    # interior : grated floor, ceiling neon, status lamps over each door, side air jets, inner panel
    for k in range(12):
        cbox(bm, (2.1, 0.06, 0.012), (0, -0.62 - k * 0.205, FL + 0.004), DARK, 0.004, 0.0)
    for sx in (-1, 1):
        pill(bm, (0.035, 2.4, 0.035), (sx * 0.915, -1.81, 2.565), NEON)
        for yy in (-1.0, -1.8, -2.6):
            cbox(bm, (0.04, 0.22, 0.16), (sx * 1.19, yy, 1.6), DARK, 0.03)
            oct_cyl(bm, 0.035, 0.0, 0.03, METAL, Matrix.Translation((sx * 1.17, yy, 1.6)) @ Matrix.Rotation(sx * -math.pi / 2, 4, 'Y'), ch=0.005)
    for y in (Y_INW[1] - 0.012, Y_OUTW[0] + 0.012):
        pill(bm, (0.3, 0.025, 0.06), (0, y, 2.62), STATUS)
    cbox(bm, (0.28, 0.05, 0.40), (0.95, Y_OUTW[0] + 0.03, 1.3), DARK, 0.04, 0.004)
    cbox(bm, (0.18, 0.012, 0.1), (0.95, Y_OUTW[0] + 0.058, 1.42), INNER, 0.01, 0.002)
    pill(bm, (0.06, 0.02, 0.06), (0.95, Y_OUTW[0] + 0.06, 1.22), ORANGE)
    # ceiling fan housing (the rotor is Fan_Inner)
    M2 = Matrix.Translation((0, -1.81, 2.85))
    add_revolve(bm, [(0.42, 0.0), (0.42, -0.08), (0.36, -0.10), (0.34, -0.10), (0.34, 0.0)], [DARK] * 5, 16, M2, closed=True)
    return bm

def build_fan(r, blades, z_dir=1):
    bm = bmesh.new()
    oct_cyl(bm, r * 0.22, -0.03, 0.03, METAL, ch=0.01)
    for k in range(blades):
        a = 2 * math.pi * k / blades
        rot = Matrix.Rotation(a, 3, 'Z') @ Matrix.Rotation(math.radians(25), 3, 'Y')
        cbox(bm, (r * 0.75, 0.10 * r / 0.33, 0.012), Matrix.Rotation(a, 3, 'Z') @ Vector((r * 0.55, 0, 0)), DARK, 0.0, 0.003, rot=rot)
    return bm

# ================================================================== SETUP
def setup():
    for ob in list(bpy.data.objects):
        if not ob.name.startswith("_"): bpy.data.objects.remove(ob)
    for a in list(bpy.data.actions): bpy.data.actions.remove(a)
    sc = bpy.context.scene
    col = bpy.data.collections.get("Models") or bpy.data.collections.new("Models")
    if col.name not in sc.collection.children: sc.collection.children.link(col)
    # ---- connector
    cm = make_mats("DomeConnector")
    croot = bpy.data.objects.new("DomeConnector", None); col.objects.link(croot); croot.empty_display_size = 1
    finish2("DomeConnector_Body", build_connector(), col, croot, cm)
    sock = bpy.data.objects.new("Socket", None); col.objects.link(sock); sock.parent = croot
    sock.location = (0, FLANGE_Y, 0); sock.empty_display_type = 'ARROWS'; sock.empty_display_size = 0.5
    # ---- airlock (placed on the connector socket for preview)
    am = make_mats("Airlock")
    aroot = bpy.data.objects.new("Airlock", None); col.objects.link(aroot); aroot.empty_display_size = 1
    aroot.location = (0, FLANGE_Y, 0)
    finish2("Airlock_Body", build_airlock_body(), col, aroot, am)
    for wall, tag in ((Y_INW, "Inner"), (Y_OUTW, "Outer")):
        yc = (wall[0] + wall[1]) / 2
        for side, s in ((-1, "L"), (1, "R")):
            finish2(f"Door_{tag}_{s}", build_leaf(side, yc, 1), col, aroot, am)
    fi = finish2("Fan_Inner", build_fan(0.33, 6), col, aroot, am, origin=(0, 0, 0))
    fi.location = (0, -1.81, 2.80)
    fr = finish2("Fan_Roof", build_fan(0.45, 8), col, aroot, am)
    fr.location = (0, -1.81, 3.25 + 0.30)
    bpy.context.view_layer.update()
    tri = lambda o: sum(len(p.vertices) - 2 for p in o.data.polygons)
    return {"connector_tris": tri(bpy.data.objects["DomeConnector_Body"]),
            "airlock_tris": sum(tri(o) for o in aroot.children if o.type == 'MESH')}

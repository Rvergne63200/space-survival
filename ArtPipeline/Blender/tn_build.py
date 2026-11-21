# Tunnel kit (glass roof) : Straight 2 m, Extension (stretchable 1 m), Elbow 45 / 90, T junction hub.
# Same standard flange as DomeConnector / Airlock. OUTWARD / travel = Blender -Y (-> Unity +Z), z = 0 ground.
import bpy, bmesh, math
from mathutils import Matrix, Vector
SP = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival/ArtPipeline/Blender"
exec(open(SP + r"\al_build.py", encoding="utf8").read())      # SHELL, PASS, FL, hollow_y, offset_poly, finish2, make_mats...
GLASS = 9
Z = Vector((0, 0, 1))

def tunnel_mats():
    ms = make_mats("Tunnel")
    g = bpy.data.materials.get("M_Tunnel_Glass") or bpy.data.materials.new("M_Tunnel_Glass")
    b = next(x for x in g.node_tree.nodes if x.type == 'BSDF_PRINCIPLED')
    b.inputs["Base Color"].default_value = (0.25, 0.6, 0.8, 1); b.inputs["Roughness"].default_value = 0.08
    b.inputs["Alpha"].default_value = 0.35; g.surface_render_method = 'BLENDED'; g.use_backface_culling = False
    g.diffuse_color = (0.25, 0.6, 0.8, 0.35)
    return ms + [g]

# ------------------------------------------------------------------ path (lines + arcs around Z)
def f_dir(h): return Vector((math.sin(h), -math.cos(h), 0))
def x_dir(h): return Vector((math.cos(h), math.sin(h), 0))

def path_frame(segs, s, P0=Vector((0, 0, 0)), h0=0.0):
    P, h = P0.copy(), h0
    for i, seg in enumerate(segs):
        last = i == len(segs) - 1
        if seg[0] == 'L':
            L = seg[1]
            if s <= L + 1e-9 or last: return P + f_dir(h) * s, h
            P = P + f_dir(h) * L; s -= L
        else:
            R, th = seg[1], seg[2]; sg = 1 if th > 0 else -1; arc = R * abs(th)
            C = P + x_dir(h) * R * sg
            if s <= arc + 1e-9 or last:
                t = sg * s / R
                return C - x_dir(h + t) * R * sg, h + t
            P = C - x_dir(h + th) * R * sg; h += th; s -= arc
    return P, h

def seg_spans(segs):
    out, s = [], 0.0
    for seg in segs:
        L = seg[1] if seg[0] == 'L' else seg[1] * abs(seg[2])
        out.append((s, s + L, seg[0] == 'A')); s += L
    return out, s

def stations(segs, s0, s1, curved, step=math.radians(7.5), P0=Vector((0, 0, 0)), h0=0.0):
    if curved:
        h_a, h_b = path_frame(segs, s0, P0, h0)[1], path_frame(segs, s1, P0, h0)[1]
        n = max(1, math.ceil(abs(h_b - h_a) / step))
    else:
        n = 1
    return [path_frame(segs, s0 + (s1 - s0) * k / n, P0, h0) for k in range(n + 1)]

def W(st, x, z): P, h = st; return P + x_dir(h) * x + Z * z

def sweep(bm, poly, sts, mats):
    rings = [[bm.verts.new(W(st, x, z)) for x, z in poly] for st in sts]
    n = len(poly)
    pick = lambda i: mats[i] if isinstance(mats, (list, tuple)) else mats
    for a, b in zip(rings[:-1], rings[1:]):
        for i in range(n):
            j = (i + 1) % n
            bm.faces.new((a[i], a[j], b[j], b[i])).material_index = pick(i)
    bm.faces.new(rings[0]).material_index = pick(0)
    bm.faces.new(rings[-1][::-1]).material_index = pick(0)

def hollow_sweep(bm, outer, inner, sts, mo=DARK, mi=DARK, mc=DARK):
    n = len(outer)
    O = [[bm.verts.new(W(st, x, z)) for x, z in outer] for st in sts]
    I = [[bm.verts.new(W(st, x, z)) for x, z in inner] for st in sts]
    pick = lambda m, i: m[i] if isinstance(m, (list, tuple)) else m
    for k in range(len(sts) - 1):
        for i in range(n):
            j = (i + 1) % n
            bm.faces.new((O[k][i], O[k][j], O[k + 1][j], O[k + 1][i])).material_index = pick(mo, i)
            bm.faces.new((I[k][j], I[k][i], I[k + 1][i], I[k + 1][j])).material_index = pick(mi, i)
    for k, rev in ((0, False), (-1, True)):
        for i in range(n):
            j = (i + 1) % n
            q = (O[k][j], O[k][i], I[k][i], I[k][j])
            bm.faces.new(q[::-1] if rev else q).material_index = mc

def mirror(poly): return [(-x, z) for x, z in reversed(poly)]

def rail(cx, cz, s=0.075, c=0.025):
    return [(cx - s + c, cz - s), (cx + s - c, cz - s), (cx + s, cz - s + c), (cx + s, cz + s - c),
            (cx + s - c, cz + s), (cx - s + c, cz + s), (cx - s, cz + s - c), (cx - s, cz - s + c)]
RAIL_M = [DARK, METAL, WHITE, METAL, WHITE, METAL, WHITE, METAL]

# ------------------------------------------------------------------ tunnel cross-section (constant along the path)
A_R, B_R = (1.24, 2.40), (0.74, 2.90)          # roof facet corners (right side)
WALL_TOP = 1.20                                 # solid wall height = dome wall + ring beam
FT = 0.12                                       # flange thickness
def pane(a, b, t=0.018):
    a, b = Vector(a), Vector(b); d = (b - a).normalized(); n = Vector((d.y, -d.x))   # outward normal (CCW side)
    if n.y < 0: n = -n
    return [tuple(a), tuple(b), tuple(b - n * t), tuple(a - n * t)]

def section():
    wallR = [(0.9, FL), (1.3, FL), (1.3, 2.35), (0.9, 2.35)]
    """(poly, mats, side) ; side 'R' = right-hand wall elements (removed where a T branch opens)."""
    E = [
        ([(-1.3, 0.0), (1.3, 0.0), (1.3, FL), (-1.3, FL)], [DARK, DARK, METAL, DARK], 'C'),       # floor slab
        ([(-1.37, -0.3), (1.37, -0.3), (1.37, 0.1), (1.34, 0.14), (-1.34, 0.14), (-1.37, 0.1)], DARK, 'C'),   # buried skirt
        (pane(B_R, (-B_R[0], B_R[1])), GLASS, 'C'),                                                 # top glazing
    ]
    for sx in (1, -1):
        side = 'R' if sx > 0 else 'L'
        M = (lambda p: p) if sx > 0 else mirror
        wall = [(0.9, FL), (1.3, FL), (1.3, WALL_TOP), (0.9, WALL_TOP)]             # solid wall = dome wall height
        sill = [(0.86, WALL_TOP - 0.06), (1.33, WALL_TOP - 0.06), (1.35, WALL_TOP - 0.04), (1.35, WALL_TOP + 0.02),
                (1.33, WALL_TOP + 0.04), (0.86, WALL_TOP + 0.04)]          # 1.35 < rib skin 1.36 : no coplanar faces
        sill_neon = [(0.90, WALL_TOP + 0.04), (1.04, WALL_TOP + 0.04), (1.04, WALL_TOP + 0.052), (0.90, WALL_TOP + 0.052)]
        neon = [(1.29, 0.86), (1.325, 0.875), (1.325, 0.925), (1.29, 0.94)]
        band = [(1.29, 0.28), (1.33, 0.32), (1.33, 0.50), (1.29, 0.54)]
        strip = [(0.44, FL - 0.002), (0.56, FL - 0.002), (0.56, FL + 0.006), (0.44, FL + 0.006)]
        E += [
            (M(wall), [DARK, WHITE, DARK, WHITE] if sx > 0 else [WHITE, DARK, WHITE, DARK], side),
            (M(sill), DARK, side), (M(sill_neon), NEON, side), (M(neon), NEON, side), (M(band), DARK, side),
            (M(strip), DARK, 'C'),
            (M(pane((A_R[0], WALL_TOP + 0.04), A_R)), GLASS, side),                      # vertical glass wall
            (M(pane(A_R, B_R)), GLASS, side),                                            # roof chamfer glazing
            (M(rail(*A_R)), RAIL_M, side), (M(rail(*B_R)), RAIL_M, side),
        ]
    return E

def add_body(bm, segs, s0, s1, P0=Vector((0, 0, 0)), h0=0.0, sides=('C', 'L', 'R')):
    spans, _ = seg_spans(segs)
    for a, b, curved in spans:
        a2, b2 = max(a, s0), min(b, s1)
        if b2 - a2 < 1e-6: continue
        sts = stations(segs, a2, b2, curved, P0=P0, h0=h0)
        for poly, m, side in section():
            if side in sides: sweep(bm, poly, sts, m)

def add_flange(bm, segs, s_face, s_back, P0=Vector((0, 0, 0)), h0=0.0):
    """flange solid between s_face (mating plane) and s_back ; bolts on the back face."""
    lo, hi = min(s_face, s_back), max(s_face, s_back)
    hollow_sweep(bm, offset_poly(SHELL, 0.15), PASS, [path_frame(segs, lo, P0, h0), path_frame(segs, hi, P0, h0)])
    st = path_frame(segs, s_back, P0, h0)
    nrm = f_dir(st[1]) * (1 if s_back > s_face else -1)
    for (x, z) in densify(offset_poly(SHELL, 0.075), 0.55):
        if z < 0.1: continue
        add_screw(bm, W(st, x, z), nrm, r=0.025, mat=METAL)

def add_rib(bm, segs, s, w=0.12, P0=Vector((0, 0, 0)), h0=0.0):
    hollow_sweep(bm, offset_poly(SHELL, 0.06), offset_poly(SHELL, -0.10),
                 [path_frame(segs, s - w / 2, P0, h0), path_frame(segs, s + w / 2, P0, h0)], mo=DARK, mi=DARK, mc=DARK)

def side_panels(bm, segs, s):
    """graphite vent panel on both outer walls at path position s (straight parts only)."""
    st = path_frame(segs, s); P, h = st
    rot = Matrix((x_dir(h), -f_dir(h), Z)).transposed()
    for sx in (1, -1):
        c = W(st, sx * 1.305, 0.70)
        cbox(bm, (0.03, 0.62, 0.22), c, DARK, 0.04, 0.004, rot=rot)
        for k in range(3):
            cbox(bm, (0.025, 0.46, 0.018), W(st, sx * 1.318, 0.63 + k * 0.07), METAL, 0.004, 0.0, rot=rot)
        pill(bm, (0.03, 0.04, 0.04), W(st, sx * 1.32, 0.70) + f_dir(h) * 0.25, ORANGE)

# ------------------------------------------------------------------ pieces
def piece_straight(L=2.0):
    segs = [('L', L)]; bm = bmesh.new()
    add_body(bm, segs, FT, L - FT)      # body stops at the flanges (no coplanar overlap)
    add_flange(bm, segs, 0, 0.12); add_flange(bm, segs, L, L - 0.12)
    add_rib(bm, segs, L / 2)
    side_panels(bm, segs, L * 0.27); side_panels(bm, segs, L * 0.73)
    return bm, [("Socket", path_frame(segs, L))]

def piece_extension(L=1.0):
    segs = [('L', L)]; bm = bmesh.new()
    add_body(bm, segs, 0, L)          # constant section only : can be scaled along its length
    return bm, [("Socket", path_frame(segs, L))]

def piece_elbow(deg, R=2.5, lead=0.35):
    th = math.radians(deg)
    segs = [('L', lead), ('A', R, th), ('L', lead)]
    _, L = seg_spans(segs); bm = bmesh.new()
    add_body(bm, segs, FT, L - FT)
    add_flange(bm, segs, 0, 0.12); add_flange(bm, segs, L, L - 0.12)
    arc0, arc1 = lead, L - lead
    nrib = 1 if deg <= 45 else 2
    for k in range(nrib):
        add_rib(bm, segs, arc0 + (arc1 - arc0) * (k + 1) / (nrib + 1))
    return bm, [("Socket", path_frame(segs, L))]

# ---- T junction : straight run (4 m) + side branch on +X, glazed groin vault where the two roofs meet
ARM, SQ = 0.7, 1.3                     # arm length beyond the 2.6 m centre square, half square
T_LEN = 2 * ARM + 2 * SQ               # 4.0
T_C = Vector((0, -(ARM + SQ), 0))      # centre of the junction

def glass_poly(bm, pts, nrm, t=0.018):
    nrm = Vector(nrm).normalized()
    a = [bm.verts.new(Vector(p)) for p in pts]; b = [bm.verts.new(Vector(p) - nrm * t) for p in pts]
    n = len(pts)
    bm.faces.new(a).material_index = GLASS; bm.faces.new(b[::-1]).material_index = GLASS
    for i in range(n):
        j = (i + 1) % n
        bm.faces.new((a[j], a[i], b[i], b[j])).material_index = GLASS

def piece_junction(branches=(1,)):
    """branches: 1 = right (+X), -1 = left (-X). (1,) -> T, (1, -1) -> cross (+)."""
    bm = bmesh.new()
    segs = [('L', T_LEN)]
    s_in, s_out = ARM, ARM + 2 * SQ
    open_sides = {('R' if b > 0 else 'L') for b in branches}
    closed = tuple(s for s in ('L', 'R') if s not in open_sides)
    # main run : walls removed over the centre square on the branch side(s)
    add_body(bm, segs, FT, T_LEN - FT, sides=('C',) + closed)
    for s in open_sides:
        add_body(bm, segs, FT, s_in, sides=(s,)); add_body(bm, segs, s_out, T_LEN - FT, sides=(s,))
    add_flange(bm, segs, 0, FT); add_flange(bm, segs, T_LEN, T_LEN - FT)
    add_rib(bm, segs, s_in); add_rib(bm, segs, s_out)
    st = path_frame(segs, T_LEN / 2); rot = Matrix((x_dir(0), -f_dir(0), Z)).transposed()
    for s in closed:   # vent panel on a closed side
        sx = -1 if s == 'L' else 1
        cbox(bm, (0.03, 1.2, 0.22), W(st, sx * 1.305, 0.70), DARK, 0.04, 0.004, rot=rot)
        for k in range(3):
            cbox(bm, (0.025, 1.0, 0.018), W(st, sx * 1.318, 0.63 + k * 0.07), METAL, 0.004, 0.0, rot=rot)
    cx, cy = T_C.x, T_C.y
    zt, xb, xa, za = B_R[1], B_R[0], A_R[0], A_R[1]           # 2.90, 0.74, 1.24, 2.40
    socks = [("Socket", path_frame(segs, T_LEN))]
    for su in branches:
        # branch arm : built from its flange, heading back into the junction
        P0, h0 = T_C + Vector((su * (SQ + ARM), 0, 0)), -su * math.pi / 2
        bsegs = [('L', ARM)]
        add_body(bm, bsegs, FT, ARM, P0, h0)
        add_flange(bm, bsegs, 0, FT, P0, h0)
        add_rib(bm, bsegs, ARM - 0.075, P0=P0, h0=h0)     # sits just outside the main rib (no shared face)
        name = "Socket_Side" if len(branches) == 1 else ("Socket_Right" if su > 0 else "Socket_Left")
        socks.append((name, (P0, su * math.pi / 2)))
        # groin vault glazing (roof = the higher of the two tunnel roofs)
        P = lambda u, v, z, su=su: Vector((cx + su * u, cy + v, z))
        glass_poly(bm, [P(xb, -xb, zt), P(SQ, -xb, zt), P(SQ, xb, zt), P(xb, xb, zt)], (0, 0, 1))
        for sv in (1, -1):
            main = [P(xb, sv * xb, zt), P(xa, sv * xa, za), P(xa, sv * SQ, za), P(xb, sv * SQ, zt)]
            br = [P(xb, sv * xb, zt), P(SQ, sv * xb, zt), P(SQ, sv * xa, za), P(xa, sv * xa, za)]
            glass_poly(bm, main, (su, 0, 1))
            glass_poly(bm, br, (0, sv, 1))
            beam(bm, P(xb, sv * xb, zt), P(SQ - 0.02, sv * (SQ - 0.02), za - (SQ - 0.02 - xa)), 0.15, 0.13, WHITE, 0.03)
            beam(bm, P(xb, sv * xb, zt), P(xb, sv * SQ, zt), 0.15, 0.15, WHITE, 0.03)
            beam(bm, P(xb, sv * xb, zt), P(SQ, sv * xb, zt), 0.15, 0.15, WHITE, 0.03)
            oct_cyl(bm, 0.11, zt - 0.09, zt + 0.10, DARK, Matrix.Translation((cx + su * xb, cy + sv * xb, 0)), ch=0.03)
            pill(bm, (0.05, 0.05, 0.05), P(SQ - 0.12, sv * (SQ - 0.12), 1.95), ORANGE)
        for k in range(3):   # hazard stripes across the branch mouth
            cbox(bm, (0.06, 1.5, 0.006), P(0.98 + k * 0.12, 0, FL + 0.003), HAZARD, 0.0, 0.0)
    if len(branches) == 2:   # cross : centre floor medallion + top cap where the four roofs meet
        oct_cyl(bm, 0.45, FL - 0.002, FL + 0.004, DARK, Matrix.Translation((cx, cy, 0)), ch=0.0, sides=16)
        oct_cyl(bm, 0.38, FL, FL + 0.0075, NEON, Matrix.Translation((cx, cy, 0)), ch=0.0, sides=16)
        oct_cyl(bm, 0.30, FL, FL + 0.0095, METAL, Matrix.Translation((cx, cy, 0)), ch=0.0, sides=16)
    return bm, socks

def piece_tee(): return piece_junction((1,))
def piece_cross(): return piece_junction((1, -1))

# ------------------------------------------------------------------ setup
PIECES = [("Tunnel_Straight", piece_straight), ("Tunnel_Extension", piece_extension),
          ("Tunnel_Elbow45", lambda: piece_elbow(45)), ("Tunnel_Elbow90", lambda: piece_elbow(90)),
          ("Tunnel_T", piece_tee), ("Tunnel_Cross", piece_cross)]
def setup():
    for ob in list(bpy.data.objects):
        if not ob.name.startswith("_"): bpy.data.objects.remove(ob)
    for me in list(bpy.data.meshes):
        if me.users == 0: bpy.data.meshes.remove(me)
    sc = bpy.context.scene
    col = bpy.data.collections.get("Models") or bpy.data.collections.new("Models")
    if col.name not in sc.collection.children: sc.collection.children.link(col)
    mats = tunnel_mats()
    out = {}
    for i, (name, fn) in enumerate(PIECES):
        root = bpy.data.objects.new(name, None); col.objects.link(root); root.empty_display_size = 1
        root.location = (i * 7.0, 0, 0)                     # preview layout (reset to 0 for export)
        bm, socks = fn()
        body = finish2(name + "_Body", bm, col, root, mats)
        for sname, (P, h) in socks:
            s = bpy.data.objects.new(sname, None); col.objects.link(s); s.parent = root
            s.location = P; s.rotation_euler = (0, 0, h)
            s.empty_display_type = 'ARROWS'; s.empty_display_size = 0.6
        out[name] = sum(len(p.vertices) - 2 for p in body.data.polygons)
    return out

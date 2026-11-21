# Habitat dome (12 m) - same DA as the craft station (white / graphite / blue neon / orange lights)
import bpy, bmesh, math
from mathutils import Matrix, Vector
SP = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival/ArtPipeline/Blender"
exec(open(SP + r"\cs_helpers.py", encoding="utf8").read())

WHITE, DARK, METAL, NEON, ORANGE, GLASS = range(6)
MAT_DEF = [
    ("M_Dome_White",  (0.82, 0.85, 0.88), None),
    ("M_Dome_Dark",   (0.035, 0.045, 0.06), None),
    ("M_Dome_Metal",  (0.16, 0.18, 0.22), None),
    ("M_Dome_Neon",   (0.55, 0.92, 1.0), (0.05, 0.65, 1.0)),
    ("M_Dome_Orange", (1.0, 0.38, 0.03), (1.0, 0.25, 0.0)),
    ("M_Dome_Glass",  (0.25, 0.6, 0.8), None),
]

# ---- dimensions (m) ; z = 0 is the ground, floor at FLOOR
R_OUT, R_IN = 6.0, 5.75          # plain wall (cut by Unity for connections)
FLOOR = 0.20
WALL_Z0, WALL_Z1 = 0.15, 0.95    # low wall (about half the player's height)
RING_Z1 = 1.20                   # top of the ring beam = dome equator (1.20 m max)
N_PANELS = 24                    # wall cladding panels (15 deg each)
R_DOME = 5.90
SEG = 128                        # wall / ring smoothness

def make_mats():
    out = []
    for name, col, em in MAT_DEF:
        m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
        b = next(n for n in m.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
        b.inputs["Base Color"].default_value = (*col, 1)
        b.inputs["Roughness"].default_value = 0.08 if name.endswith("Glass") else 0.5
        if em:
            b.inputs["Emission Color"].default_value = (*em, 1)
            b.inputs["Emission Strength"].default_value = 3.0
        if name.endswith("Glass"):
            b.inputs["Alpha"].default_value = 0.35
            m.surface_render_method = 'BLENDED'
            m.use_backface_culling = False
        m.diffuse_color = (*col, 0.35 if name.endswith("Glass") else 1)
        out.append(m)
    return out

def revolve_profile(bm, pts, segs=SEG):
    """pts: list of (r, z, mat) ; mat = material of the segment starting at that point; closed loop."""
    add_revolve(bm, [(r, z) for r, z, _ in pts], [m for _, _, m in pts], segs, closed=True)

# ------------------------------------------------------------------ base + floor
def build_base():
    bm = bmesh.new()
    # foundation: buried 0.5 m for uneven terrain, small lip outside the wall
    revolve_profile(bm, [
        (R_IN - 0.05, -0.5, DARK), (R_OUT + 0.14, -0.5, DARK), (R_OUT + 0.14, 0.07, DARK), (R_OUT + 0.08, WALL_Z0, DARK),
        (R_IN - 0.05, WALL_Z0, DARK)])
    # floor slab (graphite) with concentric grooves, neon ring and centre hatch
    # grooves (r0, r1, mat) cut 12 mm into the slab
    grooves = [(1.05, 1.15, DARK), (2.60 * R_OUT / 6.0, 2.60 * R_OUT / 6.0 + 0.06, DARK), (R_IN - 1.20, R_IN - 1.05, NEON)]
    pts, mats = [(0.0, FLOOR)], []
    for r0, r1, m in grooves:
        pts += [(r0, FLOOR), (r0, FLOOR - 0.012), (r1, FLOOR - 0.012), (r1, FLOOR)]
        mats += [METAL, m, m, m]
    pts += [(R_IN, FLOOR), (R_IN, FLOOR - 0.06), (0.0, FLOOR - 0.06)]
    mats += [METAL, METAL, METAL, METAL]
    add_revolve(bm, pts, mats, SEG)
    # centre hatch bolts
    for i in range(8):
        a = 2*math.pi*i/8 + math.pi/8
        add_screw(bm, (0.9*math.cos(a), 0.9*math.sin(a), FLOOR), (0, 0, 1), r=0.03, mat=DARK)
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-5)
    return bm

# ------------------------------------------------------------------ plain wall
def curved_slab(bm, a0, a1, z0, z1, r0, r1, ch, face_mat=WHITE, ch_mat=METAL, segs=4):
    """panel following the wall curvature, chamfered on its 4 outer edges (back face open, against the core)."""
    def prof(inset):
        rr = r1 - inset
        return [(r0, z0 + 0.0), (rr - ch + inset, z0 + inset), (rr, z0 + ch + inset * 0.0 + inset),
                (rr, z1 - ch - inset), (rr - ch + inset, z1 - inset), (r0, z1)]
    da = ch / r1
    angles = [a0] + [a0 + da + (a1 - a0 - 2 * da) * k / segs for k in range(segs + 1)] + [a1]
    secs = []
    for i, a in enumerate(angles):
        end = i in (0, len(angles) - 1)
        p = prof(ch if end else 0.0)
        c, s = math.cos(a), math.sin(a)
        secs.append([bm.verts.new((r * c, r * s, z)) for r, z in p])
    pm = [ch_mat, ch_mat, face_mat, ch_mat, ch_mat]          # bottom chamfer.. top chamfer
    for i in range(len(secs) - 1):
        A, B = secs[i], secs[i + 1]
        endseg = i in (0, len(secs) - 2)
        for k in range(len(A) - 1):
            f = bm.faces.new((A[k], B[k], B[k + 1], A[k + 1]))
            f.material_index = ch_mat if (endseg and k == 2) else pm[k]
    for S, rev in ((secs[0], False), (secs[-1], True)):
        f = bm.faces.new(S if not rev else S[::-1]); f.material_index = ch_mat

def build_wall():
    bm = bmesh.new()
    R_CORE = R_OUT - 0.04
    # core ring (graphite) + bottom trim + top cap ; inner face white with blue neon strip
    revolve_profile(bm, [
        (R_IN, WALL_Z0, DARK), (R_OUT + 0.07, WALL_Z0, DARK), (R_OUT + 0.07, 0.24, DARK),
        (R_OUT + 0.04, 0.27, METAL), (R_CORE, 0.27, DARK), (R_CORE, 0.86, DARK),
        (R_OUT + 0.04, 0.86, METAL), (R_OUT + 0.07, 0.89, DARK), (R_OUT + 0.07, WALL_Z1, DARK),
        (R_IN, WALL_Z1, WHITE), (R_IN, 0.90, NEON), (R_IN + 0.015, 0.89, NEON), (R_IN + 0.015, 0.84, NEON),
        (R_IN, 0.83, WHITE), (R_IN, FLOOR + 0.08, DARK), (R_IN + 0.02, FLOOR + 0.06, DARK),
    ])
    gap = 0.07 / R_OUT
    step = 2 * math.pi / N_PANELS
    z0, z1 = 0.30, 0.83
    for i in range(N_PANELS):
        a0, a1 = i * step + gap / 2, (i + 1) * step - gap / 2
        am = (a0 + a1) / 2
        n = Vector((math.cos(am), math.sin(am), 0))
        t = Vector((-math.sin(am), math.cos(am), 0))
        rot = Matrix((t, n, (0, 0, 1))).transposed()        # x = tangent, y = outward, z = up
        # main panel
        curved_slab(bm, a0, a1, z0, z1, R_CORE, R_OUT + 0.035, 0.035, WHITE, METAL, segs=4)
        RF = R_OUT + 0.035
        # graphite inset strip with a detail, alternating
        if i % 2 == 0:   # vent grille
            add_box(bm, (0.62, 0.02, 0.22), n * (RF + 0.002) + Vector((0, 0, 0.555)), DARK, bevel=0.01, segs=1, rot=rot)
            for k in range(4):
                add_box(bm, (0.56, 0.025, 0.02), n * (RF + 0.012) + Vector((0, 0, 0.475 + k * 0.053)), METAL, rot=rot)
        else:            # status plate + orange LED
            add_box(bm, (0.42, 0.02, 0.12), n * (RF + 0.002) + t * -0.12 + Vector((0, 0, 0.60)), DARK, bevel=0.012, segs=1, rot=rot)
            M = Matrix.Translation(n * (RF + 0.012) + t * 0.24 + Vector((0, 0, 0.60))) @ n.to_track_quat('Z', 'Y').to_matrix().to_4x4()
            add_revolve(bm, [(0, 0), (0.035, 0), (0.035, 0.012), (0.025, 0.02), (0, 0.02)], [DARK, DARK, ORANGE, ORANGE], 8, M)
            add_box(bm, (0.30, 0.012, 0.025), n * (RF + 0.012) + t * -0.12 + Vector((0, 0, 0.60)), NEON, rot=rot)
        # corner screws
        half = (a1 - a0) / 2 - 0.09 / R_OUT
        for s in (-1, 1):
            a = am + s * half
            nn = Vector((math.cos(a), math.sin(a), 0))
            for z in (z0 + 0.07, z1 - 0.07):
                add_screw(bm, nn * RF + Vector((0, 0, z)), nn, r=0.018, mat=METAL)
    return bm

# ------------------------------------------------------------------ ring beam + geodesic frame
def dome_sphere():
    tmp = bmesh.new()
    bmesh.ops.create_icosphere(tmp, subdivisions=3, radius=1.0)   # frequency 4, clean equator
    verts = [v.co.copy() for v in tmp.verts]
    idx = {v: i for i, v in enumerate(tmp.verts)}
    faces = [[idx[v] for v in f.verts] for f in tmp.faces if f.calc_center_median().z > 0]
    edges = set()
    for f in faces:
        for i in range(3):
            a, b = sorted((f[i], f[(i+1) % 3])); edges.add((a, b))
    tmp.free()
    return verts, faces, sorted(edges)

C0 = Vector((0, 0, RING_Z1))
def P(v): return C0 + v * R_DOME

def strut(bm, a, b, mat, w=0.10, d_out=0.07, d_in=0.13, ch=0.03, trim=0.16):
    pa, pb = P(a), P(b)
    t = (pb - pa).normalized()
    n = ((a + b) / 2).normalized()
    bvec = t.cross(n).normalized()
    n = bvec.cross(t).normalized()
    sec = [(w - ch, d_out), (-(w - ch), d_out), (-w, d_out - ch), (-w, -d_in + ch),
           (-(w - ch), -d_in), (w - ch, -d_in), (w, -d_in + ch), (w, d_out - ch)]
    s0, s1 = pa + t * trim, pb - t * trim
    r0 = [bm.verts.new(s0 + bvec * x + n * y) for x, y in sec]
    r1 = [bm.verts.new(s1 + bvec * x + n * y) for x, y in sec]
    k = len(sec)
    fm = [mat, METAL, mat, METAL, DARK, METAL, mat, METAL]   # outer face white, wide graphite chamfers
    for i in range(k):
        j = (i + 1) % k
        f = bm.faces.new((r0[i], r0[j], r1[j], r1[i])); f.material_index = fm[i]
    for ring in (r0, r1[::-1]):
        f = bm.faces.new(ring[::-1]); f.material_index = mat

def node(bm, v, big=False):
    n = v.normalized()
    M = Matrix.Translation(P(v)) @ n.to_track_quat('Z', 'Y').to_matrix().to_4x4()
    r = 0.24 if big else 0.19
    add_revolve(bm, [(0, -0.16), (r * 0.8, -0.16), (r, -0.12), (r, 0.06), (r * 0.75, 0.11),
                     (r * 0.45, 0.11), (r * 0.45, 0.13), (0, 0.13)],
                [DARK, DARK, DARK, DARK, DARK, METAL, METAL], 8, M)
    if big:  # pentagon hubs get a small orange light
        add_revolve(bm, [(0, 0.13), (0.05, 0.13), (0.05, 0.16), (0, 0.17)], [ORANGE] * 3, 8, M)

def build_frame():
    bm = bmesh.new()
    # ring beam (graphite) with blue neon line and orange lamps
    revolve_profile(bm, [
        (R_OUT - 0.38, WALL_Z1, DARK), (R_OUT + 0.10, WALL_Z1, DARK), (R_OUT + 0.16, WALL_Z1 + 0.05, DARK), (R_OUT + 0.16, WALL_Z1 + 0.10, NEON),
        (R_OUT + 0.14, WALL_Z1 + 0.11, NEON), (R_OUT + 0.14, WALL_Z1 + 0.14, NEON), (R_OUT + 0.16, WALL_Z1 + 0.15, DARK), (R_OUT + 0.16, WALL_Z1 + 0.20, WHITE),
        (R_OUT + 0.10, RING_Z1, WHITE), (R_OUT - 0.38, RING_Z1, DARK), (R_OUT - 0.42, RING_Z1 - 0.04, DARK),
        (R_OUT - 0.42, WALL_Z1 + 0.04, DARK)])
    for i in range(0):   # orange lights now on the wall panels
        a = 2*math.pi*(i + 0.5)/20
        n = Vector((math.cos(a), math.sin(a), 0))
        M = Matrix.Translation(n * 6.16 + Vector((0, 0, 3.39))) @ n.to_track_quat('Z', 'Y').to_matrix().to_4x4()
        add_revolve(bm, [(0, 0), (0.07, 0), (0.07, 0.02), (0.05, 0.035), (0, 0.035)], [DARK, DARK, ORANGE, ORANGE], 8, M)
    verts, faces, edges = dome_sphere()
    valence = {}
    for a, b in edges:
        valence[a] = valence.get(a, 0) + 1; valence[b] = valence.get(b, 0) + 1
    for a, b in edges:
        va, vb = verts[a], verts[b]
        if abs(va.z) < 1e-4 and abs(vb.z) < 1e-4: continue   # equator = ring beam
        strut(bm, va, vb, WHITE)
    for i, v in enumerate(verts):
        if v.z < -1e-4 or i not in valence: continue
        if v.z < 1e-4:   # equator feet: anchor block on the ring
            n = Vector((v.x, v.y, 0)).normalized()
            rot = n.to_track_quat('X', 'Z').to_matrix()
            add_box(bm, (0.34, 0.30, 0.16), P(v) + Vector((0, 0, 0.06)) - n * 0.03, DARK, bevel=0.05, segs=1, rot=rot)
            continue
        node(bm, v, big=(valence[i] == 5 and v.z < 0.99))
    # apex cap: air recycler
    M = Matrix.Translation(P(Vector((0, 0, 1))) + Vector((0, 0, -0.05)))
    add_revolve(bm, [(0, 0.0), (0.75, 0.0), (0.78, 0.05), (0.78, 0.16), (0.70, 0.22), (0.55, 0.22),
                     (0.55, 0.30), (0.62, 0.30), (0.62, 0.42), (0.40, 0.50), (0, 0.50)],
                [DARK, DARK, WHITE, WHITE, DARK, METAL, METAL, DARK, DARK, DARK], 16, M)
    for i in range(12):   # vent slats
        a = 2*math.pi*i/12
        rot = Matrix.Rotation(a, 3, 'Z')
        add_box(bm, (0.10, 0.03, 0.07), M @ Vector((0.6*math.cos(a), 0.6*math.sin(a), 0.26)), DARK, rot=rot)
    add_revolve(bm, [(0, 0.5), (0.08, 0.5), (0.08, 0.56), (0, 0.6)], [ORANGE] * 3, 8, M)
    # opaque lower panels (first row above the ring) : white, slightly recessed, with a graphite rim
    for f in faces:
        tri = [verts[i] for i in f]
        if True: continue   # opaque lower panels disabled: the whole dome is glazed
        c = sum(tri, Vector()) / 3
        nrm = c.normalized()
        outer = [P(c + (v - c) * 0.9) - nrm * 0.02 for v in tri]
        inner = [P(c + (v - c) * 0.72) + nrm * 0.0 for v in tri]
        vo = [bm.verts.new(p) for p in outer]; vi = [bm.verts.new(p) for p in inner]
        for i in range(3):
            j = (i + 1) % 3
            fr = bm.faces.new((vo[i], vo[j], vi[j], vi[i])); fr.material_index = DARK
        fc = bm.faces.new(vi); fc.material_index = WHITE
        # back side (interior)
        bo = [bm.verts.new(p - nrm * 0.08) for p in outer]
        fb = bm.faces.new(bo[::-1]); fb.material_index = WHITE
        for i in range(3):
            j = (i + 1) % 3
            fs = bm.faces.new((vo[j], vo[i], bo[i], bo[j])); fs.material_index = DARK
    return bm, verts, faces

def build_glass(verts, faces):
    bm = bmesh.new()
    for f in faces:
        tri = [verts[i] for i in f]
        c = sum(tri, Vector()) / 3
        nrm = c.normalized()
        pts = [P(c + (v - c) * 0.97) - nrm * 0.03 for v in tri]
        # thin closed pane (1.5 cm): consistent normals -> visible from outside AND inside in Unity
        a = [bm.verts.new(p) for p in pts]
        b = [bm.verts.new(p - nrm * 0.015) for p in pts]
        if (pts[1] - pts[0]).cross(pts[2] - pts[0]).dot(nrm) < 0:
            a, b = a[::-1], b[::-1]
        bm.faces.new(a)                 # outer face (normal outward)
        bm.faces.new(b[::-1])           # inner face (normal inward)
        for i in range(3):
            j = (i + 1) % 3
            bm.faces.new((a[j], a[i], b[i], b[j]))
    return bm

def setup():
    for ob in list(bpy.data.objects):
        if not ob.name.startswith("_"): bpy.data.objects.remove(ob)
    mats = make_mats()
    col = bpy.context.scene.collection
    root = bpy.data.objects.new("Dome", None); col.objects.link(root)
    root.empty_display_size = 2
    base = finish("Dome_Base", build_base(), (0, 0, 0), col, root, mats=mats)
    wall = finish("Dome_Wall", build_wall(), (0, 0, 0), col, root, mats=mats)
    fbm, verts, faces = build_frame()
    frame = finish("Dome_Frame", fbm, (0, 0, 0), col, root, mats=mats)
    glass = finish("Dome_Glass", build_glass(verts, faces), (0, 0, 0), col, root, mats=[mats[GLASS]], sharp_deg=5)
    for ob in (base, wall, frame, glass):
        ob.matrix_parent_inverse = Matrix()
    bpy.context.view_layer.update()
    tris = sum(sum(len(p.vertices) - 2 for p in o.data.polygons) for o in (base, wall, frame, glass))
    return {"tris": tris, "objects": [o.name for o in root.children]}

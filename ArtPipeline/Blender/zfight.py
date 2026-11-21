# Detect z-fighting: faces of DIFFERENT islands (or objects of the same root) that are coplanar,
# face the same way and overlap in area.
import bpy, bmesh
from mathutils import Vector
from collections import defaultdict

def islands_of(bm):
    bm.faces.ensure_lookup_table(); comp = {}; cid = 0
    for f0 in bm.faces:
        if f0.index in comp: continue
        st = [f0]; comp[f0.index] = cid
        while st:
            f = st.pop()
            for e in f.edges:
                for g in e.link_faces:
                    if g.index not in comp: comp[g.index] = cid; st.append(g)
        cid += 1
    return comp

def overlap_area(fa, fb, n):
    # project both polygons on the plane, clip (convex assumption via Sutherland-Hodgman)
    ax = n.orthogonal().normalized(); ay = n.cross(ax)
    P = lambda f: [Vector((v.dot(ax), v.dot(ay))) for v in f]
    subj, clip = P(fa), P(fb)
    def area(p): return 0.5 * sum(p[i].x * p[(i+1) % len(p)].y - p[(i+1) % len(p)].x * p[i].y for i in range(len(p)))
    if area(clip) < 0: clip = clip[::-1]
    out = subj
    for i in range(len(clip)):
        a, b = clip[i], clip[(i+1) % len(clip)]
        inp, out = out, []
        if not inp: break
        side = lambda p: (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x)
        for j in range(len(inp)):
            p, q = inp[j], inp[(j+1) % len(inp)]
            sp, sq = side(p), side(q)
            if sp >= 0: out.append(p)
            if (sp >= 0) != (sq >= 0):
                t = sp / (sp - sq); out.append(p + (q - p) * t)
    return abs(area(out)) if len(out) >= 3 else 0.0

def check(objs, min_area=2e-4, tol=1e-3):
    faces = []   # (key, world verts, normal, owner id)
    for ob in objs:
        bm = bmesh.new(); bm.from_mesh(ob.data); comp = islands_of(bm)
        M = ob.matrix_world; R = M.to_3x3()
        for f in bm.faces:
            if f.calc_area() < 1e-5: continue
            n = (R @ f.normal).normalized(); vs = [M @ v.co for v in f.verts]
            faces.append((n, vs, (ob.name, comp[f.index]), ob.data.materials[f.material_index].name if ob.data.materials else ""))
        bm.free()
    buckets = defaultdict(list)
    for i, (n, vs, own, m) in enumerate(faces):
        d = n.dot(vs[0])
        buckets[(round(n.x, 2), round(n.y, 2), round(n.z, 2), round(d / tol))].append(i)
    hits = []
    for key, idx in buckets.items():
        if len(idx) < 2: continue
        for a in range(len(idx)):
            for b in range(a + 1, len(idx)):
                fa, fb = faces[idx[a]], faces[idx[b]]
                if fa[2] == fb[2]: continue
                ar = overlap_area(fa[1], fb[1], fa[0])
                if ar > min_area:
                    c = sum(fa[1], Vector()) / len(fa[1])
                    hits.append((round(ar, 4), fa[2][0], fb[2][0], fa[3], fb[3], [round(x, 3) for x in c]))
    hits.sort(reverse=True)
    return hits

def faces_of(objs):
    out = []
    for ob in objs:
        bm = bmesh.new(); bm.from_mesh(ob.data); comp = islands_of(bm); M = ob.matrix_world; R = M.to_3x3()
        for f in bm.faces:
            if f.calc_area() < 1e-5: continue
            out.append(((R @ f.normal).normalized(), [M @ v.co for v in f.verts], (ob.name, comp[f.index]),
                        ob.data.materials[f.material_index].name if ob.data.materials else ""))
        bm.free()
    return out

def check2(objs, skip=lambda n, c: c.z < 0):
    faces = faces_of(objs); buckets = defaultdict(list)
    for i, (n, vs, own, m) in enumerate(faces):
        buckets[(round(n.x, 2), round(n.y, 2), round(n.z, 2), round(n.dot(vs[0]) / 1e-3))].append(i)
    hits = []
    for idx in buckets.values():
        for a in range(len(idx)):
            for b in range(a + 1, len(idx)):
                fa, fb = faces[idx[a]], faces[idx[b]]
                if fa[2] == fb[2]: continue
                ar = overlap_area(fa[1], fb[1], fa[0])
                if ar > 2e-4:
                    c = sum(fa[1], Vector()) / len(fa[1])
                    if skip(fa[0], c): continue
                    hits.append((round(ar, 4), fa[2][0], fb[2][0], fa[3], fb[3], [round(x, 3) for x in c], [round(x, 2) for x in fa[0]]))
    hits.sort(reverse=True); return hits

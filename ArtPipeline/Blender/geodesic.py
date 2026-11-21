# Class-I geodesic hemisphere of any frequency, with a flat base (boundary snapped onto the equator).
import math
from mathutils import Vector, Matrix

def icosahedron():
    t = (1 + 5 ** 0.5) / 2
    V = [Vector(v).normalized() for v in [(-1, t, 0), (1, t, 0), (-1, -t, 0), (1, -t, 0), (0, -1, t), (0, 1, t),
                                         (0, -1, -t), (0, 1, -t), (t, 0, -1), (t, 0, 1), (-t, 0, -1), (-t, 0, 1)]]
    F = [(0, 11, 5), (0, 5, 1), (0, 1, 7), (0, 7, 10), (0, 10, 11), (1, 5, 9), (5, 11, 4), (11, 10, 2), (10, 7, 6),
         (7, 1, 8), (3, 9, 4), (3, 4, 2), (3, 2, 6), (3, 6, 8), (3, 8, 9), (4, 9, 5), (2, 4, 11), (6, 2, 10),
         (8, 6, 7), (9, 8, 1)]
    return V, F

def orient_matrix(V, F, orient):
    if orient == 'vertex': axis = V[0]
    elif orient == 'face': axis = (V[F[0][0]] + V[F[0][1]] + V[F[0][2]]).normalized()
    else: axis = (V[0] + V[1]).normalized()          # edge midpoint up
    return axis.rotation_difference(Vector((0, 0, 1))).to_matrix()

def geodesic_sphere(n, orient='vertex'):
    V, F = icosahedron()
    R = orient_matrix(V, F, orient)
    V = [R @ v for v in V]
    verts, index, faces = [], {}, []
    def vid(p):
        key = tuple(round(c, 6) for c in p)
        if key not in index:
            index[key] = len(verts); verts.append(p)
        return index[key]
    for a, b, c in F:
        A, B, C = V[a], V[b], V[c]
        grid = {}
        for i in range(n + 1):
            for j in range(n + 1 - i):
                k = n - i - j
                p = (A * i + B * j + C * k) / n
                grid[(i, j)] = vid(p.normalized())
        for i in range(n):
            for j in range(n - i):
                faces.append((grid[(i, j)], grid[(i + 1, j)], grid[(i, j + 1)]))
                if i + j < n - 1:
                    faces.append((grid[(i + 1, j)], grid[(i + 1, j + 1)], grid[(i, j + 1)]))
    return verts, faces

def hemisphere(n, orient='vertex'):
    """faces with centroid above the equator ; boundary vertices snapped onto z = 0 (moved along their meridian)."""
    verts, faces = geodesic_sphere(n, orient)
    keep = [f for f in faces if sum(verts[i].z for i in f) / 3 > 1e-6]
    used = sorted({i for f in keep for i in f})
    edges = {}
    for f in keep:
        for e in ((f[0], f[1]), (f[1], f[2]), (f[2], f[0])):
            k = tuple(sorted(e)); edges[k] = edges.get(k, 0) + 1
    boundary = {i for e, c in edges.items() if c == 1 for i in e}
    delta = max((abs(verts[i].z) for i in boundary), default=0)
    out = {}
    for i in used:
        v = verts[i].copy()
        if i in boundary:
            h = Vector((v.x, v.y, 0)).normalized(); v = h
        out[i] = v
    keep = [f for f in keep if not all(i in boundary for i in f)]     # flattened onto the ring : dropped
    used = sorted({i for f in keep for i in f})
    edges = {}
    for f in keep:
        for e in ((f[0], f[1]), (f[1], f[2]), (f[2], f[0])):
            k = tuple(sorted(e)); edges[k] = edges.get(k, 0) + 1
    remap = {i: k for k, i in enumerate(used)}
    V2 = [out[i] for i in used]
    F2 = [tuple(remap[i] for i in f) for f in keep]
    E2 = sorted({tuple(sorted((remap[a], remap[b]))) for (a, b) in edges})
    return V2, F2, E2, delta

def stats(n, orient, R):
    V, F, E, d = hemisphere(n, orient)
    L = [(V[a] - V[b]).length * R for a, b in E if not (abs(V[a].z) < 1e-6 and abs(V[b].z) < 1e-6)]
    return {"n": n, "orient": orient, "edge_avg": round(sum(L) / len(L), 3), "edge_min": round(min(L), 3),
            "edge_max": round(max(L), 3), "snap_m": round(d * R, 3), "faces": len(F),
            "apex_vertex": any(abs(v.z - 1) < 1e-6 for v in V)}

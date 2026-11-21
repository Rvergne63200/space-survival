import bpy, bmesh, math
from mathutils import Matrix, Vector

ZONES = [
    # name, base_on, base_off, emission (None = not emissive)
    ("Z_White",  (0.82, 0.85, 0.88), (0.82, 0.85, 0.88), None),
    ("Z_Dark",   (0.035, 0.045, 0.06), (0.035, 0.045, 0.06), None),
    ("Z_Metal",  (0.16, 0.18, 0.22), (0.16, 0.18, 0.22), None),
    ("Z_Neon",   (0.55, 0.92, 1.0), (0.07, 0.11, 0.15), (0.05, 0.65, 1.0)),
    ("Z_Orange", (1.0, 0.38, 0.03), (0.3, 0.11, 0.02), (1.0, 0.25, 0.0)),
    ("Z_InnerGlow", (0.1, 0.4, 0.85), (0.04, 0.07, 0.11), (0.01, 0.2, 0.55)),
    ("Z_Plate",  (0.55, 0.65, 0.75), (0.45, 0.5, 0.56), None),
    ("Z_Hazard", (0.95, 0.6, 0.04), (0.95, 0.6, 0.04), None),
]
WHITE, DARK, METAL, NEON, ORANGE, INNER, PLATE, HAZARD = range(8)

def add_screw(bm, center, normal, r=0.011, mat=2):
    n = Vector(normal).normalized()
    M = Matrix.Translation(Vector(center)) @ n.to_track_quat('Z', 'Y').to_matrix().to_4x4()
    add_revolve(bm, [(0, 0.0), (r, 0.0), (r, 0.004), (r*0.75, 0.007), (0, 0.007)], [mat]*4, 6, M)

def zone_mats(off=False):
    out = []
    for name, on_c, off_c, em in ZONES:
        m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
        b = next(n for n in m.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
        col = off_c if off else on_c
        b.inputs["Base Color"].default_value = (*col, 1)
        b.inputs["Roughness"].default_value = 0.5
        if em and not off:
            b.inputs["Emission Color"].default_value = (*em, 1)
            b.inputs["Emission Strength"].default_value = 3.0
        else:
            b.inputs["Emission Strength"].default_value = 0.0
        m.diffuse_color = (*col, 1)
        out.append(m)
    return out

def _copy_into(dst, src, mat):
    vmap = {v: dst.verts.new(v.co) for v in src.verts}
    for f in src.faces:
        nf = dst.faces.new([vmap[v] for v in f.verts])
        nf.material_index = mat if mat is not None else f.material_index

def add_box(bm, size, center, mat, bevel=0.0, segs=2, rot=Matrix()):
    tmp = bmesh.new()
    bmesh.ops.create_cube(tmp, size=1.0)
    bmesh.ops.scale(tmp, vec=Vector(size), verts=tmp.verts)
    if bevel > 0:
        bmesh.ops.bevel(tmp, geom=list(tmp.edges) + list(tmp.verts), offset=bevel, segments=segs,
                        profile=0.5, affect='EDGES', clamp_overlap=True)
    M = Matrix.Translation(Vector(center)) @ rot.to_4x4()
    bmesh.ops.transform(tmp, matrix=M, verts=tmp.verts)
    _copy_into(bm, tmp, mat); tmp.free()

def add_revolve(bm, prof, mats, segs=24, M=Matrix(), closed=False):
    rings = []
    for r, z in prof:
        if r < 1e-6: rings.append([bm.verts.new(M @ Vector((0, 0, z)))])
        else: rings.append([bm.verts.new(M @ Vector((r*math.cos(2*math.pi*i/segs), r*math.sin(2*math.pi*i/segs), z))) for i in range(segs)])
    n = len(rings)
    pairs = [(k, k+1) for k in range(n-1)] + ([(n-1, 0)] if closed else [])
    for si, (ka, kb) in enumerate(pairs):
        a, b = rings[ka], rings[kb]
        for i in range(segs):
            j = (i+1) % segs
            if len(a) == 1: vs = (a[0], b[j], b[i])
            elif len(b) == 1: vs = (a[i], a[j], b[0])
            else: vs = (a[i], a[j], b[j], b[i])
            f = bm.faces.new(vs)
            f.material_index = mats[si] if isinstance(mats, (list, tuple)) else mats

def add_cyl(bm, r, z0, z1, mat, segs=20, M=Matrix(), cap=None):
    c = mat if cap is None else cap
    add_revolve(bm, [(0, z0), (r, z0), (r, z1), (0, z1)], [c, mat, c], segs, M)

def finish(name, bm, origin, col, parent=None, parent_origin=(0,0,0), mats=None, sharp_deg=35):
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    old = bpy.data.objects.get(name)
    me = bpy.data.meshes.new(name + "_new")
    bmesh.ops.translate(bm, vec=-Vector(origin), verts=bm.verts)
    bm.to_mesh(me); bm.free()
    for m in mats: me.materials.append(m)
    me.shade_smooth(); me.set_sharp_from_angle(angle=math.radians(sharp_deg))
    # keep zone index for future re-bakes
    za = me.attributes.new("zone", 'INT', 'FACE')
    za.data.foreach_set("value", [p.material_index for p in me.polygons])
    if old:  # keep object (transforms, parenting, animation), swap mesh
        oldm = old.data
        old.data = me
        if oldm.users == 0: bpy.data.meshes.remove(oldm)
        me.name = name
        return old
    oldm = bpy.data.meshes.get(name)
    if oldm and oldm.users == 0: bpy.data.meshes.remove(oldm)
    me.name = name
    ob = bpy.data.objects.new(name, me)
    col.objects.link(ob)
    if parent:
        ob.parent = parent
        ob.location = Vector(origin) - Vector(parent_origin)
    else:
        ob.location = origin
    return ob

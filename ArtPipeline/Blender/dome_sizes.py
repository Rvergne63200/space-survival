# Domes 8 / 12 / 16 m + matching DomeConnector, same panel size (geodesic frequency) and same wall height.
import bpy, math
from mathutils import Vector, Matrix
SP = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival/ArtPipeline/Blender"
SRC = {n: open(SP + "\\" + n, encoding="utf8").read() for n in ("dome_build.py", "al_build.py", "geodesic.py")}

SIZES = {   # diameter label : (R_OUT, panels, geodesic frequency, orientation) ; 12 m keeps its original icosphere
    "8m":  (4.0, 16, 3, 'vertex'),
    "12m": (6.0, 24, None, None),
    "16m": (8.0, 32, 6, 'vertex'),
}

SURFACE_CODE = '''
def dome_surface_y(x, z):
    if z < 0.95: Rs = R_OUT_D + 0.07
    elif z < 1.20: Rs = R_OUT_D + 0.16
    else: Rs = math.sqrt(max((R_DOME_D + 0.13) ** 2 - (z - 1.20) ** 2, 0.01))
    return DOME_C_Y - math.sqrt(max(Rs * Rs - x * x, 0.01))
'''

def build_size(label, x_offset=0.0):
    R_OUT, panels, freq, orient = SIZES[label]
    # ---------------- dome
    d = {}; exec(SRC["dome_build.py"], d)
    d.update(R_OUT=R_OUT, R_IN=R_OUT - 0.25, R_DOME=R_OUT - 0.10, N_PANELS=panels)
    if freq:
        g = {}; exec(SRC["geodesic.py"], g)
        V, F, E, _ = g["hemisphere"](freq, orient)
        d["dome_sphere"] = lambda V=V, F=F, E=E: ([v.copy() for v in V], [list(f) for f in F], list(E))
    col = bpy.context.scene.collection
    mats = d["make_mats"]()
    root = bpy.data.objects.new(f"Dome_{label}", None); col.objects.link(root); root.empty_display_size = 2
    root.location = (x_offset, 0, 0)
    fin = d["finish"]
    base = fin(f"Dome_Base_{label}", d["build_base"](), (0, 0, 0), col, root, mats=mats)
    wall = fin(f"Dome_Wall_{label}", d["build_wall"](), (0, 0, 0), col, root, mats=mats)
    fbm, verts, faces = d["build_frame"]()
    frame = fin(f"Dome_Frame_{label}", fbm, (0, 0, 0), col, root, mats=mats)
    glass = fin(f"Dome_Glass_{label}", d["build_glass"](verts, faces), (0, 0, 0), col, root, mats=[mats[d["GLASS"]]], sharp_deg=5)
    for ob in (base, wall, frame, glass):
        ob.matrix_parent_inverse = Matrix(); ob.location = (0, 0, 0)
        ob["export_name"] = ob.name.rsplit("_", 1)[0]          # Dome_Base, Dome_Wall... inside the FBX
    # ---------------- connector (origin on the wall, outward = -Y ; dome centre at +R_OUT)
    c = {}; exec(SRC["al_build.py"], c)
    c.update(DOME_C_Y=R_OUT, R_OUT_D=R_OUT, R_DOME_D=R_OUT - 0.10)
    exec(SURFACE_CODE, c)
    cm = c["make_mats"]("DomeConnector")
    croot = bpy.data.objects.new(f"DomeConnector_{label}", None); col.objects.link(croot); croot.empty_display_size = 1
    croot.location = (x_offset, -R_OUT, 0)                    # preview : on the dome's front wall
    body = c["finish2"](f"DomeConnector_Body_{label}", c["build_connector"](), col, croot, cm)
    body["export_name"] = "DomeConnector_Body"
    sock = bpy.data.objects.new(f"Socket_{label}", None); col.objects.link(sock); sock.parent = croot
    sock.location = (0, c["FLANGE_Y"], 0); sock.empty_display_type = 'ARROWS'; sock.empty_display_size = 0.5
    sock["export_name"] = "Socket"
    tri = lambda o: sum(len(p.vertices) - 2 for p in o.data.polygons)
    return {"dome_tris": sum(tri(o) for o in (base, wall, frame, glass)), "connector_tris": tri(body),
            "glass_panes": len(faces)}

def build_all():
    for ob in list(bpy.data.objects):
        if not ob.name.startswith("_"): bpy.data.objects.remove(ob)
    for me in list(bpy.data.meshes):
        if me.users == 0: bpy.data.meshes.remove(me)
    out = {}
    for label, x in (("8m", -14.0), ("12m", 0.0), ("16m", 18.0)):
        out[label] = build_size(label, x)
    return out

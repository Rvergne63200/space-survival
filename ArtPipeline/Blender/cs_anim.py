# Animations for station v6 (single global timeline, split in Unity by ModelImportSetup.cs)
import bpy, math, random
from mathutils import Vector
SP = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival/ArtPipeline/Blender"
exec(open(SP + r"\cs_v5.py", encoding="utf8").read())

O = bpy.data.objects
SHO, ELB, HEAD, PLATE_O, LASER = O["Arm_Shoulder"], O["Arm_Elbow"], O["Arm_Head"], O["BuildPlate"], O["Laser"]
HOLO, RING = O["Hologram"], O["Hologram_Ring"]
HEAD_Z0 = HEAD.location.z
PLATE_Z0 = TOP + 0.02 + 0.06   # plate raised on its column at work height (always above the worktop)
RING_Z0 = TOP + 0.14
LASER_LEN = (SH_Z - 0.06 - 0.2 - 0.096) - (TOP + 0.042)   # beam length built in the mesh (plate at geometry height)
PRINT_DROP = 0.06        # plate lowers while printing (back to worktop level, still visible)
OFF_DROP = 0.035         # plate position when powered off
HOP = 0.005              # tiny head hop, stays hidden inside the head mount
OFF = 0.001

def ik(xy, prev=None):
    d = Vector(xy) - COL
    D = min(d.length, L1 + L2 - 1e-4)
    a2 = math.acos(max(-1, min(1, (D*D - L1*L1 - L2*L2) / (2*L1*L2))))
    a1 = math.atan2(d.y, d.x) - math.atan2(L2*math.sin(a2), L1 + L2*math.cos(a2)) + math.pi/2
    if prev is not None:
        while a1 - prev[0] > math.pi: a1 -= 2*math.pi
        while a1 - prev[0] < -math.pi: a1 += 2*math.pi
    return a1, a2

PARK = (math.radians(-90), math.radians(120))
WORK = ik(PC)

def clear():
    for ob in (SHO, ELB, HEAD, PLATE_O, LASER, HOLO, RING, O.get("Hologram_Screen")):
        if ob and ob.animation_data: ob.animation_data_clear()
    for a in list(bpy.data.actions): bpy.data.actions.remove(a)

def key(ob, path, idx, f, v, interp='BEZIER'):
    getattr(ob, path)[idx] = v
    ob.keyframe_insert(path, index=idx, frame=f)
    ad = ob.animation_data
    fc = ad.action.layers[0].strips[0].channelbag(ad.action_slot).fcurves.find(path, index=idx)
    for kp in fc.keyframe_points:
        if abs(kp.co[0] - f) < 1e-4: kp.interpolation = interp

def k_arm(f, a, interp='BEZIER'):
    key(SHO, "rotation_euler", 2, f, a[0], interp); key(ELB, "rotation_euler", 2, f, a[1], interp)
def k_head(f, dz, interp='BEZIER'): key(HEAD, "location", 2, f, HEAD_Z0 + dz, interp)
def k_plate(f, dz, interp='BEZIER'): key(PLATE_O, "location", 2, f, PLATE_Z0 + dz, interp)
def k_scale(ob, f, s, interp='BEZIER'):
    for i, v in enumerate(s if isinstance(s, tuple) else (s, s, s)): key(ob, "scale", i, f, v, interp)
def k_laser(f, sxy, drop, interp='BEZIER'):
    # beam length = tip -> plate top ; plate top = geometry height + 0.06 - drop
    length = LASER_LEN - 0.06 + drop
    k_scale(LASER, f, (sxy, sxy, length/LASER_LEN if sxy > OFF else OFF), interp)
def k_ring(f, dz, rot, s, interp='BEZIER'):
    key(RING, "location", 2, f, RING_Z0 + dz, interp); key(RING, "rotation_euler", 2, f, rot, interp)
    k_scale(RING, f, s, interp)

def state(f, on, arm, drop=0.0, laser=OFF, ring=(0.0, 0.0), hop=0.0, interp='BEZIER'):
    """key every animated channel (clips are self-contained)."""
    k_arm(f, arm, interp); k_head(f, hop, interp)
    k_plate(f, (0.0 if on else -OFF_DROP) - drop, interp)
    k_laser(f, laser, drop, interp)
    s = 1.0 if on else OFF
    k_scale(HOLO, f, s, interp)            # Hologram_Screen is a child -> follows exactly
    k_ring(f, ring[0], ring[1], s, interp)

def print_path():
    r = 0.13; pts = [Vector((0, 0))]
    ys = [-0.1, -0.06, -0.02, 0.02, 0.06, 0.1]
    for i, y in enumerate(ys):
        x = math.sqrt(max(r*r - y*y, 0)) * 0.95
        a, b = (-x, x) if i % 2 == 0 else (x, -x)
        pts += [Vector((a, y)), Vector((b, y))]
    pts += [Vector((r*math.cos(t), r*math.sin(t))) for t in [2*math.pi*k/24 for k in range(25)]]
    pts.append(Vector((0, 0)))
    return [PC + p for p in pts]

def build_anim():
    clear()
    sc = bpy.context.scene
    # ---------- Power_On 1-49
    state(1, False, PARK)
    k_plate(20, 0.0); k_scale(HOLO, 20, 1.0); k_ring(18, 0.0, 0.0, 1.0)
    k_arm(12, PARK)
    state(49, True, WORK)
    k_arm(40, WORK)
    # ---------- Power_Off 60-108
    state(60, True, WORK)
    k_arm(68, WORK)
    k_arm(94, PARK); k_plate(96, 0.0); k_scale(HOLO, 96, 1.0); k_ring(98, 0.0, 0.0, 1.0)
    state(108, False, PARK)
    # ---------- Print_Start 120-128 : plate lowers, laser fires
    state(120, True, WORK)
    k_plate(125, -PRINT_DROP); k_laser(124, OFF, PRINT_DROP); k_laser(126, 1.4, PRINT_DROP)
    state(128, True, WORK, drop=PRINT_DROP, laser=1.0)
    # ---------- Print_Loop 130-322
    F0, F1 = 130, 322
    path = print_path()
    seg = [(path[i+1] - path[i]).length for i in range(len(path) - 1)]
    total = sum(seg)
    state(F0, True, ik(path[0]), drop=PRINT_DROP, laser=1.0, interp='LINEAR')
    prev = ik(path[0])
    for f in range(F0 + 2, F1, 2):
        s = (f - F0) / (F1 - F0) * total
        i = 0
        while i < len(seg) - 1 and s > seg[i]: s -= seg[i]; i += 1
        p = path[i].lerp(path[i+1], min(1.0, s / seg[i]))
        prev = ik(p, prev)
        k_arm(f, prev, 'LINEAR')
        k_head(f, HOP if (f // 8) % 2 else 0.0, 'LINEAR')
    random.seed(3)
    for f in range(F0 + 2, F1, 2):
        k_laser(f, random.uniform(0.7, 1.35), PRINT_DROP, 'LINEAR')
    for f, dz in ((F0, 0.0), (F0 + 48, 0.14), (F0 + 96, 0.0), (F0 + 144, 0.14), (F1, 0.0)):
        key(RING, "location", 2, f, RING_Z0 + dz, 'BEZIER')
    key(RING, "rotation_euler", 2, F0, 0.0, 'LINEAR'); key(RING, "rotation_euler", 2, F1, math.pi/2, 'LINEAR')
    end = ik(path[-1], prev)
    state(F1, True, end, drop=PRINT_DROP, laser=1.0, ring=(0.0, math.pi/2), interp='LINEAR')
    # ---------- Print_End 330-338 : laser off, plate back up
    state(330, True, end, drop=PRINT_DROP, laser=1.0, ring=(0.0, math.pi/2))
    k_laser(333, 1.3, PRINT_DROP); k_laser(335, OFF, PRINT_DROP)
    state(338, True, WORK, ring=(0.0, math.pi/2))
    sc.frame_start, sc.frame_end = 1, 338
    sc.render.fps = 24
    sc.frame_set(1)
    return {"WORK_deg": [math.degrees(a) for a in WORK], "path_pts": len(path)}

def head_clearance():
    """max head top height vs. bottom of the head mount (must stay <= 0 -> hidden)."""
    return {"max_hop": HOP, "mount_overlap": 0.04}

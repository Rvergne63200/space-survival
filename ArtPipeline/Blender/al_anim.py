# Airlock animations : single global timeline, split in Unity by ModelImportSetup.cs
import bpy, math
O = bpy.data.objects
LEAVES = {("Outer", -1): O["Door_Outer_L"], ("Outer", 1): O["Door_Outer_R"],
          ("Inner", -1): O["Door_Inner_L"], ("Inner", 1): O["Door_Inner_R"]}
FANS = [O["Fan_Inner"], O["Fan_Roof"]]
TRAVEL = 0.80
CLIPS = [("Outer_Open", 1, 30), ("Outer_Close", 40, 70), ("Inner_Open", 80, 110),
         ("Inner_Close", 120, 150), ("Cycle", 160, 280)]

def key(ob, path, idx, f, v, interp='BEZIER'):
    getattr(ob, path)[idx] = v
    ob.keyframe_insert(path, index=idx, frame=f)
    ad = ob.animation_data
    fc = ad.action.layers[0].strips[0].channelbag(ad.action_slot).fcurves.find(path, index=idx)
    for kp in fc.keyframe_points:
        if abs(kp.co[0] - f) < 1e-4: kp.interpolation = interp

def doors(f, outer, inner, interp='BEZIER'):
    """outer / inner = opening ratio 0..1"""
    for (tag, side), ob in LEAVES.items():
        k = outer if tag == "Outer" else inner
        key(ob, "location", 0, f, side * TRAVEL * k, interp)

def fans(f, turns, interp='BEZIER'):
    for ob, mult in zip(FANS, (1, -0.6)):
        key(ob, "rotation_euler", 2, f, turns * 2 * math.pi * mult, interp)

def state(f, outer, inner, turns=0.0):
    doors(f, outer, inner); fans(f, turns)

def build():
    for ob in list(LEAVES.values()) + FANS:
        if ob.animation_data: ob.animation_data_clear()
    for a in list(bpy.data.actions): bpy.data.actions.remove(a)
    # Outer_Open : unlock pause, then slide (only one door open at a time)
    state(1, 0, 0); doors(7, 0, 0); state(30, 1, 0)
    state(40, 1, 0); state(70, 0, 0); doors(64, 0.03, 0)
    state(80, 0, 0); doors(86, 0, 0); state(110, 0, 1)
    state(120, 0, 1); state(150, 0, 0); doors(144, 0, 0.03)
    # Cycle : doors locked, air recyclers spin up / down (status colour handled by AirlockAnimator.cs)
    state(160, 0, 0, 0.0)
    fans(175, 0.4); fans(265, 7.6)
    state(280, 0, 0, 8.0)
    sc = bpy.context.scene
    sc.frame_start, sc.frame_end = 1, 280
    sc.render.fps = 24
    sc.frame_set(1)
    return {"clips": CLIPS}

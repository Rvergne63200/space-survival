# Station v5 : "établi à bras" (workbench + SCARA arm). Model only, flat preview materials.
SP = r"F:/Users/Richard/Documents/Richard/Dev/Jeux/Projets Unity/Space Survival/ArtPipeline/Blender"
exec(open(SP + r"\cs_helpers.py", encoding="utf8").read())

EB = 0.006   # edge break everywhere

def _bevel_tmp(tmp, ch, edges=None):
    if ch > 0:
        bmesh.ops.bevel(tmp, geom=list(edges if edges is not None else tmp.edges), offset=ch, segments=1,
                        profile=0.5, affect='EDGES', clamp_overlap=True)

def cbox(bm, size, center, mat, ch, eb=EB, rot=Matrix()):
    tmp = bmesh.new()
    bmesh.ops.create_cube(tmp, size=1.0)
    bmesh.ops.scale(tmp, vec=Vector(size), verts=tmp.verts)
    _bevel_tmp(tmp, ch); _bevel_tmp(tmp, eb)
    bmesh.ops.transform(tmp, matrix=Matrix.Translation(Vector(center)) @ rot.to_4x4(), verts=tmp.verts)
    _copy_into(bm, tmp, mat); tmp.free()

def pill(bm, size, center, mat, rot=Matrix()):
    add_box(bm, size, center, mat, bevel=min(size)*0.45, segs=2, rot=rot)

def prism_x(bm, prof_yz, x0, x1, mat, end_ch=0.03, eb=EB):
    """extrude a (chamfered) YZ profile along X, chamfer the end caps, break all edges."""
    tmp = bmesh.new()
    a = [tmp.verts.new((x0, y, z)) for y, z in prof_yz]
    b = [tmp.verts.new((x1, y, z)) for y, z in prof_yz]
    n = len(prof_yz)
    for i in range(n):
        j = (i + 1) % n
        tmp.faces.new((a[i], a[j], b[j], b[i]))
    tmp.faces.new(list(reversed(a))); tmp.faces.new(b)
    bmesh.ops.recalc_face_normals(tmp, faces=tmp.faces)
    cap_edges = [e for e in tmp.edges if abs(e.verts[0].co.x - e.verts[1].co.x) < 1e-6]
    _bevel_tmp(tmp, end_ch, cap_edges); _bevel_tmp(tmp, eb)
    _copy_into(bm, tmp, mat); tmp.free()

def oct_cyl(bm, r, z0, z1, mat, M=Matrix(), ch=0.015, sides=8):
    """faceted cylinder (octagonal) with chamfered caps."""
    rot = Matrix.Rotation(math.pi/sides, 4, 'Z')
    add_revolve(bm, [(0, z0), (r - ch, z0), (r, z0 + ch), (r, z1 - ch), (r - ch, z1), (0, z1)], [mat]*5, sides, M @ rot)

def beam(bm, p0, p1, w, h, mat, ch=0.02):
    p0, p1 = Vector(p0), Vector(p1); d = p1 - p0
    rot = d.to_track_quat('Y', 'Z').to_matrix()
    cbox(bm, (w, d.length, h), (p0 + p1)/2, mat, ch, rot=rot)

def tube(bm, pts, r, mat, ring_mat=None):
    pts = [Vector(p) for p in pts]
    for a, b in zip(pts[:-1], pts[1:]):
        d = b - a
        M = Matrix.Translation(a) @ d.to_track_quat('Z', 'Y').to_matrix().to_4x4()
        add_cyl(bm, r, 0.0, d.length, mat, 8, M)
        if ring_mat is not None:
            add_revolve(bm, [(0, -0.01), (r*1.35, -0.01), (r*1.35, 0.01), (0, 0.01)], [ring_mat]*3, 8, M)

# ------------------------------------------------------------------ layout
PC = Vector((-0.15, -0.10))          # plate centre (xy)
COL = Vector((-0.15, 0.34))          # SCARA column (xy)
TOP = 0.915                          # worktop height
SH_Z = 1.46                          # shoulder joint height
L1, L2 = 0.32, 0.30

def build_v5(MATS):
    col = bpy.data.collections["CraftStation"]
    root = bpy.data.objects["CraftStation"]

    # ======================= BODY (static)
    bm = bmesh.new()
    cab = [(-0.40, 0.10), (0.42, 0.10), (0.42, 0.79), (0.37, 0.84), (-0.36, 0.84), (-0.45, 0.75), (-0.45, 0.19)]
    prism_x(bm, cab, -0.72, 0.72, WHITE, end_ch=0.04)
    cbox(bm, (1.34, 0.66, 0.11), (0, 0.03, 0.055), DARK, 0.025)                       # toe kick
    for x in (-0.6, 0.6):
        cbox(bm, (0.12, 0.2, 0.03), (x, -0.32, 0.015), METAL, 0.01)                   # feet pads
    cbox(bm, (1.5, 0.9, TOP - 0.83), (0, -0.02, (TOP + 0.83) / 2), DARK, 0.03)         # worktop (sunk into the cabinet: no coplanar faces)
    cbox(bm, (1.44, 0.04, 0.025), (0, -0.465, TOP - 0.045), WHITE, 0.01, 0.003)       # front lip trim
    # front door panels with seams
    for i, x in enumerate((-0.47, 0.0, 0.47)):
        cbox(bm, (0.44, 0.03, 0.5), (x, -0.462, 0.45), WHITE, 0.035)
        if i == 1:
            cbox(bm, (0.32, 0.02, 0.12), (x, -0.48, 0.56), DARK, 0.03, 0.004)
            pill(bm, (0.26, 0.02, 0.045), (x, -0.49, 0.56), NEON)
            cbox(bm, (0.12, 0.012, 0.04), (x, -0.479, 0.3), METAL, 0.008, 0.002)     # id plate
        else:
            for k in range(6):                                                         # vents
                cbox(bm, (0.26, 0.02, 0.014), (x, -0.478, 0.33 + k*0.03), DARK, 0.005, 0.0)
            cbox(bm, (0.16, 0.03, 0.03), (x, -0.485, 0.6), METAL, 0.01, 0.003)        # handle
        for sx in (-1, 1):
            for sz in (-1, 1):
                add_screw(bm, (x + sx*0.19, -0.478, 0.45 + sz*0.22), (0, -1, 0), 0.008)
    # orange status leds on right door
    for k in range(3):
        pill(bm, (0.02, 0.02, 0.02), (0.62 - k*0.035, -0.48, 0.66), ORANGE)
    # neon strip on the front top chamfer (45°)
    R45 = Matrix.Rotation(math.radians(45), 3, 'X')
    cbox(bm, (1.2, 0.05, 0.03), (0, -0.407, 0.797), DARK, 0.01, 0.003, rot=R45)
    pill(bm, (1.14, 0.03, 0.018), (0, -0.416, 0.806), NEON, rot=R45)
    # side panels (graphite insets + vertical neon)
    for sx in (-1, 1):
        cbox(bm, (0.03, 0.56, 0.52), (sx*0.722, 0.0, 0.46), DARK, 0.05, 0.004)
        pill(bm, (0.025, 0.05, 0.36), (sx*0.735, -0.14, 0.46), NEON)
        for k in range(7):
            cbox(bm, (0.02, 0.24, 0.014), (sx*0.734, 0.1, 0.33 + k*0.035), METAL, 0.004, 0.0)
    # plate well ring
    add_revolve(bm, [(0.28, TOP), (0.33, TOP), (0.33, TOP + 0.012), (0.315, TOP + 0.022), (0.285, TOP + 0.022), (0.28, TOP + 0.015)],
                [METAL]*6, 16, Matrix.Translation((PC.x, PC.y, 0)) @ Matrix.Rotation(math.pi/16, 4, 'Z'), closed=True)
    # console (right) : wedge with screen + keys
    # holographic projector base (front right, outside the arm reach)
    HX, HY = EMIT_XY
    cbox(bm, (0.32, 0.26, 0.035), (HX, HY, TOP + 0.0175), DARK, 0.03, 0.004)
    pill(bm, (0.26, 0.012, 0.012), (HX, HY - 0.131, TOP + 0.02), NEON)
    cbox(bm, (0.24, 0.08, 0.006), (HX, HY - 0.07, TOP + 0.036), INNER, 0.012, 0.002)     # touch strip
    for k in range(4):
        pill(bm, (0.04, 0.03, 0.008), (HX - 0.09 + k*0.06, HY - 0.07, TOP + 0.041), ORANGE if k == 3 else WHITE)
    ML = Matrix.Translation((HX, HY + 0.05, 0))
    oct_cyl(bm, 0.05, TOP + 0.035, TOP + 0.055, METAL, ML, ch=0.01)
    oct_cyl(bm, 0.032, TOP + 0.055, TOP + 0.066, NEON, ML, ch=0.006)
    # material tank (back right corner) + pipe to the column
    T = Vector((0.65, 0.32))
    MT = Matrix.Translation((T.x, T.y, 0))
    oct_cyl(bm, 0.1, TOP, 1.46, WHITE, MT, ch=0.03)
    for z in (1.0, 1.36):
        oct_cyl(bm, 0.107, z - 0.02, z + 0.02, DARK, MT, ch=0.006)
    cbox(bm, (0.04, 0.03, 0.26), (T.x, T.y - 0.095, 1.18), DARK, 0.012, 0.003)
    pill(bm, (0.022, 0.02, 0.22), (T.x, T.y - 0.105, 1.18), NEON)
    oct_cyl(bm, 0.055, 1.46, 1.52, METAL, MT, ch=0.012)
    tube(bm, [(T.x - 0.1, T.y + 0.05, 1.25), (0.3, 0.42, 1.25), (0.0, 0.44, 1.15), (COL.x + 0.06, COL.y + 0.09, 1.08)], 0.022, DARK, METAL)
    # SCARA column (static part, wide chamfers)
    cbox(bm, (0.27, 0.27, 0.07), (COL.x, COL.y, TOP + 0.035), DARK, 0.06)
    hcol = SH_Z - 0.02 - (TOP + 0.07)
    cbox(bm, (0.17, 0.17, hcol), (COL.x, COL.y, TOP + 0.07 + hcol/2), WHITE, 0.045)
    cbox(bm, (0.18, 0.18, 0.04), (COL.x, COL.y, 1.1), DARK, 0.045, 0.004)
    cbox(bm, (0.05, 0.02, 0.2), (COL.x, COL.y - 0.083, 1.22), DARK, 0.012, 0.003)
    pill(bm, (0.02, 0.014, 0.17), (COL.x, COL.y - 0.092, 1.22), NEON)
    for sx in (-1, 1):
        for sy in (-1, 1):
            add_screw(bm, (COL.x + sx*0.09, COL.y + sy*0.09, TOP + 0.07), (0, 0, 1), 0.008)
    finish("Body", bm, (0, 0, 0), col, root, mats=MATS, sharp_deg=30)

    # ======================= BUILD PLATE (moves Z)
    P_O = (PC.x, PC.y, TOP + 0.02)
    bm = bmesh.new()
    MP = Matrix.Translation((PC.x, PC.y, 0))
    oct_cyl(bm, 0.265, TOP + 0.005, TOP + 0.035, METAL, MP, ch=0.012, sides=16)
    oct_cyl(bm, 0.24, TOP + 0.03, TOP + 0.042, PLATE, MP, ch=0.006, sides=16)
    add_revolve(bm, [(0.262, TOP + 0.012), (0.272, TOP + 0.012), (0.272, TOP + 0.028), (0.262, TOP + 0.028)], [NEON]*4, 16,
                MP @ Matrix.Rotation(math.pi/16, 4, 'Z'), closed=True)
    oct_cyl(bm, 0.07, TOP - 0.25, TOP + 0.006, METAL, MP, ch=0.0)
    finish("BuildPlate", bm, P_O, col, root, mats=MATS, sharp_deg=30)

    # ======================= ARM_SHOULDER (rot Z) : hub + link 1 (pointing -Y at rest)
    S_O = Vector((COL.x, COL.y, SH_Z))
    E_O = S_O + Vector((0, -L1, 0))
    bm = bmesh.new()
    MS = Matrix.Translation(S_O)
    cbox(bm, (0.22, 0.22, 0.13), S_O + Vector((0, 0, 0.04)), WHITE, 0.06)
    cbox(bm, (0.226, 0.226, 0.02), S_O + Vector((0, 0, -0.005)), DARK, 0.062, 0.003)
    for sx in (-1, 1):
        pill(bm, (0.012, 0.08, 0.022), S_O + Vector((sx*0.112, 0, 0.05)), NEON)
    beam(bm, S_O + Vector((0, 0, 0.055)), E_O + Vector((0, 0, 0.055)), 0.13, 0.085, WHITE, 0.035)
    beam(bm, S_O + Vector((0, -0.09, 0.1)), E_O + Vector((0, 0.08, 0.1)), 0.07, 0.012, DARK, 0.005)
    pill(bm, (0.012, 0.12, 0.01), S_O + Vector((0, -0.16, 0.107)), NEON)
    cbox(bm, (0.16, 0.16, 0.09), E_O + Vector((0, 0, 0.06)), WHITE, 0.05)
    add_screw(bm, S_O + Vector((0, 0, 0.105)), (0, 0, 1), 0.022)
    finish("Arm_Shoulder", bm, tuple(S_O), col, root, mats=MATS, sharp_deg=30)

    # ======================= ARM_ELBOW (rot Z) : hub + link 2 (lower level)
    H_O = E_O + Vector((0, -L2, -0.06))
    bm = bmesh.new()
    ME = Matrix.Translation(E_O)
    cbox(bm, (0.13, 0.13, 0.12), E_O + Vector((0, 0, -0.04)), DARK, 0.04)
    for sx in (-1, 1):
        pill(bm, (0.01, 0.06, 0.018), E_O + Vector((sx*0.066, 0, -0.04)), NEON)
    beam(bm, E_O + Vector((0, 0, -0.06)), H_O, 0.11, 0.07, WHITE, 0.03)
    beam(bm, E_O + Vector((0, -0.08, -0.022)), H_O + Vector((0, 0.07, 0.038)), 0.06, 0.012, DARK, 0.005)
    cbox(bm, (0.14, 0.14, 0.08), H_O, WHITE, 0.045)
    pill(bm, (0.018, 0.012, 0.018), E_O + Vector((0.04, -0.14, -0.022)), ORANGE)
    finish("Arm_Elbow", bm, tuple(E_O), col, mats=MATS, sharp_deg=30)

    # ======================= ARM_HEAD (slides Z) + NOZZLE + LASER
    bm = bmesh.new()
    MH = Matrix.Translation(H_O)
    cbox(bm, (0.11, 0.11, 0.2), H_O + Vector((0, 0, -0.1)), WHITE, 0.035)
    cbox(bm, (0.02, 0.07, 0.12), H_O + Vector((0.052, 0, -0.1)), DARK, 0.015, 0.002)
    pill(bm, (0.012, 0.04, 0.09), H_O + Vector((0.06, 0, -0.1)), NEON)
    cbox(bm, (0.08, 0.08, 0.02), H_O + Vector((0, 0, -0.2)), DARK, 0.025, 0.003)
    pill(bm, (0.016, 0.01, 0.016), H_O + Vector((-0.02, -0.052, -0.04)), ORANGE)
    finish("Arm_Head", bm, tuple(H_O), col, mats=MATS, sharp_deg=30)

    N_O = H_O + Vector((0, 0, -0.2))
    bm = bmesh.new()
    MN = Matrix.Translation(N_O)
    oct_cyl(bm, 0.04, -0.03, 0.0, METAL, MN, ch=0.008)
    add_revolve(bm, [(0, -0.03), (0.03, -0.03), (0.022, -0.06), (0.01, -0.085), (0.004, -0.095), (0, -0.096)], [METAL]*5, 8,
                MN @ Matrix.Rotation(math.pi/8, 4, 'Z'))
    add_revolve(bm, [(0.026, -0.036), (0.034, -0.036), (0.034, -0.046), (0.024, -0.046)], [NEON]*4, 8,
                MN @ Matrix.Rotation(math.pi/8, 4, 'Z'), closed=True)
    finish("Nozzle", bm, tuple(N_O), col, mats=MATS, sharp_deg=30)
    return dict(S_O=S_O, E_O=E_O, H_O=H_O, N_O=N_O, tip=N_O + Vector((0, 0, -0.096)))

def setup_v5():
    """clear old station objects/actions, rebuild hierarchy."""
    col = bpy.data.collections["CraftStation"]
    for n in ["Body", "Gantry_Y", "Carriage_X", "Nozzle", "BuildPlate", "Laser", "Arm_Shoulder", "Arm_Elbow", "Arm_Head"]:
        o = bpy.data.objects.get(n)
        if o: bpy.data.objects.remove(o, do_unlink=True)
    for a in list(bpy.data.actions): bpy.data.actions.remove(a)
    for m in list(bpy.data.meshes):
        if m.users == 0: bpy.data.meshes.remove(m)
    MATS = zone_mats()
    info = build_v5(MATS)
    O = bpy.data.objects
    # parenting (local offsets)
    def parent(child, par, c_o, p_o):
        ob = O[child]; ob.parent = O[par]; ob.location = Vector(c_o) - Vector(p_o)
    parent("Arm_Elbow", "Arm_Shoulder", info["E_O"], info["S_O"])
    parent("Arm_Head", "Arm_Elbow", info["H_O"], info["E_O"])
    parent("Nozzle", "Arm_Head", info["N_O"], info["H_O"])
    # laser : from nozzle tip down to plate top
    tip = info["tip"]; top = TOP + 0.042
    lm = bpy.data.materials.get("M_CraftStation_Laser") or bpy.data.materials.new("M_CraftStation_Laser")
    nt = lm.node_tree; nt.nodes.clear()
    bsdf = nt.nodes.new("ShaderNodeBsdfPrincipled"); out = nt.nodes.new("ShaderNodeOutputMaterial")
    nt.links.new(bsdf.outputs[0], out.inputs["Surface"])
    bsdf.inputs["Base Color"].default_value = (0.5, 0.9, 1, 1)
    bsdf.inputs["Emission Color"].default_value = (0.3, 0.8, 1, 1); bsdf.inputs["Emission Strength"].default_value = 8
    bm = bmesh.new()
    L = tip.z - top
    add_cyl(bm, 0.005, -L, 0.0, 0, 8)
    add_revolve(bm, [(0.004, 0.0), (0.013, -0.02), (0.013, -L + 0.01), (0.03, -L + 0.001)], [0, 0, 0], 8)
    add_cyl(bm, 0.035, -L + 0.0005, -L + 0.002, 0, 12)
    me = bpy.data.meshes.new("Laser"); bm.to_mesh(me); bm.free(); me.materials.append(lm)
    lz = bpy.data.objects.new("Laser", me); col.objects.link(lz)
    lz.parent = O["Nozzle"]; lz.location = tip - info["N_O"]
    return info

# ------------------------------------------------------------------ holograms
EMIT_XY = (0.56, -0.26)

def holo_material():
    m = bpy.data.materials.get("M_CraftStation_Hologram") or bpy.data.materials.new("M_CraftStation_Hologram")
    nt = m.node_tree; nt.nodes.clear()
    out = nt.nodes.new("ShaderNodeOutputMaterial"); mix = nt.nodes.new("ShaderNodeMixShader")
    tr = nt.nodes.new("ShaderNodeBsdfTransparent"); em = nt.nodes.new("ShaderNodeEmission")
    em.inputs["Color"].default_value = (0.05, 0.45, 1.0, 1); em.inputs["Strength"].default_value = 2.5
    mix.inputs[0].default_value = 0.6
    m.surface_render_method = "BLENDED"; m.use_backface_culling = False
    nt.links.new(tr.outputs[0], mix.inputs[1]); nt.links.new(em.outputs[0], mix.inputs[2])
    nt.links.new(mix.outputs[0], out.inputs["Surface"])
    m.diffuse_color = (0.15, 0.6, 1.0, 0.5)
    return m

def build_holo():
    col = bpy.data.collections["CraftStation"]; root = bpy.data.objects["CraftStation"]
    for n in ("Hologram", "Hologram_Ring"):
        o = bpy.data.objects.get(n)
        if o: bpy.data.objects.remove(o, do_unlink=True)
    hm = holo_material()
    HX, HY = EMIT_XY
    L0 = Vector((HX, HY + 0.05, TOP + 0.066))
    Cs = Vector((HX, HY + 0.09, 1.2))
    W, H = 0.36, 0.22
    R = Matrix.Rotation(math.radians(-12), 3, 'X')
    def sp(u, v):  # screen-plane point
        return Cs + R @ Vector((u, 0, v))
    bm = bmesh.new()
    def sbox(size, u, v, dy=0.0):
        add_box(bm, size, sp(u, v) + R @ Vector((0, dy, 0)), 0, rot=R)
    for v in (-H/2, H/2): sbox((W, 0.004, 0.008), 0, v)                 # frame only (screen is a separate object)
    for u in (-W/2, W/2): sbox((0.008, 0.004, H), u, 0)
    # projection beam (open frustum)
    b0 = [bm.verts.new(L0 + Vector((x, y, 0))) for x, y in ((-0.025, -0.025), (0.025, -0.025), (0.025, 0.025), (-0.025, 0.025))]
    b1 = [bm.verts.new(sp(u, -H/2) + R @ Vector((0, dy, 0))) for u, dy in ((-W/2, -0.01), (W/2, -0.01), (W/2, 0.01), (-W/2, 0.01))]
    for i in range(4):
        j = (i + 1) % 4
        bm.faces.new((b0[i], b0[j], b1[j], b1[i]))
    bmesh.ops.translate(bm, vec=-L0, verts=bm.verts)
    me = bpy.data.meshes.new("Hologram"); bm.to_mesh(me); bm.free(); me.materials.append(hm)
    ob = bpy.data.objects.new("Hologram", me); col.objects.link(ob); ob.parent = root; ob.location = L0
    # empty screen surface : single quad, UV 0..1, origin at its centre, front faces -Y (tilted)
    o = bpy.data.objects.get("Hologram_Screen")
    if o: bpy.data.objects.remove(o, do_unlink=True)
    sw, sh = W - 0.012, H - 0.012
    me = bpy.data.meshes.new("Hologram_Screen")
    me.from_pydata([(-sw/2, 0, -sh/2), (sw/2, 0, -sh/2), (sw/2, 0, sh/2), (-sw/2, 0, sh/2)], [], [(0, 1, 2, 3)])
    uv = me.uv_layers.new(name="UVMap")
    for li, (u, v) in zip(range(4), ((0, 0), (1, 0), (1, 1), (0, 1))): uv.data[li].uv = (u, v)
    me.materials.append(hm)                       # same blue hologram material as the frame
    holo = bpy.data.objects["Hologram"]
    ob = bpy.data.objects.new("Hologram_Screen", me); col.objects.link(ob)
    ob.parent = holo                              # child of the frame -> appears/disappears in sync
    ob.location = Cs - L0; ob.rotation_euler = (math.radians(-12), 0, 0)
    if bpy.data.materials.get("M_CraftStation_HoloScreen"): bpy.data.materials.remove(bpy.data.materials["M_CraftStation_HoloScreen"])
    # scan ring above the plate
    bm = bmesh.new()
    add_revolve(bm, [(0.27, 0.0), (0.285, 0.0), (0.285, 0.01), (0.27, 0.01)], [0]*4, 32, closed=True)
    add_revolve(bm, [(0.255, 0.004), (0.262, 0.004), (0.262, 0.006), (0.255, 0.006)], [0]*4, 32, closed=True)
    me = bpy.data.meshes.new("Hologram_Ring"); bm.to_mesh(me); bm.free(); me.materials.append(hm)
    ob = bpy.data.objects.new("Hologram_Ring", me); col.objects.link(ob); ob.parent = root
    ob.location = (PC.x, PC.y, TOP + 0.08)
    return [sp(u, v) for u in (-W/2, W/2) for v in (-H/2, H/2)]

def reach_report(holo_corners):
    reach = L1 + L2 + 0.075    # arm length + half head width
    T = Vector((0.65, 0.32))
    rep = {"reach": reach, "tank_clear": (T - COL).length - 0.1 - reach}
    rep["holo_clear"] = min((Vector((c.x, c.y)) - COL).length for c in holo_corners) - reach
    rep["emitter_clear"] = min((Vector((EMIT_XY[0] + dx, EMIT_XY[1] + dy)) - COL).length
                               for dx in (-0.16, 0.16) for dy in (-0.13, 0.13)) - reach
    return rep

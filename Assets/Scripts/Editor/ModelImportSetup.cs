#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Configuration automatique des modèles déposés dans Assets/Resources/Models/&lt;Nom&gt;/ :
///  - FBX : conversion d'axes, rig Generic, découpage des clips (CraftStation), boucle des clips *_Loop,
///          remplacement des matériaux importés par les .mat du dossier.
///  - Textures : Normal map / linéaire selon le suffixe du fichier.
///  - Crée automatiquement les matériaux (shader Standard) et l'Animator Controller de la station.
/// </summary>
public class ModelImportSetup : AssetPostprocessor
{
    const string Root = "Assets/Resources/Models/";

    // Timelines Blender (une seule prise par FBX, découpée ici) : name, first, last, loop
    static readonly System.Collections.Generic.Dictionary<string, (string name, float first, float last, bool loop)[]> ModelClips =
        new System.Collections.Generic.Dictionary<string, (string, float, float, bool)[]>
    {
        ["CraftStation"] = new (string, float, float, bool)[]
        {
            ("Power_On",    1,   49,  false),
            ("Power_Off",   60,  108, false),
            ("Print_Start", 120, 128, false),
            ("Print_Loop",  130, 322, true),
            ("Print_End",   330, 338, false),
        },
        ["Airlock"] = new (string, float, float, bool)[]
        {
            ("Outer_Open",  1,   30,  false),
            ("Outer_Close", 40,  70,  false),
            ("Inner_Open",  80,  110, false),
            ("Inner_Close", 120, 150, false),
            ("Cycle",       160, 280, false),
        },
        ["RelayAntenna"] = new (string, float, float, bool)[]
        {
            ("Dish_Deploy",     1,   37,  false),
            ("Dish_Undeploy",   50,  86,  false),
            ("Antenna_Extend",  100, 124, false),
            ("Antenna_Retract", 130, 154, false),
            ("Rotate_Loop",     170, 266, true),
        },
        ["SolarPanel"] = new (string, float, float, bool)[]
        {
            ("Panel_Deploy", 1,   72,  false),
            ("Panel_Fold",   90,  162, false),
            ("Rotate_Loop",  180, 276, true),
        },
        ["WindTurbine"] = new (string, float, float, bool)[]
        {
            ("Turbine_Deploy", 1,   96,  false),
            ("Turbine_Fold",   110, 200, false),
            ("Rotor_Spin",     220, 268, true),
        },
        ["CoalGenerator"] = new (string, float, float, bool)[]
        {
            ("Hatch_Open",  1,   24,  false),
            ("Hatch_Close", 36,  60,  false),
            ("Power_On",    72,  84,  false),
            ("Power_Off",   96,  108, false),
            ("Fan_Spin",    120, 168, true),
        },
        ["LandingCapsule"] = new (string, float, float, bool)[]
        {
            ("Door_Open",  1,  48,  false),
            ("Door_Close", 60, 108, false),
        },
        ["Drill"] = new (string, float, float, bool)[]
        {
            ("Drill_Lower", 1,   48,  false),
            ("Drill_Raise", 60,  108, false),
            ("Drill_Spin",  120, 168, true),
        },
        ["Crate_Small"] = new (string, float, float, bool)[]
        {
            ("Lid_Open",  1,  24, false),
            ("Lid_Close", 36, 60, false),
        },
        ["Crate_Large"] = new (string, float, float, bool)[]
        {
            ("Lid_Open",  1,  30, false),
            ("Lid_Close", 42, 72, false),
        },
        ["Cabinet"] = new (string, float, float, bool)[]
        {
            ("Doors_Open",  1,  30, false),
            ("Doors_Close", 42, 72, false),
        },
    };
    // état par défaut de l'Animator Controller et transitions automatiques (from -> to, sur fin de clip)
    static readonly System.Collections.Generic.Dictionary<string, string> DefaultState =
        new System.Collections.Generic.Dictionary<string, string> { ["CraftStation"] = "Power_Off", ["Airlock"] = "Inner_Close", ["SolarPanel"] = "Panel_Fold", ["LandingCapsule"] = "Door_Close",
                                                                    ["Crate_Small"] = "Lid_Close", ["Crate_Large"] = "Lid_Close", ["Cabinet"] = "Doors_Close" };
    static readonly (string model, string from, string to)[] AutoTransitions = { ("CraftStation", "Print_Start", "Print_Loop") };

    // Modèles à mouvements indépendants : chaque clip ne garde que les courbes de ses objets,
    // et l'Animator Controller a une couche par groupe (Write Defaults désactivé).
    static readonly System.Collections.Generic.Dictionary<string, (string layer, string[] targets, string[] clips, string defaultClip)[]> ModelLayers =
        new System.Collections.Generic.Dictionary<string, (string, string[], string[], string)[]>
    {
        ["RelayAntenna"] = new (string, string[], string[], string)[]
        {
            ("Dish",     new[] { "Arm" },                  new[] { "Dish_Undeploy", "Dish_Deploy" },       "Dish_Undeploy"),
            ("Mast",     new[] { "Mast_Mid", "Mast_Top" }, new[] { "Antenna_Retract", "Antenna_Extend" }, "Antenna_Retract"),
            ("Rotation", new[] { "Turret" },               new[] { "Rotate_Loop" },                        null),   // null : état "Idle" vide
        },
        ["SolarPanel"] = new (string, string[], string[], string)[]
        {
            ("Panel",    new[] { "Panel_Tilt", "Wing_L", "Wing_R" }, new[] { "Panel_Fold", "Panel_Deploy" }, "Panel_Fold"),
            ("Rotation", new[] { "Panel_Yaw" },                       new[] { "Rotate_Loop" },               null),
        },
        ["WindTurbine"] = new (string, string[], string[], string)[]
        {
            ("Deploy", new[] { "Mast_Mid", "Mast_Top", "Blade_1", "Blade_2", "Blade_3" }, new[] { "Turbine_Fold", "Turbine_Deploy" }, "Turbine_Fold"),
            ("Rotor",  new[] { "Rotor" },                                                new[] { "Rotor_Spin" },                     null),   // param RotorSpeed
        },
        ["CoalGenerator"] = new (string, string[], string[], string)[]
        {
            ("Hatch", new[] { "Hatch" }, new[] { "Hatch_Close", "Hatch_Open" }, "Hatch_Close"),
            ("Power", new[] { "Lever" }, new[] { "Power_Off", "Power_On" },     "Power_Off"),
            ("Fan",   new[] { "Fan" },   new[] { "Fan_Spin" },                  null),   // param FanSpeed
        },
        ["Drill"] = new (string, string[], string[], string)[]
        {
            ("Feed", new[] { "Carriage" },   new[] { "Drill_Raise", "Drill_Lower" }, "Drill_Raise"),
            ("Spin", new[] { "Drill_Head" }, new[] { "Drill_Spin" },                 null),   // param SpinSpeed
        },
    };

    // à incrémenter quand les réglages d'import changent : Unity réimporte alors les modèles concernés
    public override uint GetVersion() => 7;

    static bool InRoot(string p) => p.Replace('\\', '/').StartsWith(Root);
    static bool IsModelFolder(string dir) => dir.Replace('\\', '/').TrimEnd('/') + "/" != Root;   // pas la racine Models/

    // ---------------------------------------------------------------- textures
    void OnPreprocessTexture()
    {
        if (!InRoot(assetPath)) return;
        var ti = (TextureImporter)assetImporter;
        string n = Path.GetFileNameWithoutExtension(assetPath);
        ti.maxTextureSize = 2048;
        if (n.EndsWith("_Normal")) ti.textureType = TextureImporterType.NormalMap;
        else { ti.textureType = TextureImporterType.Default; ti.sRGBTexture = !(n.EndsWith("_MetallicSmoothness") || n.EndsWith("_Occlusion")); }
    }

    // ---------------------------------------------------------------- models
    void OnPreprocessModel()
    {
        if (!InRoot(assetPath) || !assetPath.ToLower().EndsWith(".fbx")) return;
        var mi = (ModelImporter)assetImporter;
        // bakeAxisConversion n'est pas appliqué aux courbes d'animation (bug Unity : le modèle bascule de 90°).
        // Pour TOUS les modèles : conversion d'axes gardée sur le nœud racine Blender (enfant), et
        // preserveHierarchy garantit une racine de prefab neutre (rotation 0, échelle 1).
        string modelName = Path.GetFileNameWithoutExtension(assetPath);
        bool animated = ModelClips.ContainsKey(modelName);
        mi.bakeAxisConversion = false;
        mi.preserveHierarchy = true;
        mi.animationType = animated ? ModelImporterAnimationType.Generic : ModelImporterAnimationType.None;
        mi.importAnimation = animated;      // modèles statiques : aucune prise d'animation parasite
        mi.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        // Dôme : mesh lisible au runtime (découpe du mur Dome_Wall pour les connexions)
        if (modelName.StartsWith("Dome_")) mi.isReadable = true;     // Dome_8m / Dome_12m / Dome_16m
        // Tunnel droit et extension : meshes lisibles au runtime (cisaillés en pente, CorridorPieces)
        if (modelName == "Tunnel_Straight" || modelName == "Tunnel_Extension") mi.isReadable = true;
        string dir = Path.GetDirectoryName(assetPath).Replace('\\', '/');
        foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { dir }))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetDirectoryName(p).Replace('\\', '/') != dir) continue;   // FindAssets est récursif : seulement ce dossier
            var mat = AssetDatabase.LoadAssetAtPath<Material>(p);
            if (mat != null) mi.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), mat.name), mat);
        }
    }

    /// <summary>Modèles à couches : retire de chaque clip les courbes des objets qui ne lui appartiennent pas.</summary>
    void OnPostprocessAnimation(GameObject root, AnimationClip clip)
    {
        if (!InRoot(assetPath) || !ModelLayers.TryGetValue(Path.GetFileNameWithoutExtension(assetPath), out var layers)) return;
        var group = layers.FirstOrDefault(l => l.clips.Contains(clip.name));
        if (group.clips == null) return;
        foreach (var b in AnimationUtility.GetCurveBindings(clip))
        {
            string leaf = b.path.Contains("/") ? b.path.Substring(b.path.LastIndexOf('/') + 1) : b.path;
            if (!group.targets.Contains(leaf)) AnimationUtility.SetEditorCurve(clip, b, null);
        }
    }

    void OnPreprocessAnimation()
    {
        if (!InRoot(assetPath)) return;
        var mi = (ModelImporter)assetImporter;
        bool hasTable = ModelClips.TryGetValue(Path.GetFileNameWithoutExtension(assetPath), out var table);
        bool hasClips = mi.clipAnimations != null && mi.clipAnimations.Length > 0;
        // modèles avec table : la table fait foi (plages Blender) ; autres : garde les réglages manuels
        if (hasClips && !hasTable) return;
        var takes = mi.defaultClipAnimations;
        if (takes == null || takes.Length == 0) return;

        if (hasTable)
        {
            string take = takes[0].takeName;
            mi.clipAnimations = table.Select(c => new ModelImporterClipAnimation
            { name = c.name, takeName = take, firstFrame = c.first, lastFrame = c.last, loopTime = c.loop }).ToArray();
        }
        else
        {
            foreach (var t in takes)
            {
                t.name = t.takeName.Contains("|") ? t.takeName.Substring(t.takeName.LastIndexOf('|') + 1) : t.takeName;
                t.loopTime = t.name.EndsWith("_Loop");
            }
            mi.clipAnimations = takes;
        }
    }

    // ---------------------------------------------------------------- materials / controller
    static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        var dirs = imported.Concat(moved).Where(InRoot)
            .Select(p => Path.GetDirectoryName(p).Replace('\\', '/')).Distinct().ToArray();
        if (dirs.Length == 0) return;
        EditorApplication.delayCall += () => { foreach (var d in dirs) SetupFolder(d); };
    }

    static void SetupFolder(string dir)
    {
        if (!IsModelFolder(dir)) return;      // fichiers posés directement dans Models/ : ignorés
        string folder = Path.GetFileName(dir);
        // un dossier = un modèle (Dome/Dome.fbx) ou un kit de pièces partageant leurs matériaux (Tunnel/Tunnel_*.fbx)
        var fbxs = Directory.GetFiles(dir, "*.fbx").Select(p => p.Replace('\\', '/'))
            .Where(p => AssetDatabase.LoadAssetAtPath<GameObject>(p) != null).ToArray();
        if (fbxs.Length == 0) return;

        bool created = CreateLitMaterial(dir, folder);
        foreach (var fbx in fbxs) created |= CreateFlatMaterials(dir, fbx);
        if (created)
            foreach (var fbx in fbxs) AssetDatabase.ImportAsset(fbx, ImportAssetOptions.ForceUpdate); // applique le remap

        foreach (var fbx in fbxs)
        {
            string model = Path.GetFileNameWithoutExtension(fbx);
            if (ModelClips.ContainsKey(model) && AssetDatabase.LoadAssetAtPath<AnimatorController>($"{dir}/{model}.controller") == null)
                CreateController(dir, fbx, model);
        }
    }

    static Texture2D Tex(string dir, string prefix, params string[] suffixes)
    {
        foreach (var s in suffixes)
        {
            var t = AssetDatabase.LoadAssetAtPath<Texture2D>($"{dir}/{prefix}{s}.png");
            if (t != null) return t;
        }
        return null;
    }

    static bool CreateLitMaterial(string dir, string prefix)
    {
        string path = $"{dir}/M_{prefix}.mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return false;
        var baseMap = Tex(dir, prefix, "_BaseColor_On", "_BaseColor");
        if (baseMap == null) return false;

        var m = new Material(Shader.Find("Standard")) { name = "M_" + prefix };
        m.SetTexture("_MainTex", baseMap);
        m.SetColor("_Color", Color.white);
        var ms = Tex(dir, prefix, "_MetallicSmoothness");
        if (ms) { m.SetTexture("_MetallicGlossMap", ms); m.SetFloat("_GlossMapScale", 1f); m.SetFloat("_SmoothnessTextureChannel", 0f); m.EnableKeyword("_METALLICGLOSSMAP"); }
        var nm = Tex(dir, prefix, "_Normal");
        if (nm) { m.SetTexture("_BumpMap", nm); m.SetFloat("_BumpScale", 1f); m.EnableKeyword("_NORMALMAP"); }
        var oc = Tex(dir, prefix, "_Occlusion");
        if (oc) { m.SetTexture("_OcclusionMap", oc); m.SetFloat("_OcclusionStrength", 1f); }
        var em = Tex(dir, prefix, "_Emission_On", "_Emission");
        if (em)
        {
            m.SetTexture("_EmissionMap", em);
            m.SetColor("_EmissionColor", Color.white * 2.5f);
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        AssetDatabase.CreateAsset(m, path);
        return true;
    }

    // Matériaux unis (modèle sans textures) : suffixe du nom de matériau Blender -> réglages Standard
    // (couleur, metallic, smoothness, couleur d'émission HDR)
    static readonly (string suffix, Color color, float metal, float gloss, Color emission)[] FlatPalette =
    {
        ("_White",  new Color(0.82f, 0.85f, 0.88f), 0.0f, 0.45f, Color.black),
        ("_Dark",   new Color(0.06f, 0.07f, 0.09f), 0.2f, 0.5f,  Color.black),
        ("_Metal",  new Color(0.30f, 0.33f, 0.38f), 0.6f, 0.55f, Color.black),
        ("_Plate",  new Color(0.60f, 0.66f, 0.74f), 0.5f, 0.7f,  Color.black),
        ("_Neon",   new Color(0.60f, 0.85f, 1.00f), 0.0f, 0.8f,  new Color(0.0f, 0.55f, 1.0f) * 3f),
        ("_Orange", new Color(1.00f, 0.45f, 0.10f), 0.0f, 0.7f,  new Color(1.0f, 0.35f, 0.0f) * 2f),
        ("_Glow",   new Color(0.10f, 0.40f, 0.90f), 0.0f, 0.8f,  new Color(0.05f, 0.35f, 1.0f) * 1.5f),
        ("_Laser",  new Color(0.30f, 0.85f, 1.00f), 0.0f, 0.9f,  new Color(0.3f, 0.85f, 1.0f) * 4f),
        ("_Hazard", new Color(0.95f, 0.60f, 0.04f), 0.0f, 0.5f,  Color.black),
        ("_Cell",   new Color(0.03f, 0.08f, 0.22f), 0.3f, 0.85f, Color.black),                  // cellules solaires
        ("_Status", new Color(1.00f, 0.45f, 0.05f), 0.0f, 0.7f,  new Color(1.0f, 0.3f, 0.0f) * 2.5f),   // couleur pilotée par script
        ("_Fire",   new Color(1.00f, 0.35f, 0.08f), 0.0f, 0.6f,  new Color(1.0f, 0.28f, 0.02f) * 3f),   // foyer (intensité pilotée par script)
        // (_Glass et _Hologram : cas transparents gérés dans CreateFlatMaterials)
    };

    /// <summary>Crée un .mat pour chaque matériau du FBX nommé M_&lt;...&gt;_&lt;Suffixe&gt; connu (si absent).</summary>
    static bool CreateFlatMaterials(string dir, string fbx)
    {
        bool created = false;
        var names = AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<Material>().Select(m => m.name).Distinct().ToArray();
        foreach (var name in names)
        {
            string path = $"{dir}/{name}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) continue;
            Material m = null;
            if (name.EndsWith("_Glass"))
            {
                m = new Material(Shader.Find("Standard")) { name = name };   // vitre : Standard en mode Transparent
                m.SetColor("_Color", new Color(0.25f, 0.6f, 0.8f, 0.3f));
                m.SetFloat("_Metallic", 0f); m.SetFloat("_Glossiness", 0.95f);
                m.SetFloat("_Mode", 3); m.SetInt("_SrcBlend", 1); m.SetInt("_DstBlend", 10); m.SetInt("_ZWrite", 0);
                m.EnableKeyword("_ALPHAPREMULTIPLY_ON"); m.renderQueue = 3000;
            }
            else if (name.EndsWith("_Hologram"))
            {
                var holo = Shader.Find("Custom/Hologram");
                if (holo != null)
                {
                    m = new Material(holo) { name = name };
                    if (m.HasProperty("_Color")) m.SetColor("_Color", new Color(0.2f, 0.6f, 1f, 0.3f));
                }
                else
                {
                    m = new Material(Shader.Find("Standard")) { name = name };   // fallback : Standard en mode Fade
                    m.SetColor("_Color", new Color(0.1f, 0.5f, 1f, 0.35f));
                    m.SetFloat("_Mode", 2); m.SetInt("_SrcBlend", 5); m.SetInt("_DstBlend", 10); m.SetInt("_ZWrite", 0);
                    m.EnableKeyword("_ALPHABLEND_ON"); m.renderQueue = 3000;
                    m.SetColor("_EmissionColor", new Color(0.05f, 0.45f, 1f) * 1.5f); m.EnableKeyword("_EMISSION");
                }
            }
            else
            {
                var p = FlatPalette.FirstOrDefault(e => name.EndsWith(e.suffix));
                if (p.suffix == null) continue;
                m = new Material(Shader.Find("Standard")) { name = name };
                m.SetColor("_Color", p.color);
                m.SetFloat("_Metallic", p.metal);
                m.SetFloat("_Glossiness", p.gloss);
                if (p.emission.maxColorComponent > 0f)
                {
                    m.SetColor("_EmissionColor", p.emission);
                    m.EnableKeyword("_EMISSION");
                    m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                }
            }
            AssetDatabase.CreateAsset(m, path);
            created = true;
        }
        return created;
    }

    static void CreateController(string dir, string fbx, string model)
    {
        var table = ModelClips[model];
        var clips = AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<AnimationClip>()
            .Where(c => !c.name.StartsWith("__preview__")).ToDictionary(c => c.name);
        if (!table.All(c => clips.ContainsKey(c.name))) return; // clips pas encore importés

        var ctrl = AnimatorController.CreateAnimatorControllerAtPath($"{dir}/{model}.controller");
        if (ModelLayers.TryGetValue(model, out var groups))
        {
            // une couche par groupe, Write Defaults off : chaque couche ne touche que ses objets
            for (int i = 0; i < groups.Length; i++)
            {
                var g = groups[i];
                if (i == 0) { var ls = ctrl.layers; ls[0].name = g.layer; ctrl.layers = ls; }
                else ctrl.AddLayer(g.layer);
                var lsm = ctrl.layers[i].stateMachine;
                AnimatorState layerDefault = null;
                if (g.defaultClip == null) { layerDefault = lsm.AddState("Idle"); layerDefault.writeDefaultValues = false; }
                foreach (var cn in g.clips)
                {
                    var lst = lsm.AddState(cn); lst.motion = clips[cn]; lst.writeDefaultValues = false;
                    if (cn == g.defaultClip) layerDefault = lst;
                    if (clips[cn].isLooping || cn.EndsWith("_Loop"))   // vitesse réglable : paramètre <Couche>Speed
                    {
                        string p = g.layer + "Speed";
                        if (!ctrl.parameters.Any(x => x.name == p))
                            ctrl.AddParameter(new AnimatorControllerParameter { name = p, type = AnimatorControllerParameterType.Float, defaultFloat = 1f });
                        lst.speedParameterActive = true; lst.speedParameter = p;
                    }
                }
                lsm.defaultState = layerDefault;
            }
            var layers = ctrl.layers;
            for (int i = 0; i < layers.Length; i++) layers[i].defaultWeight = 1f;
            ctrl.layers = layers;
            AssetDatabase.SaveAssets();
            return;
        }
        var sm = ctrl.layers[0].stateMachine;
        var states = new System.Collections.Generic.Dictionary<string, AnimatorState>();
        foreach (var c in table)
        {
            var st = sm.AddState(c.name); st.motion = clips[c.name];
            states[c.name] = st;
        }
        if (DefaultState.TryGetValue(model, out var defaultName) && states.TryGetValue(defaultName, out var ds)) sm.defaultState = ds;
        foreach (var (m, from, to) in AutoTransitions)
        {
            if (m != model || !states.ContainsKey(from) || !states.ContainsKey(to)) continue;
            var t = states[from].AddTransition(states[to]); t.hasExitTime = true; t.exitTime = 1f; t.duration = 0f;
        }
        AssetDatabase.SaveAssets();
    }
}
#endif

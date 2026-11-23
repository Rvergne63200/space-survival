using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor previews of the terrain shape (TerrainShape), to tune the generation without playing:
/// a top-down shaded map of a large area, and 3D renders of the same area with the terrain material and layers.
/// Menu Tools > Terrain Preview, or called from code (Map / Render3D).
/// </summary>
public static class TerrainPreview
{
    const string OutDir = "ArtPipeline/Renders";
    public const string SettingsPath = "Assets/Resources/" + TerrainShapeSettings.ResourcePath + ".asset";

    [MenuItem("Tools/Terrain Preview/Map 4 km")]
    static void MenuMap() => Debug.Log(Map(1234u, 4096f, 1024, LoadSettings()));

    [MenuItem("Tools/Terrain Preview/3D 1 km")]
    static void Menu3D() => Debug.Log(Render3D(1234u, 1024f, LoadSettings()));

    /// <summary>The settings asset of the game, or the default settings if it does not exist.</summary>
    public static TerrainShapeSettings LoadSettings()
    {
        var s = AssetDatabase.LoadAssetAtPath<TerrainShapeSettings>(SettingsPath);
        return s != null ? s : ScriptableObject.CreateInstance<TerrainShapeSettings>();
    }

    static TerrainSample[,] SampleGrid(uint seed, float size, int res, TerrainShapeSettings settings, out float minH, out float maxH)
    {
        var shape = new TerrainShape(settings, seed);
        var g = new TerrainSample[res, res];
        minH = float.MaxValue; maxH = float.MinValue;
        float step = size / (res - 1);
        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                var t = shape.Sample(x * step - size / 2f, y * step - size / 2f);
                g[y, x] = t;
                minH = Mathf.Min(minH, t.height); maxH = Mathf.Max(maxH, t.height);
            }
        }
        return g;
    }

    /// <summary>Top-down map : ground colour from the masks, lit by a low sun from the north-west.</summary>
    public static string Map(uint seed, float size, int res, TerrainShapeSettings s, string file = "Terrain_map.png")
    {
        var g = SampleGrid(seed, size, res, s, out float minH, out float maxH);
        float step = size / (res - 1);
        Color rust = new Color(0.71f, 0.34f, 0.18f), ochre = new Color(0.75f, 0.45f, 0.27f);
        Color dust = new Color(0.40f, 0.26f, 0.21f), sediment = new Color(0.81f, 0.68f, 0.53f);
        Vector3 sun = new Vector3(-0.6f, 0.55f, 0.6f).normalized;
        float sand = 0f, sed = 0f, rock = 0f;
        var tex = new Texture2D(res, res, TextureFormat.RGB24, false);
        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                var t = g[y, x];
                sand += t.sand; sed += t.sediment; rock += t.rock;
                float hx = g[y, Mathf.Min(x + 1, res - 1)].height - g[y, Mathf.Max(x - 1, 0)].height;
                float hz = g[Mathf.Min(y + 1, res - 1), x].height - g[Mathf.Max(y - 1, 0), x].height;
                Vector3 n = new Vector3(-hx, 2f * step, -hz).normalized;
                float light = 0.35f + 0.75f * Mathf.Max(0f, Vector3.Dot(n, sun));
                Color c = rust;
                c = Color.Lerp(c, ochre, t.sand);
                c = Color.Lerp(c, sediment, t.sediment);
                c = Color.Lerp(c, dust, t.rock * 0.8f);
                tex.SetPixel(x, y, c * light);
            }
        }
        tex.Apply();
        string path = Path.Combine(OutDir, file);
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        float pct = res * res / 100f;
        return $"{path} height {minH:0}..{maxH:0} m, sand {sand / pct:0}% sediment {sed / pct:0}% rock {rock / pct:0}%";
    }

    /// <summary>3D renders of an area, with the terrain material of the Chunk prefab and the 4 ground layers.</summary>
    public static string Render3D(uint seed, float size, TerrainShapeSettings s, string prefix = "Terrain_3d")
    {
        int res = 1025;
        var g = SampleGrid(seed, size, res, s, out float minH, out float maxH);
        float range = Mathf.Max(1f, maxH - minH);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/Terrain/Chunk.prefab");
        var src = prefab.GetComponentInChildren<Terrain>();
        string[] zones = { "Rust", "Ochre", "Dust", "Sediment" };
        var layers = new TerrainLayer[zones.Length];
        for (int i = 0; i < zones.Length; i++)
            layers[i] = AssetDatabase.LoadAssetAtPath<TerrainLayer>($"Assets/Terrain/Ground_{zones[i]}.terrainlayer");

        var data = new TerrainData { heightmapResolution = res };
        data.size = new Vector3(size, range, size);
        var heights = new float[res, res];
        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
                heights[y, x] = (g[y, x].height - minH) / range;
        data.SetHeights(0, 0, heights);
        data.terrainLayers = layers;
        int ares = 512;
        data.alphamapResolution = ares;
        var alpha = new float[ares, ares, 4];
        for (int y = 0; y < ares; y++)
        {
            for (int x = 0; x < ares; x++)
            {
                var t = g[y * (res - 1) / (ares - 1), x * (res - 1) / (ares - 1)];
                float sand = t.sand, sed = t.sediment, rock = t.rock;
                float rust = Mathf.Max(0f, 1f - sand - sed - rock);
                float sum = rust + sand + rock + sed + 1e-4f;
                alpha[y, x, 0] = rust / sum; alpha[y, x, 1] = sand / sum; alpha[y, x, 2] = rock / sum; alpha[y, x, 3] = sed / sum;
            }
        }
        data.SetAlphamaps(0, 0, alpha);

        Scene scene = EditorSceneManager.NewPreviewScene();
        string result = "";
        try
        {
            var go = new GameObject("Terrain");
            SceneManager.MoveGameObjectToScene(go, scene);
            var terrain = go.AddComponent<Terrain>();
            terrain.terrainData = data;
            terrain.materialTemplate = src.materialTemplate;
            terrain.drawInstanced = src.drawInstanced;
            terrain.basemapDistance = 5000f;
            terrain.heightmapPixelError = 2f;
            go.transform.position = new Vector3(-size / 2f, 0f, -size / 2f);
            Shader.SetGlobalVector("_WorldUVTerrainSize", new Vector4(size, size, 0, 0));

            var lgo = new GameObject("Sun");
            SceneManager.MoveGameObjectToScene(lgo, scene);
            var light = lgo.AddComponent<Light>();
            light.type = LightType.Directional; light.intensity = 1.25f; light.color = new Color(1f, 0.95f, 0.88f);
            light.shadows = LightShadows.Soft;
            lgo.transform.rotation = Quaternion.Euler(28f, 40f, 0f);

            var cgo = new GameObject("Camera");
            SceneManager.MoveGameObjectToScene(cgo, scene);
            var cam = cgo.AddComponent<Camera>();
            cam.scene = scene; cam.farClipPlane = 6000f; cam.fieldOfView = 55f;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.78f, 0.55f, 0.40f);
            RenderSettings.fog = false;

            float H(float wx, float wz) => terrain.SampleHeight(new Vector3(wx, 0, wz));
            // Steepest point of the area, seen from 15 m downhill : to check the textures on the cliffs
            Vector3 cliff = SteepestPoint(g, size, res, out Vector3 downhill);
            Vector3 cliffCam = cliff + downhill * 15f;
            var views = new (string name, Vector3 pos, float lift, Vector3 look, float lookLift)[]
            {
                ("high", new Vector3(0f, 0f, -size * 0.55f), size * 0.28f, new Vector3(0f, 0f, size * 0.1f), 0f),
                ("eye", new Vector3(-size * 0.1f, 0f, -size * 0.2f), 1.8f, new Vector3(size * 0.05f, 0f, size * 0.15f), 1.5f),
                ("cliff", cliffCam, 1.8f, cliff, 4f),
            };
            foreach (var v in views)
            {
                cgo.transform.position = new Vector3(v.pos.x, H(v.pos.x, v.pos.z) + v.lift, v.pos.z);
                cgo.transform.LookAt(new Vector3(v.look.x, H(v.look.x, v.look.z) + v.lookLift, v.look.z));
                var rt = new RenderTexture(1600, 900, 24);
                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;
                var tex = new Texture2D(1600, 900, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
                tex.Apply();
                string path = Path.Combine(OutDir, $"{prefix}_{v.name}.png");
                File.WriteAllBytes(path, tex.EncodeToPNG());
                RenderTexture.active = null; cam.targetTexture = null;
                Object.DestroyImmediate(rt); Object.DestroyImmediate(tex);
                result += path + " ";
            }
        }
        finally
        {
            EditorSceneManager.ClosePreviewScene(scene);
            Object.DestroyImmediate(data);
        }
        return result + $"height {minH:0}..{maxH:0} m";
    }

    // World position (centred area) of the steepest sample of the grid (away from the edges), and the downhill direction
    static Vector3 SteepestPoint(TerrainSample[,] g, float size, int res, out Vector3 downhill)
    {
        float step = size / (res - 1), best = -1f;
        Vector3 point = Vector3.zero;
        downhill = Vector3.back;
        for (int y = res / 16; y < res * 15 / 16; y++)
        {
            for (int x = res / 16; x < res * 15 / 16; x++)
            {
                float gx = (g[y, x + 1].height - g[y, x - 1].height) / (2f * step);
                float gz = (g[y + 1, x].height - g[y - 1, x].height) / (2f * step);
                float slope = gx * gx + gz * gz;
                if (slope <= best) continue;
                best = slope;
                point = new Vector3(x * step - size / 2f, 0f, y * step - size / 2f);
                downhill = new Vector3(-gx, 0f, -gz).normalized;
            }
        }
        return point;
    }
}

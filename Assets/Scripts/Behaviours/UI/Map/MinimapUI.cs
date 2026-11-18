using UnityEngine;
using UnityEngine.UIElements;

// Round minimap of the HUD (element "minimap" of Hud.uxml), turning with the player: what is in front of the player is
// at the top. An orthographic camera above the player renders only the terrain and the buildings (layers Terrain and
// Building) into a texture; the player is not rendered, an arrow at the center replaces it
public class MinimapUI : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    // Distance shown from the center to the edge (m)
    [SerializeField] private float radius = 60f;
    [SerializeField] private int resolution = 512;
    // Height of the camera above the player (m)
    [SerializeField] private float height = 300f;
    [SerializeField] private Color backgroundColor = new Color(0.05f, 0.07f, 0.1f, 1f);

    private Camera mapCamera;
    private CameraMovement view;
    private RenderTexture texture;

    private VisualElement boundRoot;
    private VisualElement minimap;
    private VisualElement image;
    private VisualElement north;

    private void Awake()
    {
        texture = new RenderTexture(resolution, resolution, 16) { name = "Minimap" };

        GameObject cameraObject = new GameObject("MinimapCamera");
        DontDestroyOnLoad(cameraObject);

        mapCamera = cameraObject.AddComponent<Camera>();
        mapCamera.orthographic = true;
        mapCamera.orthographicSize = radius;
        mapCamera.nearClipPlane = 1f;
        mapCamera.farClipPlane = height + 1000f;
        mapCamera.cullingMask = LayerMask.GetMask("Terrain", "Building");
        mapCamera.clearFlags = CameraClearFlags.SolidColor;
        mapCamera.backgroundColor = backgroundColor;
        mapCamera.allowHDR = false;
        mapCamera.allowMSAA = false;
        mapCamera.useOcclusionCulling = false;
        mapCamera.depth = -10f;
        mapCamera.targetTexture = texture;
        mapCamera.enabled = false;
    }

    private void OnDestroy()
    {
        if (mapCamera != null)
        {
            Destroy(mapCamera.gameObject);
        }

        if (texture != null)
        {
            texture.Release();
            Destroy(texture);
        }
    }

    // The UIDocument builds its elements again each time it is enabled
    private bool Bind()
    {
        if (document == null || document.rootVisualElement == null) return false;

        VisualElement root = document.rootVisualElement;

        if (root == boundRoot && minimap != null) return true;

        boundRoot = root;
        minimap = root.Q("minimap");
        image = root.Q("minimap-image");
        north = root.Q("minimap-north");

        if (image != null)
        {
            image.style.backgroundImage = Background.FromRenderTexture(texture);
        }

        VisualElement arrow = root.Q("minimap-arrow");

        if (arrow != null)
        {
            arrow.generateVisualContent += DrawArrow;
        }

        return minimap != null;
    }

    private static void DrawArrow(MeshGenerationContext context)
    {
        Rect rect = context.visualElement.contentRect;
        Painter2D painter = context.painter2D;

        painter.fillColor = new Color(1f, 0.85f, 0.2f);
        painter.strokeColor = new Color(0f, 0f, 0f, 0.8f);
        painter.lineWidth = 1.5f;
        painter.BeginPath();
        painter.MoveTo(new Vector2(rect.width * 0.5f, 0f));
        painter.LineTo(new Vector2(rect.width, rect.height));
        painter.LineTo(new Vector2(rect.width * 0.5f, rect.height * 0.75f));
        painter.LineTo(new Vector2(0f, rect.height));
        painter.ClosePath();
        painter.Fill();
        painter.Stroke();
    }

    private void LateUpdate()
    {
        if (!Bind()) return;

        GameObject player = PlayerManager.Instance != null ? PlayerManager.Instance.Player : null;
        bool shown = player != null && !PlayerManager.Instance.IsDead;

        minimap.EnableInClassList("hidden", !shown);

        if (!shown) return;

        // Direction of the view: the body of the player only turns with it while moving. The player camera is not
        // tagged MainCamera, it is found by its CameraMovement
        if (view == null)
        {
            view = FindFirstObjectByType<CameraMovement>();
        }

        float yaw = view != null ? view.transform.eulerAngles.y : player.transform.eulerAngles.y;

        mapCamera.orthographicSize = radius;
        mapCamera.transform.SetPositionAndRotation(player.transform.position + Vector3.up * height, Quaternion.Euler(90f, yaw, 0f));
        mapCamera.Render();

        // North on the edge of the map: turned by -yaw from the top (clockwise)
        if (north != null)
        {
            float halfSize = minimap.resolvedStyle.width * 0.5f;
            float angle = -yaw * Mathf.Deg2Rad;
            float distance = halfSize - 12f;

            north.style.left = halfSize + Mathf.Sin(angle) * distance;
            north.style.top = halfSize - Mathf.Cos(angle) * distance;
        }
    }
}

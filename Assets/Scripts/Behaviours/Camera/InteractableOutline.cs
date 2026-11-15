using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Draws a screen space outline around the interactable object pointed by the player
[RequireComponent(typeof(Camera))]
public class InteractableOutline : MonoBehaviour
{
    private const int MASK_PASS = 0;
    private const int OUTLINE_PASS = 1;
    private const CameraEvent CAMERA_EVENT = CameraEvent.BeforeImageEffects;

    private static readonly int MaskId = Shader.PropertyToID("_InteractableOutlineMask");
    private static readonly int ColorId = Shader.PropertyToID("_OutlineColor");
    private static readonly int WidthId = Shader.PropertyToID("_OutlineWidth");

    [SerializeField] private Shader outlineShader;
    public Color outlineColor = Color.white;
    public float outlineWidth = 3f;

    private new Camera camera;
    private CameraContext context;
    private CommandBuffer commandBuffer;
    private Material material;
    private RenderTextureFormat maskFormat;

    private Transform target;
    private PlayerContext targetContext;
    private Transform outlinedObject;
    private readonly List<Renderer> renderers = new List<Renderer>();

    void Awake()
    {
        camera = GetComponent<Camera>();
        context = GetComponent<CameraContext>();

        if (outlineShader == null)
        {
            Debug.LogError("Interactable outline shader is empty.");
            enabled = false;
            return;
        }

        material = new Material(outlineShader) { hideFlags = HideFlags.HideAndDontSave };
        commandBuffer = new CommandBuffer { name = "Interactable outline" };

        maskFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8)
            ? RenderTextureFormat.R8
            : RenderTextureFormat.ARGB32;
    }

    private void OnEnable()
    {
        camera.AddCommandBuffer(CAMERA_EVENT, commandBuffer);
    }

    private void OnDisable()
    {
        camera.RemoveCommandBuffer(CAMERA_EVENT, commandBuffer);
    }

    private void OnDestroy()
    {
        commandBuffer?.Release();

        if (material != null)
        {
            Destroy(material);
        }
    }

    void LateUpdate()
    {
        Transform pointed = GetPointedInteractable();

        if (pointed != outlinedObject || HasDestroyedRenderer())
        {
            outlinedObject = pointed;
            CollectRenderers();
            BuildCommandBuffer();
        }

        // Read when the command buffer executes, so changes in the inspector apply directly
        material.SetColor(ColorId, outlineColor);
        material.SetFloat(WidthId, outlineWidth);
    }

    private Transform GetPointedInteractable()
    {
        Transform currentTarget = context != null && context.CameraManager != null ? context.CameraManager.Target : null;

        // The player is respawned on each scene load, so the cached context must follow the target
        if (currentTarget != target)
        {
            target = currentTarget;
            targetContext = target != null ? target.GetComponent<PlayerContext>() : null;
        }

        if (targetContext == null || targetContext.PointedObject == null)
        {
            return null;
        }

        Component interactable = targetContext.Interractable as Component;

        return interactable != null ? interactable.transform : null;
    }

    private bool HasDestroyedRenderer()
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
            {
                return true;
            }
        }

        return false;
    }

    private void CollectRenderers()
    {
        renderers.Clear();

        if (outlinedObject == null)
        {
            return;
        }

        foreach (Renderer renderer in outlinedObject.GetComponentsInChildren<Renderer>())
        {
            if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
            {
                renderers.Add(renderer);
            }
        }
    }

    private void BuildCommandBuffer()
    {
        commandBuffer.Clear();

        if (renderers.Count == 0)
        {
            return;
        }

        commandBuffer.GetTemporaryRT(MaskId, -1, -1, 0, FilterMode.Bilinear, maskFormat);
        commandBuffer.SetRenderTarget(MaskId);
        commandBuffer.ClearRenderTarget(false, true, Color.clear);

        foreach (Renderer renderer in renderers)
        {
            if (!renderer.enabled)
            {
                continue;
            }

            int subMeshCount = GetSubMeshCount(renderer);

            for (int i = 0; i < subMeshCount; i++)
            {
                commandBuffer.DrawRenderer(renderer, material, i, MASK_PASS);
            }
        }

        commandBuffer.Blit(MaskId, BuiltinRenderTextureType.CameraTarget, material, OUTLINE_PASS);
        commandBuffer.ReleaseTemporaryRT(MaskId);
    }

    private int GetSubMeshCount(Renderer renderer)
    {
        Mesh mesh = null;

        if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
        {
            mesh = skinnedMeshRenderer.sharedMesh;
        }
        else
        {
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            mesh = meshFilter != null ? meshFilter.sharedMesh : null;
        }

        return mesh != null ? Mathf.Max(mesh.subMeshCount, 1) : 1;
    }
}

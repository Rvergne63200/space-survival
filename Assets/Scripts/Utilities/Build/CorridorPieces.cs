using System.Collections.Generic;
using UnityEngine;

// Model of a straight corridor (CorridorData), built from pieces under the child "Pieces" of the corridor (its pivot is
// the center of its floor, Z along the corridor, always level), the same way for the ghost, the plan and the finished
// corridor:
// - as many straight pieces (Tunnel_Straight, 2 m) as fit in the length, end to end, centered
// - at each end, an extension (Tunnel_Extension, 1 m, stretched along Z) up to the flange of the connector
// Each extension is between 0.25 and 0.75 straight piece (CorridorData.MaxExtension): when less is left at each end, one
// straight piece less and longer extensions (a short extension shows). The pieces start at their origin and go toward +Z.
// Slope: between two domes at different heights, the corridor is not tilted but sheared: every vertex of every piece
// goes up by slope x its position along the corridor (copies of the meshes). The sections stay vertical, so the ends
// fit the vertical flanges of the connectors, and the floor, the roof and the sides are straight lines from one dome to
// the other. Length and slope are horizontal (the pieces are slightly longer along the slope)
public static class CorridorPieces
{
    private const string ContainerName = "Pieces";

    public struct Piece
    {
        public bool Straight;
        public float Start;
        public float Length;
    }

    public static List<Piece> Layout(CorridorData data, float length)
    {
        List<Piece> pieces = new List<Piece>();
        float straight = Mathf.Max(data.StraightLength, 0.01f);
        int count = Mathf.FloorToInt(length / straight + 0.0001f);
        float rest = (length - count * straight) * 0.5f;

        // Each extension between its minimum and that + half a straight piece
        while (rest < data.MinExtensionLength - 0.0001f && count > 0)
        {
            count--;
            rest += straight * 0.5f;
        }

        if (rest <= 0.001f) rest = 0f;

        float z = -length * 0.5f;

        if (rest > 0f) pieces.Add(new Piece { Straight = false, Start = z, Length = rest });
        z += rest;

        for (int i = 0; i < count; i++)
        {
            pieces.Add(new Piece { Straight = true, Start = z, Length = straight });
            z += straight;
        }

        if (rest > 0f) pieces.Add(new Piece { Straight = false, Start = z, Length = rest });

        return pieces;
    }

    // Replaces the pieces of the corridor. slope: height change per meter along Z. material: every renderer gets it
    // (ghost, plan), null: the materials of the models. colliders: a mesh collider on each mesh (finished corridor).
    // Returns the bounds of the pieces (local space)
    public static Bounds Build(Transform corridor, CorridorData data, float length, float slope, Material material, bool colliders)
    {
        Transform old = corridor.Find(ContainerName);

        if (old != null)
        {
            old.SetParent(null);
            Object.Destroy(old.gameObject);
        }

        GameObject container = new GameObject(ContainerName);
        container.layer = corridor.gameObject.layer;
        container.transform.SetParent(corridor, false);
        OwnedMeshes owned = container.AddComponent<OwnedMeshes>();
        bool sheared = Mathf.Abs(slope) > 0.0001f;

        foreach (Piece piece in Layout(data, length))
        {
            GameObject model = piece.Straight ? data.StraightPiece : data.ExtensionPiece;

            if (model == null) continue;

            GameObject instance = Object.Instantiate(model, container.transform, false);
            instance.transform.localPosition = new Vector3(0f, 0f, piece.Start);
            instance.transform.localRotation = Quaternion.identity;

            if (!piece.Straight)
            {
                float modelLength = Mathf.Max(data.ExtensionLength, 0.01f);
                instance.transform.localScale = new Vector3(1f, 1f, piece.Length / modelLength);
            }

            if (sheared) Shear(instance, corridor, slope, owned);

            foreach (Transform part in instance.GetComponentsInChildren<Transform>(true))
            {
                part.gameObject.layer = corridor.gameObject.layer;
            }

            if (material != null)
            {
                BuildUtility.SetMaterial(instance, material);

                foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
                {
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }

            if (colliders) BuildUtility.AddMeshColliders(instance);
        }

        return BuildUtility.LocalRendererBounds(container, corridor);
    }

    // Vertical shear of the meshes of a piece (local space of the corridor): y += slope x z
    private static void Shear(GameObject piece, Transform corridor, float slope, OwnedMeshes owned)
    {
        foreach (MeshFilter filter in piece.GetComponentsInChildren<MeshFilter>(true))
        {
            if (filter.sharedMesh == null) continue;

            // Read/Write enabled on Tunnel_Straight and Tunnel_Extension (ModelImportSetup)
            if (!filter.sharedMesh.isReadable)
            {
                Debug.LogWarning("CorridorPieces: mesh " + filter.sharedMesh.name + " is not readable (Read/Write in its import settings), the corridor is not sheared.");
                continue;
            }

            Mesh mesh = Object.Instantiate(filter.sharedMesh);
            mesh.name = filter.sharedMesh.name + " (sheared)";
            owned.Add(mesh);

            Matrix4x4 toCorridor = corridor.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            Matrix4x4 fromCorridor = toCorridor.inverse;
            Vector3[] vertices = mesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 p = toCorridor.MultiplyPoint3x4(vertices[i]);
                p.y += slope * p.z;
                vertices[i] = fromCorridor.MultiplyPoint3x4(p);
            }

            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            filter.sharedMesh = mesh;
        }
    }
}

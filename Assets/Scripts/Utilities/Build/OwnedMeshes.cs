using System.Collections.Generic;
using UnityEngine;

// Meshes created at runtime for an object (deformed copies): destroyed with it
public class OwnedMeshes : MonoBehaviour
{
    private readonly List<Mesh> meshes = new List<Mesh>();

    public void Add(Mesh mesh) => meshes.Add(mesh);

    private void OnDestroy()
    {
        foreach (Mesh mesh in meshes)
        {
            if (mesh != null) Destroy(mesh);
        }
    }
}

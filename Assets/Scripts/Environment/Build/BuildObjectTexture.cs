using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class BuildObjectTexture : MonoBehaviour
{
    public BuildVerifier verifier;
    public BuildObject buildObject;

    public Material validMaterial; 
    public Material invalidMaterial;

    private List<MeshRenderer> meshs;

    private Dictionary<MeshRenderer, Material> defaultMaterials;


    private void Start()
    {
        verifier.ev_updateInvalidCollision.AddListener(new UnityAction<int>(UpdateMaterials));
        buildObject.ev_put.AddListener(new UnityAction(ResetMaterials));
        defaultMaterials = new Dictionary<MeshRenderer, Material>();
        meshs = buildObject.GetChilds<MeshRenderer>();

        foreach (MeshRenderer mesh in meshs)
        {
            defaultMaterials.Add(mesh, mesh.material);
        }
    }

    public void UpdateMaterials(int invalidCollisions)
    {
        ChangeAllMaterials(invalidCollisions > 0 ? invalidMaterial : validMaterial);
    }

    private void ChangeAllMaterials(Material material)
    {
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material = material;
        }
    }

    public void ResetMaterials()
    {
        foreach(KeyValuePair<MeshRenderer, Material> pair in defaultMaterials)
        {
            pair.Key.material = pair.Value;
        }

        Destroy(this);
    }
}

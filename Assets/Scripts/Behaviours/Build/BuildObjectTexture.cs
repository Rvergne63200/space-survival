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
        verifier.ev_updateValidity.AddListener(new UnityAction<bool>(UpdateMaterials));
        buildObject.ev_put.AddListener(new UnityAction(ResetMaterials));
        defaultMaterials = new Dictionary<MeshRenderer, Material>();
        meshs = buildObject.GetChilds<MeshRenderer>();

        foreach (MeshRenderer mesh in meshs)
        {
            defaultMaterials.Add(mesh, mesh.material);
        }
    }

    public void UpdateMaterials(bool check)
    {
        ChangeAllMaterials(check ? validMaterial : invalidMaterial);
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

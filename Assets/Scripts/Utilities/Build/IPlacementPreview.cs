using UnityEngine;

// On the root of a placed prefab whose ghost must show the object as it will be once placed (coal generator: its
// chimney goes through the roof of the dome). The Builder makes the ghost from PreviewModel (a copy, with the animation
// script of the model but without the gameplay components) instead of a copy of the meshes, and calls UpdatePreview
// each frame on the component of the prefab
public interface IPlacementPreview
{
    // Child of the prefab copied as the ghost (its pose relative to the root is kept)
    GameObject PreviewModel { get; }

    void UpdatePreview(GameObject ghost);
}

using UnityEngine;

// Landing capsule of the start (prefab SpaceCapsule), on the root of the model LandingCapsule, next to its Animator and
// LandingCapsuleAnimator:
// - Colliders: a mesh collider on each mesh of the model (Body: shell, floor and legs; Door: the leaf, which becomes the
//   ramp once open), added at Awake, so the model can be imported again without losing them
// - Door: interacting with the leaf (LandingCapsuleDoor, added on the "Door" child) opens or closes it
// - Save: the door stays open or closed (a SaveableObject is added when there is none)
[RequireComponent(typeof(LandingCapsuleAnimator))]
public class LandingCapsule : MonoBehaviour, ISaveableComponent
{
    [SerializeField] private string doorName = "Door";

    [System.Serializable]
    private class SaveState
    {
        public bool open;
    }

    private LandingCapsuleAnimator capsuleAnimator;

    public string SaveKey => "landingCapsule";

    public bool IsOpen => capsuleAnimator.IsOpen;

    private void Awake()
    {
        capsuleAnimator = GetComponent<LandingCapsuleAnimator>();

        foreach (MeshFilter filter in GetComponentsInChildren<MeshFilter>(true))
        {
            if (filter.sharedMesh == null || filter.GetComponent<Collider>() != null) continue;

            MeshCollider meshCollider = filter.gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = filter.sharedMesh;
        }

        Transform door = FindChild(transform, doorName);

        if (door != null)
        {
            door.gameObject.AddComponent<LandingCapsuleDoor>().Capsule = this;
        }
        else
        {
            Debug.LogWarning(name + ": no \"" + doorName + "\" child, the door cannot be opened.", this);
        }

        if (GetComponent<SaveableObject>() == null)
        {
            gameObject.AddComponent<SaveableObject>();
        }
    }

    public void ToggleDoor()
    {
        capsuleAnimator.Toggle();
    }

    public string CaptureState()
    {
        return JsonUtility.ToJson(new SaveState { open = capsuleAnimator.IsOpen });
    }

    // Restored without animation
    public void RestoreState(string json)
    {
        SaveState state = JsonUtility.FromJson<SaveState>(json);

        if (state != null)
        {
            capsuleAnimator.SetOpenImmediate(state.open);
        }
    }

    private static Transform FindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName) return child;
        }

        return null;
    }
}

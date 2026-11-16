using System.Collections.Generic;
using System.Text;
using UnityEngine;

// An object whose state is kept in the save: its transform, and the ISaveableComponent on it and its children
// (except those under a nested SaveableObject, which are saved with it). Three kinds of objects:
// - scene object (placed in the editor): id built from its scene and its hierarchy path
// - generated object (procedural, e.g. EnvironmentSpawner): deterministic id given by the generator
// - runtime object (built, dropped...): unique id and a prefab key, instantiated again by the SaveManager
[DisallowMultipleComponent]
public class SaveableObject : MonoBehaviour
{
    public string Id { get; private set; }
    public string PrefabKey { get; private set; }
    public bool IsRuntime => !string.IsNullOrEmpty(PrefabKey);

    private bool explicitId;
    private bool registered;

    // Started before the SaveManager exists (scene played without the Bootstrap): registered by the SaveManager in its Awake
    private static readonly List<SaveableObject> waitingForManager = new List<SaveableObject>();

    public static void RegisterWaitingObjects(SaveManager manager)
    {
        foreach (SaveableObject saveable in waitingForManager)
        {
            if (saveable != null)
            {
                saveable.registered = manager.Register(saveable);
            }
        }

        waitingForManager.Clear();
    }

    public bool IsRegistered => registered;

    // Unloaded with its container (destroyed chunk): it still exists in the world, it must not be noted as removed
    public void MarkUnloaded()
    {
        registered = false;
    }

    // Must be called right after Instantiate, before Start
    public void SetGeneratedId(string id)
    {
        Id = id;
        explicitId = true;
    }

    // Must be called right after Instantiate, before Start. Without id, a new unique id is created
    public void SetRuntime(string prefabKey, string id = null)
    {
        PrefabKey = prefabKey;
        Id = string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString("N") : id;
        explicitId = true;
    }

    private void Start()
    {
        // The hierarchy is complete at Start (the explicit ids of the parents are set right after their Instantiate)
        if (!explicitId)
        {
            Id = ComputeId();
        }

        if (SaveManager.Instance != null)
        {
            registered = SaveManager.Instance.Register(this);
        }
        else
        {
            waitingForManager.Add(this);
        }
    }

    private void OnDestroy()
    {
        waitingForManager.Remove(this);

        if (registered && SaveManager.Instance != null)
        {
            SaveManager.Instance.Unregister(this);
        }
    }

    public ObjectSaveData Capture()
    {
        ObjectSaveData data = new ObjectSaveData
        {
            id = Id,
            prefabKey = PrefabKey,
            position = transform.localPosition,
            rotation = transform.localRotation,
            scale = transform.localScale
        };

        foreach (ISaveableComponent component in GetSaveableComponents())
        {
            data.components.Add(new ComponentSaveData { key = component.SaveKey, json = component.CaptureState() });
        }

        return data;
    }

    public void Restore(ObjectSaveData data)
    {
        transform.localPosition = data.position;
        transform.localRotation = data.rotation;
        transform.localScale = data.scale;

        // An interpolated Rigidbody ignores direct transform changes
        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.position = transform.position;
            body.rotation = transform.rotation;
        }

        foreach (ISaveableComponent component in GetSaveableComponents())
        {
            foreach (ComponentSaveData componentData in data.components)
            {
                if (componentData.key == component.SaveKey)
                {
                    component.RestoreState(componentData.json);
                    break;
                }
            }
        }
    }

    private List<ISaveableComponent> GetSaveableComponents()
    {
        List<ISaveableComponent> components = new List<ISaveableComponent>();

        foreach (ISaveableComponent component in GetComponentsInChildren<ISaveableComponent>(true))
        {
            if (((Component)component).GetComponentInParent<SaveableObject>(true) == this)
            {
                components.Add(component);
            }
        }

        return components;
    }

    // Under an object with an explicit id: "<its id>/<relative path>". Otherwise: "<scene>:<path from the root>".
    // Each path element is "name[sibling index]". Renaming or reordering scene objects changes their id.
    private string ComputeId()
    {
        StringBuilder path = new StringBuilder();
        Transform current = transform;

        while (current != null)
        {
            SaveableObject parentSaveable = current != transform ? current.GetComponent<SaveableObject>() : null;

            if (parentSaveable != null && parentSaveable.explicitId)
            {
                return parentSaveable.Id + "/" + path;
            }

            string element = current.name + "[" + current.GetSiblingIndex() + "]";
            path.Insert(0, path.Length > 0 ? element + "/" : element);

            current = current.parent;
        }

        return gameObject.scene.name + ":" + path;
    }
}

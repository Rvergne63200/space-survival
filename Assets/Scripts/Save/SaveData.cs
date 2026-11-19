using System;
using System.Collections.Generic;
using UnityEngine;

// Everything written to the save file (JsonUtility: only public fields and [Serializable] classes are saved)
[Serializable]
public class SaveData
{
    // Saves of an older version are ignored (2: item ids became text, and the items were renumbered)
    public const int CurrentVersion = 2;

    public int version = CurrentVersion;

    // Shown in the main menu
    public string saveName;
    public string lastSaveTime; // DateTime.Now, round-trip format ("o")

    public int worldSeed;

    public PlayerSaveData player;

    // Objects that exist in the world: scene objects, generated objects and runtime objects (built, dropped...)
    public List<ObjectSaveData> objects = new List<ObjectSaveData>();

    // Scene and generated objects removed during the game (picked up, destroyed): not recreated when the world loads
    public List<string> destroyedObjects = new List<string>();

    public List<TerrainEditSaveData> terrainEdits = new List<TerrainEditSaveData>();
}

// A save as listed in the main menu
public class SaveSlotInfo
{
    public string id;
    public string name;
    public DateTime lastSaveTime;
    public int seed;
}

[Serializable]
public class PlayerSaveData
{
    public Vector3 position;
    public Quaternion rotation;
    public float cameraPitch;
    public float cameraYaw;

    public List<StatSaveData> stats = new List<StatSaveData>();

    public List<ItemSlotSaveData> inventory = new List<ItemSlotSaveData>();
    public List<ItemSlotSaveData> toolBar = new List<ItemSlotSaveData>();
    public List<ItemSlotSaveData> equipment = new List<ItemSlotSaveData>();
    public int handedIndex;
}

[Serializable]
public class StatSaveData
{
    public StatName name;
    public float value;
    public float maxValue;
}

[Serializable]
public class ItemSlotSaveData
{
    // Empty: empty slot
    public string itemId;
    public int quantity;
    // Value of the bar of the item (Item.Gauge), when it has one
    public bool hasGauge;
    public float gauge;
}

[Serializable]
public class ObjectSaveData
{
    public string id;

    // Empty for scene and generated objects. For runtime objects, what to instantiate (see SaveManager.InstantiateRuntimeObject)
    public string prefabKey;

    // Local transform (world transform for root objects)
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public List<ComponentSaveData> components = new List<ComponentSaveData>();
}

[Serializable]
public class ComponentSaveData
{
    public string key;
    public string json;
}

[Serializable]
public class TerrainEditSaveData
{
    public Vector3 position;
    public float radius;
    public float height;
}

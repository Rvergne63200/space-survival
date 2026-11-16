using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    [SerializeField]
    private List<SerializableKeyValuePair<StatName, Stat>> stats;

    public void Update()
    {
        foreach(SerializableKeyValuePair<StatName, Stat> stat in stats)
        {
            stat.Value.Actualize(Time.deltaTime);
        }
    }

    public List<StatSaveData> Capture()
    {
        List<StatSaveData> saved = new List<StatSaveData>();

        foreach (SerializableKeyValuePair<StatName, Stat> stat in stats)
        {
            saved.Add(new StatSaveData { name = stat.Key, value = stat.Value.Value, maxValue = stat.Value.MaxValue });
        }

        return saved;
    }

    public void Restore(List<StatSaveData> saved)
    {
        if (saved == null) return;

        foreach (StatSaveData data in saved)
        {
            Stat stat = Get(data.name);

            if (stat == null) continue;

            // Max first: the value is clamped by the max
            stat.MaxValue = data.maxValue;
            stat.Value = data.value;
        }
    }

    public Stat Get(StatName id)
    {
        foreach(SerializableKeyValuePair<StatName, Stat> stat in stats)
        {
            if(stat.Key == id)
            {
                return stat.Value;
            }
        }

        return null;
    }
}

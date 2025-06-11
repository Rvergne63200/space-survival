using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    [SerializeField]
    private List<SerializableKeyValuePair<string, Stat>> stats;

    public void Update()
    {
        foreach(SerializableKeyValuePair<string, Stat> stat in stats)
        {
            stat.Value.Actualize(Time.deltaTime);
        }
    }

    public Stat Get(string id)
    {
        foreach(SerializableKeyValuePair<string, Stat> stat in stats)
        {
            if(stat.Key == id)
            {
                return stat.Value;
            }
        }

        return null;
    }
}

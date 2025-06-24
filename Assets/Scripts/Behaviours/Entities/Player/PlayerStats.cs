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

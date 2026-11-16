using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    [SerializeField]
    private List<SerializableKeyValuePair<StatName, Stat>> stats;

    // Vital stats: while one of them is empty, the health decreases by emptyVitalDamage per second (cumulative)
    [SerializeField]
    private List<StatName> vitalStats = new List<StatName> { StatName.Hunger, StatName.Thirst, StatName.Oxygen };

    [SerializeField]
    private float emptyVitalDamage = 1f;

    public void Update()
    {
        // This component is on the PlayerManager, which persists in the main menu: the stats only change in game
        // Frozen while dead (death screen)
        if (PlayerManager.Instance == null || PlayerManager.Instance.Player == null || PlayerManager.Instance.IsDead) return;

        foreach(SerializableKeyValuePair<StatName, Stat> stat in stats)
        {
            stat.Value.Actualize(Time.deltaTime);
        }

        Stat health = Get(StatName.Health);

        if (health == null) return;

        foreach (StatName vital in vitalStats)
        {
            Stat stat = Get(vital);

            if (stat != null && stat.Value <= 0f)
            {
                health.Consume(emptyVitalDamage * Time.deltaTime);
            }
        }

        if (health.Value <= 0f)
        {
            PlayerManager.Instance.KillPlayer();
        }
    }

    // Every stat back to its max (respawn after death)
    public void RefillAll()
    {
        foreach (SerializableKeyValuePair<StatName, Stat> stat in stats)
        {
            stat.Value.Value = stat.Value.MaxValue;
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

using System;
using UnityEngine;

// Changes a stat of the player by "amount" (negative to remove): instantly, or spread over "duration" seconds
[Serializable]
public class StatModifier : AbstractModifier
{
    [SerializeField]
    public StatName stat;

    [SerializeField]
    public float amount;

    // 0: instant
    [SerializeField]
    public float duration;

    public override void Modify(GameObject modified)
    {
        Stat target = modified?.GetComponent<PlayerStats>()?.Get(stat);

        if (target == null) return;

        if (duration <= 0f)
        {
            target.Value += amount;
        }
        else
        {
            // A marker adds its value per second
            target.AddMarker("effect_" + stat + "_" + Guid.NewGuid().ToString("N"), amount / duration, duration);
        }
    }
}

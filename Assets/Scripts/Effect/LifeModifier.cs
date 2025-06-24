using System;
using UnityEngine;

[Serializable]
public class LifeModifier : AbstractModifier
{
    [SerializeField]
    public float life;

    [SerializeField]
    public float duration;

    public override void Modify(GameObject modified)
    {
        Stat stat = modified?.GetComponent<PlayerStats>()?.Get(StatName.Health);
        if (stat != null)
        {
            stat.AddMarker("effect_life_" + Time.fixedTime, life, duration);
        }
    }
}

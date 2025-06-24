using System;
using UnityEngine;

[Serializable]
public class EnduranceModifier : AbstractModifier
{
    [SerializeField]
    public int endurance;

    public override void Modify(GameObject modified)
    {
        Debug.Log("endurance : " + endurance);
    }
}

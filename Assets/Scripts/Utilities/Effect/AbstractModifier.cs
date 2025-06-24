using UnityEngine;
using System;

[Serializable]
public abstract class AbstractModifier
{
    public virtual void Modify(GameObject modified) { }
}

using System;
using UnityEngine;

[Serializable]
public class SerializableKeyValuePair<TKey, TValue>
{
    [SerializeField]
    private TKey _key;
    public TKey Key { get => _key; set => _key = value; }


    [SerializeField]
    TValue _value;
    public TValue Value { get => _value; set => _value = value; }
}

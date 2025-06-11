using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

[Serializable]
public class Stat
{
    public UnityEvent<float> ev_updateValue;
    public UnityEvent<float> ev_updateMaxValue;
    public UnityEvent<float> ev_updateRecuperation;

    private Dictionary<string, float> recuperationMarkers;

    public float baseRegeneration;

    public Stat()
    {
        recuperationMarkers = new Dictionary<string, float>();
    }

    [SerializeField]
    private float _value;
    public float Value { 
        get { 
            return _value; 
        } 
        
        set {
            _value = Mathf.Clamp(value, 0, _maxValue);
            ev_updateValue.Invoke(_value); 
        } 
    }

    [SerializeField]
    private float _maxValue;
    public float MaxValue { get { return _maxValue; } set { _maxValue = Mathf.Clamp(value, 0, float.MaxValue); ev_updateMaxValue.Invoke(_maxValue); } }

    public void Actualize(float speed)
    {
        float recuperation = baseRegeneration;

        foreach(float marker in recuperationMarkers.Values)
        {
            recuperation += marker;
        }

        Value += recuperation * speed;
    }

    public void AddMarker(string key, float value)
    {
        recuperationMarkers[key] = value;
    }

    public void RemoveMarker(string key)
    {
        recuperationMarkers.Remove(key);
    }

    public void Consume(float value, float speed)
    {
        Value -= value * speed; 
    }
}

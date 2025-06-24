using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Stat
{
    public UnityEvent<float> ev_updateValue;
    public UnityEvent<float> ev_updateMaxValue;
    public UnityEvent<float> ev_updateRecuperation;

    private Dictionary<string, StatMarker> recuperationMarkers;

    public float baseRegeneration;

    public Stat()
    {
        recuperationMarkers = new Dictionary<string, StatMarker>();
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

        foreach(KeyValuePair<string, StatMarker> pair in recuperationMarkers)
        {
            if(pair.Value.Duration <= 0)
            {
                RemoveMarker(pair.Key);
                break;
            }

            if(pair.Value.Duration < speed)
            {
                recuperation += pair.Value.Modifier * (pair.Value.Duration / speed);
                pair.Value.Actualize(pair.Value.Duration);
            }
            else
            {
                recuperation += pair.Value.Modifier;
                pair.Value.Actualize(speed);
            }
        }

        Value += recuperation * speed;
    }

    public void AddMarker(string key, float value, float duration = Mathf.Infinity)
    {
        StatMarker marker = new StatMarker(value, duration);
        recuperationMarkers[key] = marker;
    }

    public void RemoveMarker(string key)
    {
        recuperationMarkers.Remove(key);
    }

    public void Consume(float value)
    {
        Value -= value; 
    }
}

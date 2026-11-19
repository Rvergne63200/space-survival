using System;
using UnityEngine;

// Jetpack (back slot): while the jump key is held in the air, it pushes the player up (PlayerMovement), and its bar
// goes down with its consumption. Empty, it no longer pushes until recharged
[CreateAssetMenu(menuName = "Item/Jetpack")]
[Serializable]
public class JetpackData : EquipmentData
{
    // Upward acceleration (m/s²), on top of what cancels the gravity of the player
    [SerializeField] private float _thrust = 12f;
    public float Thrust => _thrust;

    // No more push above this rising speed (m/s)
    [SerializeField] private float _maxRiseSpeed = 6f;
    public float MaxRiseSpeed => _maxRiseSpeed;

    // While pushing, per second (W for a charge)
    [SerializeField] private float _consumption = 2000f;
    public float Consumption => _consumption;
}

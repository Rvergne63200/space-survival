using UnityEngine;

// Shows the state of a battery (EnergyStorage on the same object) on its model, through the BatteryAnimator of the
// model (child "Model"): charge level on the gauge, blinking segment while it charges, status light
[RequireComponent(typeof(EnergyStorage))]
public class BatteryIndicator : MonoBehaviour
{
    private EnergyStorage storage;
    private BatteryAnimator display;

    private void Awake()
    {
        storage = GetComponent<EnergyStorage>();
        display = GetComponentInChildren<BatteryAnimator>(true);

        if (display == null) Debug.LogWarning(name + ": no BatteryAnimator in the children, the gauge is not shown", this);
    }

    private void Update()
    {
        if (display == null) return;

        float capacity = storage.Capacity;
        float level = float.IsPositiveInfinity(capacity) ? 1f : capacity > 0f ? storage.Stored / capacity : 0f;

        display.SetState(level, storage.Charge > storage.Discharge + 0.01f, storage.IsOn);
    }
}

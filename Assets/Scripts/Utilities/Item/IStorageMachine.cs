// A machine whose interface is its storage (coal generator): the storage interface (ItemStorageUI) shows, above the
// slots, its state, a gauge and a button to switch it on or off
public interface IStorageMachine
{
    // Name shown as the title of the interface
    string MachineName { get; }

    // State (running, off, no fuel...)
    string MachineState { get; }

    // Gauge from 0 to 1, and its text (coal burning, for example)
    float Gauge { get; }
    string GaugeText { get; }

    bool MachineOn { get; }

    // False while a distribution panel holds it off
    bool CanSwitchMachine { get; }

    void SetMachineOn(bool on);
}

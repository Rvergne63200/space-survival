using System.Collections.Generic;
using UnityEngine;

// Drill (placed item 37, prefab Prefabs/Environment/Energy/Drill, model Resources/Models/Drill moved by its DrillAnimator,
// on the model child), placed outside only:
// - Consumer ("consumption", 500 W): while it is powered and its reserve can take the next ore, it drills. One ore every
//   "extractionTime" seconds (20), drawn at random from "outputs" by weight (rock mostly, titanium ore rarely). There are
//   no deposits in the world yet: it drills the same things anywhere
// - The time only runs while the head is down in the ground (the model lowers it after spinning it up)
// - Reserve full (no room for the next ore): it stops, and starts again when the player empties it
// - Interaction: anywhere on the machine, its interface (the storage interface, IStorageMachine): the reserve next to
//   the inventory, the state, a gauge of the ore being drilled and the button to switch it on or off
// - Model: the holographic screen shows while it is powered, the head spins and goes down while it drills
// - Save: the reserve (ItemStorage) and the switch (EnergyDevice). Not the ore being drilled: lost at the reload
[RequireComponent(typeof(ItemStorage))]
public class Drill : EnergyConsumer, IInterractable, IStorageMachine
{
    [System.Serializable]
    public class Ore
    {
        public ItemData item;

        // Relative chance to be drawn
        public float weight = 1f;
    }

    // Seconds to drill one ore (head down)
    [SerializeField] private float extractionTime = 20f;
    [SerializeField] private List<Ore> outputs = new List<Ore>();

    private ItemStorage reserve;
    private DrillAnimator drillAnimator;

    // Ore being drilled, and the seconds already spent on it
    private ItemData next;
    private float progress;

    // Last order given to the animator (true: StartDrilling)
    private bool wantsDrilling;

    // Player whose interface shows the reserve
    private Searcher searcher;

    public ItemStorage Reserve => reserve;

    private void Awake()
    {
        reserve = GetComponent<ItemStorage>();
        drillAnimator = GetComponentInChildren<DrillAnimator>(true);

        if (drillAnimator == null) Debug.LogWarning(name + ": no DrillAnimator on the model, the drill is not animated", this);
        if (outputs.Count == 0) Debug.LogWarning(name + ": no output set, the drill gives nothing", this);

        BuildUtility.AddMeshColliders(gameObject);
    }

    private void Update()
    {
        if (next == null) next = Draw();

        bool running = IsPowered && next != null && HasRoomFor(next);

        UpdateAnimator(running);

        // The head is down in the ground (no model: as soon as it runs)
        bool drilling = running && (drillAnimator == null || drillAnimator.IsDrilling);

        if (drilling)
        {
            progress += Time.deltaTime;

            if (progress >= extractionTime)
            {
                progress = 0f;
                reserve.Content.Add(Item.CreateFromData(next), 1);
                next = Draw();
            }
        }

        // The interface is closed (or shows another storage)
        if (searcher != null && searcher.CurrentSearchedStorage != reserve) searcher = null;
    }

    private ItemData Draw()
    {
        float total = 0f;

        foreach (Ore output in outputs)
        {
            if (output != null && output.item != null && output.weight > 0f) total += output.weight;
        }

        if (total <= 0f) return null;

        float pick = Random.value * total;

        foreach (Ore output in outputs)
        {
            if (output == null || output.item == null || output.weight <= 0f) continue;

            pick -= output.weight;

            if (pick <= 0f) return output.item;
        }

        return null;
    }

    // An empty slot, or a stack of that item that is not full
    private bool HasRoomFor(ItemData data)
    {
        List<ItemSlot<Item>> slots = reserve.Content.Slots;
        int filled = 0;

        foreach (ItemSlot<Item> slot in slots)
        {
            if (slot == null || slot.IsEmpty()) return true;

            filled++;

            if (slot.Item.Data == data && !slot.IsFull()) return true;
        }

        return filled < reserve.Content.MaxSize;
    }

    public bool IsFull => next != null && !HasRoomFor(next);

    // ---------- Model ----------

    private void UpdateAnimator(bool running)
    {
        if (drillAnimator == null) return;

        if (drillAnimator.Powered != IsPowered) drillAnimator.Powered = IsPowered;

        // StartDrilling and StopDrilling start a sequence over: only when the state asked changes
        bool started = drillAnimator.IsDrilling || (drillAnimator.IsBusy && wantsDrilling);

        if (running && !started)
        {
            wantsDrilling = true;
            drillAnimator.StartDrilling();
        }
        else if (!running && wantsDrilling)
        {
            wantsDrilling = false;
            drillAnimator.StopDrilling();
        }
    }

    // ---------- Interaction ----------

    // Opens its interface (the reserve, with the state and the switch)
    public void Interract(GameObject interractor)
    {
        Searcher interfaceSearcher = interractor.GetComponent<Searcher>();

        if (interfaceSearcher == null) return;

        interfaceSearcher.Search(reserve);

        if (interfaceSearcher.CurrentSearchedStorage == reserve) searcher = interfaceSearcher;
    }

    // ---------- Interface (IStorageMachine) ----------

    public string MachineName => DisplayName;

    public string MachineState
    {
        get
        {
            if (ForcedOff) return "Éteinte par le tableau";
            if (!SwitchedOn) return "Éteinte";
            if (!IsPowered) return "Pas alimentée (" + EnergyValue.Format(Consumption, "W") + ")";
            if (IsFull) return "Réserve pleine";
            if (drillAnimator != null && !drillAnimator.IsDrilling) return "Mise en route";
            return "En forage : " + EnergyValue.Format(Consumption, "W");
        }
    }

    public float Gauge => extractionTime > 0f ? Mathf.Clamp01(progress / extractionTime) : 0f;

    public string GaugeText => progress > 0f
        ? "Minerai suivant : " + Mathf.CeilToInt(extractionTime - progress) + " s"
        : "Aucun forage en cours";

    public bool MachineOn => SwitchedOn;

    public bool CanSwitchMachine => !ForcedOff;

    public void SetMachineOn(bool on) => SetSwitchedOn(on);

    public override string GetInfo()
    {
        return base.GetInfo() + (IsOn && IsFull ? ", réserve pleine" : "");
    }
}

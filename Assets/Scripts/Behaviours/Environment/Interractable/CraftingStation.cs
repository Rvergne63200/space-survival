using System;
using System.Collections.Generic;
using UnityEngine;

// Crafting station (placed item 24_Crafter, model Resources/Models/CraftStation moved by its CraftingStationAnimator):
// - Electricity: an EnergyConsumer on the same object (20 W, cabled to a network). Powered and on: the animator powers
//   the station on (screen and holograms). Off or not powered: it powers off, nothing can be launched, the queue waits.
//   Switched on / off from its interface (CraftingStationUI) and from the control panel, like the other machines
// - Queue: each crafted unit takes "craftTime" seconds (2), one after the other. The ingredients are taken when the
//   crafts are launched (Crafter.Craft); cancelling gives back those of the crafts not finished. The animator prints
//   while a craft runs
// - Output: a finished unit goes into the inventory of the player when he is close ("pickupDistance") and has room,
//   otherwise it waits on the station (stacked with the same item). A craft whose result does not fit in the output
//   waits. Interacting takes what waits (what fits), or opens the interface when nothing waits
// - Save: the queue (indexes of the recipes), the progress of the current craft and the output
[RequireComponent(typeof(EnergyConsumer))]
public class CraftingStation : MonoBehaviour, IInterractable, ISaveableComponent
{
    [SerializeField] private RecipeData[] _recipes;
    public RecipeData[] Recipes { get => _recipes; }

    [SerializeField] private float craftTime = 2f;
    [SerializeField] private float pickupDistance = 4.5f;

    private EnergyConsumer power;
    private CraftingStationAnimator stationAnimator;

    private readonly List<RecipeData> queue = new List<RecipeData>();
    private float progress;

    private readonly ItemSlot<Item> output = new ItemSlot<Item>();

    // Called when the queue, the progress or the output changes (interface)
    public event Action Changed;

    public EnergyConsumer Power => power;

    // Switched on (its own setting, and no panel above holding it off) and powered by its network
    public bool IsUsable => power != null && power.IsOn && power.IsPowered;

    public IReadOnlyList<RecipeData> Queue => queue;
    public RecipeData Current => queue.Count > 0 ? queue[0] : null;
    public float Progress => craftTime > 0f ? Mathf.Clamp01(progress / craftTime) : 1f;
    public ItemSlot<Item> Output => output;

    private void Awake()
    {
        power = GetComponent<EnergyConsumer>();
        stationAnimator = GetComponentInChildren<CraftingStationAnimator>();
    }

    private void Update()
    {
        UpdateAnimator();

        if (!IsUsable || Current == null) return;

        progress += Time.deltaTime;

        if (progress < craftTime) return;

        // Finished: waits until its result fits in the output
        if (!Deliver(Current))
        {
            progress = craftTime;
            return;
        }

        queue.RemoveAt(0);
        progress = 0f;
        Changed?.Invoke();
    }

    // Powered on with the network, prints while a craft runs (CraftingStationAnimator)
    private void UpdateAnimator()
    {
        if (stationAnimator == null) return;

        if (IsUsable) stationAnimator.PowerOn();
        else stationAnimator.PowerOff();

        bool printing = IsUsable && Current != null && progress < craftTime;

        if (printing && !stationAnimator.IsPrinting) stationAnimator.StartPrint();
        else if (!printing && stationAnimator.IsPrinting) stationAnimator.StopPrint();
    }

    // ---------- Crafts ----------

    // Adds crafts of the recipe (its ingredients are already taken by Crafter)
    public void Enqueue(RecipeData recipe, int count)
    {
        for (int i = 0; i < count; i++) queue.Add(recipe);
        Changed?.Invoke();
    }

    // Gives back to the player the ingredients of the crafts not finished
    public void CancelQueue()
    {
        foreach (RecipeData recipe in queue)
        {
            foreach (RecipeData.ItemBlock ingredient in recipe.Ingredients)
            {
                BuildUtility.GiveToPlayer(ingredient.Item, ingredient.Quantity);
            }
        }

        queue.Clear();
        progress = 0f;
        Changed?.Invoke();
    }

    public void SetSwitchedOn(bool on)
    {
        power.SetSwitchedOn(on);
        Changed?.Invoke();
    }

    // Into the inventory of the close player (what fits), the rest into the output. False (nothing done) when the
    // output cannot take all that is left
    private bool Deliver(RecipeData recipe)
    {
        PlayerInventory inventory = PlayerManager.Instance != null ? PlayerManager.Instance.Inventory : null;
        GameObject player = PlayerManager.Instance != null ? PlayerManager.Instance.Player : null;
        bool close = inventory != null && player != null && InteractionDistance.To(this, player.transform.position) <= pickupDistance;

        // Check first that everything fits somewhere
        foreach (RecipeData.ItemBlock result in recipe.Results)
        {
            int forOutput = close ? Mathf.Max(0, result.Quantity - SpaceFor(inventory, result.Item)) : result.Quantity;

            if (forOutput > 0 && !OutputAccepts(result.Item, forOutput)) return false;
        }

        foreach (RecipeData.ItemBlock result in recipe.Results)
        {
            int rest = result.Quantity;

            if (close)
            {
                rest = inventory.Add(Item.CreateFromData(result.Item), rest);
                int taken = result.Quantity - rest;

                if (taken > 0) NotifyPickup(player, taken, result.Item);
            }

            if (rest > 0) output.Add(Item.CreateFromData(result.Item), rest);
        }

        return true;
    }

    // How many of the item the inventory can take (tool bar and content, like PlayerInventory.Add)
    private static int SpaceFor(PlayerInventory inventory, ItemData item)
    {
        int space = 0;

        foreach (ItemCollection<Item> collection in new[] { inventory.ToolBar, inventory.Content })
        {
            foreach (ItemSlot<Item> slot in collection.Slots)
            {
                if (slot == null || !slot.Accepts(item)) continue;

                if (slot.IsEmpty()) space += slot.Limit(item);
                else if (slot.Item.Data == item) space += Mathf.Max(0, slot.Limit(item) - slot.Quantity);
            }
        }

        return space;
    }

    private bool OutputAccepts(ItemData item, int count)
    {
        if (output.IsEmpty()) return count <= item.StackMax;

        return output.Item.Data == item && output.Quantity + count <= item.StackMax;
    }

    private static void NotifyPickup(GameObject player, int count, ItemData item)
    {
        Pickuper pickuper = player.GetComponent<Pickuper>();

        if (pickuper != null) pickuper.ev_pickup?.Invoke(count, item);
    }

    // What waits on the station, into the inventory (what fits). False when nothing waits
    public bool TakeOutput(GameObject interractor)
    {
        if (output.IsEmpty()) return false;

        PlayerInventory inventory = PlayerManager.Instance != null ? PlayerManager.Instance.Inventory : null;

        if (inventory == null) return true;

        ItemData data = output.Item.Data;
        int count = output.Quantity;
        int rest = inventory.Add(Item.CreateFromData(data), count);
        int taken = count - rest;

        if (taken > 0)
        {
            output.Remove(taken);
            NotifyPickup(interractor, taken, data);
            Changed?.Invoke();
        }
        else
        {
            NotificationUI.Show("Inventaire plein.", NotificationType.Warning);
        }

        return true;
    }

    // ---------- Interaction ----------

    public string GetInfo()
    {
        if (!output.IsEmpty()) return "Récupérer " + output.Quantity + "x " + output.Item.Data.Name;

        return power != null ? power.GetInfo() : "Fabriquer un objet";
    }

    // Takes what waits, otherwise opens the interface
    public void Interract(GameObject interractor)
    {
        if (TakeOutput(interractor)) return;

        Crafter crafter = interractor.GetComponent<Crafter>();

        if (crafter != null)
        {
            crafter.Check(this);
        }
    }

    // ---------- Save ----------

    [Serializable]
    private class SaveState
    {
        public List<int> queue = new List<int>();
        public float progress;
        public string outputItem;
        public int outputQuantity;
    }

    public string SaveKey => "craftingStation";

    public string CaptureState()
    {
        SaveState state = new SaveState { progress = progress };

        foreach (RecipeData recipe in queue)
        {
            state.queue.Add(Array.IndexOf(_recipes, recipe));
        }

        if (!output.IsEmpty())
        {
            state.outputItem = output.Item.Data.Id;
            state.outputQuantity = output.Quantity;
        }

        return JsonUtility.ToJson(state);
    }

    public void RestoreState(string json)
    {
        SaveState state = JsonUtility.FromJson<SaveState>(json);

        if (state == null) return;

        queue.Clear();

        foreach (int index in state.queue)
        {
            if (index >= 0 && index < _recipes.Length) queue.Add(_recipes[index]);
        }

        progress = state.progress;
        output.Clear();

        if (!string.IsNullOrEmpty(state.outputItem) && state.outputQuantity > 0 && SaveManager.Instance != null)
        {
            ItemData data = SaveManager.Instance.GetItem(state.outputItem);
            if (data != null) output.Add(Item.CreateFromData(data), state.outputQuantity);
        }

        Changed?.Invoke();
    }
}

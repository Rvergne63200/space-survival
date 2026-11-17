using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Plan of a structure (BuildUtility.CreateBlueprint): interacting with it brings the missing resources from the
// inventory. Once the whole cost is brought, it becomes the real structure. Destroyed, it gives back what was brought
public class StructureBlueprint : MonoBehaviour, IInterractable, ISaveableComponent
{
    public StructureData Data { get; private set; }

    // Quantity brought for each cost line (same index as Data.Cost)
    private int[] delivered = new int[0];

    [System.Serializable]
    private class SaveState
    {
        public List<int> delivered = new List<int>();
    }

    public string SaveKey => "blueprint";

    public void Initialize(StructureData data)
    {
        Data = data;
        delivered = new int[data.Cost.Count];
    }

    public string GetInfo()
    {
        if (Data == null) return "";

        StringBuilder info = new StringBuilder(Data.Name + " (plan)");

        for (int i = 0; i < Data.Cost.Count; i++)
        {
            RecipeData.ItemBlock cost = Data.Cost[i];
            info.Append(i == 0 ? " : " : ", ");
            info.Append(cost.Item != null ? cost.Item.Name : "?").Append(' ').Append(delivered[i]).Append('/').Append(cost.Quantity);
        }

        return info.ToString();
    }

    // Brings everything that is missing and in the inventory
    public void Interract(GameObject interractor)
    {
        if (Data == null || PlayerManager.Instance == null) return;

        PlayerInventory inventory = PlayerManager.Instance.Inventory;
        int totalTaken = 0;

        for (int i = 0; i < Data.Cost.Count; i++)
        {
            ItemData item = Data.Cost[i].Item;
            int missing = Data.Cost[i].Quantity - delivered[i];

            if (item == null || missing <= 0) continue;

            int taken = Mathf.Min(missing, inventory.Count(item));

            if (taken <= 0) continue;

            // Remove returns what could not be removed
            int notRemoved = inventory.Remove(Item.CreateFromData(item), taken);
            delivered[i] += taken - notRemoved;
            totalTaken += taken - notRemoved;
        }

        if (IsComplete())
        {
            BuildUtility.InstantiateStructure(Data, transform.position, transform.rotation);
            NotificationUI.Show(Data.Name + " construit.");
            Destroy(gameObject);
        }
        else if (totalTaken == 0)
        {
            NotificationUI.Show("Aucune des ressources manquantes dans l'inventaire.", NotificationType.Warning);
        }
    }

    private bool IsComplete()
    {
        for (int i = 0; i < Data.Cost.Count; i++)
        {
            if (delivered[i] < Data.Cost[i].Quantity) return false;
        }

        return true;
    }

    public void Deconstruct()
    {
        if (Data != null)
        {
            for (int i = 0; i < Data.Cost.Count; i++)
            {
                BuildUtility.GiveToPlayer(Data.Cost[i].Item, delivered[i]);
            }
        }

        Destroy(gameObject);
    }

    public string CaptureState()
    {
        return JsonUtility.ToJson(new SaveState { delivered = new List<int>(delivered) });
    }

    public void RestoreState(string json)
    {
        SaveState state = JsonUtility.FromJson<SaveState>(json);

        for (int i = 0; i < delivered.Length && i < state.delivered.Count; i++)
        {
            delivered[i] = state.delivered[i];
        }
    }
}

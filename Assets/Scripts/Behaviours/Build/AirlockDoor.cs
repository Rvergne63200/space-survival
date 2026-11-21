using UnityEngine;

// Leaf of a door of an airlock (added by Airlock on Door_Inner_* and Door_Outer_*): the interaction opens or closes
// its door (Airlock.UseDoor)
public class AirlockDoor : MonoBehaviour, IInterractable
{
    public Airlock Airlock { get; set; }
    public bool Outer { get; set; }

    public string GetInfo()
    {
        if (Airlock == null) return "";
        if (Airlock.DoorsBusy) return "Sas en cours de cycle";

        return Airlock.IsDoorOpen(Outer) ? "Fermer la porte" : "Ouvrir la porte";
    }

    public void Interract(GameObject interractor)
    {
        if (Airlock != null) Airlock.UseDoor(Outer);
    }
}

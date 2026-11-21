using UnityEngine;

// Door leaf of the landing capsule: the interaction (pointed collider of the leaf) opens or closes it
public class LandingCapsuleDoor : MonoBehaviour, IInterractable
{
    public LandingCapsule Capsule { get; set; }

    public string GetInfo()
    {
        if (Capsule == null) return "";

        return Capsule.IsOpen ? "Fermer la porte" : "Ouvrir la porte";
    }

    public void Interract(GameObject interractor)
    {
        if (Capsule != null) Capsule.ToggleDoor();
    }
}

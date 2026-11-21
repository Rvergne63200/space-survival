using UnityEngine;

// Terminal inside a building (child "PanelTerminal" of the dome prefabs): the interaction opens the interface of the
// panel built into the building (BuildingPowerPanel), like its socket outside. Not an electrical device: nothing can be
// cabled to it
public class BuildingPanelTerminal : MonoBehaviour, IInterractable
{
    private BuildingPowerPanel panel;

    private BuildingPowerPanel Panel
    {
        get
        {
            if (panel == null)
            {
                BuiltStructure structure = GetComponentInParent<BuiltStructure>();
                panel = structure != null ? structure.GetComponentInChildren<BuildingPowerPanel>() : null;
            }

            return panel;
        }
    }

    public string GetInfo()
    {
        return Panel != null ? Panel.GetInfo() : "";
    }

    public void Interract(GameObject interractor)
    {
        if (Panel != null && DistributionPanelUI.Instance != null)
        {
            DistributionPanelUI.Instance.Open(Panel);
        }
    }
}

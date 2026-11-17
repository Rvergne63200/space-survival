using UnityEngine;
using UnityEngine.UIElements;

// Tool bar of the player, in the HUD (element "toolbar-slots" of Hud.uxml), always shown in game
public class PlayerToolBarUI : ItemCollectionUI
{
    [SerializeField] private UIDocument document;

    public override void OnUpdateManager(UIManager manager)
    {
        if (manager == null || document == null)
        {
            return;
        }

        Bind(manager.PlayerInventory.ToolBar, document.rootVisualElement.Q("toolbar-slots"));
    }
}

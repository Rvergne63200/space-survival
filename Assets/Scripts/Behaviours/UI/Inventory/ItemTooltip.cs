using UnityEngine;
using UnityEngine.UIElements;

// Information of the item under the mouse (ItemSlotUI): name, description and value of its bar. One element per
// UIDocument, created on first use at the root of the document (where Game.uss applies), next to the slot
public static class ItemTooltip
{
    private const string TooltipName = "item-tooltip";
    private const float Margin = 8f;

    public static void Show(VisualElement slotElement, Item item)
    {
        VisualElement root = DocumentRoot(slotElement);

        if (root == null || item == null || item.Data == null)
        {
            Hide(slotElement);
            return;
        }

        VisualElement tooltip = root.Q(TooltipName);

        if (tooltip == null)
        {
            tooltip = new VisualElement { name = TooltipName, pickingMode = PickingMode.Ignore };
            tooltip.AddToClassList("item-tooltip");
            tooltip.Add(NewLabel("item-tooltip-name"));
            tooltip.Add(NewLabel("item-tooltip-description"));
            tooltip.Add(NewLabel("item-tooltip-gauge"));
            root.Add(tooltip);
        }

        SetText(tooltip.Q<Label>(className: "item-tooltip-name"), item.Data.Name);
        SetText(tooltip.Q<Label>(className: "item-tooltip-description"), item.Data.Description);
        SetText(tooltip.Q<Label>(className: "item-tooltip-gauge"), GaugeText(item));

        tooltip.style.display = DisplayStyle.Flex;
        tooltip.BringToFront();

        // Right of the slot, or left of it near the right edge of the screen
        Rect slot = root.WorldToLocal(slotElement.worldBound);
        float width = tooltip.resolvedStyle.width;
        float left = slot.xMax + Margin;

        if (!float.IsNaN(width) && left + width > root.resolvedStyle.width)
        {
            left = slot.xMin - Margin - width;
        }

        tooltip.style.left = left;
        tooltip.style.top = slot.yMin;
    }

    public static void Hide(VisualElement slotElement)
    {
        VisualElement root = DocumentRoot(slotElement);
        VisualElement tooltip = root != null ? root.Q(TooltipName) : null;

        if (tooltip != null)
        {
            tooltip.style.display = DisplayStyle.None;
        }
    }

    public static string GaugeText(Item item)
    {
        if (item == null || !item.HasGauge) return "";

        ItemGauge gauge = item.Data.Gauge;
        return (gauge.Rechargeable ? "Charge : " : "Usure : ") + gauge.Format(item.Gauge);
    }

    // The element just under the visual tree of the panel: the root of the UIDocument
    private static VisualElement DocumentRoot(VisualElement element)
    {
        if (element == null || element.panel == null) return null;

        VisualElement tree = element.panel.visualTree;
        VisualElement current = element;

        while (current.parent != null && current.parent != tree)
        {
            current = current.parent;
        }

        return current;
    }

    private static Label NewLabel(string className)
    {
        Label label = new Label { pickingMode = PickingMode.Ignore };
        label.AddToClassList(className);
        return label;
    }

    private static void SetText(Label label, string text)
    {
        if (label == null) return;

        label.text = text;
        label.style.display = string.IsNullOrEmpty(text) ? DisplayStyle.None : DisplayStyle.Flex;
    }
}

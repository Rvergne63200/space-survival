using UnityEngine;
using UnityEngine.UIElements;

// One stat bar (element "stat" of StatBar.uxml), created by PlayerStatsUI: icon, fill (value / max) and "value/max"
public class StatBarUI
{
    private readonly VisualElement icon;
    private readonly VisualElement fill;
    private readonly Label text;

    private Stat stat;
    private float value;
    private float maxValue;

    public StatBarUI(VisualElement element)
    {
        icon = element.Q("stat-icon");
        fill = element.Q("stat-fill");
        text = element.Q<Label>("stat-text");
    }

    public void SetStat(Stat stat)
    {
        Unbind();

        this.stat = stat;
        value = stat.Value;
        maxValue = stat.MaxValue;

        UpdateUI();

        stat.ev_updateValue.AddListener(UpdateValue);
        stat.ev_updateMaxValue.AddListener(UpdateMaxValue);
    }

    // The stats outlive this UI (they belong to the PlayerManager)
    public void Unbind()
    {
        if (stat == null) return;

        stat.ev_updateValue.RemoveListener(UpdateValue);
        stat.ev_updateMaxValue.RemoveListener(UpdateMaxValue);
        stat = null;
    }

    private void UpdateValue(float value)
    {
        this.value = value;
        UpdateUI();
    }

    private void UpdateMaxValue(float maxValue)
    {
        this.maxValue = maxValue;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (text != null)
        {
            text.text = ((int)value).ToString() + "/" + ((int)maxValue).ToString();
        }

        if (fill != null)
        {
            float ratio = maxValue > 0f ? Mathf.Clamp01(value / maxValue) : 0f;
            fill.style.width = new Length(ratio * 100f, LengthUnit.Percent);
        }
    }

    public void SetColor(Color color)
    {
        if (fill != null)
        {
            fill.style.backgroundColor = color;
        }
    }

    public void SetIcon(Sprite sprite)
    {
        if (icon != null && sprite != null)
        {
            icon.style.backgroundImage = new StyleBackground(sprite);
        }
    }
}

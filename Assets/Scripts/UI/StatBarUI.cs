using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class StatBarUI : ParentedUI
{
    public TextMeshProUGUI text;
    public Image icon;
    public Slider slider;

    private float value;
    private float maxValue;

    public void SetStat(Stat stat)
    {
        value = stat.Value;
        maxValue = stat.Value;

        UpdateUI();

        stat.ev_updateValue.AddListener(new UnityAction<float>(UpdateValue));
        stat.ev_updateMaxValue.AddListener(new UnityAction<float>(UpdateMaxValue));
    }

    public void UpdateValue(float value)
    {
        this.value = value;
        UpdateUI();
    }

    public void UpdateMaxValue(float maxValue)
    {
        this.maxValue = maxValue;
        UpdateUI();
    }

    private void UpdateUI()
    {
        text.text = ((int) value).ToString() + "/" + ((int) maxValue).ToString();
        slider.value = value / maxValue;
    }

    public void SetColor(Color color)
    {
        ColorBlock colorBlock = slider.colors;

        colorBlock.normalColor = color;

        slider.colors = colorBlock;
    }

    public void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
    }
}

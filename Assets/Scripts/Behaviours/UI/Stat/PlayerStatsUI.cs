using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Stat bars of the player, in the HUD (element "stats" of Hud.uxml): one StatBar.uxml per stat of the collection
public class PlayerStatsUI : ParentedUI
{
    public StatUIData[] statUIDataCollection;

    [SerializeField] private UIDocument document;
    [SerializeField] private VisualTreeAsset statTemplate;

    private readonly List<StatBarUI> bars = new List<StatBarUI>();

    public override void OnUpdateManager(UIManager manager)
    {
        if (manager == null || document == null || statTemplate == null)
        {
            return;
        }

        VisualElement container = document.rootVisualElement.Q("stats");

        if (container == null)
        {
            return;
        }

        Clear(container);

        foreach (StatUIData data in statUIDataCollection)
        {
            Stat stat = manager.PlayerStats.Get(data.name);

            if (stat == null) continue;

            VisualElement element = statTemplate.Instantiate().Q("stat");
            container.Add(element);

            StatBarUI bar = new StatBarUI(element);
            bar.SetColor(data.color);
            bar.SetIcon(data.sprite);
            bar.SetStat(stat);

            bars.Add(bar);
        }
    }

    protected override void OnDestroy()
    {
        Clear(null);
        base.OnDestroy();
    }

    private void Clear(VisualElement container)
    {
        foreach (StatBarUI bar in bars)
        {
            bar.Unbind();
        }

        bars.Clear();
        container?.Clear();
    }
}

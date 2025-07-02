using UnityEngine;

public class PlayerStatsUI : ParentedUI
{
    public StatUIData[] statUIDataCollection;
    public GameObject statPrefab;

    private PlayerStats playerStats;

    public override void AfterUpdatePlayer()
    {
        base.AfterUpdatePlayer();

        playerStats = parentUI.PlayerStats;

        foreach (StatUIData stat in statUIDataCollection)
        {
            GameObject statUIObject = Instantiate(statPrefab, transform);
            StatBarUI UI = statUIObject.GetComponent<StatBarUI>();

            UI.parentUI = parentUI;
            UI.SetColor(stat.color);
            UI.SetIcon(stat.sprite);
            UI.SetStat(playerStats.Get(stat.name));
        }
    }
}

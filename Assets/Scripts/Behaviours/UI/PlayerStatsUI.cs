using UnityEngine;

public class PlayerStatsUI : ParentedUI
{
    public StatUIData[] statUIDataCollection;
    public GameObject statPrefab;

    private PlayerStats playerStats;


    private void Start()
    {
        playerStats = parentUI.playerStats;

        foreach(StatUIData stat in statUIDataCollection)
        {
            GameObject statUIObject = Instantiate(statPrefab, transform);
            StatBarUI UI = statUIObject.GetComponent<StatBarUI>();
            UI.SetColor(stat.color);
            UI.SetIcon(stat.sprite);
            UI.SetStat(playerStats.Get(stat.name));
        }
    }
}

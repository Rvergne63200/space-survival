using UnityEngine;

public class StatMarker
{
    public float Duration { get; private set; }
    public float Modifier { get; private set; }


    public StatMarker(float Modifier, float Duration = Mathf.Infinity)
    {
        this.Duration = Duration;
        this.Modifier = Modifier;
    }

    public void Actualize(float speed)
    {
        Duration -= speed;
    }
}

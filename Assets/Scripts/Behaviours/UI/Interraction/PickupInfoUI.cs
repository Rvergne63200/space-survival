using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PickupInfoUI : ParentedUI
{
    public float durability = 5f;
    public Image image;
    public TextMeshProUGUI text;

    private CanvasGroup group;
    private float countDown = 0f;

    public void Start()
    {
        countDown = durability;
        group = GetComponent<CanvasGroup>();
    }

    // Shown again for its whole duration (grouped pickups, see PickupInfoGroupUI)
    public void Restart()
    {
        countDown = durability;

        if (group != null)
        {
            group.alpha = 1f;
        }
    }

    public void SetInfobulle(string text, Sprite sprite)
    {
        image.sprite = sprite;
        this.text.text = text;
    }

    public void Update()
    {
        countDown -= Time.deltaTime;

        group.alpha = countDown / durability;

        if(countDown <= 0f)
        {
            Destroy(gameObject);
        }
    }
}

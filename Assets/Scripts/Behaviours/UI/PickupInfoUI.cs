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

    protected override void Start()
    {
        base.Start();

        countDown = durability;
        group = GetComponent<CanvasGroup>();
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

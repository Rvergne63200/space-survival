using TMPro;
using UnityEngine;

public class PointedInterractorInformationUI : MonoBehaviour
{
    private TextMeshProUGUI text;
    public string input;
    public PlayerInterractor interractor;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        interractor.ev_UpdatePointed.AddListener(UpdatePointed);
        UpdatePointed(null);
    }

    public void UpdatePointed(IInterractable pointed)
    {
        if(pointed == null)
        {
            gameObject.SetActive(false);
            return;
        }

        text.text = pointed.GetAction() + " <" + input.ToUpper() + ">";
        gameObject.SetActive(true);
    }
}

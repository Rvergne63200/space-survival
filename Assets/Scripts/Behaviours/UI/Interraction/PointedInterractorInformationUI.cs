using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PointedInterractorInformationUI : ParentedUI
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image icon;

    private CanvasGroup canvasGroup;
    public string input;

    private Coroutine fadeCoroutine;
    public float fadeDuration = 0.5f;

    private UnityEvent<Transform> ev_updatePointedObject;

    private IInterractable pointed = null;

    protected override void Awake()
    {
        base.Awake();
        canvasGroup = GetComponent<CanvasGroup>();

        if (pointed != null) { 
            canvasGroup.alpha = 1f; 
            gameObject.SetActive(true); 
        } else { 
            canvasGroup.alpha = 0f; 
            gameObject.SetActive(false); 
        }
    }

    public override void OnUpdatePlayerContext(PlayerContext context)
    {
        if(context == null)
        {
            return;
        }

        if (ev_updatePointedObject != null)
        {
            ev_updatePointedObject.RemoveListener(UpdatePointedObject);
        }

        ev_updatePointedObject = context.ev_updatePointedObject;
        ev_updatePointedObject.AddListener(UpdatePointedObject);
    }

    public void UpdatePointedObject(Transform pointedObject)
    {
        var oldPointed = pointed;
        pointed = parentUI.UIManager.PlayerContext.Interractable;

        if (pointed == null)
        {
            if (oldPointed != null)
            {
                FadeOut();
            }

            return;
        }

        // Also when going directly from an interactable to another one
        RefreshInfo();

        if (oldPointed == null)
        {
            FadeIn();
        }
    }

    private void Update()
    {
        // The info can change while pointing at the same object (partial pickup): keep it up to date.
        // A fully picked up object is destroyed until the pointer finds the next one
        if (pointed != null && !(pointed is Object pointedObject && pointedObject == null))
        {
            RefreshInfo();
        }
    }

    private void RefreshInfo()
    {
        string info = pointed.GetInfo() + " [" + input.ToUpper() + "]";

        if (text.text != info)
        {
            text.text = info;
        }

        icon.sprite = pointed.GetIcon();
    }

    public void FadeIn()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(Fade(1f));
    }

    public void FadeOut() { 
        if (!gameObject.activeSelf) { 
            gameObject.SetActive(true); 
        } 
        
        if (fadeCoroutine != null) { 
            StopCoroutine(fadeCoroutine); 
        } 
        
        fadeCoroutine = StartCoroutine(Fade(0f)); 
    }

    private IEnumerator Fade(float target)
    {
        float start = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }

        canvasGroup.alpha = target;

        if(target == 0f)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}

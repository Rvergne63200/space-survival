using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PointedInterractorInformationUI : ParentedUI
{
    private TextMeshProUGUI text;
    private CanvasGroup canvasGroup;
    public string input;

    private Coroutine fadeCoroutine;
    public float fadeDuration = 0.5f;

    private UnityEvent<Transform> ev_updatePointedObject;

    private IInterractable pointed = null;

    protected override void Awake()
    {
        base.Awake();
        text = GetComponent<TextMeshProUGUI>();
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

        if (oldPointed != null && pointed == null)
        {
            FadeOut();
            return;
        }

        if (oldPointed == null && pointed != null)
        {
            text.text = pointed.GetInfo() + " <" + input.ToUpper() + ">";
            FadeIn();
            return;
        }
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

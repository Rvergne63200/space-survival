using UnityEngine;
using UnityEngine.UIElements;

// Banner at the bottom of the screen while the Builder is in placement, build or destruction mode: the mode and its
// controls. Layout: BuildModeHint.uxml (label "build-mode-label"), style in Menus.uss, editable in the UI Builder
public class BuildModeUI : MonoBehaviour
{
    public static BuildModeUI Instance { get; private set; }

    [SerializeField] private GameObject panel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void Show(string text)
    {
        if (panel == null) return;

        // The UIDocument builds its elements when it is enabled
        panel.SetActive(true);

        Label label = panel.GetComponent<UIDocument>().rootVisualElement.Q<Label>("build-mode-label");

        if (label != null)
        {
            label.text = text;
        }
    }

    public void Hide()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
}

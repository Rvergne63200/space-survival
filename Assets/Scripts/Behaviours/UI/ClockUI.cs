using UnityEngine;
using UnityEngine.UIElements;

// Clock of the HUD (element "clock" of Hud.uxml, under the minimap): day and time of the world (DayNightCycle), with a
// sun by day and a moon by night. Hidden when the scene has no day / night cycle
public class ClockUI : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private Color sunColor = new Color(1f, 0.82f, 0.35f);
    [SerializeField] private Color moonColor = new Color(0.75f, 0.82f, 1f);

    private VisualElement boundRoot;
    private VisualElement clock;
    private VisualElement icon;
    private Label label;

    private string shownText;
    private bool shownDay;

    // The UIDocument builds its elements again each time it is enabled
    private bool Bind()
    {
        if (document == null || document.rootVisualElement == null) return false;

        VisualElement root = document.rootVisualElement;

        if (root == boundRoot && clock != null) return true;

        boundRoot = root;
        clock = root.Q("clock");
        icon = root.Q("clock-icon");
        label = root.Q<Label>("clock-label");
        shownText = null;

        if (icon != null)
        {
            icon.generateVisualContent += DrawIcon;
        }

        return clock != null && label != null;
    }

    private void Update()
    {
        if (!Bind()) return;

        DayNightCycle cycle = DayNightCycle.Instance;

        clock.style.display = cycle != null ? DisplayStyle.Flex : DisplayStyle.None;

        if (cycle == null) return;

        int minutes = Mathf.FloorToInt(cycle.Hour * 60f) % (24 * 60);
        string text = "Jour " + cycle.Day + "  " + (minutes / 60).ToString("00") + ":" + (minutes % 60).ToString("00");

        if (text != shownText)
        {
            shownText = text;
            label.text = text;
        }

        if (cycle.IsDay != shownDay)
        {
            shownDay = cycle.IsDay;
            icon?.MarkDirtyRepaint();
        }
    }

    // Sun: a disc and eight rays. Moon: a crescent (a disc minus the same disc moved to the right)
    private void DrawIcon(MeshGenerationContext context)
    {
        Rect rect = context.visualElement.contentRect;
        Vector2 center = rect.center;
        float size = Mathf.Min(rect.width, rect.height) / 2f;
        Painter2D painter = context.painter2D;

        if (shownDay)
        {
            float radius = size * 0.5f;

            painter.fillColor = sunColor;
            painter.BeginPath();
            painter.Arc(center, radius, 0f, 360f);
            painter.Fill();

            painter.strokeColor = sunColor;
            painter.lineWidth = Mathf.Max(1.5f, size * 0.14f);
            painter.lineCap = LineCap.Round;

            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4f;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                painter.BeginPath();
                painter.MoveTo(center + direction * radius * 1.35f);
                painter.LineTo(center + direction * size * 0.95f);
                painter.Stroke();
            }
        }
        else
        {
            float radius = size * 0.85f;
            float offset = radius * 0.6f;
            // Angle, from the center of each disc, of the two points where the discs cross
            float half = Mathf.Atan2(Mathf.Sqrt(radius * radius - offset * offset / 4f), offset / 2f) * Mathf.Rad2Deg;
            Vector2 inner = center + new Vector2(offset, 0f);

            painter.fillColor = moonColor;
            painter.BeginPath();
            painter.Arc(center, radius, half, 360f - half, ArcDirection.Clockwise);
            painter.Arc(inner, radius, 180f + half, 180f - half, ArcDirection.CounterClockwise);
            painter.ClosePath();
            painter.Fill();
        }
    }
}

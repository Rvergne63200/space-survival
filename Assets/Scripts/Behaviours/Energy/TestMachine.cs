using UnityEngine;

// Test of the electrical network: the object is red while its consumer is powered, white otherwise
[RequireComponent(typeof(EnergyConsumer))]
public class TestMachine : MonoBehaviour
{
    [SerializeField] private Color poweredColor = Color.red;
    [SerializeField] private Color unpoweredColor = Color.white;

    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private EnergyConsumer consumer;
    private MaterialPropertyBlock block;

    private void Awake()
    {
        consumer = GetComponent<EnergyConsumer>();
        block = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        consumer.ev_OnPoweredChanged.AddListener(SetPowered);
        SetPowered(consumer.IsPowered);
    }

    private void OnDisable()
    {
        consumer.ev_OnPoweredChanged.RemoveListener(SetPowered);
    }

    private void SetPowered(bool powered)
    {
        block.SetColor(ColorId, powered ? poweredColor : unpoweredColor);

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.SetPropertyBlock(block);
        }
    }
}

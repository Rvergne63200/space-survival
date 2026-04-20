using UnityEngine;
using UnityEngine.Events;

public class BuildVerifier : MonoBehaviour
{
    public UnityEvent<bool> ev_updateValidity;


    private int _invalidCollision = 0;
    private int InvalidCollision { 
        get => _invalidCollision; 
        set {
            if (_invalidCollision == value) return;

            _invalidCollision = value;
            RecalculateValidity();
        } 
    }
    

    private bool _isValid = false;
    public bool IsValid
    {
        get => _isValid;
        set
        {
            if(_isValid == value) return;

            _isValid = value;
            ev_updateValidity?.Invoke(_isValid);
        }
    }


    private int terrainMask;


    private Vector3 lastPosition;
    private float moveThreshold = 0.5f;

    private void Awake()
    {
        ev_updateValidity = new UnityEvent<bool>();
        terrainMask = LayerMask.GetMask("Terrain");
    }

    private void RecalculateValidity()
    {
        IsValid = (InvalidCollision <= 0) && CheckFlatness();
    }

    public void OnTriggerEnter(Collider other)
    {
        InvalidCollision++;
    }

    public void OnTriggerExit(Collider other)
    {
        InvalidCollision--;
    }

    public bool CheckFlatness(float maxSlope = 2f, int samples = 5)
    {
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        float size = Mathf.Max(transform.localScale.x, transform.localScale.z);

        bool hasHit = false;

        for (int x = 0; x < samples; x++)
        {
            for (int z = 0; z < samples; z++)
            {
                Vector3 offset = new Vector3(
                    (x / (float)(samples - 1) - 0.5f) * size,
                    10f,
                    (z / (float)(samples - 1) - 0.5f) * size
                );

                Vector3 origin = transform.position + offset;

                if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 50f, terrainMask))
                {
                    hasHit = true;

                    minY = Mathf.Min(minY, hit.point.y);
                    maxY = Mathf.Max(maxY, hit.point.y);
                }
            }
        }

        if(!hasHit)
        {
            return false;
        }

        return (maxY - minY) <= maxSlope;
    }

    private void LateUpdate()
    {
        if (Vector3.Distance(transform.position, lastPosition) > moveThreshold)
        {
            RecalculateValidity();
            lastPosition = transform.position;
        }
    }
}

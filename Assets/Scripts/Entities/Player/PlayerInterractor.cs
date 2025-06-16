using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInterractor : MonoBehaviour
{
    public Transform source;
    private PlayerInputActions inputActions;

    public UnityEvent<IInterractable> ev_UpdatePointed;

    private IInterractable _pointed;
    public IInterractable Pointed { 
        get 
        { 
            return _pointed; 
        } 
        
        set 
        { 
            _pointed = value;
            ev_UpdatePointed.Invoke(value);
        } 
    }

    void Awake()
    {
        ev_UpdatePointed = new UnityEvent<IInterractable>();
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Actions.Enable();
        inputActions.Actions.Interract.performed += OnInterractPerformed;
    }

    private void OnDisable()
    {
        inputActions.Actions.Interract.performed -= OnInterractPerformed;
        inputActions.Actions.Disable();
    }

    private void OnInterractPerformed(InputAction.CallbackContext context)
    {
        Interract();
    }

    private void Update()
    {
        Ray ray = new Ray(source.position, source.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            IInterractable interractable = hit.transform.GetComponent<IInterractable>();

            if (Pointed != interractable)
            {
                Pointed = interractable;
            }
        }
        else if(Pointed != null)
        {
            Pointed = null;
        }
    }

    private void Interract()
    {
        if (Pointed != null)
        {
            Pointed.Interract(gameObject);
        }
    }
}

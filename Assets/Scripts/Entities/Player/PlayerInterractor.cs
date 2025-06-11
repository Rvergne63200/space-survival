using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInterractor : MonoBehaviour
{
    public Transform source;
    private PlayerInputActions inputActions;

    void Awake()
    {
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

    private void Interract()
    {
        Ray ray = new Ray(source.position, source.forward);
        
        if(Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            IInterractable[] interractables = hit.transform.GetComponents<IInterractable>();

            foreach(IInterractable interractable in interractables)
            {
                interractable.Interract(gameObject);
            }
        }
    }
}

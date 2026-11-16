using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInterractor : MonoBehaviour
{
    private PlayerInputActions inputActions;
    private PlayerContext playerContext;

    void Awake()
    {
        inputActions = new PlayerInputActions();
        playerContext = GetComponent<PlayerContext>();
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
        playerContext.Interractable?.Interract(gameObject);
    }
}

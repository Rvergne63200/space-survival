using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInterractor : MonoBehaviour
{
    private PlayerInputActions inputActions;
    private PlayerContext playerContext => GetComponent<PlayerContext>();

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
        playerContext.Interractable?.Interract(gameObject);
    }
}

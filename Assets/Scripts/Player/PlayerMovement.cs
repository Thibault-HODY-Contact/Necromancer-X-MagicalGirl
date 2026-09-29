using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] InputActionReference movementInput;
    [SerializeField] float speed = 5f;

    PlayerInput playerInput;
    InputAction moveAction;
    Vector2 moveDirection;

    void Start()
    {
        playerInput = GetComponentInParent<PlayerInput>();

       
        playerInput.SwitchCurrentActionMap("Player");

        moveAction = playerInput.actions.FindAction(movementInput.action.id);

        moveAction.performed += HandleMove;
        moveAction.canceled += HandleMove;
    }

    void OnDestroy()
    {
        if (moveAction == null) return;

        moveAction.performed -= HandleMove;
        moveAction.canceled -= HandleMove;
    }

    void HandleMove(InputAction.CallbackContext context)
    {
        Debug.Log($"Joueur {playerInput.playerIndex} reçoit {context.ReadValue<Vector2>()} " +
                  $"depuis {context.control.device.displayName}");
        moveDirection = context.ReadValue<Vector2>();
    }

    void Update()
    {
        Vector3 move = new Vector3(moveDirection.x, 0f, moveDirection.y);
        transform.Translate(move * speed * Time.deltaTime, Space.World);
    }
}
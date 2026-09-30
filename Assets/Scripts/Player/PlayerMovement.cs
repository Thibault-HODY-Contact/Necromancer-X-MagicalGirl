using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] InputActionReference _movementInput;
    [SerializeField] float _speed = 5f;

    PlayerInput _playerInput;
    InputAction _moveAction;
    Vector2 _moveDirection;

    void Start()
    {
        _playerInput = GetComponent<PlayerInput>();


        _playerInput.SwitchCurrentActionMap("Player");

        _moveAction = _playerInput.actions.FindAction(_movementInput.action.id);

        _moveAction.performed += HandleMove;
        _moveAction.canceled += HandleMove;
    }

    void OnDestroy()
    {
        if (_moveAction == null) return;

        _moveAction.performed -= HandleMove;
        _moveAction.canceled -= HandleMove;
    }

    void HandleMove(InputAction.CallbackContext context)
    {
        _moveDirection = context.ReadValue<Vector2>();
    }

    void Update()
    {
        Vector3 move = new Vector3(_moveDirection.x, 0f, _moveDirection.y);
        transform.Translate(move * _speed * Time.deltaTime, Space.Self);
    }
}
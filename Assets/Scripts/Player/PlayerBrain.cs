using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBrain : MonoBehaviour
{
    [SerializeField] InputActionReference _movementInput; 
    [SerializeField] PlayerMovement _playerMovement;

    PlayerInput _playerInput; 
    InputAction _moveAction; 

    void Start()
    {
        #region PlayerInput
        _playerInput = GetComponent<PlayerInput>();

        _playerInput.SwitchCurrentActionMap("Player");
        #endregion

        #region Move
        _moveAction = _playerInput.actions.FindAction(_movementInput.action.id);

        _moveAction.performed += UpdateMove;
        _moveAction.canceled += UpdateMove;
        #endregion
    }

    void OnDestroy()
    {
        if (_moveAction == null) return;

        _moveAction.performed -= UpdateMove;
        _moveAction.canceled -= UpdateMove;
    }

    void UpdateMove(InputAction.CallbackContext context)
    {
        _playerMovement.SetDirection(context.ReadValue<Vector2>()); 
    }

}

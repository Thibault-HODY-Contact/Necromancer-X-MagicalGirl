using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBrain : MonoBehaviour
{
    [SerializeField] InputActionReference _movementInput; 
    [SerializeField] InputActionReference _lookInput;

    [SerializeField] MoveSystem _moveSystem;
    [SerializeField] LookSystem _lookSystem;

    //[SerializeField] Transform _modelTransform;

    PlayerInput _playerInput; 
    InputAction _moveAction; 
    InputAction _lookAction;


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

        #region Look
        _lookAction = _playerInput.actions.FindAction(_lookInput.action.id);

        _lookAction.performed += UpdateLook;
        _lookAction.canceled += UpdateLook;
        #endregion
    }

    void OnDestroy()
    {
        if (_moveAction == null) return;

        _moveAction.performed -= UpdateMove;
        _moveAction.canceled -= UpdateMove;

        _lookAction.performed -= UpdateLook;
        _lookAction.canceled -= UpdateLook;
    }

    void UpdateMove(InputAction.CallbackContext context)
    {
        _moveSystem.MoveDirection = context.ReadValue<Vector2>(); 
    }

    void UpdateLook(InputAction.CallbackContext context)
    {
        _lookSystem.LookDirection = context.ReadValue<Vector2>();
    }
}

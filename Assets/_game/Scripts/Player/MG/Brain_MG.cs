using UnityEngine;
using UnityEngine.InputSystem;

public class Brain_MG : MonoBehaviour
{
    [SerializeField] InputActionReference _movementInput; 
    [SerializeField] InputActionReference _lookInput;
    [SerializeField] InputActionReference _attackInput;


    [SerializeField] MoveSystem _moveSystem;
    [SerializeField] LookSystem _lookSystem;
    [SerializeField] ShootSystem _shootSystem;


    PlayerInput _playerInput; 
    InputAction _moveAction; 
    InputAction _lookAction;
    InputAction _attackAction;

    // Clocks
    [SerializeField, ReadOnly] float _competence1Timer = 0f;
    [SerializeField]           float _competence1Cooldown = 0.5f;

    [SerializeField, ReadOnly] float _competence2Timer = 0f;
    [SerializeField]           float _competence2Cooldown = 0.5f;


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

        #region Attack
        _attackAction = _playerInput.actions.FindAction(_attackInput.action.id);

        _competence1Timer = _competence1Cooldown;

        _attackAction.canceled += _shootSystem.CancelShoot;
        #endregion
    }

    void OnDestroy()
    {
        if (_moveAction == null) return;

        _moveAction.performed -= UpdateMove;
        _moveAction.canceled -= UpdateMove;

        _lookAction.performed -= UpdateLook;
        _lookAction.canceled -= UpdateLook;

        _attackAction.canceled -= _shootSystem.CancelShoot;
    }

    void Update()
    {
        _competence1Timer = Mathf.Max(0, _competence1Timer - Time.deltaTime);

        if (_attackAction.IsPressed() && _competence1Timer == 0 && _shootSystem.CanShoot)
        {
            _shootSystem.Shoot();
            _competence1Timer = _competence1Cooldown;
        }
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

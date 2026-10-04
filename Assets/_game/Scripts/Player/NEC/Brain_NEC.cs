using UnityEngine;
using UnityEngine.InputSystem;

public class Brain_NEC : MonoBehaviour
{
    [SerializeField] InputActionReference _movementInput;
    [SerializeField] InputActionReference _lookInput;
    [SerializeField] InputActionReference _attack2Input;

    [SerializeField] MoveSystem _moveSystem;
    [SerializeField] LookSystem _lookSystem;
    [SerializeField] AttackSystem _attack2System;
    [SerializeField] TeleportationSystem _teleportationSystem;

    [SerializeField, ReadOnly] float _competence2Timer = 0f;
    [SerializeField] float _competence2Cooldown = 2f;
    [SerializeField, Min(0f)] float _teleportationDelay = 0.25f;
    [SerializeField, ReadOnly] float _teleportationTimer;

    PlayerInput _playerInput;
    InputAction _moveAction;
    InputAction _lookAction;
    InputAction _attack2Action;

    bool _wasMoveEnabled;
    bool _wasLookEnabled;
    public bool IsUsingCompetence2 { get; private set; }

    void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerInput.SwitchCurrentActionMap("Player");

        _moveAction = _playerInput.actions.FindAction(_movementInput.action.id);
        _moveAction.performed += UpdateMove;
        _moveAction.canceled += UpdateMove;

        _lookAction = _playerInput.actions.FindAction(_lookInput.action.id);
        _lookAction.performed += UpdateLook;
        _lookAction.canceled += UpdateLook;

        _attack2Action = _playerInput.actions.FindAction(_attack2Input.action.id);
    }

    void Update()
    {
        _competence2Timer = Mathf.Max(0f, _competence2Timer - Time.deltaTime);

        if (IsUsingCompetence2)
        {
            _teleportationTimer = Mathf.Max(0f, _teleportationTimer - Time.deltaTime);
            if (_teleportationTimer > 0f) return;

            _attack2System.StopAttack();
            _teleportationSystem.Teleport();
            _attack2System.Attack(ignoreDelay: true);
            EndCompetence2();
            return;
        }

        if (_attack2Action.IsPressed() && _competence2Timer == 0f && _attack2System.CanAttack)
        {
            _wasMoveEnabled = _moveSystem.enabled;
            _wasLookEnabled = _lookSystem.enabled;
            _moveSystem.enabled = false;
            _lookSystem.enabled = false;

            IsUsingCompetence2 = true;
            _teleportationTimer = _teleportationDelay;
            _competence2Timer = _competence2Cooldown;
            _attack2System.Attack();
        }
    }

    void EndCompetence2()
    {
        IsUsingCompetence2 = false;
        _teleportationTimer = 0f;

        if (_moveSystem != null) _moveSystem.enabled = _wasMoveEnabled;
        if (_lookSystem != null) _lookSystem.enabled = _wasLookEnabled;
    }

    void OnDisable()
    {
        if (IsUsingCompetence2) EndCompetence2();
        if (_attack2System != null) _attack2System.StopAttack();
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

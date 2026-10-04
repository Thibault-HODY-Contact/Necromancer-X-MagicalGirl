using UnityEngine;
using UnityEngine.InputSystem;

public class Brain_MG : MonoBehaviour
{
    [SerializeField] InputActionReference _movementInput; 
    [SerializeField] InputActionReference _lookInput;
    [SerializeField] InputActionReference _attackInput;
    [SerializeField] InputActionReference _attack2Input;


    [SerializeField] MoveSystem _moveSystem;
    [SerializeField] LookSystem _lookSystem;
    [SerializeField] ShootSystem _shootSystem;
    [SerializeField] ShootSystem _shoot2System;


    PlayerInput _playerInput; 
    InputAction _moveAction; 
    InputAction _lookAction;
    InputAction _attackAction;
    InputAction _attack2Action;

    Brain_Proj2_MG _competence2Projectile;
    bool _wasLookEnabled;

    public bool IsUsingCompetence2 => _competence2Projectile != null
        || (_shoot2System != null && _shoot2System.HasPendingShoot);

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

        #region Attack2
        if (_attack2Input != null && _shoot2System != null)
        {
            _attack2Action = _playerInput.actions.FindAction(_attack2Input.action.id);

            _competence2Timer = _competence2Cooldown;

            _attack2Action.canceled += _shoot2System.CancelShoot;
            _shoot2System.OnShoot += OnShoot2;
        }
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

        if (_attack2Action != null)
        {
            _attack2Action.canceled -= _shoot2System.CancelShoot;
            _shoot2System.OnShoot -= OnShoot2;

            if (_competence2Projectile != null)
            {
                EndCompetence2();
            }
        }
    }

    void Update()
    {
        _competence1Timer = Mathf.Max(0, _competence1Timer - Time.deltaTime);
        _competence2Timer = Mathf.Max(0, _competence2Timer - Time.deltaTime);

        if (IsUsingCompetence2) return;

        if (_attack2Action != null && _attack2Action.IsPressed()
            && _competence2Timer == 0 && _shoot2System.CanShoot)
        {
            _shootSystem.CancelShoot();
            _shoot2System.Shoot();
            _competence2Timer = _competence2Cooldown;
            return;
        }

        if (_attackAction.IsPressed() && _competence1Timer == 0 && _shootSystem.CanShoot)
        {
            _shootSystem.Shoot();
            _competence1Timer = _competence1Cooldown;
        }
    }

    void OnShoot2()
    {
        _competence2Projectile = _shoot2System.LastProjectile.GetComponentInChildren<Brain_Proj2_MG>();
        if (_competence2Projectile == null) return;

        _wasLookEnabled = _lookSystem.enabled;
        _lookSystem.enabled = false;

        _competence2Projectile.Rotated += Rotate;
        _competence2Projectile.Finished += EndCompetence2;
    }

    void Rotate(float angle)
    {
        _lookSystem.Rotate(angle);
    }

    void EndCompetence2()
    {
        if (_competence2Projectile != null)
        {
            _competence2Projectile.Rotated -= Rotate;
            _competence2Projectile.Finished -= EndCompetence2;
        }

        _competence2Projectile = null;

        if (_lookSystem != null)
        {
            _lookSystem.enabled = _wasLookEnabled;
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

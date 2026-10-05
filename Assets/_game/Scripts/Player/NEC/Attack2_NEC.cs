using UnityEngine;

public class Attack2_NEC : MonoBehaviour
{
    [SerializeField] AttackSystem _attackSystem;
    [SerializeField] TeleportationSystem _teleportationSystem;
    [SerializeField] MoveSystem _moveSystem;
    [SerializeField] LookSystem _lookSystem;

    [SerializeField, Min(0f)] float _teleportationDelay = 0.25f;
    [SerializeField, ReadOnly] float _teleportationTimer;

    bool _isTeleporting;
    bool _wasMoveEnabled;
    bool _wasLookEnabled;

    public bool IsAttacking => _isTeleporting || _attackSystem.IsAttacking;
    public bool CanAttack => isActiveAndEnabled && !IsAttacking && _attackSystem.CanAttack;

    public void StartAttack()
    {
        if (!CanAttack) return;

        _wasMoveEnabled = _moveSystem.enabled;
        _wasLookEnabled = _lookSystem.enabled;
        _moveSystem.enabled = false;
        _lookSystem.enabled = false;

        _isTeleporting = true;
        _teleportationTimer = _teleportationDelay;
        _attackSystem.Attack();
    }

    void Update()
    {
        if (!_isTeleporting) return;

        _teleportationTimer = Mathf.Max(0f, _teleportationTimer - Time.deltaTime);
        if (_teleportationTimer > 0f) return;

        _attackSystem.StopAttack();
        _teleportationSystem.Teleport();
        _attackSystem.Attack(ignoreDelay: true);
        EndTeleportation();
    }

    void EndTeleportation()
    {
        _isTeleporting = false;
        _teleportationTimer = 0f;

        if (_moveSystem != null) _moveSystem.enabled = _wasMoveEnabled;
        if (_lookSystem != null) _lookSystem.enabled = _wasLookEnabled;
    }

    public void StopAttack()
    {
        if (_isTeleporting) EndTeleportation();
        if (_attackSystem != null) _attackSystem.StopAttack();
    }

    void OnDisable()
    {
        StopAttack();
    }
}

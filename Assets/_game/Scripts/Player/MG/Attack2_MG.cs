using UnityEngine;

public class Attack2_MG : MonoBehaviour
{
    [SerializeField] ShootSystem _shootSystem;
    [SerializeField] LookSystem _lookSystem;

    Brain_Proj2_MG _projectile;
    bool _wasLookEnabled;

    public bool IsAttacking => _projectile != null || _shootSystem.HasPendingShoot;
    public bool CanAttack => isActiveAndEnabled && !IsAttacking
        && _shootSystem.isActiveAndEnabled && _shootSystem.CanShoot;

    void OnEnable()
    {
        _shootSystem.OnShoot += OnShoot;
    }

    void OnDisable()
    {
        if (_shootSystem != null)
        {
            _shootSystem.OnShoot -= OnShoot;
        }

        StopAttack();
    }

    public void StartAttack()
    {
        if (!CanAttack) return;

        _shootSystem.Shoot();
    }

    public void CancelAttack()
    {
        // Relâcher la commande annule le tir en attente, sans interrompre l'orbite.
        if (_shootSystem != null) _shootSystem.CancelShoot();
    }

    public void StopAttack()
    {
        CancelAttack();
        if (_projectile == null) return;

        GameObject instance = _projectile.transform.root.gameObject;
        EndAttack();
        if (instance != null) Destroy(instance);
    }

    void OnShoot()
    {
        _projectile = _shootSystem.LastProjectile.GetComponentInChildren<Brain_Proj2_MG>();
        if (_projectile == null) return;

        _wasLookEnabled = _lookSystem.enabled;
        _lookSystem.enabled = false;

        _projectile.Rotated += Rotate;
        _projectile.Finished += EndAttack;
    }

    void Rotate(float angle)
    {
        _lookSystem.Rotate(angle);
    }

    void EndAttack()
    {
        if (_projectile != null)
        {
            _projectile.Rotated -= Rotate;
            _projectile.Finished -= EndAttack;
        }

        _projectile = null;
        if (_lookSystem != null) _lookSystem.enabled = _wasLookEnabled;
    }
}

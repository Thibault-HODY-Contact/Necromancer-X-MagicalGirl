using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShootSystem : MonoBehaviour
{
    [SerializeField] Transform _shootPoint;
    [SerializeField] GameObject _prefab;

    [SerializeField, ReadOnly] float _delayTimer;
    [SerializeField, Min(0f)]  float _delay;

    bool _hasShoot = false;
    public bool CanShoot => _delayTimer == 0f && !_hasShoot;

    public Action OnShoot;

    void Reset()
    {
        _delay = 0.5f;
        _delayTimer = _delay;
    }

    private void Start()
    {
    }

    void Update()
    {
        if (_delay == 0f) return;

        _delayTimer = Mathf.Max(0f, _delayTimer - Time.deltaTime);

        if (_delayTimer == 0f && _hasShoot)
        {
            Instantiate(_prefab, _shootPoint.position, _shootPoint.rotation);
            OnShoot?.Invoke();
            _hasShoot = false;
        }
    }

    public void Shoot()
    {
        if (_delay == 0f)
        {
            Instantiate(_prefab, _shootPoint.position, _shootPoint.rotation);
            OnShoot?.Invoke();
            return;
        }
        if (_delayTimer > 0f) return;

        _hasShoot = true;
        _delayTimer = _delay;
    }

    public void CancelShoot(InputAction.CallbackContext context)
    {
        _hasShoot = false;
        _delayTimer = 0f;
    }
}

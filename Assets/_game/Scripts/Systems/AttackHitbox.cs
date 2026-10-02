using System;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private Collider _collider;

    public Collider Collider
    {
        get
        {
            // Also works when the hitbox GameObject has not run Awake yet.
            if (_collider == null)
            {
                _collider = GetComponent<Collider>();
            }

            return _collider;
        }
    }

    public event Action<Collider, Collider> ContactDetected;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        ContactDetected?.Invoke(Collider, other);
    }
}

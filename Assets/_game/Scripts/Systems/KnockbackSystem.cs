using System;
using UnityEngine;

public class KnockbackSystem : MonoBehaviour
{
    [SerializeField] Rigidbody _rb;

    public Action<int, Vector2> OnKnockback;



    public void Knockback(int Amount, Vector3 direction)
    {
        _rb.AddForce(direction * Amount, ForceMode.Impulse);

        OnKnockback?.Invoke(Amount, direction);
    }
}

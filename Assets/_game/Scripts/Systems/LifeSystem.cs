using System;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class LifeSystem : MonoBehaviour
{
    [SerializeField, Min(0)] int _maxHealth = 100;
    [SerializeField, ReadOnly] int _health = 100;

    public int Health
    {
        get => _health;
        private set
        {
            if (value > _maxHealth)
            {
                value = _maxHealth;
            }
            if (value < 0)
            {
                value = 0;
            }
            _health = value;

            if (_health == 0)
            {
                OnDeath?.Invoke();
            }
        }
    }

    public bool IsDead => _health == 0;

    public Action<int, int> OnHeal;
    public Action<int, int> OnDamage;
    public Action OnDeath;

    private void Reset()
    {
        _maxHealth = 100;
        _health = _maxHealth;
    }

    public void Heal(int amount) {
        if (IsDead) return;

        int newHealth = Mathf.Min(Health + amount, _maxHealth);

        OnHeal?.Invoke(Health, newHealth);

        Health = newHealth;
    }

    public void TakeDamage(int amount) {
        if (IsDead) return;

        Debug.Log($"{gameObject.name} took {amount} damage.");

        int newHealth = Mathf.Max(Health - amount, 0);

        OnDamage?.Invoke(Health, newHealth);

        Health = newHealth;

        Debug.Log($"{gameObject.name} health is now {Health}.");
    }
}

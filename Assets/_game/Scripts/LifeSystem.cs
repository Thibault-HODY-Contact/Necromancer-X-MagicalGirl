using System;
using UnityEngine;

public class LifeSystem : MonoBehaviour
{
    int _health = 100;
    int _maxHealth = 100;

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
            if (value > _health)
            {
                OnHeal?.Invoke(_health, value);
            }
            else if (value < _health)
            {
                OnDamage?.Invoke(_health, value);
            }
            _health = value;
        }
    }

    public Action<int, int> OnHeal;
    public Action<int, int> OnDamage;

    public void Heal(int amount) {
        Health += amount;
    }

    public void Damage(int amount) {
        Health -= amount;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HitboxSettings
{
    public AttackHitbox hitbox;
    
    [SerializeField, Min(0f)] float _delay;
    public float Delay => _delay;
    [SerializeField,ReadOnly] internal float _delayTimer;

    [SerializeField, Min(0f)] float _duration;
    public float Duration => _duration;
    [SerializeField,ReadOnly] internal float _durationTimer;

    [SerializeField,ReadOnly] internal bool isActivated;
    [SerializeField,ReadOnly] internal bool isDesactivated;

    [SerializeField, Min(0)] int _damage;
    [SerializeField] int _knockback;

    public int Damage { get => _damage; set => _damage = value; }
    public int Knockback { get => _knockback; set => _knockback = value; }
}

public class AttackSystem : MonoBehaviour
{
    [SerializeField] List<HitboxSettings> _hitboxes = new List<HitboxSettings>();
    [SerializeField] List<string> _damageableTags = new List<string>();
    [SerializeField] List<string> _knockbackableTags = new List<string>();

    public List<HitboxSettings> Hitboxes => _hitboxes;

    private bool _isInitialized;

    public bool IsAttacking { get; private set; }
    public bool CanAttack => _isInitialized && isActiveAndEnabled
        && !IsAttacking;

    public event Action Activated;
    public event Action Desactivated;
    public event Action OnHit;

    private void Awake()
    {
        _isInitialized = true;
        if (_hitboxes == null || _hitboxes.Count == 0)
        {
            Debug.LogError("AttackSystem : configure au moins une hitbox.", this);
            enabled = false;
            return;
        }

        foreach (HitboxSettings settings in _hitboxes)
        {
            if (settings == null || settings.hitbox == null
                || settings.hitbox.Collider == null)
            {
                Debug.LogError("AttackSystem : chaque hitbox doit avoir une référence et un Collider.", this);
                enabled = false;
                return;
            }
        }

        foreach (HitboxSettings settings in _hitboxes)
        {
            ResetHitbox(settings);
        }

        _isInitialized = true;
    }


    private void OnEnable()
    {
        if (!_isInitialized) return;

        foreach (HitboxSettings settings in _hitboxes)
        {
            if (settings.hitbox != null)
            {
                settings.hitbox.ContactDetected += HandleContact;
            }
        }
    }

    private void Update()
    {
        if (!IsAttacking) return;

        bool hasPendingHitbox = false;

        foreach (HitboxSettings settings in _hitboxes)
        {
            if (settings.isDesactivated) continue;

            if (settings.hitbox == null || settings.hitbox.Collider == null)
            {
                ResetHitbox(settings);
                continue;
            }

            float deltaTime = Time.deltaTime;

            if (!settings.isActivated)
            {
                float waitingTime = Mathf.Min(settings._delayTimer, deltaTime);
                settings._delayTimer = settings._delayTimer - deltaTime;
                deltaTime -= waitingTime;

                if (settings._delayTimer > 0f)
                {
                    hasPendingHitbox = true;
                    continue;
                }

                settings.isActivated = true;
                settings.hitbox.Collider.enabled = true;
                Activated?.Invoke();

                if (!isActiveAndEnabled || !IsAttacking) return;
            }

            settings._durationTimer = settings._durationTimer - deltaTime;

            if (settings._durationTimer <= 0f)
            {
                settings.isDesactivated = true;
                settings.hitbox.Collider.enabled = false;
                Desactivated?.Invoke();

                if (!isActiveAndEnabled || !IsAttacking) return;
            }
            else
            {
                hasPendingHitbox = true;
            }
        }

        IsAttacking = hasPendingHitbox;
    }

    private void OnDisable()
    {
        if (!_isInitialized) return;

        IsAttacking = false;

        foreach (HitboxSettings settings in _hitboxes)
        {
            if (settings.hitbox != null)
            {
                settings.hitbox.ContactDetected -= HandleContact;
            }

            bool wasActive = settings.isActivated && !settings.isDesactivated;
            ResetHitbox(settings);

            if (wasActive)
            {
                Desactivated?.Invoke();
            }
        }
    }

    private static void ResetHitbox(HitboxSettings settings)
    {
        settings._delayTimer = 0f;
        settings._durationTimer = 0f;
        settings.isActivated = true;
        settings.isDesactivated = true;

        if (settings.hitbox != null && settings.hitbox.Collider != null)
        {
            settings.hitbox.Collider.enabled = false;
        }
    }

    private void HandleContact(Collider source, Collider target)
    {
        GameObject root = target.transform.root.gameObject;

        foreach (string tag in _damageableTags)
        {
            LifeSystem lifeSystem = root.GetComponentInChildren<LifeSystem>();
            if (root.CompareTag(tag) && lifeSystem != null)
            {
                Debug.Log($"{target.name} est endommagé par {source.name}");

                HitboxSettings settings = _hitboxes.Find(candidate => candidate.hitbox.Collider == source);

                if (settings == null) return;

                lifeSystem.TakeDamage(settings.Damage);
                OnHit?.Invoke();
                break;
            }
        }
    }

    public void Attack()
    {
        if (!CanAttack) return;

        IsAttacking = true;

        foreach (HitboxSettings settings in _hitboxes)
        {
            ResetHitbox(settings);
            settings._delayTimer = settings.Delay;
            settings._durationTimer = settings.Duration;
            settings.isActivated = false;
            settings.isDesactivated = false;
        }
    }
}

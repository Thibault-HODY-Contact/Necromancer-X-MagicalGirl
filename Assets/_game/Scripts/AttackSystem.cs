using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HitboxSettings
{
    public AttackHitbox hitbox;


    [SerializeField, Min(0f)] private float _delay;
    public float Delay
    {
        get => _delay;
        private set => _delay = Mathf.Max(0f, value);
    }

    private float _delayTimer;
    public float DelayTimer
    {
        get => _delayTimer;
        set => _delayTimer = Mathf.Max(0f, value);
    }

    [SerializeField, ReadOnly]
    internal bool isActivated;

    [SerializeField, Min(0f)] private float _duration;
    public float Duration
    {
        get => _duration;
        private set => _duration = Mathf.Max(0f, value);
    }

    private float _durationTimer;
    public float DurationTimer
    {
        get => _durationTimer;
        set => _durationTimer = Mathf.Max(0f, value);
    }

    [SerializeField, ReadOnly]
    internal bool isDesactivated;

    public int damage;
    public int knockback;
}

public class AttackSystem : MonoBehaviour
{
    [SerializeField] List<HitboxSettings> _hitboxes = new List<HitboxSettings>();
    [SerializeField, Min(0f)] float _cooldown;
    public float Cooldown
    {
        get => _cooldown;
        set => _cooldown = Mathf.Max(0f, value);
    }

    private float _cooldownTimer;
    private bool _isInitialized;

    public bool IsAttacking { get; private set; }
    public float CooldownRemaining => _cooldownTimer;
    public bool CanAttack => _isInitialized && isActiveAndEnabled
        && !IsAttacking && _cooldownTimer <= 0f;

    public event Action Activated;
    public event Action Desactivated;

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
        _cooldownTimer = Mathf.Max(0f, _cooldownTimer - Time.deltaTime);

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
                float waitingTime = Mathf.Min(settings.DelayTimer, deltaTime);
                settings.DelayTimer = settings.DelayTimer - deltaTime;
                deltaTime -= waitingTime;

                if (settings.DelayTimer > 0f)
                {
                    hasPendingHitbox = true;
                    continue;
                }

                settings.isActivated = true;
                settings.hitbox.Collider.enabled = true;
                Activated?.Invoke();

                if (!isActiveAndEnabled || !IsAttacking) return;
            }

            settings.DurationTimer = settings.DurationTimer - deltaTime;

            if (settings.DurationTimer <= 0f)
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
        settings.DelayTimer = 0f;
        settings.DurationTimer = 0f;
        settings.isActivated = true;
        settings.isDesactivated = true;

        if (settings.hitbox != null && settings.hitbox.Collider != null)
        {
            settings.hitbox.Collider.enabled = false;
        }
    }

    private void HandleContact(Collider source, Collider target)
    {
        Debug.Log($"{source.name} a touché {target.name}");
    }

    public void Attack()
    {
        if (!CanAttack) return;

        IsAttacking = true;
        _cooldownTimer = Cooldown;

        foreach (HitboxSettings settings in _hitboxes)
        {
            ResetHitbox(settings);
            settings.DelayTimer = settings.Delay;
            settings.DurationTimer = settings.Duration;
            settings.isActivated = false;
            settings.isDesactivated = false;
        }
    }
}
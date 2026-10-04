using System;
using UnityEngine;

public class OrbitSystem : MonoBehaviour
{
    [SerializeField] Transform _rootTransform;
    [SerializeField, Min(0f)] float _radius = 2f;
    [SerializeField, Min(0f)] float _speed = 180f;
    [SerializeField, Min(0f)] float _acceleration = 180f;
    [SerializeField] float _height = 1f;

    [SerializeField, ReadOnly] Transform _center;
    [SerializeField, ReadOnly] float _angle;
    [SerializeField, ReadOnly] float _rotationAngle;
    [SerializeField, ReadOnly] float _currentSpeed;

    public float Duration
    {
        get
        {
            if (_acceleration > 0f)
            {
                // Temps nécessaire pour parcourir 360 degrés avec une accélération constante.
                return 720f / (_speed + Mathf.Sqrt(_speed * _speed + 720f * _acceleration));
            }

            return _speed > 0f ? 360f / _speed : 0f;
        }
    }
    public bool IsOrbiting { get; private set; }

    public event Action<float> Rotated;
    public event Action Completed;

    void Reset()
    {
        _rootTransform = transform.root;
    }

    void Awake()
    {
        if (_rootTransform == null)
        {
            _rootTransform = transform.root;
        }
    }

    public void Orbit(Transform center)
    {
        _center = center;
        _rotationAngle = 0f;
        _currentSpeed = _speed;
        IsOrbiting = _center != null && Duration > 0f;
        if (!IsOrbiting) return;

        Vector3 direction = _rootTransform.position - _center.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
        {
            direction = _center.forward;
        }

        _angle = Vector3.SignedAngle(Vector3.forward, direction, Vector3.up);
        UpdatePosition();
    }

    void LateUpdate()
    {
        if (!IsOrbiting || _center == null) return;

        float deltaTime = Time.deltaTime;
        float rotation = _currentSpeed * deltaTime + 0.5f * _acceleration * deltaTime * deltaTime;
        _currentSpeed += _acceleration * deltaTime;
        rotation = Mathf.Min(rotation, 360f - _rotationAngle);
        _rotationAngle += rotation;
        _angle = Mathf.Repeat(_angle + rotation, 360f);
        UpdatePosition();

        Rotated?.Invoke(rotation);

        if (_rotationAngle >= 360f)
        {
            IsOrbiting = false;
            Completed?.Invoke();
        }
    }

    void UpdatePosition()
    {
        Vector3 offset = Quaternion.AngleAxis(_angle, Vector3.up) * (Vector3.forward * _radius);

        _rootTransform.position = _center.position + offset + Vector3.up * _height;
        _rootTransform.forward = Quaternion.AngleAxis(_angle, Vector3.up) * Vector3.forward;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] bool _hitOnceUntilExit;

    private Collider _collider;
    readonly HashSet<Collider> _contacts = new HashSet<Collider>();
    readonly HashSet<Transform> _hitTargets = new HashSet<Transform>();

    public Transform IgnoredRoot { get; set; }

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
        _contacts.Add(other);
        DetectContact(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (_hitOnceUntilExit) DetectContact(other);
    }

    void DetectContact(Collider other)
    {
        if (!isActiveAndEnabled || !Collider.enabled) return;
        Transform root = other.transform.root;
        if (IgnoredRoot != null && root == IgnoredRoot.root) return;
        if (_hitOnceUntilExit && !_hitTargets.Add(root)) return;

        ContactDetected?.Invoke(Collider, other);
    }

    private void OnTriggerExit(Collider other)
    {
        _contacts.Remove(other);
        Transform root = other.transform.root;
        foreach (Collider contact in _contacts)
        {
            if (contact != null && contact.transform.root == root) return;
        }

        _hitTargets.Remove(root);
    }

    public void ResetHits()
    {
        _hitTargets.Clear();
    }

    private void OnDisable()
    {
        _contacts.Clear();
        ResetHits();
    }
}

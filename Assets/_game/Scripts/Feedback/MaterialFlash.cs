using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MaterialFlash : MonoBehaviour
{
    [SerializeField] GameObject _visualRoot;

    [SerializeField] LifeSystem _lifeSystem;
    [SerializeField] Material _flashMaterial;

    [SerializeField, Min(0f)] float _flashDuration;
    [SerializeField, ReadOnly] float _flashTimer;

    List<Renderer> _renderers = new List<Renderer>();
    List<Material[]> _originalMaterials = new List<Material[]>();
    List<Material[]> _flashMaterials = new List<Material[]>();

    bool _isFlashing = false;

    void Start()
    {
        _renderers = new List<Renderer>(_visualRoot.GetComponentsInChildren<Renderer>());

        foreach (Renderer renderer in _renderers)
        {
            _originalMaterials.Add(renderer.sharedMaterials);

            Material[] materials = new Material[renderer.sharedMaterials.Length];

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = _flashMaterial;
            }

            _flashMaterials.Add(materials);
        }
    }

    void Flash()
    {
        for (int i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].sharedMaterials = _flashMaterials[i];
        }

        _isFlashing = true;
        _flashTimer = _flashDuration;
    }

    void Update()
    {
        if (_isFlashing)
        {
            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0f)
            {
                _isFlashing = false;
                for (int i = 0; i < _renderers.Count; i++)
                {
                    _renderers[i].sharedMaterials = _originalMaterials[i];
                }
            }
        }
    }
}
using UnityEngine;

public class TeleportationSystem : MonoBehaviour
{
    [SerializeField] Transform _rootTransform;
    [SerializeField] Transform _directionTransform;
    [SerializeField] Transform _raycastPoint;
    [SerializeField] Rigidbody _rb;
    [SerializeField] CapsuleCollider _bodyCollider;

    [SerializeField, Min(0f)] float _distance = 5f;
    [SerializeField, Min(0f)] float _wallMargin = 0.05f;
    [SerializeField] LayerMask _blockingLayers;

    void Reset()
    {
        _rootTransform = transform.root;
        _directionTransform = _rootTransform;
        _raycastPoint = _rootTransform;
        _rb = _rootTransform.GetComponent<Rigidbody>();
        _blockingLayers = LayerMask.GetMask("Environment");
    }

    public void Teleport()
    {
        Vector3 direction = _directionTransform.forward;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;
        direction.Normalize();

        // Le déplacement utilise Transform : synchroniser avant les requêtes physiques.
        Physics.SyncTransforms();

        float distance = _distance;
        float radius = _bodyCollider != null ? _bodyCollider.radius : 0f;

        if (Physics.Raycast(_raycastPoint.position, direction, out RaycastHit hit,
            _distance + radius + _wallMargin, _blockingLayers, QueryTriggerInteraction.Ignore))
        {
            distance = Mathf.Clamp(hit.distance - radius - _wallMargin, 0f, distance);
        }

        if (_bodyCollider != null)
        {
            Bounds bounds = _bodyCollider.bounds;
            radius = Mathf.Min(bounds.extents.x, bounds.extents.z);
            Vector3 offset = Vector3.up * Mathf.Max(0f, bounds.extents.y - radius);
            Vector3 top = bounds.center + offset;
            Vector3 bottom = bounds.center - offset;

            // Le rayon choisit l'arrivée ; la capsule évite de rentrer dans un bord de mur.
            if (Physics.CheckCapsule(top, bottom, radius, _blockingLayers, QueryTriggerInteraction.Ignore)) return;

            if (Physics.CapsuleCast(top, bottom, radius, direction, out hit,
                distance, _blockingLayers, QueryTriggerInteraction.Ignore))
            {
                distance = Mathf.Max(0f, hit.distance - _wallMargin);
            }
        }

        Vector3 position = _rootTransform.position + direction * distance;
        if (_rb != null)
        {
            _rb.position = position;
        }
        _rootTransform.position = position;
    }
}

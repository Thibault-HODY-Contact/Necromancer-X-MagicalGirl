using UnityEngine;

public class LookSystem : MonoBehaviour
{
    [SerializeField] Transform _rootTransform;
    public Vector2 LookDirection { get; set; }

    void Reset()
    {
        _rootTransform = transform.root;
    }

    void Update()
    {
        if (LookDirection.sqrMagnitude > 0.01f)
        {
            Vector3 lookDir = new Vector3(LookDirection.x, 0f, LookDirection.y);
            _rootTransform.forward = lookDir;
        }
    }
}

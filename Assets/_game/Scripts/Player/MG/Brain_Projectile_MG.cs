using UnityEngine;

public class Brain_Projectile_MG : MonoBehaviour
{
    [SerializeField] MoveSystem _moveSystem;

    Transform _rootTransform;

    [SerializeField] float _lifeDuration;

    private void Reset()
    {
        _lifeDuration = 5f;
    }

    private void Start()
    {
        _rootTransform = transform.root;
    }
    void Update()
    {
        _lifeDuration -= Time.deltaTime;
        if (_lifeDuration <= 0f)
        {
            Destroy(_rootTransform.gameObject);
            return;
        }

        _moveSystem.MoveDirection = new Vector2(_rootTransform.forward.x, _rootTransform.forward.z);
    }
}

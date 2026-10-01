using UnityEngine;
using UnityEngine.InputSystem;

public class MoveSystem : MonoBehaviour
{
    [SerializeField] Transform _rootTransform;
    [SerializeField] float _speed = 5f;
    public Vector2 MoveDirection { get; set; }

    private void Reset()
    {
        _rootTransform = transform.root;
    }

    void Update()
    {
        Vector3 move = new Vector3(MoveDirection.x, 0f, MoveDirection.y);
        _rootTransform.Translate(move * _speed * Time.deltaTime, Space.Self);
    }
}
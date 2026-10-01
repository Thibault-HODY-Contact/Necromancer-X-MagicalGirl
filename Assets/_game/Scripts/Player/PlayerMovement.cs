using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float _speed = 5f;
    Vector2 _moveDirection;  

    public void SetDirection(Vector2 direction)
        { _moveDirection = direction; }


    void Update()
    {
        Vector3 move = new Vector3(_moveDirection.x, 0f, _moveDirection.y);
        transform.root.Translate(move * _speed * Time.deltaTime, Space.Self);
    }
}
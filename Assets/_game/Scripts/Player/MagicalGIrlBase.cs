using UnityEngine;
using UnityEngine.InputSystem;

public class MagicalGIrlBase : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance.MagicalGirl = transform.root.gameObject;
        GameManager.Instance.magicalGirlInput = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class LevierBase : MonoBehaviour
{
    [SerializeField] Animator _animator;
    bool _isOn = false;
    bool _wasClicked = false;
    bool _wasClickedLastFrame = false;
    public bool isOn {  get { return _isOn; } private set { _isOn = isOn; } }

    public bool wasClicked { get { return _wasClicked; } private set { _wasClicked = wasClicked; } }
    public bool wasClickedLastFrame { get { return _wasClickedLastFrame; } private set { _wasClickedLastFrame = wasClickedLastFrame; } }

    void OnClick(InputAction.CallbackContext context)
    {
        wasClicked = true;
        isOn = !isOn;

       

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
 

        if (!wasClickedLastFrame && !wasClicked) return;
        else if (wasClickedLastFrame && !wasClicked)
        {
            _wasClickedLastFrame = false;
        }
        else if (!wasClickedLastFrame && wasClicked)
        {
            _wasClickedLastFrame = true;
        }

        _animator.SetBool("isOn", isOn);
        _animator.SetBool("wasClicked", wasClicked);
    }
}

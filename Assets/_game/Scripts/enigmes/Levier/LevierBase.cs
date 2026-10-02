using System.Net;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevierBase : InteractInterface
{
    [SerializeField] Animator _animator;
    bool _isOn = false;
    bool _wasClicked = false;
    bool _wasClickedLastFrame = false;
    public bool isOn {  get { return _isOn; } private set { _isOn = value; } }

    public bool wasClicked { get { return _wasClicked; } private set { _wasClicked = value; } }
    public bool wasClickedLastFrame { get { return _wasClickedLastFrame; } private set { _wasClickedLastFrame = value; } }

    override public void OnInteract (InputAction.CallbackContext context) 
    {
        Debug.Log("E");
        wasClicked = true;  
        isOn = !isOn;
        Debug.Log("_isOn = " + _isOn + " _wasClicked = " + _wasClicked + " _wasClickedLastFrame = " + _wasClickedLastFrame);

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {


        if (wasClickedLastFrame && !wasClicked)
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

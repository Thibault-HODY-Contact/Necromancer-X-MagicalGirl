using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractInterface : MonoBehaviour
{
    [SerializeField] InputActionReference _Interact;

    public InputActionReference Interact { get { return _Interact; } }

    virtual public void OnInteract(InputAction.CallbackContext context)
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerInteractor"))
        {
            Debug.Log("enter");


            Interact.action.performed += OnInteract;
            Interact.action.canceled += OnInteract;

        }

    }
    private void OnTriggerExit(Collider other)
    {

        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerInteractor"))
        {
            Debug.Log("exit");


            Interact.action.performed -= OnInteract;
            Interact.action.canceled -= OnInteract;


        }

    }

}

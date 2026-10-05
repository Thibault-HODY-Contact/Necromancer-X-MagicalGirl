using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{

    [SerializeField] InputActionReference _Interact;
    private List<Action<InputAction.CallbackContext>> _EventCallBackList;
    void Start()
    {
        foreach (Action<InputAction.CallbackContext> action in _EventCallBackList)
        { 
            _Interact.action.performed += action; 
        
        }
    }
    private void OnDestroy()
    {
        foreach (Action<InputAction.CallbackContext> action in _EventCallBackList)
        {
            _Interact.action.performed -= action;

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;

public class ConnectPlayer : MonoBehaviour
{
    int playerNo = 0;
    void Update()
    {

        foreach (InputDevice device in InputSystem.devices)
        {
            if (device is XInputControllerWindows)
            {
            }
        }
    }
}

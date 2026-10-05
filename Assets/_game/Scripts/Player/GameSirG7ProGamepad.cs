using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Correspondance du rapport HID de la G7 Pro vers les controles Gamepad du jeu.
// Les offsets viennent du descripteur du peripherique (VID 0x3537, PID 0x1022).
[StructLayout(LayoutKind.Explicit, Size = 10)]
public struct GameSirG7ProState : IInputStateTypeInfo
{
    public FourCC format => new FourCC('H', 'I', 'D');

    [FieldOffset(0)] public byte ReportId;

    [InputControl(name = "buttonSouth", bit = 0)]
    [InputControl(name = "buttonEast", bit = 1)]
    [InputControl(name = "buttonWest", bit = 2)]
    [InputControl(name = "buttonNorth", bit = 3)]
    [InputControl(name = "leftShoulder", bit = 4)]
    [InputControl(name = "rightShoulder", bit = 5)]
    [InputControl(name = "select", bit = 6)]
    [InputControl(name = "start", bit = 7)]
    [InputControl(name = "leftStickPress", bit = 8)]
    [InputControl(name = "rightStickPress", bit = 9)]
    [FieldOffset(1)] public ushort Buttons;

    [InputControl(name = "dpad", layout = "Dpad", format = "BIT", sizeInBits = 4, defaultState = 8)]
    [InputControl(name = "dpad/up", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, parameters = "minValue=7,maxValue=1,nullValue=8,wrapAtValue=7")]
    [InputControl(name = "dpad/right", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, parameters = "minValue=1,maxValue=3,nullValue=8")]
    [InputControl(name = "dpad/down", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, parameters = "minValue=3,maxValue=5,nullValue=8")]
    [InputControl(name = "dpad/left", layout = "DiscreteButton", format = "BIT", bit = 0, sizeInBits = 4, parameters = "minValue=5,maxValue=7,nullValue=8")]
    [FieldOffset(3)] public byte Dpad;

    [InputControl(name = "leftStick", layout = "Stick", format = "VC2B")]
    [InputControl(name = "leftStick/x", offset = 0, format = "BYTE", defaultState = 127, parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
    [InputControl(name = "leftStick/y", offset = 1, format = "BYTE", defaultState = 127, parameters = "invert,normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
    [InputControl(name = "leftStick/left", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
    [InputControl(name = "leftStick/right", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1")]
    [InputControl(name = "leftStick/up", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
    [InputControl(name = "leftStick/down", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1,invert=false")]
    [FieldOffset(4)] public byte LeftStickX;
    [FieldOffset(5)] public byte LeftStickY;

    [InputControl(name = "rightStick", layout = "Stick", format = "VC2B")]
    [InputControl(name = "rightStick/x", offset = 0, format = "BYTE", defaultState = 127, parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
    [InputControl(name = "rightStick/y", offset = 1, format = "BYTE", defaultState = 127, parameters = "invert,normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
    [InputControl(name = "rightStick/left", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
    [InputControl(name = "rightStick/right", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1")]
    [InputControl(name = "rightStick/up", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
    [InputControl(name = "rightStick/down", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1,invert=false")]
    [FieldOffset(6)] public byte RightStickX;
    [FieldOffset(7)] public byte RightStickY;

    [InputControl(name = "leftTrigger", format = "BYTE")]
    [FieldOffset(8)] public byte LeftTrigger;

    [InputControl(name = "rightTrigger", format = "BYTE")]
    [FieldOffset(9)] public byte RightTrigger;
}

[InputControlLayout(stateType = typeof(GameSirG7ProState), displayName = "GameSir G7 Pro (HID)")]
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class GameSirG7ProGamepad : Gamepad
{
    static GameSirG7ProGamepad()
    {
        InputSystem.RegisterLayout<GameSirG7ProGamepad>(matches: new InputDeviceMatcher()
            .WithInterface("HID")
            .WithCapability("vendorId", 0x3537)
            .WithCapability("productId", 0x1022)
            .WithCapability("usagePage", 1)
            .WithCapability("usage", 5)
            .WithCapability("inputReportSize", 10));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        // Declenche le constructeur statique aussi dans le build.
    }
}

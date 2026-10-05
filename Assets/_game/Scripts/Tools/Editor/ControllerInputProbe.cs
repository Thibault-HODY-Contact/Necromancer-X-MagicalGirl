using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.LowLevel;

// Temporary capture of the device and controls actually paired to each player.
[InitializeOnLoad]
internal static class ControllerInputProbe
{
    private const string Report = "C:/Users/Ryad/AppData/Local/Temp/NMG-Controller-ba7c33fea829481aa69af14d88282ae5/LiveInput.txt";
    private static string _previousSnapshot;
    private static double _nextCapture;
    private static double _captureEnds;
    private static bool _auxiliaryInterfacesChecked;

    static ControllerInputProbe()
    {
        EditorApplication.delayCall += Capture;
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                _captureEnds = EditorApplication.timeSinceStartup + 45;
                _previousSnapshot = null;
            }
            Capture();
        };
        InputSystem.onAfterUpdate += Update;
    }

    private static void Update()
    {
        double now = EditorApplication.timeSinceStartup;
        if (!EditorApplication.isPlaying || InputState.currentUpdateType != InputUpdateType.Dynamic
            || now > _captureEnds || now < _nextCapture) return;
        _nextCapture = now + 0.25;
        Capture();
    }

    private static void Capture()
    {
        // Retire uniquement les interfaces annexes accepteees par la premiere version
        // du matcher. La correspondance finale ne les accepte plus.
        if (!EditorApplication.isPlayingOrWillChangePlaymode && !_auxiliaryInterfacesChecked)
        {
            _auxiliaryInterfacesChecked = true;
            foreach (InputDevice device in InputSystem.devices.ToArray())
            {
                if (!(device is GameSirG7ProGamepad)) continue;
                HID.HIDDeviceDescriptor descriptor = HID.HIDDeviceDescriptor.FromJson(device.description.capabilities);
                if (descriptor.usagePage == HID.UsagePage.GenericDesktop && descriptor.usage == 5 && descriptor.inputReportSize == 10) continue;
                InputSystem.RemoveDevice(device);
            }
        }
        StringBuilder report = new StringBuilder();
        report.AppendLine("Playing=" + EditorApplication.isPlaying);
        foreach (InputDevice device in InputSystem.devices)
        {
            report.AppendLine("Device=" + device.deviceId + " Type=" + device.GetType().Name + " Layout=" + device.layout + " Enabled=" + device.enabled);
            if (device is Gamepad || device is Joystick)
            {
                report.AppendLine(device.description.ToJson());
                foreach (InputControl control in device.children)
                    report.AppendLine("Raw " + control.name + "=" + control.ReadValueAsObject());
            }
        }
        foreach (PlayerInput player in Resources.FindObjectsOfTypeAll<PlayerInput>())
        {
            if (EditorUtility.IsPersistent(player) || !player.gameObject.scene.IsValid()) continue;
            report.AppendLine("Player=" + player.transform.root.name + " Index=" + player.playerIndex + " Scheme=" + player.currentControlScheme + " Map=" + player.currentActionMap?.name + " Active=" + player.inputIsActive);
            foreach (InputDevice device in player.devices) report.AppendLine("Paired=" + device.deviceId + ":" + device.layout);
            foreach (string name in new[] { "Move", "Look", "Attack", "Attack2" })
            {
                InputAction action = player.actions?.FindAction("Player/" + name);
                if (action == null) { report.AppendLine(name + "=null"); continue; }
                report.AppendLine(name + " Enabled=" + action.enabled + " Phase=" + action.phase + " Value=" + action.ReadValueAsObject() + " Pressed=" + action.IsPressed());
                foreach (InputControl control in action.controls) report.AppendLine("  Control=" + control.path);
            }
            LookSystem look = player.transform.root.GetComponentInChildren<LookSystem>(true);
            if (look != null) report.AppendLine("LookSystem Enabled=" + look.enabled + " Direction=" + look.LookDirection);
        }
        string snapshot = report.ToString();
        if (snapshot == _previousSnapshot) return;
        _previousSnapshot = snapshot;
        File.AppendAllText(Report, DateTime.Now.ToString("O") + "\n" + snapshot + "\n");
    }
}

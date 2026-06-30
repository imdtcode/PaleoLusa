using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using XRInputDevice    = UnityEngine.XR.InputDevice;
using XRCommonUsages   = UnityEngine.XR.CommonUsages;

// Attach to XR Origin (XR Rig).
// Uses XR InputDevices (works on real Meta Quest) + Gamepad fallback (XR Simulator).
public class VRInputHandler : MonoBehaviour
{
    [Header("Dino Panel Interaction")]
    public float rayDistance = 15f;

    [Header("Reticle")]
    [Tooltip("Small sphere parented to Main Camera — changes colour when pointing at a dino.")]
    public MeshRenderer reticle;
    public Material reticleDefault;
    public Material reticleHighlight;

    [Header("Menu Button (Simulator / Keyboard)")]
    public Key menuKey = Key.M;

    [Header("Volume Control (triggers when menu open)")]
    [Tooltip("Volume change per trigger press. 0.1 = 10% per step.")]
    public float volumeStep = 0.1f;

    private Camera            _cam;
    private DinoPanelSwitcher _currentPointed;

    // XR controller devices
    private XRInputDevice _rightHand;
    private XRInputDevice _leftHand;

    // Previous frame states — needed to detect "just pressed" for XR devices
    private bool  _prevA, _prevB, _prevX, _prevY, _prevMenu;
    private float _prevLeftTrigger, _prevRightTrigger;

    void Update()
    {
        if (_cam == null) _cam = FindActiveCamera();

        RefreshDevices();

        // --- Read raw XR button states ---
        bool aHeld    = GetXRButton(_rightHand, XRCommonUsages.primaryButton);
        bool bHeld    = GetXRButton(_rightHand, XRCommonUsages.secondaryButton);
        bool xHeld    = GetXRButton(_leftHand,  XRCommonUsages.primaryButton);
        bool yHeld    = GetXRButton(_leftHand,  XRCommonUsages.secondaryButton);
        bool menuHeld = GetXRButton(_leftHand,  XRCommonUsages.menuButton);

        // --- Detect "just pressed" (rising edge) ---
        bool aPressed    = !_prevA    && aHeld;
        bool bPressed    = !_prevB    && bHeld;
        bool xPressed    = !_prevX    && xHeld;
        bool menuPressed = !_prevMenu && menuHeld;

        // --- Gamepad / Simulator fallback ---
        if (Gamepad.current != null)
        {
            aPressed    |= Gamepad.current.buttonSouth.wasPressedThisFrame;
            bPressed    |= Gamepad.current.buttonEast.wasPressedThisFrame;
            xPressed    |= Gamepad.current.buttonWest.wasPressedThisFrame;
            menuPressed |= Gamepad.current.startButton.wasPressedThisFrame
                        || Gamepad.current.buttonNorth.wasPressedThisFrame;
        }
        // Keyboard fallback for menu and A button (simulator testing)
        if (Keyboard.current != null)
        {
            menuPressed |= Keyboard.current[menuKey].wasPressedThisFrame;
            aPressed    |= Keyboard.current[Key.Space].wasPressedThisFrame;
        }

        // --- Store previous states ---
        _prevA    = aHeld;
        _prevB    = bHeld;
        _prevX    = xHeld;
        _prevY    = yHeld;
        _prevMenu = menuHeld;

        // --- Reticle ---
        _currentPointed = GetPointedDino();
        if (reticle != null)
            reticle.material = _currentPointed != null ? reticleHighlight : reticleDefault;

        // --- Menu toggle (Y / Menu button / M key) ---
        if (menuPressed)
        {
            PauseMenuController.Instance?.Toggle();
            return;
        }

        // Settings menu is open — A activates pointed button, right stick adjusts volume
        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsOpen)
        {
            if (aPressed)
                GetPointedSettingsButton()?.Press();

            // Right trigger = volume up, left trigger = volume down (one step per press)
            float rTrig = GetXRFloat(_rightHand, XRCommonUsages.trigger);
            float lTrig = GetXRFloat(_leftHand,  XRCommonUsages.trigger);
            if (Gamepad.current != null)
            {
                rTrig = Gamepad.current.rightTrigger.ReadValue();
                lTrig = Gamepad.current.leftTrigger.ReadValue();
            }

            bool volUp   = rTrig > 0.7f && _prevRightTrigger <= 0.7f;
            bool volDown = lTrig > 0.7f && _prevLeftTrigger  <= 0.7f;
            _prevRightTrigger = rTrig;
            _prevLeftTrigger  = lTrig;

            // Keyboard fallback for XR Simulator (PageUp = right trigger, PageDown = left trigger)
            if (Keyboard.current != null)
            {
                volUp   |= Keyboard.current[Key.PageUp].wasPressedThisFrame;
                volDown |= Keyboard.current[Key.PageDown].wasPressedThisFrame;
            }

            if (volUp || volDown)
            {
                float newVol = Mathf.Clamp01(AudioListener.volume + (volUp ? volumeStep : -volumeStep));
                AudioListener.volume = newVol;
                PauseMenuController.Instance?.SyncVolumeUI(newVol);
            }

            return;
        }

        if (_cam == null) return;

        // A (right) — open / close dino panel
        if (aPressed)
        {
            if (_currentPointed != null) _currentPointed.TogglePanel();
            else                         DinoPanelSwitcher.CloseOpenPanel();
        }

        // B (right) — cycle sub-panels: Info → Compare → X-ray
        if (bPressed)
            DinoPanelSwitcher.NextOpenPanel();

        // X (left) — close open panel
        if (xPressed)
            DinoPanelSwitcher.CloseOpenPanel();
    }

    // Try to get XR controller devices if not already found
    void RefreshDevices()
    {
        if (!_rightHand.isValid)
        {
            var list = new List<XRInputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, list);
            if (list.Count > 0) _rightHand = list[0];
        }
        if (!_leftHand.isValid)
        {
            var list = new List<XRInputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, list);
            if (list.Count > 0) _leftHand = list[0];
        }
    }

    bool GetXRButton(XRInputDevice device, InputFeatureUsage<bool> usage)
    {
        if (!device.isValid) return false;
        device.TryGetFeatureValue(usage, out bool value);
        return value;
    }

    Vector2 GetXRAxis(XRInputDevice device, InputFeatureUsage<Vector2> usage)
    {
        if (!device.isValid) return Vector2.zero;
        device.TryGetFeatureValue(usage, out Vector2 value);
        return value;
    }

    float GetXRFloat(XRInputDevice device, InputFeatureUsage<float> usage)
    {
        if (!device.isValid) return 0f;
        device.TryGetFeatureValue(usage, out float value);
        return value;
    }

    private DinoPanelSwitcher GetPointedDino()
    {
        if (_cam == null) return null;
        if (Physics.Raycast(_cam.transform.position, _cam.transform.forward,
                out RaycastHit hit, rayDistance))
            return hit.collider.GetComponentInParent<DinoPanelSwitcher>();
        return null;
    }

    private SettingsButton GetPointedSettingsButton()
    {
        if (_cam == null) return null;
        if (Physics.Raycast(_cam.transform.position, _cam.transform.forward,
                out RaycastHit hit, rayDistance))
            return hit.collider.GetComponentInParent<SettingsButton>();
        return null;
    }

    private static Camera FindActiveCamera()
    {
        XROrigin xrOrigin = Object.FindFirstObjectByType<XROrigin>();
        if (xrOrigin != null)
        {
            if (xrOrigin.Camera != null && xrOrigin.Camera.isActiveAndEnabled)
                return xrOrigin.Camera;
            Camera childCam = xrOrigin.GetComponentInChildren<Camera>();
            if (childCam != null && childCam.isActiveAndEnabled)
                return childCam;
        }
        return Camera.main;
    }
}

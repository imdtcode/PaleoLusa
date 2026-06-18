using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

// Attach this to the XR Origin (XR Rig) GameObject.
// All VR controller buttons are mapped here — one place, no conflicts.
public class VRInputHandler : MonoBehaviour
{
    [Header("Dino Panel Interaction")]
    public float rayDistance = 15f;

    [Header("Menu Button")]
    [Tooltip("Keyboard key to open/close the settings menu (for testing in simulator)")]
    public Key menuKey = Key.M;

    private Camera _cam;

    void Update()
    {
        if (_cam == null)
            _cam = FindActiveCamera();

        // Menu toggle — keyboard M key OR gamepad Start/Menu button OR Y button
        bool menuPressed = (Keyboard.current != null && Keyboard.current[menuKey].wasPressedThisFrame)
                        || (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
                        || (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame);

        if (menuPressed)
        {
            PauseMenuController.Instance?.Toggle();
            return;
        }

        // When the settings menu is open, only Escape/M closes it
        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsOpen)
            return;

        if (_cam == null || Gamepad.current == null)
            return;

        // A button — look at a dino and press to open its panel
        if (Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            DinoPanelSwitcher pointed = GetPointedDino();
            if (pointed != null)
                pointed.TogglePanel();
            else
                DinoPanelSwitcher.CloseOpenPanel();
        }

        // B button — cycle through sub-panels (Info → Compare → Xray → Info...)
        if (Gamepad.current.buttonEast.wasPressedThisFrame)
            DinoPanelSwitcher.NextOpenPanel();

        // X button — close the open panel
        if (Gamepad.current.buttonWest.wasPressedThisFrame)
            DinoPanelSwitcher.CloseOpenPanel();
    }

    private DinoPanelSwitcher GetPointedDino()
    {
        if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out RaycastHit hit, rayDistance))
            return hit.collider.GetComponentInParent<DinoPanelSwitcher>();
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

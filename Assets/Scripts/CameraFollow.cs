using UnityEngine;
using Unity.XR.CoreUtils;

public class DinoPanelLookAtPlayer : MonoBehaviour
{
    [Header("Camera — drag the XR camera from inside XR Origin here")]
    public Camera playerCamera;

    [Header("Dinosaur — auto-set from parent if empty")]
    public Transform dinosaur;

    [Header("Position (above the dinosaur)")]
    public float heightOffset = 1.5f;

    [Header("Rotation")]
    public float rotationSpeed = 10f;
    [Tooltip("How much the panel tilts up/down to follow your gaze. 0 = no tilt, 1 = full gaze. 0.4 is a good default.")]
    public float verticalGazeInfluence = 0.4f;
    [Tooltip("Maximum tilt angle in degrees. Prevents extreme rotation when very close.")]
    public float maxTiltDegrees = 65f;

    void OnEnable()
    {
        if (playerCamera == null)
            playerCamera = FindActiveCamera();
    }

    void LateUpdate()
    {
        if (dinosaur == null && transform.parent != null)
            dinosaur = transform.parent;

        if (playerCamera == null)
            playerCamera = FindActiveCamera();

        if (playerCamera == null || dinosaur == null)
            return;

        // Always directly above the dinosaur — no horizontal drift
        transform.position = dinosaur.position + Vector3.up * heightOffset;

        // Face the camera — close or far, the panel always points toward the player.
        Vector3 toPlayer = playerCamera.transform.position - transform.position;
        if (toPlayer.sqrMagnitude > 0.001f)
        {
            // Gaze influence — adds extra tilt when the player looks up or down
            toPlayer.y -= playerCamera.transform.forward.y
                        * toPlayer.magnitude * verticalGazeInfluence;

            // Clamp so the panel never rotates past maxTiltDegrees from vertical
            Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z);
            float flatMag = flat.magnitude;
            if (flatMag > 0.001f)
            {
                float maxY = flatMag * Mathf.Tan(maxTiltDegrees * Mathf.Deg2Rad);
                toPlayer.y = Mathf.Clamp(toPlayer.y, -maxY, maxY);
            }

            Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized)
                                 * Quaternion.Euler(0f, 180f, 0f);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }
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

        if (Camera.main != null && Camera.main.isActiveAndEnabled)
            return Camera.main;

        return null;
    }
}

using UnityEngine;

public class XRStickToTerrain : MonoBehaviour
{
    public Transform cameraTransform;
    public LayerMask groundLayer;

    public float rayHeight = 50f;
    public float rayDistance = 200f;
    public float yOffset = 0f;
    public float smoothSpeed = 15f;

    void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        Vector3 rayStart = new Vector3(
            cameraTransform.position.x,
            cameraTransform.position.y + rayHeight,
            cameraTransform.position.z
        );

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayDistance, groundLayer))
        {
            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(pos.y, hit.point.y + yOffset, Time.deltaTime * smoothSpeed);
            transform.position = pos;
        }
    }
}
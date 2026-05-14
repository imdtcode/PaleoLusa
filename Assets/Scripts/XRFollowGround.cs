using UnityEngine;

public class XRFollowGround : MonoBehaviour
{
    public LayerMask groundLayer;
    public float playerHeight = 1.7f;
    public float rayHeight = 20f;
    public float rayDistance = 100f;
    public float smoothSpeed = 12f;

    void Update()
    {
        Vector3 rayStart = transform.position + Vector3.up * rayHeight;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayDistance, groundLayer))
        {
            float targetY = hit.point.y;
            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * smoothSpeed);
            transform.position = pos;
        }
    }
}
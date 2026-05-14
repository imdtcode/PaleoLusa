using UnityEngine;

public class DinoFollowGround : MonoBehaviour
{
    public float yOffset = 0f;
    public float rayHeight = 20f;
    public float rayDistance = 100f;
    public LayerMask groundLayer;

    void Update()
    {
        Vector3 rayStart = transform.position + Vector3.up * rayHeight;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayDistance, groundLayer))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y + yOffset;
            transform.position = pos;
        }
    }
}
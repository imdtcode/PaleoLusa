using UnityEngine;

public class XRBoundsConstraint : MonoBehaviour
{
    public Transform targetRig;
    public Vector3 worldCenter;
    public Vector3 worldSize = new Vector3(40f, 5f, 40f);
    public bool constrainY;
    public float padding = 0.25f;

    void Reset()
    {
        targetRig = transform;
        worldCenter = transform.position;
    }

    void LateUpdate()
    {
        Transform target = targetRig != null ? targetRig : transform;
        Vector3 halfSize = worldSize * 0.5f;
        Vector3 min = worldCenter - halfSize;
        Vector3 max = worldCenter + halfSize;

        min.x += padding;
        min.z += padding;
        max.x -= padding;
        max.z -= padding;

        if (constrainY)
        {
            min.y += padding;
            max.y -= padding;
        }

        Vector3 clampedPosition = target.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, min.x, max.x);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, min.z, max.z);

        if (constrainY)
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, min.y, max.y);

        target.position = clampedPosition;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.35f);
        Gizmos.DrawWireCube(worldCenter, worldSize);
    }
}

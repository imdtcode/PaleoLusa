using UnityEngine;

public class DinoPanelLookAtPlayer : MonoBehaviour
{
    [Header("Position")]
    [Tooltip("How far from the dino centre the panel sits (horizontal). Try 3-4 for large dinos.")]
    public float orbitDistance = 3f;
    [Tooltip("Fine-tune vertical offset from player eye height. 0 = exactly at eye level.")]
    public float eyeHeightOffset = 0f;

    void LateUpdate()
    {
        Camera cam = Camera.main;
        Transform dino = transform.parent;   // DinoInfo is a child of the dino

        if (cam == null || dino == null) return;

        // --- Place panel in front of dino, at player eye height ---
        Vector3 toPlayer = cam.transform.position - dino.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude > 0.001f)
        {
            Vector3 pos     = dino.position + toPlayer.normalized * orbitDistance;
            pos.y           = cam.transform.position.y + eyeHeightOffset;
            transform.position = pos;
        }

        // --- Face the player (no vertical tilt) ---
        Vector3 faceDir = cam.transform.position - transform.position;
        faceDir.y = 0f;
        if (faceDir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(faceDir.normalized)
                               * Quaternion.Euler(0f, 180f, 0f);
        }
    }
}

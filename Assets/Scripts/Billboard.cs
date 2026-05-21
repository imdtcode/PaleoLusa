using UnityEngine;

public class DinoPanelLookAtPlayer : MonoBehaviour
{
    public Transform playerCamera;
    public Transform dinosaur;

    public Vector3 offsetFromDino = new Vector3(0f, 2f, 2f);

    public float yRotationOffset = 180f;
    public bool invertVerticalLook = true;

    void LateUpdate()
    {
        if (playerCamera == null)
        {
            if (Camera.main == null) return;
            playerCamera = Camera.main.transform;
        }

        if (dinosaur == null) return;

        // Mantém o painel preso ao dinossauro
        transform.position = dinosaur.position + offsetFromDino;

        // Direção para o player
        Vector3 direction = playerCamera.position - transform.position;

        // Corrige a inclinação vertical
        if (invertVerticalLook)
            direction.y *= -1f;

        if (direction.sqrMagnitude < 0.001f) return;

        // Rotação instantânea
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        targetRotation *= Quaternion.Euler(0f, yRotationOffset, 0f);

        transform.rotation = targetRotation;
    }
}
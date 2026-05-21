using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class XRCharacterCollisionMover : MonoBehaviour
{
    public Transform xrCamera;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 lastPosition;
    private float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        lastPosition = transform.position;
    }

    void LateUpdate()
    {
        Vector3 movement = transform.position - lastPosition;

        transform.position = lastPosition;

        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        movement.y = verticalVelocity * Time.deltaTime;

        controller.Move(movement);

        lastPosition = transform.position;
    }
}
using UnityEngine;

// Attach to XR Origin (XR Rig) ONLY if GravityProvider (Locomotion > Gravity) is disabled.
// Do NOT use both at the same time — they will fight.
[RequireComponent(typeof(CharacterController))]
public class XRCharacterCollisionMover : MonoBehaviour
{
    public float gravity = -9.81f;
    public float groundedForce = -2f;

    private CharacterController _cc;
    private float _verticalVelocity;

    void Awake() => _cc = GetComponent<CharacterController>();

    void LateUpdate()
    {
        if (_cc == null) return;

        if (_cc.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = groundedForce;
        else
            _verticalVelocity += gravity * Time.deltaTime;

        _verticalVelocity = Mathf.Max(_verticalVelocity, -20f);
        _cc.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
    }
}

using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RaptorWander : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 0.8f;
    public float rotationSpeed = 80f;
    public float wanderRadius = 4f;
    public float obstacleCheckDistance = 2f;

    [Header("Integration")]
    public DinoAnimationCycle animCycle;

    private Animator _animator;
    private Vector3  _target;
    private bool     _walking;

    void Awake() => _animator = GetComponent<Animator>();

    void OnDisable()
    {
        if (_walking) StopWalking();
    }

    void Update()
    {
        if (_animator == null || !_animator.gameObject.activeInHierarchy) return;

        bool inWalkState = _animator.GetCurrentAnimatorStateInfo(0).IsTag("Walk");

        if (inWalkState && !_walking)
            StartWalking();
        else if (!inWalkState && _walking)
            StopWalking();

        if (!_walking) return;

        Vector3 toTarget = _target - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude <= 0.3f)
        {
            PickNewTarget();
            return;
        }

        Vector3 rayOrigin = transform.position + Vector3.up;
        if (Physics.Raycast(rayOrigin, transform.forward, out RaycastHit obsHit,
                obstacleCheckDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (!obsHit.transform.IsChildOf(transform) && obsHit.transform != transform)
            {
                float sideAngle = Random.value > 0.5f
                    ? Random.Range(90f, 150f)
                    : Random.Range(-150f, -90f);
                Vector3 fwd2D = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
                _target = transform.position + Quaternion.Euler(0f, sideAngle, 0f) * fwd2D * wanderRadius;
                return;
            }
        }

        Quaternion desired = Quaternion.LookRotation(toTarget.normalized);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, desired, rotationSpeed * Time.deltaTime);
        transform.position += transform.forward * walkSpeed * Time.deltaTime;
    }

    void StartWalking()
    {
        PickNewTarget();
        _walking = true;
        if (animCycle != null) animCycle.enabled = false;
    }

    void StopWalking()
    {
        _walking = false;
        if (animCycle != null) animCycle.enabled = true;
    }

    void PickNewTarget()
    {
        float angle   = Random.Range(-90f, 90f);
        Vector3 fwd2D = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        if (fwd2D.sqrMagnitude < 0.01f) fwd2D = Vector3.forward;
        _target = transform.position +
                  Quaternion.Euler(0f, angle, 0f) * fwd2D *
                  Random.Range(wanderRadius * 0.3f, wanderRadius);
    }
}

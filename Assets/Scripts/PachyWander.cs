using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class PachyWander : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 0.7f;
    public float rotationSpeed = 80f;
    public float wanderRadius = 4f;
    public float obstacleCheckDistance = 2.5f;

    [Header("Territory")]
    [Tooltip("Max distance from spawn point. Dino never picks a target outside this radius.")]
    public float homeRadius = 20f;

    [Header("Obstacle Response")]
    public float minWaitTime = 1.5f;
    public float maxWaitTime = 3f;

    [Header("Integration")]
    public DinoAnimationCycle animCycle;

    private Animator            _animator;
    private CharacterController _cc;
    private Vector3             _target;
    private Vector3             _home;
    private bool                _walking;
    private float               _verticalVelocity;
    private float               _steerCooldown;
    private float               _waitTimer;
    private bool                _waiting;

    void Awake()
    {
        _animator      = GetComponent<Animator>();
        _cc            = GetComponent<CharacterController>();
        _cc.stepOffset = 0.05f;
    }

    void Start() => _home = transform.position;

    void OnDisable()
    {
        if (_walking) StopWalking();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        bool isDino = hit.collider.gameObject.layer == LayerMask.NameToLayer("Dino");
        if (!isDino && hit.normal.y > 0.6f) return;
        if (hit.transform.IsChildOf(transform) || hit.transform == transform) return;

        if (_walking && !_waiting)
            StartWaiting();
    }

    void Update()
    {
        if (_animator == null || !_animator.gameObject.activeInHierarchy) return;

        bool inWalkState = _animator.GetCurrentAnimatorStateInfo(0).IsTag("Walk");

        if (inWalkState && !_walking)
            StartWalking();
        else if (!inWalkState && _walking)
            StopWalking();

        ApplyGravity();

        // Freeze all movement while diegetic panel is open
        if (_animator.speed == 0f) return;

        if (!_walking) return;

        if (_waiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
                FinishWaiting();
            return;
        }

        if (_steerCooldown > 0f)
        {
            _steerCooldown -= Time.deltaTime;
            RotateTowardTarget();
            return;
        }

        Vector3 toTarget = _target - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude <= 0.3f)
        {
            PickNewTarget();
            return;
        }

        // Detect other dinos in any direction (CC vs CC events are unreliable)
        int dinoMask = 1 << LayerMask.NameToLayer("Dino");
        Collider[] nearbyDinos = Physics.OverlapSphere(transform.position + Vector3.up, 3f, dinoMask, QueryTriggerInteraction.Ignore);
        foreach (var col in nearbyDinos)
        {
            if (col.transform == transform || col.transform.IsChildOf(transform)) continue;
            StartWaiting();
            return;
        }

        Vector3 rayOrigin   = transform.position + Vector3.up;
        Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;

        if (Physics.SphereCast(rayOrigin, 0.4f, flatForward, out RaycastHit obsHit,
                obstacleCheckDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (!obsHit.transform.IsChildOf(transform) && obsHit.transform != transform)
            {
                StartWaiting();
                return;
            }
        }

        RotateTowardTarget();
        _cc.Move(flatForward * walkSpeed * Time.deltaTime);
    }

    void StartWaiting()
    {
        _waiting   = true;
        _waitTimer = Random.Range(minWaitTime, maxWaitTime);
    }

    void FinishWaiting()
    {
        _waiting = false;
        Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        float sideAngle = Random.value > 0.5f
            ? Random.Range(90f, 150f)
            : Random.Range(-150f, -90f);
        _target        = ClampToHome(transform.position +
            Quaternion.Euler(0f, sideAngle, 0f) * flatForward * wanderRadius);
        _steerCooldown = 1.2f;
    }

    void RotateTowardTarget()
    {
        Vector3 toTarget = _target - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.01f) return;
        Quaternion desired = Quaternion.LookRotation(toTarget.normalized);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, desired, rotationSpeed * Time.deltaTime);
    }

    void ApplyGravity()
    {
        if (_cc.isGrounded)
            _verticalVelocity = -2f;
        else
            _verticalVelocity -= 9.81f * Time.deltaTime;

        _cc.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
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
        _waiting = false;
        if (animCycle != null) animCycle.enabled = true;
    }

    void PickNewTarget()
    {
        float angle   = Random.Range(-90f, 90f);
        Vector3 fwd2D = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        if (fwd2D.sqrMagnitude < 0.01f) fwd2D = Vector3.forward;
        Vector3 raw = transform.position +
                      Quaternion.Euler(0f, angle, 0f) * fwd2D *
                      Random.Range(wanderRadius * 0.3f, wanderRadius);
        _target = ClampToHome(raw);
    }

    Vector3 ClampToHome(Vector3 candidate)
    {
        Vector3 flat   = new Vector3(candidate.x, _home.y, candidate.z);
        Vector3 offset = flat - _home;
        if (offset.magnitude > homeRadius)
            flat = _home + offset.normalized * homeRadius;
        flat.y = candidate.y;
        return flat;
    }
}

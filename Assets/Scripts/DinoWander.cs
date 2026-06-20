using UnityEngine;

// Attach to a dinosaur GameObject that already has an Animator.
// Also add this script to DinoPanelSwitcher.scriptsToDisable so the dino
// stops wandering while its info panel is open.
[RequireComponent(typeof(Animator))]
public class DinoWander : MonoBehaviour
{
    [Header("Animator State Names (must match your Animator exactly)")]
    public string walkStateName = "Walk";
    public string idleStateName = "Idle";
    public float crossFadeDuration = 0.25f;

    [Header("Movement")]
    public float walkSpeed = 0.6f;
    public float rotationSpeed = 80f;
    public float wanderRadius = 4f;
    [Tooltip("How far ahead to check for obstacles like trees. 2 works well for most dinos.")]
    public float obstacleCheckDistance = 2f;

    [Header("Timing")]
    public float minIdleTime = 5f;
    public float maxIdleTime = 12f;
    public float minWalkTime = 3f;
    public float maxWalkTime = 7f;

    [Header("Ground Snap")]
    public LayerMask groundMask;

    [Header("Integration")]
    public DinoAnimationCycle animCycle;

    private Animator  _animator;
    private Vector3   _origin;
    private Vector3   _target;
    private bool      _walking;
    private float     _timer;
    private bool      _ready;

    void Awake() => _animator = GetComponent<Animator>();

    void Start()
    {
        _origin = transform.position;
        _ready  = true;
        ScheduleIdle();
    }

    void OnEnable()
    {
        // Resume idle when re-enabled after panel closes
        if (_ready) ScheduleIdle();
    }

    void OnDisable()
    {
        CrossFadeSafe(idleStateName);
        if (animCycle != null)
            animCycle.enabled = true;
    }

    void Update()
    {
        _timer -= Time.deltaTime;

        if (_walking)
        {
            Vector3 toTarget = _target - transform.position;
            toTarget.y = 0f;

            if (toTarget.magnitude > 0.3f)
            {
                // If a tree, wall or other dino is ahead, steer 90-150° sideways
                Vector3 rayOrigin = transform.position + Vector3.up;
                Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
                if (Physics.SphereCast(rayOrigin, 0.4f, flatForward, out RaycastHit obsHit,
                        obstacleCheckDistance, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (!obsHit.transform.IsChildOf(transform) && obsHit.transform != transform)
                    {
                        float sideAngle = Random.value > 0.5f
                            ? Random.Range(90f, 150f)
                            : Random.Range(-150f, -90f);
                        _target = transform.position + Quaternion.Euler(0f, sideAngle, 0f) * flatForward * wanderRadius;
                        return;
                    }
                }

                // Rotate smoothly toward target
                Quaternion desired = Quaternion.LookRotation(toTarget.normalized);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, desired, rotationSpeed * Time.deltaTime);

                // Move forward — XZ only so dino never climbs collider edges
                transform.position += flatForward * walkSpeed * Time.deltaTime;
                SnapToGround();
            }
            else
            {
                _timer = 0f; // reached destination early
            }

            if (_timer <= 0f) ScheduleIdle();
        }
        else
        {
            if (_timer <= 0f) ScheduleWalk();
        }
    }

    void ScheduleIdle()
    {
        _walking = false;
        _timer   = Random.Range(minIdleTime, maxIdleTime);
        CrossFadeSafe(idleStateName);
        if (animCycle != null) animCycle.enabled = true;
    }

    void ScheduleWalk()
    {
        // Pick a point ahead of the dino's current facing, biased forward to avoid U-turns
        float angle = Random.Range(-90f, 90f);
        Vector3 forward2D = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        if (forward2D.sqrMagnitude < 0.01f) forward2D = Vector3.forward;
        Vector3 dir = Quaternion.Euler(0f, angle, 0f) * forward2D;

        float dist = Random.Range(wanderRadius * 0.3f, wanderRadius);
        _target  = transform.position + dir * dist;
        _walking = true;
        _timer   = Random.Range(minWalkTime, maxWalkTime);

        CrossFadeSafe(walkStateName);
        if (animCycle != null) animCycle.enabled = false;
    }

    void CrossFadeSafe(string stateName)
    {
        if (_animator != null && _animator.gameObject.activeInHierarchy)
            _animator.CrossFade(stateName, crossFadeDuration);
    }

    void SnapToGround()
    {
        if (groundMask == 0) return;
        Ray ray = new Ray(transform.position + Vector3.up * 2f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 5f, groundMask))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y;
            transform.position = pos;
        }
    }
}

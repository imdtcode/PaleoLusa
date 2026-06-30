using UnityEngine;

public class DinoAnimationCycle : MonoBehaviour
{
    public Animator animator;

    [System.Serializable]
    public struct AnimState
    {
        public string stateName;
        [Min(0.1f)] public float duration;
    }

    public AnimState[] states;
    [Min(0f)] public float crossFadeTime = 0.5f;

    private int _index;
    private float _timer;
    private bool _started;

    void Awake()
    {
        // Always self-find so duplicated prefabs don't point to the original's Animator
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        if (animator != null && states.Length > 0)
            PlayCurrent();
    }

    void Update()
    {
        if (animator == null || states.Length < 2)
            return;

        // Animator.speed is set to 0 by DinoPanelSwitcher when panel is open — don't advance timer then
        if (animator.speed == 0f)
            return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _index = (_index + 1) % states.Length;
            PlayCurrent();
        }
    }

    private void PlayCurrent()
    {
        animator.CrossFadeInFixedTime(states[_index].stateName, crossFadeTime);
        _timer = states[_index].duration;
    }
}

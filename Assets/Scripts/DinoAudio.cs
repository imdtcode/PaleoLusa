using UnityEngine;

public class DinoAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] clips;

    [Header("Timing")]
    public float minInterval = 8f;
    public float maxInterval = 25f;

    private float _timer;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        ScheduleNext();
    }

    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            PlayRandom();
            ScheduleNext();
        }
    }

    void PlayRandom()
    {
        if (clips == null || clips.Length == 0 || audioSource == null) return;
        if (audioSource.isPlaying) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    void ScheduleNext()
    {
        _timer = Random.Range(minInterval, maxInterval);
    }
}

using UnityEngine;

public class StateAudioBehaviour : StateMachineBehaviour
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (clip == null) return;
        AudioSource src = animator.gameObject.GetComponent<AudioSource>();
        if (src != null) src.PlayOneShot(clip, volume);
    }
}

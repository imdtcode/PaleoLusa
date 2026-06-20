using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StegaSoundEffects : MonoBehaviour
{
    //Variables

    AudioSource audioSource;


    //Sound Variants

    public AudioClip[] growlClips;

    public AudioClip[] yelpClips;

    public AudioClip[] barkClips;

    public AudioClip[] roarClips;

    public AudioClip[] deathClips;


    //Gather variables

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    //Growl Sounds (Random)

    public void Growl()
    {
        if (growlClips == null || growlClips.Length == 0 || audioSource == null) return;
        AudioClip clip = growlClips[Random.Range(0, growlClips.Length)];
        if (clip != null) audioSource.PlayOneShot(clip);
    }

    //Yelp Sounds (Random)

    public void Yelp()
    {
        if (yelpClips == null || yelpClips.Length == 0 || audioSource == null) return;
        AudioClip clip = yelpClips[Random.Range(0, yelpClips.Length)];
        if (clip != null) audioSource.PlayOneShot(clip);
    }

    //Bark Sounds (Random)

    public void Bark()
    {
        if (barkClips == null || barkClips.Length == 0 || audioSource == null) return;
        AudioClip clip = barkClips[Random.Range(0, barkClips.Length)];
        if (clip != null) audioSource.PlayOneShot(clip);
    }

    //Roar Sounds (Random)

    public void Roar()
    {
        if (roarClips == null || roarClips.Length == 0 || audioSource == null) return;
        AudioClip clip = roarClips[Random.Range(0, roarClips.Length)];
        if (clip != null) audioSource.PlayOneShot(clip);
    }

    //Death Sounds (Random)

    public void Death()
    {
        if (deathClips == null || deathClips.Length == 0 || audioSource == null) return;
        AudioClip clip = deathClips[Random.Range(0, deathClips.Length)];
        if (clip != null) audioSource.PlayOneShot(clip);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    private AudioSource source;

    private void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlayClip(AudioClip clip)
    {
        if (clip != null)
        {
            source.clip = clip;
            source.Play();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandler : MonoBehaviour
{
    public static AudioHandler audio;
    public AudioClip discardSound, winSound, shuffleSound;
    public AudioSource source;
    bool networked;

    // Start is called before the first frame update
    void Start()
    {
        if(audio != null && audio != this)
        {
            Destroy(gameObject);
        }
        else
        {
            audio = this;
        }

    }

    public void PlayDiscard()
    {
        source.PlayOneShot(discardSound);
    }

    public void PlayShuffle()
    {
        source.PlayOneShot(shuffleSound);

    }

    public void PlayWin()
    {
        source.PlayOneShot(winSound);

    }

}

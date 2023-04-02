using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandler : MonoBehaviour
{
    public static AudioHandler audioHandler;
    public AudioClip discardSound, winSound, shuffleSound, diceSound;
    public AudioSource source;
    bool networked;

    // Start is called before the first frame update
    void Awake()
    {
        if(audioHandler != null && audioHandler != this)
        {
            Destroy(gameObject);
        }
        else
        {
            audioHandler = this;
        }
        source = GetComponent<AudioSource>();
    }

    public void PlayDiscard()
    {
        Debug.Log("discard noise");
        // source.PlayOneShot(discardSound);
    }

    public void PlayShuffle()
    {
        Debug.Log("shuffle noise");

        // source.PlayOneShot(shuffleSound);

    }

    public void PlayWin()
    {
        Debug.Log("win noise");

        // source.PlayOneShot(winSound);

    }
    public void PlayDiceRoll()
    {
        Debug.Log("dice sound");

        // source.PlayOneShot(diceSound);
    }

}

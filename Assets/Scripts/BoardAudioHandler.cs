using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardAudioHandler : MonoBehaviour
{
    public static BoardAudioHandler audioHandler;
    public AudioClip shuffleSound;
    public AudioClip[] discardSounds, diceSounds;
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
        Debug.Log("Playing Discard");

        source.PlayOneShot(discardSounds[(int) Random.Range(0, discardSounds.Length)]);
    }

    public void PlayShuffle()
    {
        Debug.Log("Playing Shuffle");
        source.PlayOneShot(shuffleSound);


    }
    public void PlayDiceRoll()
    {
        source.PlayOneShot(diceSounds[(int) Random.Range(0, diceSounds.Length)]);
    }

}

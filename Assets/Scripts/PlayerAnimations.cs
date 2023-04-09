using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    public Animation anim;
    public AnimationClip idle, discard, win, meld;
    public AudioSource voice, tileset;
    public AudioClip[] pongClips, kangClips, chowClips, todasClips;
    public AudioClip tileThud;
    public bool debug;
    void Awake()
    {
        voice = GetComponentInChildren<AudioSource>();
        tileset = transform.parent.GetComponent<MahjongPlayerBase>().closedHandParent.GetComponent<AudioSource>();
    }
    // Update is called once per frame
    void Update()
    {
        if (debug)
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                PlayDiscardAnim();
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                PlayStealAnim();
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                PlayWinAnim();
            }
        }

    }

    public void PlayDrawAnim()
    {
        anim[discard.name].normalizedTime = 1;
        anim[discard.name].speed = -1.25f;
        anim.Play(discard.name);
        // anim[discard.name].speed = 1;
        anim.PlayQueued(idle.name);
    }

    public void PlayShuffleAnim()
    {
        anim.Play(win.name);
        anim.PlayQueued(win.name);
        // anim.PlayQueued(win.name);
        // anim.PlayQueued(win.name);
        anim.PlayQueued(idle.name);
    }
    public void PlayDiscardAnim()
    {
        anim[discard.name].normalizedTime = 0;
        anim[discard.name].speed = 1f;
        anim.Play(discard.name);
        anim.PlayQueued(idle.name);
    }
    public void PlayStealAnim()
    {
        anim.Play(meld.name);
        anim.PlayQueued(idle.name);

        switch (transform.parent.GetComponent<MahjongPlayerBase>().currentDecision)
        {
            case decision.pong:
                voice.PlayOneShot(pongClips[Random.Range(0, pongClips.Length)]);
                break;
            case decision.kang:
                voice.PlayOneShot(kangClips[Random.Range(0, kangClips.Length)]);
                break;
            case decision.chow:
                voice.PlayOneShot(chowClips[Random.Range(0, chowClips.Length)]);
                break;
        }
    }
    public void PlayWinAnim()
    {
        anim.Play(win.name);
        anim.PlayQueued(idle.name);
        voice.PlayOneShot(todasClips[Random.Range(0, todasClips.Length)]);
        StartCoroutine(DelayThud());
    }
    IEnumerator DelayThud()
    {
        yield return new WaitForSeconds(1f);
        tileset.PlayOneShot(tileThud);
    }
}

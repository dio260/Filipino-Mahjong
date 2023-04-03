using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    public Animation anim;
    public AnimationClip idle, discard, win, meld;
    void Start()
    {
        // PlayDrawAnim();
    }
    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.I))
        // {
        //     PlayDiscardAnim();
        // }
        // if (Input.GetKeyDown(KeyCode.O))
        // {
        //     PlayStealAnim();
        // }
        // if (Input.GetKeyDown(KeyCode.P))
        // {
        //     PlayWinAnim();
        // }
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
        anim.PlayQueued(win.name);
        anim.PlayQueued(win.name);
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
    }
    public void PlayWinAnim()
    {
        anim.Play(win.name);
        anim.PlayQueued(idle.name);
    }
}

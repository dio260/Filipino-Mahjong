using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAnimations : MonoBehaviour
{
    public Animation anim;
    public AnimationClip idle, discard, win, meld;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            anim.clip = idle;
            anim.Play();
        }
        if(Input.GetKeyDown(KeyCode.O))
        {
            anim.clip = discard;
            anim.Play();
        }
        if(Input.GetKeyDown(KeyCode.P))
        {
            anim.clip = win;
            anim.Play();
        }
        if(Input.GetKeyDown(KeyCode.U))
        {
            anim.clip = meld;
            anim.Play();
        }
    }
}

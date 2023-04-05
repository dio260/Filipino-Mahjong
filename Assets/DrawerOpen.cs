using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawerOpen : MonoBehaviour
{
    public TMPro.TMP_InputField roomInput;
    public Animator anim;
    public bool open;
    // Start is called before the first frame update
    void Start()
    {
        open = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (roomInput.text.Length > 0 && !open)
        {
            Debug.Log("roomname exists");
            anim.SetTrigger("OpenTrigger");
            open = true;
        }
        else if (roomInput.text.Length == 0 && open)
        {
            Debug.Log("no room name");
            anim.SetTrigger("OpenTrigger");
            open = false;
        }
    }
}

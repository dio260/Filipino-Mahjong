using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawerOpen : MonoBehaviour
{
    public TMPro.TMP_InputField roomInput, nameInput;
    public Animator anim;
    public Button joinButton;
    public bool open;
    // Start is called before the first frame update
    void Start()
    {
        open = false;
        joinButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (roomInput.text.Length > 0 && nameInput.text.Length > 0 && !open)
        {
            Debug.Log("roomname and player name exists");
            anim.SetTrigger("OpenTrigger");
            open = true;
            joinButton.gameObject.SetActive(true);

        }
        else if ((roomInput.text.Length == 0 || nameInput.text.Length == 0) && open)
        {
            Debug.Log("no room name or player name");
            anim.SetTrigger("OpenTrigger");
            open = false;
            joinButton.gameObject.SetActive(false);

        }
    }
}

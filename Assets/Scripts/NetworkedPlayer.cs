using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkedPlayer : MonoBehaviourPunCallbacks
{
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            Debug.Log("mine");
            LocalPlayerInstance = gameObject;
            
        }
        else
        {
            Debug.Log("not mine");

            GetComponent<Camera>().enabled = false;
            GetComponentInChildren<Canvas>().gameObject.SetActive(false);
            // Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

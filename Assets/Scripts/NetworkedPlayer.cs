using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class NetworkedPlayer : MonoBehaviourPunCallbacks
{
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    public bool playAgain;

    Canvas playerCanvas;
    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            Debug.Log("mine");
            LocalPlayerInstance = gameObject;
            MultiplayerGameManager.Instance.multiplayerCanvas.
            transform.Find("RestartButton").GetComponent<Button>().
            onClick.AddListener(() => photonView.RPC("VoteToPlayAgain", RpcTarget.All));
        }
        else
        {
            Debug.Log("not mine");

            GetComponent<Camera>().enabled = false;
            GetComponent<AudioListener>().enabled = false;

            playerCanvas = GetComponentInChildren<Canvas>();
            playerCanvas.gameObject.SetActive(false);
            // Destroy(this);
        }
    }

    void VoteToPlayAgain()
    {
        playAgain = true;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

using Random = UnityEngine.Random;
public class MultiplayerMahjongManager : MonoBehaviourPunCallbacks
{
    public static MultiplayerMahjongManager multiMahjongManager;
    
    
    void Awake()
    {
        if (multiMahjongManager != null && multiMahjongManager != this)
        {
            Destroy(gameObject);
        }
        else
        {
            multiMahjongManager = this;
        }

        
    }
    void Start()
    {
        photonView.RPC("UpdatePlayerList", RpcTarget.All);

    }
    public void MasterRPCCall(string command, object data = null)
    {
        Debug.Log("Calling RPC " + command);

        // PhotonView photonView = PhotonView.Get(this);
        switch (command)
        {
            case "start":    
                photonView.RPC("StartGame", RpcTarget.All);
                break;
            case "message":
                string message = (string) data;
                photonView.RPC("SendClientsMessage", RpcTarget.All, message);
                break;
            case "dealer":
                int dealer = (int) data;
                photonView.RPC("SetDealer", RpcTarget.All, data);
                break;
        }
    }
    [PunRPC]
    public void StartGame()
    {
        Debug.Log("starting game for all clients");
        MultiplayerGameManager.Instance.multiplayerCanvas.SetActive(false);
        MahjongManager.mahjongManager.InitializeGame();
    }


    [PunRPC]
    public void SendClientsMessage(string message)
    {
        foreach(MahjongPlayerBase player in MahjongManager.mahjongManager.GetPlayers())
        {
            if (player.TryGetComponent<HumanPlayer>(out HumanPlayer human))
            {
                human.debugText.text = message;
            }
        }
    }

    [PunRPC]
    public void UpdatePlayerList()
    {
        MultiplayerGameManager.Instance.RemotePlayerListUpdate();
    }
    [PunRPC]
    public void RemoteSetDealer(int index)
    {
        MahjongManager.mahjongManager.SetDealer(index);
    }

    #region 
    

    #endregion
}

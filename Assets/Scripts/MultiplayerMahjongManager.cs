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
                photonView.RPC("RemoteSetDealer", RpcTarget.All, data);
                break;
            case "turn1":
                photonView.RPC("StartFirstTurn", RpcTarget.Others);
                break;
            case "discardSound":
                photonView.RPC("StartFirstTurn", RpcTarget.Others);
                break;
            case "shuffleSound":
                photonView.RPC("StartFirstTurn", RpcTarget.Others);
                break;
            case "winSound":
                photonView.RPC("StartFirstTurn", RpcTarget.Others);
                break;
        }
    }
    [PunRPC]
    public void StartGame()
    {
        SendClientsMessage("Starting game...");
        MultiplayerGameManager.Instance.multiplayerCanvas.SetActive(false);
        MahjongManager.mahjongManager.InitializeGame();
    }


    [PunRPC]
    public void SendClientsMessage(string message)
    {
        MahjongManager.mahjongManager.SendPlayersMessage(message);
    }

    [PunRPC]
    public void UpdatePlayerList()
    {
        Debug.Log("Updating Client Player List");
        MultiplayerGameManager.Instance.RemotePlayerListUpdate();
    }
    [PunRPC]
    public void RemoteSetDealer(int index)
    {
        MahjongManager.mahjongManager.SetDealer(index);
    }
    [PunRPC]
    public void StartFirstTurn()
    {
        MahjongManager.mahjongManager.FirstNetworkedTurn();
    }

    #region 
    

    #endregion
}

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
                string message = (string)data;
                photonView.RPC("SendClientsMessage", RpcTarget.All, message);
                break;
            case "dealer":
                int dealer = (int)data;
                photonView.RPC("RemoteSetDealer", RpcTarget.All, data);
                break;
            case "turn1":
                photonView.RPC("StartFirstTurn", RpcTarget.Others);
                break;
            case "discardSound":
                photonView.RPC("PlayAudio", RpcTarget.Others, command);
                break;
            case "shuffleSound":
                photonView.RPC("PlayAudio", RpcTarget.Others, command);
                break;
            case "winSound":
                photonView.RPC("PlayAudio", RpcTarget.Others, command);
                break;
            case "diceSound":
                photonView.RPC("PlayAudio", RpcTarget.Others, command);
                break;
            case "discardAnim":
                photonView.RPC("PlayAnimation", RpcTarget.Others, command);
                break;
            case "shuffleAnim":
                photonView.RPC("PlayAnimation", RpcTarget.Others, command);
                break;
            case "winAnim":
                photonView.RPC("PlayAnimation", RpcTarget.Others, command);
                break;
            case "stealAnim":
                photonView.RPC("PlayAnimation", RpcTarget.Others, command);
                break;
            case "drawAnim":
                photonView.RPC("PlayAnimation", RpcTarget.Others, command);
                break;
            case "drawFlowersAnim":
                photonView.RPC("PlayAnimation", RpcTarget.Others, command);
                break;
        }
    }
    [PunRPC]
    public void StartGame()
    {
        // SendClientsMessage("Starting game...");
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
    [PunRPC]
    public void PlayAudio(string audioClip)
    {
        switch (audioClip)
        {
            case "discardSound":
                BoardAudioHandler.audioHandler.PlayDiscard();
                break;
            case "shuffleSound":
                BoardAudioHandler.audioHandler.PlayShuffle();
                break;
            case "diceSound":
                BoardAudioHandler.audioHandler.PlayDiceRoll();
                break;
        }
    }
    [PunRPC]
    public void PlayAnimation(string animClip)
    {
        switch (animClip)
        {
            case "drawAnim":
                MahjongManager.mahjongManager.currentPlayer.currentAvatar.PlayDrawAnim();

                break;
            case "drawFlowersAnim":
                foreach (MahjongPlayerBase player in MahjongManager.mahjongManager.GetPlayers())
                {
                    player.currentAvatar.PlayDrawAnim();
                }
                break;
            case "discardAnim":
                MahjongManager.mahjongManager.currentPlayer.currentAvatar.PlayDiscardAnim();

                break;
            case "shuffleAnim":
                foreach (MahjongPlayerBase player in MahjongManager.mahjongManager.GetPlayers())
                {
                    player.currentAvatar.PlayShuffleAnim();
                }
                break;
            case "winAnim":
                MahjongManager.mahjongManager.currentPlayer.currentAvatar.PlayWinAnim();
                break;
            case "stealAnim":
                MahjongManager.mahjongManager.currentPlayer.currentAvatar.PlayStealAnim();
                break;
        }
    }

    #region 


    #endregion
}

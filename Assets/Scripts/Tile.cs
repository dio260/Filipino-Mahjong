using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public enum suit { stick, ball, character, flower };
public class Tile : MonoBehaviour
{
    public suit tileType;
    public int number;
    public bool onBoard, closed, open;

    public TMP_Text debugText;

    //to be used in hand win algo
    public bool winning;
    bool selectedForMeld;
    // Start is called before the first frame update
    void Awake()
    {
        // if (tileType == suit.flower)
        // {
        //     gameObject.name = tileType.ToString();
        //     debugText.text = tileType.ToString();

        // }
        // else
        // {
        //     gameObject.name = number + " " + tileType.ToString();
        //     debugText.text = number + " " + tileType.ToString();
        // }

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override string ToString()
    {
        return number + " " + tileType.ToString();
    }
    public void RPCTileSet(int num, suit suit)
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SetTile", RpcTarget.All, num, suit);
    }

    // public void RPCTileAdd()
    // {
    //     PhotonView photonView = PhotonView.Get(this);
    //     photonView.RPC("AddToBoard", RpcTarget.All);
    // }
    // public void RPCDiscardTile()
    // {
    //     PhotonView photonView = PhotonView.Get(this);
    //     photonView.RPC("SetAsDiscard", RpcTarget.Others);
    // }
    // public void RPCPlayerDiscardTile()
    // {
    //     PhotonView photonView = PhotonView.Get(this);
    //     photonView.RPC("SetAsDiscard", RpcTarget.Others);
    // }

    public void TileRPCCall(string command, object data = null)
    {
        PhotonView photonView = PhotonView.Get(this);
        switch (command)
        {
            case "BoardAdd":    
                photonView.RPC("AddToBoard", RpcTarget.All);
                break;
            case "PlayerDiscard":
                photonView.RPC("SetDiscardforPlayer", RpcTarget.All);
                break;
            case "ManagerDiscard":
                photonView.RPC("SetAsDiscard", RpcTarget.All);
                break;
            case "SelectForMeld":
                photonView.RPC("SetMeldforPlayer", RpcTarget.Others);
                break;
        }
    }

    [PunRPC]
    public void SetTile(int num, suit suit)
    {
        this.number = num;
        this.tileType = suit;
        if (tileType == suit.flower)
        {
            gameObject.name = tileType.ToString();
            debugText.text = tileType.ToString();

        }
        else
        {
            gameObject.name = number + " " + tileType.ToString();
            debugText.text = number + " " + tileType.ToString();
        }
        this.transform.parent = MahjongManager.mahjongManager.InitialTileParent.transform;
    }
    [PunRPC]
    public void AddToBoard()
    {
        if (!MahjongManager.mahjongManager.GetBoard().Contains(this))
        {
            MahjongManager.mahjongManager.GetBoard().Add(this);

        }
    }
    [PunRPC]
    public void SetAsDiscard()
    {
        MahjongManager.mahjongManager.mostRecentDiscard = this;
    }
    [PunRPC]
    public void SetDiscardforPlayer()
    {
        MahjongManager.mahjongManager.currentPlayer.SetDiscardChoice(this);
    }
    [PunRPC]
    public void SetMeldforPlayer()
    {
        Debug.Log("selected tile");
        MahjongManager.mahjongManager.currentPlayer.GetComponent<HumanPlayer>().SelectMeldTile(this);
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // object[] instantiationData = info.photonView.InstantiationData;
        // if(instantiationData[1])
        // if (tileType == suit.flower)
        // {
        //     gameObject.name = tileType.ToString();
        //     debugText.text = tileType.ToString();

        // }
        // else
        // {
        //     gameObject.name = number + " " + tileType.ToString();
        //     debugText.text = number + " " + tileType.ToString();
        // }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public enum suit { stick, ball, character, flower };
public class Tile : MonoBehaviour
{
    public suit tileType;
    public int number;
    public Image tileImage;
    public bool onBoard, closed, open;

    public TMP_Text debugText;

    //to be used in hand win algo
    public bool winning;
    bool selectedForMeld;
    public MahjongPlayerBase owner;
    // Start is called before the first frame update
    void Awake()
    {
        tileImage = GetComponentInChildren<Image>();
    }
    void Start()
    {
        // Debug.Log("Tile Start Called");
        // if(!PhotonNetwork.IsMasterClient)
        //     return;
        if (GetComponent<PhotonView>() == null)
        {
            // if (tileType == suit.flower)
            // {
            //     gameObject.name = tileType.ToString();
            //     debugText.text = tileType.ToString();

            // }
            // else
            // {
                gameObject.name = number + " " + tileType.ToString();
                debugText.text = number + " " + tileType.ToString();
            // }

            switch (tileType)
            {
                case suit.ball:
                    tileImage.sprite = TileSpriteCaller.sprites.ballsprites[number - 1];
                    break;
                case suit.stick:
                    tileImage.sprite = TileSpriteCaller.sprites.sticksprites[number - 1];
                    break;
                case suit.character:
                    tileImage.sprite = TileSpriteCaller.sprites.charsprites[number - 1];
                    break;
                case suit.flower:

                    if (number == 9)
                    {
                        tileImage.sprite = TileSpriteCaller.sprites.GetFlower2();
                    }
                    else if (number == 8)
                    {
                        tileImage.sprite = TileSpriteCaller.sprites.GetFlower1();
                    }
                    else
                    {
                        tileImage.sprite = TileSpriteCaller.sprites.winddragonsprites[number - 1];
                    }

                    break;
            }
        }


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
        photonView.RPC("SetTile", RpcTarget.AllBufferedViaServer, num, suit);
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
            case "RemoveRB":
                photonView.RPC("RemoveRigidbody", RpcTarget.All);
                break;
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
                photonView.RPC("SetMeldforPlayer", RpcTarget.All);
                break;
            case "SetAsDead":
                photonView.RPC("SetAsDead", RpcTarget.All);
                break;
        }
    }
    [PunRPC]
    public void RemoveRigidbody()
    {
        Destroy(GetComponent<Rigidbody>());
    }

    [PunRPC]
    public void SetTile(int num, suit suit)
    {
        // Debug.Log("Tile Set Called");

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

        switch (tileType)
        {
            case suit.ball:
                tileImage.sprite = TileSpriteCaller.sprites.ballsprites[number - 1];
                break;
            case suit.stick:
                tileImage.sprite = TileSpriteCaller.sprites.sticksprites[number - 1];
                break;
            case suit.character:
                tileImage.sprite = TileSpriteCaller.sprites.charsprites[number - 1];
                break;
            case suit.flower:

                if (number == 9)
                {
                    tileImage.sprite = TileSpriteCaller.sprites.GetFlower2();
                }
                else if (number == 8)
                {
                    tileImage.sprite = TileSpriteCaller.sprites.GetFlower1();
                }
                else
                {
                    tileImage.sprite = TileSpriteCaller.sprites.winddragonsprites[number - 1];
                }

                break;
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
        // MahjongManager.mahjongManager.currentPlayer.SetDiscardChoice(this);
        // owner.SetDiscardChoice(this);
        if (owner.GetComponent<HumanPlayer>() != null)
        {
            owner.GetComponent<HumanPlayer>().SelectDiscardTile(this);
        }
    }
    [PunRPC]
    public void SetMeldforPlayer()
    {
        Debug.Log("selected tile");
        owner.GetComponent<HumanPlayer>().SelectMeldTile(this);
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
    }
}

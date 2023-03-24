using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public enum suit { stick, ball, character, flower };
public class Tile : MonoBehaviour, IPunInstantiateMagicCallback
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
    public void testRPC(int num, suit suit)
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SetTile", RpcTarget.All, num, suit);
    }
    public void testRPC2()
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SetTile", RpcTarget.All, this.number, this.tileType);
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

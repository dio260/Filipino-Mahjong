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
    BoxCollider tilebounds;
    List<Tile> networkedTiles = new List<Tile>();
    [Tooltip("The tile prefab")]
    [SerializeField]
    private GameObject tilePrefab;
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

        tilebounds = GameObject.Find("TileBoundaries").GetComponent<BoxCollider>();
    }
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Let's instantiate the tiles
            for (int x = 0; x < 144; x++)
            {
                GameObject tileInstance = PhotonNetwork.Instantiate(this.tilePrefab.name,
                new Vector3(Random.Range(tilebounds.bounds.min.x, tilebounds.bounds.max.x), Random.Range(0, tilebounds.bounds.max.y), Random.Range(tilebounds.bounds.min.z, tilebounds.bounds.max.z)), Quaternion.identity);
                tileInstance.transform.parent = GameObject.Find("Tiles").transform;
                networkedTiles.Add(tileInstance.GetComponent<Tile>());
                if (x < 36)
                {
                    tileInstance.GetComponent<Tile>().RPCTileSet(x / 4 + 1, suit.ball);
                    continue;
                }
                // break;
                if (x < 72)
                {
                    tileInstance.GetComponent<Tile>().RPCTileSet((x - 36) / 4 + 1, suit.character);
                    continue;
                }
                // break;
                if (x < 108)
                {
                    tileInstance.GetComponent<Tile>().RPCTileSet((x - 72) / 4 + 1, suit.stick);
                    continue;
                }
                if (x < 144)
                {
                    tileInstance.GetComponent<Tile>().RPCTileSet((x - 108) / 4 + 1, suit.flower);
                    continue;
                }

                // tile.transform.Rotate(new Vector3(0, 0, -90));
            }
        }

    }
    public void MasterRPCCall(string command)
    {
        Debug.Log("Calling RPC " + command);

        PhotonView photonView = PhotonView.Get(this);
        switch (command)
        {
            case "start":    
                photonView.RPC("StartGame", RpcTarget.All);
                break;
            case "board":

                photonView.RPC("BoardUpdate", RpcTarget.All, MahjongManager.mahjongManager.GetBoard());
                break;
            case "":
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
    public void BoardUpdate(List<Tile> board)
    {   
        MahjongManager.mahjongManager.SetRemoteBoardValues(board);
    }

    [PunRPC]
    public void SendClientsMessage(string message)
    {
        foreach(MahjongPlayerBase player in MahjongManager.mahjongManager.GetPlayers())
        {
            if (TryGetComponent<HumanPlayer>(out HumanPlayer human))
            {
                human.debugText.text = message;
            }
        }
    }

    #region 
    public override void OnPlayerEnteredRoom(Player other)
    {
       
        if (!PhotonNetwork.IsMasterClient)
        {

            //have to do this so everyone has the same tileset
            foreach (Tile tile in networkedTiles)
            {
                tile.RPCTileSet(tile.number, tile.tileType);
                tile.transform.parent = GameObject.Find("Tiles").transform;
            }
        }


    }

    #endregion
}

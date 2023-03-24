using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Photon.Realtime;
using Photon.Pun;
using TMPro;

public class MultiplayerGameManager : MonoBehaviourPunCallbacks
{
    #region Public Fields

    static public MultiplayerGameManager Instance;

    #endregion

    #region Private Fields

    private GameObject instance;

    [Tooltip("The prefab to use for representing the player")]
    [SerializeField]
    private GameObject playerPrefab;
    [Tooltip("The prefab for managing game state")]
    [SerializeField]
    private GameObject gameManagerPrefab;

    BoxCollider tilebounds;
    List<Tile> networkedTiles = new List<Tile>();
    [Tooltip("The tile prefab")]
    [SerializeField]
    private GameObject tilePrefab;
    public GameObject multiplayerCanvas;

    public Button gameStart;

    public List<MahjongPlayerBase> players;
    public Material[] playerColors;

    // PhotonView photonView;

    #endregion
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        // photonView = PhotonView.Get(this);
        multiplayerCanvas = GameObject.Find("Multiplayer Canvas");
        multiplayerCanvas.transform.Find("Room Title").GetComponent<TMP_Text>().text = "Room Name: " + PhotonNetwork.CurrentRoom.Name;
        
        tilebounds = GameObject.Find("TileBoundaries").GetComponent<BoxCollider>();

        // in case we started this demo with the wrong scene being active, simply load the menu scene
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("Main Menu");

            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            // PhotonNetwork.InstantiateRoomObject(this.gameManagerPrefab.name, Vector3.zero, Quaternion.identity, 0 , new object[]{ 7, suit.ball});
            PhotonNetwork.InstantiateRoomObject(this.gameManagerPrefab.name, Vector3.zero, Quaternion.identity);
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
        else
        {
            gameStart.gameObject.SetActive(false);
        }


        if (playerPrefab == null)
        { // #Tip Never assume public properties of Components are filled up properly, always check and inform the developer of it.

            Debug.LogError("<Color=Red><b>Missing</b></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {


            if (NetworkedPlayer.LocalPlayerInstance == null)
            {

                Debug.Log("We are Instantiating LocalPlayer from " + SceneManagerHelper.ActiveSceneName + " Current Players: " + PhotonNetwork.CurrentRoom.PlayerCount);

                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                // switch based on current player count
                GameObject player;
                switch (PhotonNetwork.CurrentRoom.PlayerCount)
                {
                    case 1:
                        player = PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 0.25f, -1.5f), Quaternion.identity, 0);
                        player.name = PhotonNetwork.NickName;
                        player.GetComponentInChildren<MeshRenderer>().material = playerColors[0];
                        players.Add(player.GetComponent<MahjongPlayerBase>());
                        break;
                    case 2:
                        player = PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(1.5f, 0.25f, 0f), Quaternion.AngleAxis(-90, Vector3.up), 0);
                        player.name = PhotonNetwork.NickName;
                        player.GetComponentInChildren<MeshRenderer>().material = playerColors[1];

                        players.Add(player.GetComponent<MahjongPlayerBase>());
                        break;
                    case 3:
                        player = PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 0.25f, 1.5f), Quaternion.AngleAxis(-180, Vector3.up), 0);
                        player.name = PhotonNetwork.NickName;
                        player.GetComponentInChildren<MeshRenderer>().material = playerColors[2];

                        players.Add(player.GetComponent<MahjongPlayerBase>());

                        break;
                    case 4:
                        player = PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(-1.5f, 0.25f, 0f), Quaternion.AngleAxis(-270, Vector3.up), 0);
                        player.name = PhotonNetwork.NickName;
                        player.GetComponentInChildren<MeshRenderer>().material = playerColors[3];

                        players.Add(player.GetComponent<MahjongPlayerBase>());

                        break;
                }


            }
            else
            {
                Debug.Log("A networked player is joining. Current Players: " + PhotonNetwork.CurrentRoom.PlayerCount);
                // Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        // "back" button of phone equals "Escape". quit app if that's pressed
        // if (Input.GetKeyDown(KeyCode.Escape))
        // {
        //     Debug.Log("byebye");
        //     Application.Quit();
        // }
    }

    public void HostStartGame()
    {
        MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("start");
    }

    /// <summary>
    /// Called when a Photon Player got connected. We need to then load a bigger scene.
    /// </summary>
    /// <param name="other">Other.</param>
    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.Log("OnPlayerEnteredRoom() " + other.NickName); // not seen if you're the player connecting

        if (PhotonNetwork.IsMasterClient)
        {

            Debug.Log("updating tileset for joined player");
            //have to do this so everyone has the same tileset
            foreach (Tile tile in networkedTiles)
            {
                tile.RPCTileSet(tile.number, tile.tileType);
                tile.transform.parent = GameObject.Find("Tiles").transform;
            }
        }


    }

    /// <summary>
    /// Called when a Photon Player got disconnected. We need to load a smaller scene.
    /// </summary>
    /// <param name="other">Other.</param>
    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.Log("OnPlayerLeftRoom() " + other.NickName); // seen when other disconnects
    }

    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Main Menu");
    }

    
    public void RemoteNameListUpdate()
    {
        multiplayerCanvas.transform.Find("Player List").Find("Player List Text").GetComponent<TMP_Text>().text = "";
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            multiplayerCanvas.transform.Find("Player List").Find("Player List Text").GetComponent<TMP_Text>().text += player.NickName + '\n';
        }
        
    }
}

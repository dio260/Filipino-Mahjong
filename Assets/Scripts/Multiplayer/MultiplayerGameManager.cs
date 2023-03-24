using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Photon.Realtime;
using Photon.Pun;

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
    public GameObject multiplayerCanvas;

    public Button gameStart;

    #endregion
    // Start is called before the first frame update
    void Start()
    {

        Instance = this;

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
                switch (PhotonNetwork.CurrentRoom.PlayerCount)
                {
                    case 1:
                        PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 0.25f, -1.5f), Quaternion.identity, 0);
                        break;
                    case 2:
                        PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(1.5f, 0.25f, 0f), Quaternion.AngleAxis(-90, Vector3.up), 0);
                        break;
                    case 3:
                        PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 0.25f, 1.5f), Quaternion.AngleAxis(-180, Vector3.up), 0);
                        break;
                    case 4:
                        PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(-1.5f, 0.25f, 0f), Quaternion.AngleAxis(-270, Vector3.up), 0);
                        break;
                }

            }
            else
            {

                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }


        }
    }

    // Update is called once per frame
    void Update()
    {
        // "back" button of phone equals "Escape". quit app if that's pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("byebye");
            Application.Quit();
        }
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
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
        }
    }

    /// <summary>
    /// Called when a Photon Player got disconnected. We need to load a smaller scene.
    /// </summary>
    /// <param name="other">Other.</param>
    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.Log("OnPlayerLeftRoom() " + other.NickName); // seen when other disconnects

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
        }
    }

    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("PunBasics-Launcher");
    }
}

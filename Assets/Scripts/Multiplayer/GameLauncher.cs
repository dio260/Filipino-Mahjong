using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Photon.Realtime;
using Photon.Pun;
using TMPro;

public class GameLauncher : MonoBehaviourPunCallbacks
{
    [Tooltip("The Ui Text to inform the user about the connection progress")]
    [SerializeField]
    private Text feedbackText;
    [SerializeField]
    private TMP_InputField roomName;

    [Tooltip("The maximum number of players per room")]
    // [SerializeField]
    private byte maxPlayersPerRoom = 4;
    bool connecting;
    string gameVersion = "1";
    void Awake()
    {
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void Connect()
    {
        connecting = true;
        LogFeedback("Connecting...");
        // #Critical, we must first and foremost connect to Photon Online Server.
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = this.gameVersion;
        ConnectToNamedGame();
    }

    public void ConnectToRandomGame()
    {
        connecting = true;
        if (PhotonNetwork.IsConnected)
        {
            LogFeedback("Joining Room...");
            //#Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            LogFeedback("Connecting...");
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = this.gameVersion;
        }
    }
    public void ConnectToNamedGame()
    {


        if (PhotonNetwork.IsConnected)
        {
            if (roomName.text == "")
            {
                LogFeedback("Joining Random Room...");
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                LogFeedback("Joining Named Room...");

                //#Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
                //PhotonNetwork.JoinRoom(gameName);

                RoomOptions options = new RoomOptions();
                options.IsVisible = false;
                options.MaxPlayers = 4;
                // options.CleanupCacheOnLeave = false;
                StartCoroutine(DelayedJoin(roomName.text, options, null));
                // PhotonNetwork.JoinOrCreateRoom(roomName.text, options, null);
            }

        }
        else
        {
            LogFeedback("Connecting...");
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = this.gameVersion;
        }

        // RoomOptions options = new RoomOptions();
        // options.IsVisible = false;
        // PhotonNetwork.JoinOrCreateRoom(roomName.text, options, null);
    }

    IEnumerator DelayedJoin(string name, RoomOptions options, TypedLobby lobby)
    {
        yield return new WaitForSeconds(3f);
        PhotonNetwork.JoinOrCreateRoom(name, options, lobby);
    }

    void LogFeedback(string message)
    {
        if (feedbackText == null)
        {
            return;
        }

        feedbackText.text += System.Environment.NewLine + message;
    }


    #region MonoBehaviourPunCallbacks CallBacks
    // PUN callbacks

    public override void OnConnectedToMaster()
    {
        // we don't want to do anything if we are not attempting to join a room. 
        // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
        // we don't want to do anything.
        if (connecting)
        {
            LogFeedback("OnConnectedToMaster: Next -> try to Join Random Room");
            Debug.Log("OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room.\n Calling: PhotonNetwork.JoinRandomRoom(); Operation will fail if no room found");

            // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
            //PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        LogFeedback("<Color=Red>OnJoinRandomFailed</Color>: Next -> Create a new Room");
        Debug.Log("OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        //PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = this.maxPlayersPerRoom });
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        LogFeedback("<Color=Red>OnDisconnected</Color> " + cause);
        Debug.LogError("PUN Basics Tutorial/Launcher:Disconnected");

        // #Critical: we failed to connect or got disconnected. There is not much we can do. Typically, a UI system should be in place to let the user attemp to connect again.
        //loaderAnime.StopLoaderAnimation();

        connecting = false;
        //controlPanel.SetActive(true);

    }

    public override void OnJoinedRoom()
    {
        LogFeedback("<Color=Green>OnJoinedRoom</Color> with " + PhotonNetwork.CurrentRoom.PlayerCount + " Player(s)");
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.\nFrom here on, your game would be running.");

        //#Critical: We only load if we are the first player, else we rely on  PhotonNetwork.AutomaticallySyncScene to sync our instance scene.
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Debug.Log("Loading the Game Room");

            // #Critical
            // Load the Room Level. 
            PhotonNetwork.LoadLevel("Multiplayer Game Room");

        }
    }

    #endregion
}

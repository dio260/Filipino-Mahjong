using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Realtime;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Public Fields

    static public GameManager Instance;

    #endregion

    #region Private Fields

    private GameObject instance;

    [Tooltip("The prefab to use for representing the player")]
    [SerializeField]
    private GameObject playerPrefab;

    #endregion
    // Start is called before the first frame update
    void Start()
    {

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

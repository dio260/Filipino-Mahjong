using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum decision { pong, kang, chow, pass }
public enum PlayerState { waiting, turn }
public class MahjongPlayerBase : MonoBehaviour
{
    // int[,] hand = new int[,] {
    //     {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
    //     {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
    //     {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
    //     {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
    // };
    private List<Tile> closedHand, openHand;
    private List<Tile> walls;
    private List<Tile> flowers;
    //private Tile drawn;
    private MahjongManager gameManager;
    //private List<Tile> hand;
    // Start is called before the first frame update
    private int score;
    private static int maxHandSize = 17;

    private bool win;
    private bool waiting;
    public decision currentDecision;
    private Tile currentTile;
    public Tile discardChoice;
    public PlayerState currentState;

    public Button chowButton, pongButton, kangButton, todasButton;

    #region Internal Calculation Variables
    List<Tile> balls = new List<Tile>();
    List<Tile> sticks = new List<Tile>();
    List<Tile> chars = new List<Tile>();
    #endregion

    //stuff to be moved to HumanPlayer child class
    Camera playerCam;
    public Transform closedHandParent;

    void Awake()
    {
        //hand = new List<Tile>(16);
        currentDecision = decision.pass;

        //move to Humanplayer
        playerCam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(Input.mousePosition);
        Vector3 mouseWorldPos = playerCam.ViewportToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, playerCam.nearClipPlane));
        
        Ray mouseWorldRay = playerCam.ScreenPointToRay(Input.mousePosition);
        Debug.Log(mouseWorldRay.origin);
        if(Physics.Raycast(mouseWorldRay, out RaycastHit hit, Vector3.Distance(transform.position, closedHandParent.position)))
        {
            Debug.Log("touched");
            if(closedHandParent.Find(hit.transform.name) != null && Input.GetMouseButton(0))
            {
                Debug.Log("holding tile");
                hit.transform.position = new Vector3(mouseWorldRay.origin.x, mouseWorldRay.origin.y, hit.transform.position.z);
            }
        }
    }
    void FixedUpdate()
    {
        if (currentState == PlayerState.turn)
        {

        }

        // CalculateHandOptions();
    }
    void CalculateHandOptions()
    {
        //make life easier by sorting the stuff;
        SortTilesBySuit();

        //most priority is a winning hand, takes precedence
        //edge case, closed hand size is 1, so waiting on the last pair
        if (closedHand.Count == 1 &&
        closedHand[0].number == MahjongManager.mahjongManager.mostRecentDiscard.number &&
        closedHand[0].tileType == MahjongManager.mahjongManager.mostRecentDiscard.tileType)
        {
            todasButton.gameObject.SetActive(true);
        }
        else if (CalculateSevenPairs())
        {
            todasButton.gameObject.SetActive(true);
        }
        else if (CalculateNormalWin())
        {
            todasButton.gameObject.SetActive(true);
        }

        //find pong and kang

    }

    void MakeDecision(decision dec)
    {
        //if conditions are fulfilled
        currentDecision = dec;
    }
    void DeclareWin()
    {
        //if conditions are fulfilled


    }

    bool CalculateSevenPairs()
    {
        return false;
    }
    bool CalculateNormalWin()
    {
        return false;
    }
    void SortTilesBySuit()
    {
        //first get suits
        balls = new List<Tile>();
        sticks = new List<Tile>();
        chars = new List<Tile>();

        foreach (Tile tile in closedHand)
        {
            switch (tile.tileType)
            {
                case suit.ball:
                    balls.Add(tile);
                    break;
                case suit.stick:
                    sticks.Add(tile);
                    break;
                case suit.character:
                    chars.Add(tile);
                    break;

            }
        }
    }

    public bool IsWaiting()
    {
        return waiting;
    }

    public void AddFlower(Tile flower)
    {
        flowers.Add(flower);
    }
    public Tile currentDrawnTile()
    {
        return currentTile;
    }
    public void StealTile()
    {
        //add stolen tile and its meld to the open hand
        openHand.Add(MahjongManager.mahjongManager.mostRecentDiscard);
        // MahjongManager.mahjongManager.wall.mostRe;
    }
    public void DrawTile()
    {
        currentTile = MahjongManager.mahjongManager.wall[0];
        MahjongManager.mahjongManager.wall.RemoveAt(0);
    }
    public void DrawFlowerTile()
    {
        flowers.Add(currentTile);
        currentTile = MahjongManager.mahjongManager.wall[walls.Count - 1];
        MahjongManager.mahjongManager.wall.RemoveAt(walls.Count - 1);
    }
}

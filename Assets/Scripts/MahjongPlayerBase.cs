using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum decision { pass, pong, kang, chow }
public enum PlayerState { waiting, turn }
public class MahjongPlayerBase : MonoBehaviour
{
    // int[,] hand = new int[,] {
    //     {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
    //     {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
    //     {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
    //     {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
    // };
    protected List<Tile> closedHand = new List<Tile>(), openHand = new List<Tile>();
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



    #region Internal Calculation Variables
    List<Tile> balls = new List<Tile>();
    List<Tile> sticks = new List<Tile>();
    List<Tile> chars = new List<Tile>();
    #endregion

    
    public Transform closedHandParent;
    public Button chowButton, pongButton, kangButton, todasButton;

    void Start()
    {
        //hand = new List<Tile>(16);
        // closedHand = new List<Tile>();
        // openHand = new List<Tile>();
        currentDecision = decision.pass;
        closedHandParent.position = transform.position + transform.forward * 0.7f;
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

        Tile discard = MahjongManager.mahjongManager.mostRecentDiscard;
        //most priority is a winning hand, takes precedence
        //edge case, closed hand size is 1, so waiting on the last pair
        if (closedHand.Count == 1 &&
        closedHand[0].number == discard.number &&
        closedHand[0].tileType == discard.tileType)
        {
            //GUI stuff probably needs to be moved to Human as well
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

        // find melds. need to figure out how to determine melds for transferring to the open hand
        //find melds
        List<Tile> temp = new List<Tile>();
        switch (discard.tileType)
        {
            case suit.ball:
                temp = balls;
                break;
            case suit.character:
                temp = chars;
                break;
            case suit.stick:
                temp = sticks;
                break;
        }

        int matchCount = 0;
        int seqCount = 0;

        foreach (Tile tile in temp)
        {
            if(tile.number == discard.number)
            {
                matchCount += 1;
            }

            //take advantage of the subarrays being sorted numerically for chow
            // if(tile.number == discard.number -)
        }

        if(matchCount > 2)
        {
            kangButton.gameObject.SetActive(true);
        }
        if(matchCount == 2)
        {
            pongButton.gameObject.SetActive(true);
        }


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

    public void VisuallySortTiles()
    {
        // SortTilesBySuit();
        Vector3 localLeft = -1 * Vector3.Cross(closedHandParent.forward.normalized, closedHandParent.up.normalized);
        float sideOffset = 1.5f / closedHand.Count;
        float placementReference = 1.5f  / 2;
        Debug.Log("left: " + localLeft + " off: " + sideOffset + " ref: "  + placementReference);
        int multiplier = 0;
        foreach(Tile tile in closedHand)
        {
            
            tile.transform.position = closedHandParent.transform.position + new Vector3() + localLeft * ( sideOffset * multiplier);
            multiplier += 1;
        }
    }

    public bool IsWaiting()
    {
        return waiting;
    }

    public void AddTile(Tile tile)
    {
        closedHand.Add(tile);
        tile.transform.parent = closedHandParent;

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

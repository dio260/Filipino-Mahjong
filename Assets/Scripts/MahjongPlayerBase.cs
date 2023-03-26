using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon;

public enum decision { none, pass, pong, kang, chow }
public enum PlayerState { waiting, deciding, discarding }
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
    private List<Tile> flowers = new List<Tile>();
    //private Tile drawn;
    private MahjongManager gameManager;
    //private List<Tile> hand;
    // Start is called before the first frame update
    private int score;
    private static int maxHandSize = 17;

    protected bool win, canPong, canKang, canChow;
    private bool waiting;
    public decision currentDecision;
    protected Tile drawnTile;
    public Tile discardChoice;
    public PlayerState currentState;

    #region Internal Calculation Variables
    protected List<Tile> balls = new List<Tile>();
    protected List<Tile> sticks = new List<Tile>();
    protected List<Tile> chars = new List<Tile>();
    protected List<Tile> pongMeld = new List<Tile>();
    protected List<Tile> kangMeld = new List<Tile>();
    protected List<Tile> chowMeldLeft = new List<Tile>();
    protected List<Tile> chowMeldMiddle = new List<Tile>();
    protected List<Tile> chowMeldRight = new List<Tile>();
    protected List<Tile> selectedTiles = new List<Tile>();
    #endregion


    public Transform closedHandParent, openHandParent, flowersParent;

    public bool networked;

    void Awake()
    {
        currentDecision = decision.none;
        closedHandParent.position = transform.position + transform.forward * 0.65f + transform.up * -0.15f;
        Vector3 left = Vector3.Cross(transform.forward.normalized, transform.up.normalized);
        flowersParent.position = transform.position + transform.forward * 0.8f + transform.up * -0.15f + left * 0.4f;
        openHandParent.position = transform.position + transform.forward * 0.8f + transform.up * -0.15f + left * -0.4f;


    }


    protected void FixedUpdate()
    {
        // if (currentState == PlayerState.de)
        // {

        // }

        // if(Input.GetKeyDown(KeyCode.Space))
        // {
        //     Debug.Log("sorting");
        //     SortTilesBySuit();
        // }

        // CalculateHandOptions();
    }
    public virtual void CalculateHandOptions()
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
            // todasButton.gameObject.SetActive(true);
        }
        else if (CalculateSevenPairs())
        {
            // todasButton.gameObject.SetActive(true);
        }
        else if (CalculateNormalWin())
        {
            // todasButton.gameObject.SetActive(true);
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

        pongMeld = new List<Tile> { discard };
        chowMeldLeft = new List<Tile> { discard };
        chowMeldMiddle = new List<Tile> { discard };
        chowMeldRight = new List<Tile> { discard };
        kangMeld = new List<Tile> { discard };

        foreach (Tile tile in temp)
        {
            if (tile.number == discard.number)
            {
                if (pongMeld.Count < 3)
                    pongMeld.Add(tile);
                kangMeld.Add(tile);
            }

            //take advantage of the subarrays being sorted numerically for chow
            //consider three types of chow melds
            //outermost number
            if (tile.number == discard.number - 2 && !HasNumber(chowMeldLeft, discard.number))
            {
                chowMeldLeft.Add(tile);
            }
            if (tile.number == discard.number + 2 && !HasNumber(chowMeldRight, discard.number))
            {
                chowMeldRight.Add(tile);
            }

            if (tile.number == discard.number - 1)
            {
                if (!HasNumber(chowMeldLeft, discard.number))
                    chowMeldLeft.Add(tile);
                if (!HasNumber(chowMeldMiddle, discard.number))
                    chowMeldMiddle.Add(tile);
            }

            if (tile.number == discard.number + 1)
            {
                if (!HasNumber(chowMeldRight, discard.number))
                    chowMeldRight.Add(tile);
                if (!HasNumber(chowMeldMiddle, discard.number))
                    chowMeldMiddle.Add(tile);
            }

        }

        if (pongMeld.Count == 3)
        {
            canPong = true;
        }
        if (kangMeld.Count == 4)
        {
            canKang = true;
        }
        if ((chowMeldLeft.Count == 3
        || chowMeldMiddle.Count == 3
        || chowMeldRight.Count == 3) &&
        (MahjongManager.mahjongManager.GetPlayers().IndexOf(MahjongManager.mahjongManager.previousPlayer) + 1) %
        MahjongManager.mahjongManager.GetPlayers().Count == MahjongManager.mahjongManager.GetPlayers().IndexOf(this)

        )
        {
            canChow = true;
        }
    }

    protected bool HasNumber(List<Tile> chowOptions, int number)
    {
        foreach (Tile tile in chowOptions)
        {
            if (tile.number == number)
                return true;
        }
        return false;
    }

    protected void MakeDecision(decision dec)
    {
        //if conditions are fulfilled
        if (networked)
        {
            switch (dec)
            {
                case decision.pass:
                    GetComponent<PhotonView>().RPC("passTurn", RpcTarget.All);
                    break;
            }

        }
        else
        {
            currentDecision = dec;

        }
    }
    protected void DeclareWin()
    {
        //if conditions are fulfilled

    }
    void DeclareSevenPairs()
    {

    }

    protected virtual void DeclarePong()
    {
        if (canPong)
        {
            openHand.AddRange(pongMeld);
            canPong = false;
        }
    }
    protected virtual void DeclareKang()
    {
        if (canKang)
        {
            openHand.AddRange(kangMeld);
            canKang = false;
        }
    }

    protected virtual void DeclareChow()
    {
        if (canChow)
        {
            // openHand.AddRange(chowMeld);
            canChow = false;
        }
    }
    [PunRPC]
    protected void passTurn()
    {
        currentDecision = decision.pass;
    }

    protected bool CalculateSevenPairs()
    {
        return false;
    }
    protected bool CalculateNormalWin()
    {
        return false;
    }

    // protected bool CalculateFlush()
    // {
    //     bool res = true;
    //     foreach()
    // }
    protected void SortTilesBySuit()
    {
        //first get suits
        balls = new List<Tile>();
        sticks = new List<Tile>();
        chars = new List<Tile>();
        // flowers = new List<Tile>();

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
        balls.Sort(CompareTileNumbers);
        sticks.Sort(CompareTileNumbers);
        chars.Sort(CompareTileNumbers);

        closedHand = new List<Tile>();
        closedHand.AddRange(balls);
        closedHand.AddRange(sticks);
        closedHand.AddRange(chars);

    }

    private static int CompareTileNumbers(Tile x, Tile y)
    {
        if (x == null)
        {
            if (y == null)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
        else
        {
            if (y == null)
            {
                return 1;
            }
            else
            {
                int retval = x.number.CompareTo(y.number);

                if (retval != 0)
                {
                    return retval;
                }
                else
                {
                    return x.number.CompareTo(y.number);
                }
            }
        }
    }

    public void ArrangeTiles()
    {
        Vector3 localLeft = 1 * Vector3.Cross(closedHandParent.forward.normalized, closedHandParent.up.normalized);
        float sideOffset = 1.25f / (float)closedHand.Count;
        float placementReference = 1.25f / 2.0f;

        foreach (Tile tile in closedHand)
        {
            tile.transform.localPosition = new Vector3(-1, 0, 0) * (placementReference);
            tile.transform.localEulerAngles = closedHandParent.up * 90;
            placementReference -= sideOffset;
        }
    }

    public void VisuallySortTiles()
    {
        //first call the sorting function
        SortTilesBySuit();
        ArrangeTiles();

    }

    public int replaceInitialFlowerTiles()
    {
        List<Tile> flowersInHand = new List<Tile>();
        // foreach(Tile tile in closedHand)
        for (int i = 0; i < closedHand.Count; i++)
        {
            if (closedHand[i].tileType == suit.flower)
            {
                flowersInHand.Add(closedHand[i]);
                AddFlower(closedHand[i]);
                i--;
            }
        }

        // flowers.AddRange(flowersInHand);

        // foreach(Tile tile in flowersInHand)
        // {
        //     closedHand.Remove(tile);
        // }

        // StartCoroutine(Wait)


        int newFlowerCount = 0;
        for (int x = 0; x < flowersInHand.Count; x++)
        {
            Tile drawnTile = MahjongManager.mahjongManager.wall[MahjongManager.mahjongManager.wall.Count - 1];
            if (drawnTile.tileType == suit.flower)
                newFlowerCount += 1;
            AddTile(drawnTile);
            MahjongManager.mahjongManager.wall.RemoveAt(MahjongManager.mahjongManager.wall.Count - 1);
        }

        return newFlowerCount;

    }

    public bool IsWaiting()
    {
        return waiting;
    }

    public void AddTile(Tile tile)
    {
        tile.owner = this;
        closedHand.Add(tile);
        tile.transform.parent = closedHandParent;

    }
    public void AddFlower(Tile flower)
    {
        flowers.Add(flower);
        flower.transform.parent = flowersParent;
        closedHand.Remove(flower);

        Vector3 localLeft = -1 * Vector3.Cross(flowersParent.forward.normalized, flowersParent.up.normalized);
        float sideOffset = 0.5f / (float)flowers.Count;
        float placementReference = 0.5f / -2.0f;
        foreach (Tile tile in flowers)
        {
            tile.transform.localPosition = new Vector3(-1, 0, 0) * (placementReference);
            tile.transform.localEulerAngles = flowersParent.up * 90 + Vector3.forward * 90;// + tile.transform.forward * -90;
            placementReference += sideOffset;
        }

    }
    public Tile currentDrawnTile()
    {
        return drawnTile;
    }
    public void StealTile()
    {
        //add stolen tile and its meld to the open hand
        // openHand.Add(MahjongManager.mahjongManager.mostRecentDiscard);
        MahjongManager.mahjongManager.mostRecentDiscard.owner = this;
        switch (currentDecision)
        {
            case decision.pong:
                openHand.AddRange(pongMeld);
            break;
            case decision.kang:
                openHand.AddRange(kangMeld);

            break;
            case decision.chow:
                openHand.AddRange(selectedTiles);
            break;
        }

        // MahjongManager.mahjongManager.mostRecentDiscard = null;
    }
    public void DrawTile()
    {
        drawnTile = MahjongManager.mahjongManager.wall[0];
        MahjongManager.mahjongManager.wall.RemoveAt(0);
        // closedHand.Add(drawnTile);
        // Debug.Log("drew tile " + drawnTile);
        // ArrangeTiles();
    }
    public void DrawFlowerTile()
    {
        flowers.Add(drawnTile);
        drawnTile = MahjongManager.mahjongManager.wall[MahjongManager.mahjongManager.wall.Count - 1];
        // closedHand.Add(drawnTile);
        MahjongManager.mahjongManager.wall.RemoveAt(MahjongManager.mahjongManager.wall.Count - 1);
        // ArrangeTiles();
    }
    public void DeclareDiscard()
    {
        if (networked)
        {
            GetComponent<PhotonView>().RPC("DiscardTile", RpcTarget.All);
        }
        else
        {
            DiscardTile();
        }
    }
    [PunRPC]
    public void DiscardTile()
    {
        MahjongManager.mahjongManager.mostRecentDiscard = discardChoice;
        closedHand.RemoveAt(closedHand.IndexOf(discardChoice));
        discardChoice.owner = null;
        drawnTile = null;
        ArrangeTiles();
    }

    public void AddDrawnTileToClosedHand()
    {
        drawnTile.owner = this;
        closedHand.Add(drawnTile);
        drawnTile.transform.parent = closedHandParent;
        ArrangeTiles();
    }

    public void SetPlayerState(PlayerState state)
    {
        currentState = state;
    }
    public void SetNullDrawnTile()
    {
        drawnTile = null;
    }
    public void SetDiscardChoice(Tile tile)
    {
        discardChoice = tile;
    }
    public void ForceDiscard()
    {
        discardChoice = currentDrawnTile();
        DeclareDiscard();
    }

}

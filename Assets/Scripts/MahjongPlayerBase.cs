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
    #region Player Properties
    protected List<Tile> closedHand = new List<Tile>(), openHand = new List<Tile>();
    private List<Tile> flowers = new List<Tile>();
    private static int maxHandSize = 17;
    private int score;
    protected bool win, canWin, canPong, canKang, canChow;
    public decision currentDecision;
    protected Tile drawnTile;
    public Tile discardChoice;
    public PlayerState currentState;
    public bool networked;
    #endregion

    #region Other Properties
    private MahjongManager gameManager;
    public Transform closedHandParent, openHandParent, flowersParent;
    #endregion

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

    #region Debugging 
    List<Tile> othertiles = new List<Tile>();
    List<Tile> visitedTiles = new List<Tile>();
    List<Tile> pair = new List<Tile>();
    List<Tile> others = new List<Tile>();
    #endregion
    

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
            canWin = true;
        }
        else if (CalculateSevenPairs())
        {
            // todasButton.gameObject.SetActive(true);
            canWin = true;
        }
        else if (CalculateNormalWin())
        {
            // todasButton.gameObject.SetActive(true);
            canWin = true;
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
            if (tile.number == discard.number - 2 && !HasNumber(chowMeldLeft, tile.number))
            {
                chowMeldLeft.Add(tile);
            }
            if (tile.number == discard.number + 2 && !HasNumber(chowMeldRight, tile.number))
            {
                chowMeldRight.Add(tile);
            }

            if (tile.number == discard.number - 1)
            {
                if (!HasNumber(chowMeldLeft, tile.number))
                    chowMeldLeft.Add(tile);
                if (!HasNumber(chowMeldMiddle, tile.number))
                    chowMeldMiddle.Add(tile);
            }

            if (tile.number == discard.number + 1)
            {
                if (!HasNumber(chowMeldRight, tile.number))
                    chowMeldRight.Add(tile);
                if (!HasNumber(chowMeldMiddle, tile.number))
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
                case decision.kang:
                    GetComponent<PhotonView>().RPC("DeclareKang", RpcTarget.All);
                    break;
                case decision.chow:
                    GetComponent<PhotonView>().RPC("DeclareChow", RpcTarget.All);
                    break;
                case decision.pong:
                    GetComponent<PhotonView>().RPC("DeclarePong", RpcTarget.All);
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
    [PunRPC]

    protected virtual void DeclarePong()
    {
        currentDecision = decision.pong;
        // if (canPong)
        // {
        //     openHand.AddRange(pongMeld);
        //     canPong = false;
        // }
    }
    [PunRPC]

    protected virtual void DeclareKang()
    {
        currentDecision = decision.kang;
        // if (canKang)
        // {
        //     openHand.AddRange(kangMeld);
        //     canKang = false;
        // }
    }
    [PunRPC]

    protected virtual void DeclareChow()
    {
        currentDecision = decision.chow;
        // if (canChow)
        // {
        //     // openHand.AddRange(chowMeld);
        //     canChow = false;
        // }
    }
    [PunRPC]
    protected void passTurn()
    {
        currentDecision = decision.pass;
    }

    protected bool CalculateSevenPairs()
    {
        SortTilesBySuit();

        //check if theres more than one meld in the open hand
        if (openHand.Count > 4)
        {
            return false;
        }

        //there is exactly one meld in the open hand
        if (openHand.Count != 0)
        {
            bool allPairs = true;
            // SortTilesBySuit();
            for (int i = 0; i < closedHand.Count - 1; i += 2)
            {
                //since the array is sorted, all pairs are logically adjacent to one another
                if (closedHand[i].number != closedHand[i + 1].number || closedHand[i].tileType != closedHand[i + 1].tileType)
                {
                    // Debug.Log("not matching");
                    allPairs = false;
                    break;
                }

            }
            Debug.Log("One Meld Closed Hand Seven Pairs " + allPairs);
            return allPairs;

        }

        //final case: entire hand is still closed
        // SortTilesBySuit();

        //if two suits have odd counts of tiles, it is logically impossible to do seven pairs and a meld
        int oddSuits = 0;
        List<Tile> oddTiles = new List<Tile>();
        List<Tile> evenTiles = new List<Tile>();
        if (balls.Count % 2 == 1)
        {
            // Debug.Log("balls are odd");
            oddTiles = balls;
            oddSuits += 1;
        }
        else
        {
            // Debug.Log("balls are even");

            evenTiles.AddRange(balls);
        }
        if (sticks.Count % 2 == 1)
        {
            // Debug.Log("sticks are odd");

            oddSuits += 1;
            oddTiles = sticks;
        }
        else
        {
            // Debug.Log("sticks are even");

            evenTiles.AddRange(sticks);
        }
        if (chars.Count % 2 == 1)
        {
            // Debug.Log("chars are odd");

            oddSuits += 1;
            oddTiles = chars;
        }
        else
        {
            // Debug.Log("chars are even");

            evenTiles.AddRange(chars);
        }
        //only one suit collection can have an odd number
        if (oddSuits > 1)
            return false;

        //check the even tiles for all pairs
        bool allEvenPairs = true;
        for (int i = 0; i < evenTiles.Count - 1; i += 2)
        {
            //since the array is sorted, all pairs are logically adjacent to one another
            if (evenTiles[i].number != evenTiles[i + 1].number || evenTiles[i].tileType != evenTiles[i + 1].tileType)
            {
                // Debug.Log("not matching");
                allEvenPairs = false;
                break;
            }
        }

        bool oddPairsAndMeld = true;
        List<Tile> visitedOddTiles = new List<Tile>();
        // for (int i = 0; i < oddTiles.Count; i++)
        // {
        // }
        //whittle the oddtiles count down to meld size
        int startIndex = 0;
        while (oddTiles.Count > 1 && startIndex < oddTiles.Count - 1)
        {
            //since the array is sorted, all pairs are logically adjacent to one another
            if (oddTiles[startIndex].number == oddTiles[startIndex + 1].number && oddTiles[startIndex].tileType == oddTiles[startIndex + 1].tileType)
            {
                // Debug.Log("matching");
                visitedOddTiles.Add(oddTiles[startIndex]);
                visitedOddTiles.Add(oddTiles[startIndex + 1]);
                oddTiles.RemoveRange(startIndex, 2);
                startIndex = 0;
            }
            else
            {
                startIndex++;
            }
        }

        // Debug.Log(oddTiles.Count);
        if (oddTiles.Count == 3)
        {
            if (oddTiles[1].number != oddTiles[0].number + 1 || oddTiles[1].number != oddTiles[2].number - 1)
                oddPairsAndMeld = false;

        }
        else if (oddTiles.Count == 1)
        {
            if (!HasNumber(visitedOddTiles, oddTiles[0].number))
                oddPairsAndMeld = false;
        }

        Debug.Log("Full Closed Hand Seven Pairs " + (oddPairsAndMeld && allEvenPairs));
        return (oddPairsAndMeld && allEvenPairs);
    }
    protected bool CalculateNormalWin()
    {
        //calculate only using tiles from the closed hand, since only melds can exist in the closed hand

        //keep track of what tiles have been checked
        List<Tile> checkedTiles = new List<Tile>();

        //take advantage of sorting function again
        SortTilesBySuit();

        //check the divisibility of each subgroup by 3.
        //only one should not match and thats the one with the pair
        int notDivisible = 0;
        // List<Tile> othertiles = new List<Tile>();
        othertiles = new List<Tile>();
        List<Tile> pairGroup = new List<Tile>();
        if (balls.Count % 3 != 0)
        {
            pairGroup = balls;
            notDivisible += 1;
        }
        else
        {
            othertiles.AddRange(balls);
        }
        if (sticks.Count % 3 != 0)
        {
            pairGroup = sticks;
            notDivisible += 1;
        }
        else
        {
            othertiles.AddRange(sticks);
        }
        if (chars.Count % 3 != 0)
        {
            pairGroup = chars;
            notDivisible += 1;
        }
        else
        {
            othertiles.AddRange(chars);
        }
        //only one suit collection can be not divisible by 3
        if (notDivisible > 1)
            return false;

        visitedTiles = new List<Tile>();

        bool MeldsAndPair = false;
        //make sure the evenTiles only have a meld and a single pair
        if (pairGroup.Count == 2)
        {
            if (MatchNumber(pairGroup[0], pairGroup[1]))
            {
                MeldsAndPair = true;
            }
        }
        else
        {
            //test every possible pair until a working pair is found
            
            for (int x = 0; x < pairGroup.Count - 1; x++)
            {
                pair = new List<Tile>();
                others = new List<Tile>();
                if(MatchTile(pairGroup[x], pairGroup[x+1]))
                {
                    pair.Add(pairGroup[x]);
                    pair.Add(pairGroup[x+1]);
                    others.AddRange(pairGroup.GetRange(0, x));
                    others.AddRange(pairGroup.GetRange(x + 2, pairGroup.Count - x - 2));

                    //do the meld check with the other tiles
                    if(CheckForAllMelds(others))
                    {
                        Debug.Log("All melds");
                        MeldsAndPair = true;
                        break;
                    }
                }
            }
        }

        //make sure the othertiles only have melds
        bool JustMelds = CheckForAllMelds(othertiles);

        Debug.Log("all melds: " + JustMelds);
        Debug.Log("one pair with all melds: " + MeldsAndPair);

        return MeldsAndPair && JustMelds;
    }

    //helper function for win, checks for melds
    protected bool CheckForAllMelds(List<Tile> tiles)
    {
        bool result = true;
        int index = 0;
        while (tiles.Count > 2 && index < tiles.Count - 2)
        {
            int startListCount = tiles.Count;

            //check for pong
            if (MatchNumber(tiles[index], tiles[index + 1]) && MatchSuit(tiles[index], tiles[index + 1]))
            {
                if (MatchNumber(tiles[index + 1], tiles[index + 2]) && MatchSuit(tiles[index + 1], tiles[index + 2]))
                {
                    visitedTiles.AddRange(tiles.GetRange(index, 3));
                    tiles.RemoveRange(index, 3);
                }
            }
            //check for chow
            else if(MatchSuit(tiles[index], tiles[index + 1]))
            {
                int chowIndex1 = -1;
                int chowIndex2 = -1;
                for (int x = index + 1; x < tiles.Count; x++)
                {
                    if (chowIndex1 == -1 && tiles[index].number == tiles[x].number - 1 && MatchSuit(tiles[x], tiles[index]))
                    {
                        chowIndex1 = x;
                    }
                    if (chowIndex2 == -1 && tiles[index].number == tiles[x].number - 2 && MatchSuit(tiles[x], tiles[index]))
                    {
                        chowIndex2 = x;
                    }

                    if (chowIndex1 != -1 && chowIndex2 != -1)
                    {
                        break;
                    }
                }

                if (chowIndex1 != -1 && chowIndex2 != -1)
                {
                    visitedTiles.Add(tiles[index]);
                    visitedTiles.Add(tiles[chowIndex1]);
                    visitedTiles.Add(tiles[chowIndex2]);
                    tiles.RemoveAt(index);
                    tiles.RemoveAt(chowIndex1 - 1);
                    tiles.RemoveAt(chowIndex2 - 2);
                }

            }

            if (tiles.Count != startListCount)
            {
                index = 0;
            }
            else
                index += 1;
        }

        if (tiles.Count != 0)
            result = false;

        return result;
    }

    //shorthand functions for tile comparison
    protected bool MatchTile(Tile a, Tile b)
    {
        return MatchNumber(a,b) && MatchSuit(a,b);
    }
    protected bool MatchSuit(Tile a, Tile b)
    {
        return (a.tileType == b.tileType);
    }
    protected bool MatchNumber(Tile a, Tile b)
    {
        return (a.number == b.number);
    }

    //internally arranges the tiles
    [PunRPC]
    protected void SortTilesBySuit()
    {
        //first reset suits
        balls = new List<Tile>();
        sticks = new List<Tile>();
        chars = new List<Tile>();

        //add tiles to each list based on suit
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

        //sort all of them
        balls.Sort(CompareTileNumbers);
        sticks.Sort(CompareTileNumbers);
        chars.Sort(CompareTileNumbers);

        //reset the closed hand and add the sorted tiles to it
        closedHand = new List<Tile>();
        closedHand.AddRange(balls);
        closedHand.AddRange(sticks);
        closedHand.AddRange(chars);
    }

    // helper function for tile sorting
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

    //physically arranges player tiles
    [PunRPC]
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

        localLeft = -1 * Vector3.Cross(flowersParent.forward.normalized, flowersParent.up.normalized);
        sideOffset = 0.5f / (float)flowers.Count;
        placementReference = 0.5f / -2.0f;
        foreach (Tile tile in flowers)
        {
            tile.transform.localPosition = new Vector3(-1, 0, 0) * (placementReference);
            tile.transform.localEulerAngles = flowersParent.up * 90 + Vector3.forward * 90;// + tile.transform.forward * -90;
            placementReference += sideOffset;
        }

        localLeft = -1 * Vector3.Cross(openHandParent.forward.normalized, openHandParent.up.normalized);
        sideOffset = 0.5f / (float)flowers.Count;
        placementReference = 0.5f / -2.0f;
        foreach (Tile tile in openHand)
        {
            tile.transform.localPosition = new Vector3(-1, 0, 0) * (placementReference);
            tile.transform.localEulerAngles = closedHandParent.up * 90;
            placementReference -= sideOffset;
        }
    }

    // RPC call for visual sorting
    public void VisuallySortTiles()
    {
        //first call the sorting function
        if (networked)
        {
            GetComponent<PhotonView>().RPC("ArrangeTiles", RpcTarget.All);
            GetComponent<PhotonView>().RPC("SortTilesBySuit", RpcTarget.All);
        }
        {
            ArrangeTiles();
            SortTilesBySuit();
        }

    }

    public int replaceInitialFlowerTiles()
    {
        List<Tile> flowersInHand = new List<Tile>();

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

    public void AddTile(Tile tile)
    {
        tile.owner = this;
        closedHand.Add(tile);
        tile.transform.parent = closedHandParent;

    }
    public void DebugAddOpenHandTile(Tile tile)
    {
        tile.owner = this;
        openHand.Add(tile);
        tile.transform.parent = closedHandParent;

    }
    public void AddFlower(Tile flower)
    {
        flowers.Add(flower);
        flower.transform.parent = flowersParent;
        closedHand.Remove(flower);

    }
    public Tile currentDrawnTile()
    {
        return drawnTile;
    }

    public void ResetMelds()
    {
        pongMeld = new List<Tile>();
        chowMeldLeft = new List<Tile>();
        chowMeldMiddle = new List<Tile>();
        chowMeldRight = new List<Tile>();
        kangMeld = new List<Tile>();
        selectedTiles = new List<Tile>();
        canChow = false;
        canKang = false;
        canPong = false;
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
                foreach (Tile tile in pongMeld)
                {
                    closedHand.RemoveAt(closedHand.IndexOf(tile));
                }
                break;
            case decision.kang:
                openHand.AddRange(kangMeld);
                foreach (Tile tile in kangMeld)
                {
                    closedHand.RemoveAt(closedHand.IndexOf(tile));
                }
                break;
            case decision.chow:
                selectedTiles.Add(MahjongManager.mahjongManager.mostRecentDiscard);
                selectedTiles.Sort(CompareTileNumbers);
                openHand.AddRange(selectedTiles);
                foreach (Tile tile in selectedTiles)
                {
                    if(tile != MahjongManager.mahjongManager.mostRecentDiscard)
                    closedHand.RemoveAt(closedHand.IndexOf(tile));
                }
                break;
        }

    }

    // draw a tile from the wall
    public void DrawTile()
    {
        drawnTile = MahjongManager.mahjongManager.wall[0];
        MahjongManager.mahjongManager.wall.RemoveAt(0);
    }

    //draw a flower tile from the flower end of the wall
    public void DrawFlowerTile()
    {
        flowers.Add(drawnTile);
        drawnTile = MahjongManager.mahjongManager.wall[MahjongManager.mahjongManager.wall.Count - 1];
        // closedHand.Add(drawnTile);
        MahjongManager.mahjongManager.wall.RemoveAt(MahjongManager.mahjongManager.wall.Count - 1);
        // ArrangeTiles();
    }

    //declare a tile to discard on your turn
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

    //networked call
    [PunRPC]
    public void DiscardTile()
    {
        MahjongManager.mahjongManager.mostRecentDiscard = discardChoice;
        closedHand.RemoveAt(closedHand.IndexOf(discardChoice));
        discardChoice.owner = null;
        drawnTile = null;
    }

    //adding the drawn tile to closed hand after wards.
    public void AddDrawnTileToClosedHand()
    {
        drawnTile.owner = this;
        closedHand.Add(drawnTile);
        drawnTile.transform.parent = closedHandParent;
        ArrangeTiles();
    }

    //for the gamemanager to set the player state with
    public void SetPlayerState(PlayerState state)
    {
        currentState = state;
    }

    //this might now be used anymore
    public void SetNullDrawnTile()
    {
        drawnTile = null;
    }

    //a method used to network discard choice
    public void SetDiscardChoice(Tile tile)
    {
        discardChoice = tile;
    }

    //to be called when the player runs out of time
    public void ForceDiscard()
    {
        if (drawnTile == null)
        {
            discardChoice = closedHand[closedHand.Count - 1];
        }
        else
        {
            discardChoice = drawnTile;
        }
        DeclareDiscard();
    }

}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutorialPlayer : HumanPlayer
{
    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();
        todasButton.onClick.AddListener(() => TutorialManager.tutorial.ResetNextButton());
        pongButton.onClick.AddListener(() => TutorialManager.tutorial.ResetNextButton());
        kangButton.onClick.AddListener(() => TutorialManager.tutorial.ResetNextButton());
        chowButton.onClick.AddListener(() => TutorialManager.tutorial.ResetNextButton());
        passButton.onClick.AddListener(() => TutorialManager.tutorial.ResetNextButton());
        discardButton.onClick.AddListener(() => TutorialManager.tutorial.ResetNextButton());
    }

    public override void Update()
    {
        if (TutorialManager.tutorial.GetGameState() == GameState.playing)
        {

            //bird's eye board view;
            if (Input.GetKey(KeyCode.Space))
            {
                playerCam.transform.rotation = Quaternion.Euler(Vector3.left * -90);
                playerCam.transform.position = new Vector3(0, 1.5f, 0);
                playerCanvas.SetActive(false);
            }
            else
            {
                playerCam.transform.rotation = Quaternion.Euler(camRotation);
                playerCam.transform.localPosition = camPosition;
                playerCanvas.SetActive(true);
            }

            //UI being set active

            //setting buttons active when conditions are fulfilled
            if (discardChoice != null)
            {
                discardButton.gameObject.SetActive(true);
            }
            else
            {
                discardButton.gameObject.SetActive(false);
            }

            if (currentState == PlayerState.deciding && currentDecision == decision.none)
            {
                passButton.gameObject.SetActive(true);
            }
            else
            {
                passButton.gameObject.SetActive(false);
            }

            // if (!networked || (networked && GetComponent<NetworkedPlayer>().photonView.IsMine))
            // {
            // Debug.Log(new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0));
            Vector3 mouseWorldPos = playerCam.ViewportToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, playerCam.nearClipPlane));

            Ray mouseWorldRay = playerCam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(mouseWorldRay.origin, mouseWorldRay.direction, Color.blue * Vector3.Distance(transform.position, closedHandParent.position), 0f);
            if (Physics.Raycast(mouseWorldRay, out RaycastHit hit, Vector3.Distance(transform.position, closedHandParent.position))
                && hit.transform.GetComponent<Tile>())
            // if (Physics.Raycast(mouseWorldRay, out RaycastHit hit, 5f))
            {
                // Debug.Log("touched tile at " + hit.point);
                if (closedHandParent.Find(hit.transform.name) != null)
                {
                    if (Input.GetMouseButtonDown(0) && currentState != PlayerState.waiting)
                    {
                        switch (currentState)
                        {
                            case PlayerState.deciding:

                                if (this != TutorialManager.tutorial.currentPlayer)
                                {
                                    if (networked)
                                    {
                                        hit.transform.GetComponent<Tile>().TileRPCCall("SelectForMeld");
                                    }
                                    else
                                    {
                                        SelectMeldTile(hit.transform.GetComponent<Tile>());
                                    }
                                }
                                break;
                            case PlayerState.discarding:
                                if (networked)
                                {
                                    hit.transform.GetComponent<Tile>().TileRPCCall("PlayerDiscard");
                                }
                                else
                                {
                                    SelectDiscardTile(hit.transform.GetComponent<Tile>());
                                    // discardChoice = hit.transform.GetComponent<Tile>();
                                }
                                break;
                        }

                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        SwapTilePosition(hit.transform.gameObject);
                    }

                }
                // }
            }

        }


    }

    public List<Tile> GetClosedHand()
    {
        return closedHandParent.GetComponentsInChildren<Tile>().ToList<Tile>();
    }

    public override int replaceInitialFlowerTiles()
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

        Debug.Log(flowersInHand.Count);


        int newFlowerCount = 0;
        for (int x = 0; x < flowersInHand.Count; x++)
        {
            Tile drawnTile = TutorialManager.tutorial.wall[TutorialManager.tutorial.wall.Count - 1];
            if (drawnTile.tileType == suit.flower)
                newFlowerCount += 1;
            AddTile(drawnTile);
            TutorialManager.tutorial.wall.RemoveAt(TutorialManager.tutorial.wall.Count - 1);
        }

        return newFlowerCount;

    }

    public override void CalculateHandOptions()
    {
        // base.CalculateHandOptions();
        Debug.Log("Calculating Hand Options");

        canPong = false;
        canKang = false;
        canChow = false;
        canWin = false;

        Tile discard = TutorialManager.tutorial.mostRecentDiscard;

        //most priority is a winning hand, takes precedence
        //edge case, closed hand size is 1, so waiting on the last pair
        if (closedHand.Count == 1 &&
        closedHand[0].number == discard.number &&
        closedHand[0].tileType == discard.tileType)
        {
            //GUI stuff probably needs to be moved to Human as well
            canWin = true;
            // StartCoroutine(todasButton.GetComponentInParent<ButtonFlip>().Flip());
        }
        else if (CalculateSevenPairs())
        {
            canWin = true;
            // StartCoroutine(todasButton.GetComponentInParent<ButtonFlip>().Flip());
        }
        else if (CalculateNormalWin())
        {
            canWin = true;
            // StartCoroutine(todasButton.GetComponentInParent<ButtonFlip>().Flip());
        }



        pongMeld = new List<Tile> { discard };
        chowMeldLeft = new List<Tile> { discard };
        chowMeldMiddle = new List<Tile> { discard };
        chowMeldRight = new List<Tile> { discard };
        kangMeld = new List<Tile> { discard };

        //auto calculate kang as a bandaid
        foreach (Tile tile in closedHand)
        {
            //need the last equivalency check to due to the simultaneous method call
            if (tile.tileType == discard.tileType && tile.number == discard.number && tile != discard)
            {
                kangMeld.Add(tile);
            }
        }

        foreach (Tile tile in selectedTiles)
        {
            if (tile.tileType == discard.tileType)
            {
                // Debug.Log("Passed suit check");

                if (tile.number == discard.number)
                {
                    if (pongMeld.Count < 3)
                        pongMeld.Add(tile);
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
        }

        if (pongMeld.Count == 3)
        {
            canPong = true;
        }
        else
        {
            canPong = false;
        }
        if (kangMeld.Count == 4)
        {
            canKang = true;
        }
        else
        {
            canKang = false;
        }
        if (chowMeldLeft.Count == 3
        || chowMeldMiddle.Count == 3
        || chowMeldRight.Count == 3 &&
        (TutorialManager.tutorial.GetPlayers().IndexOf(TutorialManager.tutorial.previousPlayer) + 1) %
        TutorialManager.tutorial.GetPlayers().Count == TutorialManager.tutorial.GetPlayers().IndexOf(this))
        {
            canChow = true;
        }
        else
        {
            canChow = false;
        }

        if (canPong && !pongButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(pongButton.GetComponentInParent<ButtonFlip>().Flip());
        }
        else if (!canPong && pongButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(pongButton.GetComponentInParent<ButtonFlip>().Flip());
        }
        if (canKang && !kangButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(kangButton.GetComponentInParent<ButtonFlip>().Flip());
        }
        else if (!canKang && kangButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(kangButton.GetComponentInParent<ButtonFlip>().Flip());
        }
        if (canChow && !chowButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(chowButton.GetComponentInParent<ButtonFlip>().Flip());
        }
        else if (!canChow && chowButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(chowButton.GetComponentInParent<ButtonFlip>().Flip());
        }

        if (canWin && !todasButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(todasButton.GetComponentInParent<ButtonFlip>().Flip());
        }
        else if (!canWin && todasButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(todasButton.GetComponentInParent<ButtonFlip>().Flip());
        }
    }
    public override bool CalculateNormalWin()
    {
        //calculate only using tiles from the closed hand, since only melds can exist in the closed hand
        Debug.Log("Calculating Normal Win");
        if (currentState == PlayerState.deciding)
            closedHand.Add(TutorialManager.tutorial.mostRecentDiscard);

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
        {
            Debug.Log("More than one collection divisible by 3");
            if (currentState == PlayerState.deciding)
                closedHand.RemoveAt(closedHand.IndexOf(TutorialManager.tutorial.mostRecentDiscard));
            return false;
        }

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
                if (MatchTile(pairGroup[x], pairGroup[x + 1]))
                {
                    pair.Add(pairGroup[x]);
                    pair.Add(pairGroup[x + 1]);
                    others.AddRange(pairGroup.GetRange(0, x));
                    others.AddRange(pairGroup.GetRange(x + 2, pairGroup.Count - x - 2));

                    //do the meld check with the other tiles
                    if (CheckForAllMelds(others))
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

        Debug.Log("all melds: " + JustMelds + " one pair with all melds: " + MeldsAndPair);
        if (currentState == PlayerState.deciding)
            closedHand.RemoveAt(closedHand.IndexOf(TutorialManager.tutorial.mostRecentDiscard));

        return MeldsAndPair && JustMelds;
    }
    public override bool CalculateSevenPairs()
    {
        Debug.Log("Calculating Seven Pairs");

        //first, factor in the discarded tile during deciding
        if (currentState == PlayerState.deciding)
        {
            closedHand.Add(TutorialManager.tutorial.mostRecentDiscard);
        }

        SortTilesBySuit();

        //check if theres more than one meld in the open hand
        if (openHand.Count > 4)
        {
            Debug.Log("More than one meld in the open hand");
            if (currentState == PlayerState.deciding)
                closedHand.RemoveAt(closedHand.IndexOf(TutorialManager.tutorial.mostRecentDiscard));

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

            if (currentState == PlayerState.deciding)
                closedHand.RemoveAt(closedHand.IndexOf(TutorialManager.tutorial.mostRecentDiscard));
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
        {
            Debug.Log("More than one collection of odd suits");
            if (currentState == PlayerState.deciding)
                closedHand.RemoveAt(closedHand.IndexOf(TutorialManager.tutorial.mostRecentDiscard));
            return false;
        }

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

        if (currentState == PlayerState.deciding)
            closedHand.RemoveAt(closedHand.IndexOf(TutorialManager.tutorial.mostRecentDiscard));
        Debug.Log("Full Closed Hand Seven Pairs " + (oddPairsAndMeld && allEvenPairs));
        return (oddPairsAndMeld && allEvenPairs);
    }

    public override void StealTile()
    {
        //add stolen tile and its meld to the open hand
        // openHand.Add(TutorialManager.tutorial.mostRecentDiscard);
        TutorialManager.tutorial.mostRecentDiscard.owner = this;
        // TutorialManager.tutorial.mostRecentDiscard.transform.parent = openHandParent.transform;
        switch (currentDecision)
        {
            case decision.pong:
                openHand.AddRange(pongMeld);
                foreach (Tile tile in pongMeld)
                {
                    if (tile != TutorialManager.tutorial.mostRecentDiscard)
                        closedHand.RemoveAt(closedHand.IndexOf(tile));
                    tile.transform.parent = openHandParent.transform;
                }
                break;
            case decision.kang:
                openHand.AddRange(kangMeld);
                foreach (Tile tile in kangMeld)
                {
                    if (tile != TutorialManager.tutorial.mostRecentDiscard)
                        closedHand.RemoveAt(closedHand.IndexOf(tile));
                    tile.transform.parent = openHandParent.transform;
                }
                break;
            case decision.chow:
                selectedTiles.Add(TutorialManager.tutorial.mostRecentDiscard);
                selectedTiles.Sort(CompareTileNumbers);
                openHand.AddRange(selectedTiles);
                foreach (Tile tile in selectedTiles)
                {
                    if (tile != TutorialManager.tutorial.mostRecentDiscard)
                        closedHand.RemoveAt(closedHand.IndexOf(tile));
                    tile.transform.parent = openHandParent.transform;
                }
                break;
        }
        ResetMelds();
        ArrangeTiles();
    }

    public override void DrawFlowerTile()
    {
        flowers.Add(drawnTile);
        drawnTile = TutorialManager.tutorial.wall[TutorialManager.tutorial.wall.Count - 1];
        // closedHand.Add(drawnTile);
        TutorialManager.tutorial.wall.RemoveAt(TutorialManager.tutorial.wall.Count - 1);
        // ArrangeTiles();
    }

    public override void DrawKangTile()
    {
        drawnTile = TutorialManager.tutorial.wall[TutorialManager.tutorial.wall.Count - 1];
        TutorialManager.tutorial.wall.RemoveAt(TutorialManager.tutorial.wall.Count - 1);
    }
    public override void DiscardTile()
    {
        discardChoice.transform.localPosition = new Vector3(0, 0.05f, 0.05f);
        TutorialManager.tutorial.mostRecentDiscard = discardChoice;
        closedHand.RemoveAt(closedHand.IndexOf(discardChoice));
        discardChoice.transform.parent = null;
        discardChoice.owner = null;
        drawnTile = null;
        discardChoice = null;
    }
}

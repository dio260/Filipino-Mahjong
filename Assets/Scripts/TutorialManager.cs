using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MahjongManager
{
    public new List<Tile> board;
    // private static int MAXTILECOUNT = 144;
    // private GameState state;
    // private MahjongPlayerBase dealer, previousPlayer, currentPlayer, nextPlayer;
    // private int round, numRounds;

    public HumanPlayer tutorialGuy;
    public Text tutorialDialogue;
    public Button nextButton, eventButton;
    // Start is called before the first frame update
    void Awake()
    {
        if (mahjongManager != null && mahjongManager != this)
        {
            Destroy(gameObject);
        }
        else
        {
            mahjongManager = this;
        }
        //initialize some stuff
        state = GameState.setup;
        // board = new List<Tile>(MAXTILECOUNT);
        wall = new List<Tile>();
        deadTiles = new List<Tile>();
        mostRecentDiscard = null;

        if(!TileBoundaries.isTrigger)
            TileBoundaries.isTrigger = true;

        //put all the tiles into board structure;
        foreach (Tile tile in InitialTileParent.transform.GetComponentsInChildren<Tile>())
        {
            board.Add(tile);
        }

        StartCoroutine(GameOverview());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator GameOverview()
    {
        tutorialDialogue.text = "Welcome to Mahjong! This tutorial will go over basic rules and steps to playing the game.";
        yield return new WaitForSeconds(2);
        tutorialDialogue.text = "Mahjong is a tile-based game in which players must form specific hands of 17 tiles to win.";
        yield return new WaitForSeconds(2);
        tutorialDialogue.text = "There are many ways of playing Mahjong, but this game focuses on a basic ruleset used in the Philippines.";
        yield return new WaitForSeconds(2);

    }

    IEnumerator BoardSetup()
    {
        tutorialDialogue.text = "All Mahjong games start by shuffling the tiles and sorting them into walls. Press the button to shuffle the tiles on the table.";

        //add a button instead of wait;
        yield return new WaitForSeconds(2);
        System.Random rand = new System.Random();

        // wall = board;
        // Debug.Log(TileBoundaries.bounds.max);
        float distanceReference = TileSizeReference.transform.localScale.z/2;
        float heightReference = TileSizeReference.transform.localScale.y/2f;
        int multiplier = 0;

        for(int x = 0; x < board.Count; x++)
        {
            if (x%36 == 0)
            {
                multiplier = 0;
            }
            if(x < 36)
            {
                if(x % 2 == 0)
                {
                    board[x].transform.position = new Vector3(TileBoundaries.bounds.max.x, TileBoundaries.bounds.min.y + 0.005f + heightReference, TileBoundaries.bounds.max.z - (distanceReference * multiplier));
                }
                else
                {
                    board[x].transform.position = board[x-1].transform.position + Vector3.up * -heightReference;
                    
                }
                multiplier += 1;
                continue;
            }
            // break;
            if (x < 72)
            {
                board[x].transform.Rotate(Vector3.left  * 90);
                if(x % 2 == 0)
                {
                    board[x].transform.position = new Vector3(TileBoundaries.bounds.max.x - (distanceReference * multiplier), TileBoundaries.bounds.min.y + 0.005f + heightReference, TileBoundaries.bounds.min.z );
                }
                else
                {
                    board[x].transform.position = board[x-1].transform.position + Vector3.up * -heightReference;
                    
                }
                multiplier += 1;
                continue;
                
            }
            // break;
            if (x < 108)
            {
                // board[x].transform.Rotate(Vector3.left  * 90);
                if(x % 2 == 0)
                {
                    board[x].transform.position = new Vector3(TileBoundaries.bounds.min.x, TileBoundaries.bounds.min.y + 0.005f + heightReference, TileBoundaries.bounds.min.z + (distanceReference * multiplier));
                }
                else
                {
                    board[x].transform.position = board[x-1].transform.position + Vector3.up * -heightReference;
                    
                }
                multiplier += 1;
                continue;
            }
            if (x < 144)
            {
                board[x].transform.Rotate(Vector3.left  * 90);
                if(x % 2 == 0)
                {
                    board[x].transform.position = new Vector3(TileBoundaries.bounds.min.x + (distanceReference * multiplier), TileBoundaries.bounds.min.y + 0.005f + heightReference, TileBoundaries.bounds.max.z );
                }
                else
                {
                    board[x].transform.position = board[x-1].transform.position + Vector3.up * -heightReference;
                    
                }
                multiplier += 1;
                continue;
            }
            // tile.transform.Rotate(new Vector3(0, 0, -90));
        }

        tutorialDialogue.text = "In a real mahjong game, all players work together to shuffle the board and arrange them into those four walls.";
        yield return new WaitForSeconds(2);
        tutorialDialogue.text = "After the walls have been made, the game's dealer then needs to be determined. Press the button to move on to that.";
        yield return new WaitForSeconds(2);

        // yield return new WaitForSeconds(2);
        // StartCoroutine(RollDice());
    }

    IEnumerator RollDice()
    {
        tutorialDialogue.text = "The dealer is determined by counting counterclockwise from a dice roll [needs explanation work]";
        yield return new WaitForSeconds(2);
        tutorialDialogue.text = "The dealer gets to split one of the walls to start distributing hands to each player";
        yield return new WaitForSeconds(2);
        tutorialDialogue.text = "For the sake of this tutorial, you will start as the dealer.";
        yield return new WaitForSeconds(2);
        
        dealer = players[0];

        //create walls
        // System.Random rand = new System.Random();
        // int dieRollResult = (rand.Next(2, 13) - 1) % 4;
        // int wallIndex = (MAXTILECOUNT / 4) * dieRollResult;
        wall.AddRange(board);


        Debug.Log("Distributing Hands");

        List<Tile> distributedTiles = wall.GetRange(0, 65);
        wall.RemoveRange(0, 65);
        dealer.GetComponent<MahjongPlayerBase>().AddTile(distributedTiles[0]);
        // distributedTiles.Remove(wall[0]);
        for (int i = 1; i < 65; i++)
        {
            players[(((i - 1) / 16)) % 4].AddTile(distributedTiles[i]);
        }

        foreach (MahjongPlayerBase player in players)
        {
            // player.VisuallySortTiles();
            player.ArrangeTiles();
        }
        // StartCoroutine(Wait());
        yield return new WaitForSeconds(2);
        tutorialDialogue.text = "Now that each player has their starting hand, the wall now has two ends.";
        yield return new WaitForSeconds(2);
        tutorialDialogue.text = "Both ends are used for drawing tiles. One end is for normal drawing, while the end where the die have been placed is for replacing flower tiles.";
        yield return new WaitForSeconds(2);

        
    }

    IEnumerator FlowersTutorial()
    {
        tutorialDialogue.text = "The ";
        yield return new WaitForSeconds(2);
        tutorialDialogue.text = "Flower tiles are bonus tiles that do not influence a player's hand. If a player draws one, they collect it to the side and may draw another tile from the flower end to replace it.";
        yield return new WaitForSeconds(2);
        tutorialDialogue.text = "The hand distribution has left you with a flower tile. Press the button to replace it.";
        yield return new WaitForSeconds(2);
        Debug.Log("Replacing Flowers");

        int needFlowers = -1;
        while (needFlowers != 0)
        {
            needFlowers = 4;
            foreach (MahjongPlayerBase player in players)
            {
                int remainingFlowers = player.replaceInitialFlowerTiles();
                if (remainingFlowers == 0)
                    needFlowers -= 1;
            }
        }

        tutorialDialogue.text = "Now your flowers are replaced and you have a proper hand. Any flowers drawn throughout the ";
        yield return new WaitForSeconds(2);

        foreach (MahjongPlayerBase player in players)
        {
            // player.VisuallySortTiles();
            player.ArrangeTiles();
        }

        currentPlayer = dealer;
        nextPlayer = players[1];

        yield return new WaitForSeconds(2);
        state = GameState.playing;
        StartCoroutine(TakeTurn(currentPlayer));
    }

    IEnumerator TakeTurn(MahjongPlayerBase player)
    {
        Debug.Log("current turn: " + player.gameObject.name);
        player.SetPlayerState(PlayerState.discarding);
        if (player.currentDecision != decision.none && player.currentDecision != decision.pass)
        {
            Debug.Log(player.gameObject.name + " stole discard");
            player.StealTile();
        }
        else
        {

            yield return new WaitForSeconds(1);
            Debug.Log(player.gameObject.name + " drawing tile");
            player.DrawTile();

            // StartCoroutine(player.DrawTile());
            yield return new WaitForSeconds(1);
            if (player.currentDrawnTile().tileType == suit.flower)
            {
                while (player.currentDrawnTile().tileType == suit.flower)
                {
                    Debug.Log(player.gameObject.name + " drew flower");
                    player.DrawFlowerTile();
                    yield return new WaitForSeconds(1);
                }
            }


            Debug.Log(player.gameObject.name + " finished drawing");
            player.AddDrawnTileToClosedHand();

        }

        //do a time based implementation so people cannot stall out the turn;
        for (int i = 30; i > 0; i--)
        {
            if (player.GetType() == typeof(HumanPlayer))
            {
                player.GetComponent<HumanPlayer>().debugText.text = i + " seconds left";
            }
            yield return new WaitForSeconds(1);
            if (mostRecentDiscard != null)
            {
                break;
            }
        }
        if (mostRecentDiscard == null)
        {
            mostRecentDiscard = player.currentDrawnTile();
            player.SetNullDrawnTile();
        }

        Debug.Log("Player Discarded " + mostRecentDiscard.number + " " + mostRecentDiscard.tileType);
        player.GetComponent<HumanPlayer>().debugText.text = "waiting";
        player.SetPlayerState(PlayerState.waiting);

        if(!debug)
            mostRecentDiscard.transform.position = Vector3.up * 0.5f;

        StartCoroutine(BetweenTurn());
    }

    public IEnumerator BetweenTurn()
    {
        Debug.Log("Deliberation time");

        foreach (MahjongPlayerBase player in players)
        {
            player.SetPlayerState(PlayerState.deciding);
        }

        bool allDone = true;
        for (int i = 10; i > 0; i--)
        {
            yield return new WaitForSeconds(1);

            foreach (MahjongPlayerBase player in players)
            {
                if (player.currentDecision == decision.none)
                {
                    allDone = false;
                    break;
                }
            }
            if (allDone)
                break;
        }
        foreach (MahjongPlayerBase player in players)
            {
                player.SetPlayerState(PlayerState.waiting);
            }

        // MahjongPlayerBase next = players[0];

        // foreach (MahjongPlayerBase player in players)
        // {
        //     if (player != currentPlayer || player != nextPlayer)
        //     {
        //         //only one person can possibly do kang at any given time
        //         //so break the loop and give them the turn
        //         if (player.currentDecision == decision.kang)
        //         {
        //             nextPlayer = player;
        //             break;
        //         }
        //         //same with pong, but kang get priority so its lower
        //         if (player.currentDecision == decision.pong)
        //         {
        //             nextPlayer = player;
        //             break;
        //         }
        //         //both a chow and a pass result in the next person taking their turn anyway
        //         //so we do not check it here
        //     }
        // }
        // previousPlayer = currentPlayer;
        // currentPlayer = nextPlayer;
        // nextPlayer = players[(players.IndexOf(nextPlayer) + 1) % players.Count];

        // StartCoroutine(TakeTurn(currentPlayer));
    }

    public void FinishGame()
    {
        state = GameState.finished;
    }
    public GameState GetGameState()
    {
        return state;
    }

    #region RPC Calls

    #endregion

}

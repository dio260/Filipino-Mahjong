using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TutorialManager : MahjongManager
{
    // private static int MAXTILECOUNT = 144;
    // private GameState state;
    // private MahjongPlayerBase dealer, previousPlayer, currentPlayer, nextPlayer;
    // private int round, numRounds;
    public static TutorialManager tutorial; 
    public HumanPlayer tutorialGuy;
    public Text tutorialDialogue;
    public Button nextButton, eventButton;
    [TextArea(5, 15)]
    public string[] dialogueLines;
    int dialogueIndex;
    bool advanceDialogue;
    // Start is called before the first frame update
    public override void Awake()
    {
        if (tutorial != null && tutorial != this)
        {
            Destroy(gameObject);
        }
        else
        {
            tutorial = this;
        }
        //always load into the same scene, so find this object in the scene
        InitialTileParent = GameObject.Find("Tiles");
        DeadTileParent = GameObject.Find("Dead Tiles");
        TileBoundaries = GameObject.Find("TileBoundaries").GetComponent<BoxCollider>();



        nextButton.onClick.AddListener(ResetNextButton);

        InitializeGame();
    }

    // Update is called once per frame
    public void ResetNextButton()
    {
        advanceDialogue = true;
        dialogueIndex++;
    }
    public void FixedUpdate()
    {
        tutorialDialogue.text = dialogueLines[dialogueIndex];
        if (!advanceDialogue)
            return;

        advanceDialogue = false;
    }

    new public void InitializeGame()
    {

        //initialize game states, tile lists, and other relevant objects
        state = GameState.setup;
        board = new List<Tile>(MAXTILECOUNT);
        wall = new List<Tile>();
        deadTiles = new List<Tile>();
        mostRecentDiscard = null;
        firstTurn = true;

        if (!TileBoundaries.isTrigger)
            TileBoundaries.isTrigger = true;

        foreach (Tile tile in InitialTileParent.transform.GetComponentsInChildren<Tile>())
        {
            board.Add(tile);
        }

        StartCoroutine(BoardSetup());


    }

    new IEnumerator BoardSetup()
    {
        Debug.Log("BoardSetup");

        while (dialogueIndex < 4)
        {
            // Debug.Log("Shuffling Board and Creating Walls");
            yield return new WaitForSeconds(1f);
        }
        tutorialGuy.playerCanvas.SetActive(true);

        Debug.Log("Shuffling Board and Creating Walls");
        System.Random rand = new System.Random();

        SendPlayersMessage("Shuffling Board and Creating Walls");
        AudioHandler.audioHandler.PlayShuffle();
        foreach (MahjongPlayerBase player in MahjongManager.mahjongManager.GetPlayers())
        {
            player.currentAvatar.PlayShuffleAnim();
        }

        float distanceReference = TileSizeReference.transform.localScale.z / 2;
        float heightReference = TileSizeReference.transform.localScale.y / 2f;
        int multiplier = 0;

        for (int x = 0; x < board.Count; x++)
        {
            if (x % 36 == 0)
            {
                multiplier = 0;
                yield return new WaitForSeconds(1);
            }
            if (x < 36)
            {
                if (x % 2 == 0)
                {
                    board[x].transform.position = new Vector3(TileBoundaries.bounds.max.x, TileBoundaries.bounds.min.y + 0.005f + heightReference, TileBoundaries.bounds.max.z - (distanceReference * multiplier));
                }
                else
                {
                    board[x].transform.position = board[x - 1].transform.position + Vector3.up * -heightReference;

                }
                multiplier += 1;
                continue;
            }
            // break;
            if (x < 72)
            {
                board[x].transform.Rotate(Vector3.left * 90);
                if (x % 2 == 0)
                {
                    board[x].transform.position = new Vector3(TileBoundaries.bounds.max.x - (distanceReference * multiplier), TileBoundaries.bounds.min.y + 0.005f + heightReference, TileBoundaries.bounds.min.z);
                }
                else
                {
                    board[x].transform.position = board[x - 1].transform.position + Vector3.up * -heightReference;

                }
                multiplier += 1;
                continue;

            }
            // break;
            if (x < 108)
            {
                // board[x].transform.Rotate(Vector3.left  * 90);
                if (x % 2 == 0)
                {
                    board[x].transform.position = new Vector3(TileBoundaries.bounds.min.x, TileBoundaries.bounds.min.y + 0.005f + heightReference, TileBoundaries.bounds.min.z + (distanceReference * multiplier));
                }
                else
                {
                    board[x].transform.position = board[x - 1].transform.position + Vector3.up * -heightReference;

                }
                multiplier += 1;
                continue;
            }
            if (x < 144)
            {
                board[x].transform.Rotate(Vector3.left * 90);
                if (x % 2 == 0)
                {
                    board[x].transform.position = new Vector3(TileBoundaries.bounds.min.x + (distanceReference * multiplier), TileBoundaries.bounds.min.y + 0.005f + heightReference, TileBoundaries.bounds.max.z);
                }
                else
                {
                    board[x].transform.position = board[x - 1].transform.position + Vector3.up * -heightReference;

                }
                multiplier += 1;
                continue;
            }
            // tile.transform.Rotate(new Vector3(0, 0, -90));
        }

        while (dialogueIndex < 7)
        {
            yield return new WaitForSeconds(1f);
        }

        StartCoroutine(RollDice());
    }

    new IEnumerator RollDice()
    {
        //newand improved real actual dice roll
        //play diceroll animation
        int dieRoll = 0;
        List<Dice> die = FindObjectsOfType<Dice>().ToList<Dice>();

        StartCoroutine(die[0].TutorialDiceRoll(Vector3.right * 90));
        StartCoroutine(die[0].TutorialDiceRoll(Vector3.zero));

        yield return new WaitForSeconds(2f);
        foreach (Dice dice in die)
        {
            dieRoll += dice.rollResult;
        }
        dieRollResult = (dieRoll - 1) % players.Count;
        dealer = players[0];

        while (dialogueIndex < 8)
        {
            yield return new WaitForSeconds(1);
        }

        int dealerIndex = players.IndexOf(dealer);
        int wallIndex = (MAXTILECOUNT / 4) * dealerIndex + dieRollResult;
        wall.AddRange(board.GetRange(wallIndex, MAXTILECOUNT - wallIndex));
        wall.AddRange(board.GetRange(0, wallIndex));
        wall.AddRange(board);

        while (dialogueIndex < 10)
        {
            yield return new WaitForSeconds(1);
        }

        //now distribute the tiles to others.
        List<Tile> distributedTiles = wall.GetRange(0, 1 + (16 * players.Count));
        wall.RemoveRange(0, 1 + (16 * players.Count));
        dealer.GetComponent<MahjongPlayerBase>().AddTile(distributedTiles[0]);

        for (int i = 1; i < 1 + (16 * players.Count); i++)
        {
            players[(dealerIndex + ((i - 1) / 16)) % players.Count].AddTile(distributedTiles[i]);
        }

        foreach (MahjongPlayerBase player in players)
        {
            // player.VisuallySortTiles();
            player.ArrangeTiles();
        }

        while (dialogueIndex < 15)
        {
            yield return new WaitForSeconds(1);
        }

        StartCoroutine(FlowersTutorial());
    }

    IEnumerator FlowersTutorial()
    {

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

        foreach (MahjongPlayerBase player in players)
        {
            // player.VisuallySortTiles();
            player.ArrangeTiles();
        }

        currentPlayer = dealer;
        nextPlayer = players[1];

        yield return new WaitForSeconds(0.1f);

        state = GameState.playing;
        StartCoroutine(TakeTurn(currentPlayer));
    }

    IEnumerator TakeTurn(MahjongPlayerBase player)
    {
        while (dialogueIndex < 16)
        {
            if(nextButton.gameObject.activeSelf == true)
            {
                nextButton.gameObject.SetActive(false);
            }
            yield return new WaitForSeconds(1);
        }
        if (!firstTurn)
        {
            yield return new WaitForSeconds(2);

            if (player.currentDecision != decision.none && player.currentDecision != decision.pass)
            {
                SendPlayersMessage(player.gameObject.name + " stole the discard for a " + player.currentDecision.ToString());
                player.currentAvatar.PlayStealAnim();
                player.StealTile();

                //draw like normal if kang
                if (player.currentDecision == decision.kang)
                {
                    yield return new WaitForSeconds(1);
                    SendPlayersMessage(player.gameObject.name + " is drawing a tile");
                    player.currentAvatar.PlayDrawAnim();


                    player.DrawKangTile();

                    yield return new WaitForSeconds(1);

                    if (player.currentDrawnTile().tileType == suit.flower)
                    {
                        while (player.currentDrawnTile().tileType == suit.flower)
                        {
                            player.DrawFlowerTile();
                            SendPlayersMessage(player.gameObject.name + " drew a flower");
                            player.currentAvatar.PlayDrawAnim();
                            yield return new WaitForSeconds(1);
                        }
                    }

                    if (network)
                    {
                        MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", player.gameObject.name + " finished drawing.");
                    }
                    else
                    {
                        SendPlayersMessage(player.gameObject.name + " finished drawing");
                    }

                    player.AddDrawnTileToClosedHand();
                }
            }
            else
            {
                //the tile was not stolen
                mostRecentDiscard.transform.parent = DeadTileParent.transform;
                deadTiles.Add(mostRecentDiscard);
                deadTiles.Sort(CompareTileNumbers);
                // mostRecentDiscard = null;


                SendPlayersMessage(player.gameObject.name + " is drawing a tile");
                player.currentAvatar.PlayDrawAnim();

                player.DrawTile();

                // StartCoroutine(player.DrawTile());
                yield return new WaitForSeconds(1);
                if (player.currentDrawnTile().tileType == suit.flower)
                {
                    while (player.currentDrawnTile().tileType == suit.flower)
                    {
                        player.DrawFlowerTile();
                        SendPlayersMessage(player.gameObject.name + " drew a flower");
                        player.currentAvatar.PlayDrawAnim();

                        yield return new WaitForSeconds(1);
                    }
                }


                SendPlayersMessage(player.gameObject.name + " finished drawing");


                player.AddDrawnTileToClosedHand();

            }
            player.ArrangeTiles();
        }
        else
        {
            firstTurn = false;
        }

        if (player.CalculateNormalWin() && player.CalculateSevenPairs())
        {
            if (player.TryGetComponent<HumanPlayer>(out HumanPlayer human))
            {
                human.FlipWinButton();
            }
        }

        //set most recent discard as null after the player has drawn so they can make the decision
        foreach (MahjongPlayerBase user in players)
            user.ResetMelds();
        player.currentDecision = decision.none;
        player.SetPlayerState(PlayerState.discarding);
        mostRecentDiscard = null;

        SendPlayersMessage(player.gameObject.name + " is deciding on a discard");


        if (player.GetComponent<TutorialAI>() != null)
        {
            player.ForceDiscard();
        }
        else
        {
            while (player.discardChoice == null)
            {
                yield return new WaitForSeconds(1);
            }
        }

        if (player.win)
        {
            StartCoroutine(GameWin());
        }

        yield return new WaitForSeconds(2);


        player.ArrangeTiles();
        player.SetPlayerState(PlayerState.waiting);


        SendPlayersMessage(player.gameObject.name + " discarded " + mostRecentDiscard.ToString());
        player.currentAvatar.PlayDiscardAnim();

        yield return new WaitForSeconds(2);

        //flip it up to be visible
        mostRecentDiscard.transform.rotation = Quaternion.Euler(0, 90, 90);
        //place the discard in the middle of the table
        mostRecentDiscard.transform.position =
                new Vector3(UnityEngine.Random.Range(TileBoundaries.bounds.min.x + 0.35f, TileBoundaries.bounds.max.x - 0.35f),
                0.065f, UnityEngine.Random.Range(TileBoundaries.bounds.min.z + 0.35f, TileBoundaries.bounds.max.z - 0.35f));

        StartCoroutine(BetweenTurn());
    }

    new public IEnumerator BetweenTurn()
    {


        foreach (MahjongPlayerBase player in players)
        {
            player.SetPlayerState(PlayerState.deciding);
            if (player.GetComponent<TutorialAI>() != null) ;
            player.currentDecision = decision.pass;
        }

        if (dialogueIndex < 31)
        {
            while (dialogueIndex < 31)
            {
                yield return new WaitForSeconds(1);
            }
        }
        else if (dialogueIndex < 28)
        {
            while (dialogueIndex < 28)
            {
                yield return new WaitForSeconds(1);
            }
        }
        else if (dialogueIndex < 24)
        {
            while (dialogueIndex < 24)
            {
                yield return new WaitForSeconds(1);
            }
        }
        while (dialogueIndex < 19)
        {
            yield return new WaitForSeconds(1);
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

        SendPlayersMessage("All players done making a decision");
        foreach (MahjongPlayerBase player in players)
        {
            player.SetPlayerState(PlayerState.waiting);
        }

        nextPlayer = players[(players.IndexOf(currentPlayer) + 1) % players.Count];
        yield return new WaitForSeconds(1);


        foreach (MahjongPlayerBase player in players)
        {
            if (player != currentPlayer)
            {
                //only one person can do any meld at any given time.
                //so break the loop and give them the turn if true
                if (player.currentDecision == decision.kang)
                {
                    nextPlayer = player;
                    // message = nextPlayer.gameObject.name + " calls kang";
                    break;
                }
                //same with pong, but kang get priority so its lower
                if (player.currentDecision == decision.pong)
                {
                    nextPlayer = player;
                    // message = nextPlayer.gameObject.name + " calls pong";
                    break;
                }

            }
        }

        currentPlayer = nextPlayer;
        foreach (MahjongPlayerBase player in players)
        {
            if (player != currentPlayer)
            {
                player.currentDecision = decision.none;
                player.GetComponent<HumanPlayer>().FlipUI();
            }
        }

        yield return new WaitForSeconds(1);

        StartCoroutine(TakeTurn(currentPlayer));
    }


}

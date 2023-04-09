using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum GameState { setup, playing, finished };

public class MahjongManager : MonoBehaviour
{

    public static MahjongManager mahjongManager;
    #region Tile-Related
    protected List<Tile> board;
    public List<Tile> wall;
    protected List<Tile> deadTiles;
    public Tile mostRecentDiscard;
    protected static int MAXTILECOUNT = 144;
    #endregion
    #region Player-Related
    public List<MahjongPlayerBase> players;
    public MahjongPlayerBase dealer, currentPlayer, nextPlayer;
    public MahjongPlayerBase previousPlayer;
    #endregion
    #region Gameplay Properties
    protected GameState state;
    protected int round, numRounds, dieRollResult;
    protected bool firstTurn;
    #endregion 
    #region Physical Elements
    public GameObject InitialTileParent, TileSizeReference, DeadTileParent;
    public BoxCollider TileBoundaries;
    #endregion
    public bool network;

    [Header("Debugging Tools")]
    public bool debug;
    public Tile discardedTile;
    public List<Tile> debugFullClosedSevenPairs;
    public List<Tile> debugOneMeldSevenPairsClosedHand;
    public List<Tile> debugOneMeldSevenPairsOpenHand;
    public List<Tile> debugFullClosedNormalWin;
    public List<Tile> debugClosedHandArrangement;
    public List<Tile> debugOpenHandArrangement;
    public List<Tile> debugFlowerArrangement;

    public virtual void Awake()
    {
        //Singleton thing
        if (mahjongManager != null && mahjongManager != this)
        {
            Destroy(gameObject);
        }
        else
        {
            mahjongManager = this;
        }

        // pass the network check
        if (GetComponent<PhotonView>() == null)
        {
            network = false;
        }
        else
        {
            network = true;
        }

        //always load into the same scene, so find this object in the scene
        InitialTileParent = GameObject.Find("Tiles");
        DeadTileParent = GameObject.Find("Dead Tiles");
        TileBoundaries = GameObject.Find("TileBoundaries").GetComponent<BoxCollider>();

        //this is called locally and networked for other players
        if (!network)
        {
            InitializeGame();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!network && debug)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                Debug.Log("StopAllCoroutines called");

                StopAllCoroutines();
                state = GameState.playing;
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                StopAllCoroutines();
                Debug.Log("Testing Seven Pairs Fully Closed");
                HumanPlayer human = FindObjectOfType<HumanPlayer>();
                human.DebugClearHand();
                foreach (Tile tile in debugFullClosedSevenPairs)
                {
                    human.AddTile(tile);
                }
                human.currentState = PlayerState.deciding;
                human.CalculateHandOptions();
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                StopAllCoroutines();
                Debug.Log("Testing Seven Pairs with one Meld");
                HumanPlayer human = FindObjectOfType<HumanPlayer>();
                human.DebugClearHand();
                foreach (Tile tile in debugOneMeldSevenPairsClosedHand)
                {
                    human.AddTile(tile);
                }
                foreach (Tile tile in debugOneMeldSevenPairsOpenHand)
                {
                    human.DebugAddOpenHandTile(tile);
                }
                human.currentState = PlayerState.deciding;
                human.CalculateHandOptions();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                StopAllCoroutines();
                Debug.Log("Testing Normal Win 1");
                HumanPlayer human = FindObjectOfType<HumanPlayer>();
                human.DebugClearHand();
                foreach (Tile tile in debugFullClosedNormalWin)
                {
                    human.AddTile(tile);
                }
                human.currentState = PlayerState.deciding;
                human.CalculateHandOptions();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                StopAllCoroutines();
                Debug.Log("Testing Normal Win with discard");
                HumanPlayer human = FindObjectOfType<HumanPlayer>();
                human.DebugClearHand();
                foreach (Tile tile in debugFullClosedNormalWin)
                {
                    human.AddTile(tile);
                }
                mostRecentDiscard = discardedTile;
                human.currentState = PlayerState.deciding;
                human.CalculateHandOptions();
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                StopAllCoroutines();
                Debug.Log("Testing Open Hand Tile arrangement");
                HumanPlayer human = FindObjectOfType<HumanPlayer>();
                human.DebugClearHand();
                foreach (Tile tile in debugClosedHandArrangement)
                {
                    human.AddTile(tile);
                }
                foreach (Tile tile in debugOpenHandArrangement)
                {
                    human.DebugAddOpenHandTile(tile);
                }
                foreach (Tile tile in debugFlowerArrangement)
                {
                    human.DebugAddFlower(tile);
                }
                state = GameState.playing;
                human.currentState = PlayerState.waiting;
                human.ArrangeTiles();
            }
        }

    }

    public void InitializeGame()
    {
        //Initialize the player list
        players = new List<MahjongPlayerBase>();
        if (!network)
            players.AddRange(FindObjectsOfType<MahjongPlayerBase>().ToList<MahjongPlayerBase>());
        else
        {
            players = MultiplayerGameManager.Instance.players;
            foreach (MahjongPlayerBase player in players)
            {
                if (player.GetComponent<HumanPlayer>() != null && player.GetComponent<NetworkedPlayer>() != null && player.GetComponent<NetworkedPlayer>().photonView.IsMine)
                {
                    player.GetComponent<HumanPlayer>().playerCanvas.SetActive(true);
                }
            }
        }


        //initialize game states, tile lists, and other relevant objects
        state = GameState.setup;
        board = new List<Tile>(MAXTILECOUNT);
        wall = new List<Tile>();
        deadTiles = new List<Tile>();
        mostRecentDiscard = null;
        firstTurn = true;

        if (!TileBoundaries.isTrigger)
            TileBoundaries.isTrigger = true;

        //put all the tiles into board structure;
        if (!network || (network && PhotonNetwork.IsMasterClient))
            foreach (Tile tile in InitialTileParent.transform.GetComponentsInChildren<Tile>())
            {
                board.Add(tile);
            }



        //now, start the board setup
        if (!network)
            StartCoroutine(BoardSetup());
        else
        {
            //only do this on the master client, the rest will be networked across others
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(BoardSetup());
            }
        }
    }

    public IEnumerator BoardSetup()
    {
        //Send a debug message to inform players of game state
        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", "Starting game");
        }
        else
        {
            SendPlayersMessage("Starting game");
        }
        yield return new WaitForSeconds(2);

        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", "Shuffling Board and Creating Walls");
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("shuffleSound");
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("shuffleAnim");
        }
        else
        {
            SendPlayersMessage("Shuffling Board and Creating Walls");
            BoardAudioHandler.audioHandler.source.volume = 0.5f;
            BoardAudioHandler.audioHandler.PlayShuffle();
            foreach (MahjongPlayerBase player in MahjongManager.mahjongManager.GetPlayers())
            {
                player.currentAvatar.PlayShuffleAnim();
            }
        }

        //randomize the board
        System.Random rand = new System.Random();
        int n = board.Count;
        while (n > 1)
        {
            int k = rand.Next(n--);
            Tile temp = board[n];
            board[n] = board[k];
            board[k] = temp;
        }

        // yield return new WaitForSeconds(2);

        //physically move the tiles
        float distanceReference = TileSizeReference.transform.localScale.z / 2;
        float heightReference = TileSizeReference.transform.localScale.y / 2.75f;
        int multiplier = 0;
        for (int x = 0; x < board.Count; x++)
        {
            board[x].transform.localRotation = Quaternion.Euler(0, 0, -90);
            if (x % 36 == 0)
            {
                multiplier = 0;
                yield return new WaitForSeconds(1);
            }
            if (x < 36)
            {
                if (x % 2 == 0)
                {
                    board[x].transform.position = new Vector3(TileBoundaries.bounds.max.x, TileBoundaries.bounds.min.y + heightReference, TileBoundaries.bounds.max.z - (distanceReference * multiplier) + 0.01f);
                }
                else
                {
                    board[x].transform.position = board[x - 1].transform.position + Vector3.up * -heightReference;

                }
                multiplier += 1;
                continue;
            }

            if (x < 72)
            {
                board[x].transform.Rotate(Vector3.left * 90);
                if (x % 2 == 0)
                {
                    board[x].transform.position = new Vector3(TileBoundaries.bounds.max.x - (distanceReference * multiplier) + 0.01f, TileBoundaries.bounds.min.y + heightReference, TileBoundaries.bounds.min.z);
                }
                else
                {
                    board[x].transform.position = board[x - 1].transform.position + Vector3.up * -heightReference;

                }
                multiplier += 1;
                continue;

            }

            if (x < 108)
            {
                if (x % 2 == 0)
                {
                    board[x].transform.position = new Vector3(TileBoundaries.bounds.min.x, TileBoundaries.bounds.min.y + heightReference, TileBoundaries.bounds.min.z + (distanceReference * multiplier) + 0.01f);
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
                    board[x].transform.position = new Vector3(TileBoundaries.bounds.min.x + (distanceReference * multiplier) + 0.01f, TileBoundaries.bounds.min.y + heightReference, TileBoundaries.bounds.max.z);
                }
                else
                {
                    board[x].transform.position = board[x - 1].transform.position + Vector3.up * -heightReference;

                }
                multiplier += 1;
                continue;
            }

        }

        yield return new WaitForSeconds(2);

        //this needs to be RPCed for every other client to have their board updated
        if (network)
        {
            foreach (Tile tile in board)
            {
                tile.TileRPCCall("BoardAdd");
            }
        }

        StartCoroutine(RollDice());

    }

    public IEnumerator RollDice()
    {
        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", "Rolling dice for dealer");
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("diceSound");
        }
        else
        {
            SendPlayersMessage("Rolling dice for dealer");
            BoardAudioHandler.audioHandler.source.volume = 1f;
            BoardAudioHandler.audioHandler.PlayDiceRoll();
        }


        //dice roll message for debugging
        int diceIndex = 1;
        string dieMessage = "";

        //newand improved real actual dice roll
        //play diceroll animation
        int dieRoll = 0;
        List<Dice> die = FindObjectsOfType<Dice>().ToList<Dice>();
        foreach (Dice dice in die)
        {
            StartCoroutine(dice.DiceRoll());
        }
        yield return new WaitForSeconds(2f);
        foreach (Dice dice in die)
        {
            dieMessage += "Dice " + diceIndex + " roll: " + dice.rollResult + " ";
            dieRoll += dice.rollResult;
        }

        Debug.Log(dieMessage + "Total roll: " + dieRoll);
        dieRollResult = (dieRoll - 1) % players.Count;

        yield return new WaitForSeconds(2);



        if (network)
            dealer = players[dieRollResult];
        else
            dealer = FindObjectOfType<HumanPlayer>();


        //send the network message to inform everyone of dealer
        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall(
                "message", "Dealer is " + dealer.name
            );
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall(
                "dealer", dieRoll
            );
        }
        else
        {
            SendPlayersMessage("Dealer is " + dealer.name);
            StartCoroutine(DistributeTiles());
        }


    }
    public IEnumerator DistributeTiles()
    {

        yield return new WaitForSeconds(2);

        //now do dealer things
        int dealerIndex = players.IndexOf(dealer);
        int wallIndex = (MAXTILECOUNT / 4) * dealerIndex + dieRollResult;
        wall.AddRange(board.GetRange(wallIndex, MAXTILECOUNT - wallIndex));
        wall.AddRange(board.GetRange(0, wallIndex));

        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", "Distributing tiles to players");
            // MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("shuffleAnim");
        }
        else
        {
            SendPlayersMessage("Distributing tiles to players");
        }


        //now distribute the tiles to others.
        List<Tile> distributedTiles = wall.GetRange(0, 1 + (16 * players.Count));
        wall.RemoveRange(0, 1 + (16 * players.Count));
        dealer.GetComponent<MahjongPlayerBase>().AddTile(distributedTiles[0]);

        for (int i = 1; i < 1 + (16 * players.Count); i++)
        {
            players[(dealerIndex + ((i - 1) / 16)) % players.Count].AddTile(distributedTiles[i]);
        }

        //arrange each player's tiles
        //this needs to be done on the master client to avoid visual issues with the tiles
        if (!network || (network && PhotonNetwork.IsMasterClient))
            foreach (MahjongPlayerBase player in players)
            {
                player.ArrangeTiles();
            }

        yield return new WaitForSeconds(2);

        //replace drawn flower tiles
        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", "Replacing flowers");
            // MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("drawAnim");

        }
        else
        {
            SendPlayersMessage("Replacing flowers");
            // foreach (MahjongPlayerBase player in MahjongManager.mahjongManager.GetPlayers())
            // {
            //     player.currentAvatar.PlayDrawAnim();
            // }
        }

        int needFlowers = -1;
        while (needFlowers != 0)
        {
            needFlowers = players.Count;
            foreach (MahjongPlayerBase player in players)
            {
                int remainingFlowers = player.replaceInitialFlowerTiles();
                if (remainingFlowers == 0)
                    needFlowers -= 1;
            }
        }

        yield return new WaitForSeconds(2);

        //arrange tiles again
        if (!network || (network && PhotonNetwork.IsMasterClient))
            foreach (MahjongPlayerBase player in players)
            {
                player.ArrangeTiles();
            }

        currentPlayer = dealer;
        nextPlayer = players[(dealerIndex + 1) % players.Count];

        yield return new WaitForSeconds(2);

        //now we may set the gamestate properly
        state = GameState.playing;

        // if (network)
        // {
        //     MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("turn1");
        // }
        // else
        // {
        //     StartCoroutine(FirstTurn(dealer));
        // }
        StartCoroutine(TakeTurn(dealer));
    }

    //a coroutine differentiating the first turn of every game
    IEnumerator FirstTurn(MahjongPlayerBase player)
    {
        // if (network)
        // {
        //     MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", player.gameObject.name + " is taking their turn.");
        // }
        // else
        // {
        //     SendPlayersMessage(player.gameObject.name + " is taking their turn.");
        // }

        //the current player's turn is set to discard

        yield return new WaitForSeconds(2);

        if (player.CalculateNormalWin() && player.CalculateSevenPairs())
        {
            if (player.TryGetComponent<HumanPlayer>(out HumanPlayer human))
            {
                human.FlipWinButton();
            }
        }

        player.SetPlayerState(PlayerState.discarding);

        //do a time based implementation so people cannot stall out the turn;
        int time;
        // time = 60;
        time = 300;
        for (int i = time; i > 0; i--)
        {
            if (network)
            {
                MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", player.gameObject.name + " is taking their turn." + "\nTurn time remaining: " + i + " seconds left");
            }
            else
            {
                SendPlayersMessage(player.gameObject.name + " is taking their turn." + "\nTurn time remaining: " + i + " seconds left");
            }
            yield return new WaitForSeconds(1);

            if (player.win)
            {
                StartCoroutine(GameWin());
            }

            if (mostRecentDiscard != null)
            {
                break;
            }
        }
        if (mostRecentDiscard == null)
        {
            player.ForceDiscard();
        }
        player.ArrangeTiles();

        player.SetPlayerState(PlayerState.waiting);


        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", player.gameObject.name + " discarded " + mostRecentDiscard.ToString());
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("discardAnim");
        }
        else
        {
            SendPlayersMessage(player.gameObject.name + " discarded " + mostRecentDiscard.ToString());
            player.currentAvatar.PlayDiscardAnim();
        }

        //the current player's turn is now set to wait

        yield return new WaitForSeconds(2);

        //flip it up to be visible
        mostRecentDiscard.transform.rotation = Quaternion.Euler(0, 90, 90);
        //place the discard in the middle of the table
        mostRecentDiscard.transform.position =
                new Vector3(UnityEngine.Random.Range(TileBoundaries.bounds.min.x + 0.35f, TileBoundaries.bounds.max.x - 0.35f),
                0.065f, UnityEngine.Random.Range(TileBoundaries.bounds.min.z + 0.35f, TileBoundaries.bounds.max.z - 0.35f));

        StartCoroutine(BetweenTurn());
    }

    //the proper take turn function
    IEnumerator TakeTurn(MahjongPlayerBase player)
    {

        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", player.gameObject.name + " is taking their turn.");
        }
        else
        {
            SendPlayersMessage(player.gameObject.name + " is taking their turn.");
        }
        if (!firstTurn)
        {

            yield return new WaitForSeconds(1);

            if (player.currentDecision != decision.none && player.currentDecision != decision.pass)
            {
                if (network)
                {
                    MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", player.gameObject.name + " stole the discard for a " + player.currentDecision.ToString());
                    MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("stealAnim");

                }
                else
                {
                    SendPlayersMessage(player.gameObject.name + " stole the discard for a " + player.currentDecision.ToString());
                    player.currentAvatar.PlayStealAnim();
                }

                player.StealTile();

                //draw like normal if kang
                if (player.currentDecision == decision.kang)
                {
                    yield return new WaitForSeconds(1);

                    if (network)
                    {
                        MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", player.gameObject.name + " is drawing a tile");
                        MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("drawAnim");
                    }
                    else
                    {
                        SendPlayersMessage(player.gameObject.name + " is drawing a tile");
                        player.currentAvatar.PlayDrawAnim();
                    }

                    player.DrawKangTile();

                    yield return new WaitForSeconds(1);

                    if (player.currentDrawnTile().tileType == suit.flower)
                    {
                        while (player.currentDrawnTile().tileType == suit.flower)
                        {
                            player.DrawFlowerTile();
                            if (network)
                            {
                                MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", player.gameObject.name + " drew a flower");
                                MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("drawAnim");
                            }
                            else
                            {
                                SendPlayersMessage(player.gameObject.name + " drew a flower");
                                player.currentAvatar.PlayDrawAnim();
                            }
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

                if (network)
                {
                    MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", player.gameObject.name + " is drawing a tile");
                    MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("drawAnim");
                }
                else
                {
                    SendPlayersMessage(player.gameObject.name + " is drawing a tile");
                    player.currentAvatar.PlayDrawAnim();
                }
                player.DrawTile();

                // StartCoroutine(player.DrawTile());
                yield return new WaitForSeconds(1);
                if (player.currentDrawnTile().tileType == suit.flower)
                {
                    while (player.currentDrawnTile().tileType == suit.flower)
                    {
                        player.DrawFlowerTile();
                        if (network)
                        {
                            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", player.gameObject.name + " drew a flower");
                            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("drawAnim");
                        }
                        else
                        {
                            SendPlayersMessage(player.gameObject.name + " drew a flower");
                            player.currentAvatar.PlayDrawAnim();
                        }
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
            // player.ArrangeTiles();
        }
        else
        {
            firstTurn = false;
        }

        yield return new WaitForSeconds(1);

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

        //do a time based implementation so people cannot stall out the turn;
        int time;
        // time = 60;
        time = 300;
        for (int i = time; i > 0; i--)
        {
            if (network)
            {
                MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", player.gameObject.name + " is deciding on a discard" + "\nTurn time remaining: " + i + " seconds left");
            }
            else
            {
                SendPlayersMessage(player.gameObject.name + " is deciding on a discard" + "\nTurn time remaining: " + i + " seconds left");
            }
            yield return new WaitForSeconds(1);

            if (player.win)
            {
                StartCoroutine(GameWin());
            }

            if (mostRecentDiscard != null)
            {
                break;
            }
        }
        if (mostRecentDiscard == null)
        {
            player.ForceDiscard();
        }

        player.ArrangeTiles();
        player.SetPlayerState(PlayerState.waiting);

        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", player.gameObject.name + " discarded " + mostRecentDiscard.ToString());
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("discardAnim");
        }
        else
        {
            SendPlayersMessage(player.gameObject.name + " discarded " + mostRecentDiscard.ToString());
            player.currentAvatar.PlayDiscardAnim();
        }

        //set to wait

        yield return new WaitForSeconds(2);

        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("discardSound");
        }
        else
        {
            BoardAudioHandler.audioHandler.source.volume = 1f;
            BoardAudioHandler.audioHandler.PlayDiscard();
        }

        //flip it up to be visible
        mostRecentDiscard.transform.rotation = Quaternion.Euler(0, 90, 90);
        //place the discard in the middle of the table
        mostRecentDiscard.transform.position =
                new Vector3(UnityEngine.Random.Range(TileBoundaries.bounds.min.x + 0.35f, TileBoundaries.bounds.max.x - 0.35f),
                0.065f, UnityEngine.Random.Range(TileBoundaries.bounds.min.z + 0.35f, TileBoundaries.bounds.max.z - 0.35f));

        StartCoroutine(BetweenTurn());
    }

    public IEnumerator BetweenTurn()
    {
        //set the player that just went to the previous player
        previousPlayer = currentPlayer;

        foreach (MahjongPlayerBase player in players)
        {
            if (player != currentPlayer)
            {
                player.SetPlayerState(PlayerState.deciding);

            }
            else
            {
                player.currentDecision = decision.pass;
            }
        }


        foreach (MahjongPlayerBase player in players)
        {
            if (player != currentPlayer)
            {
                //this is causing a tile to be added to the open hand for some reason
                //separating its execution order to see what happens
                player.CalculateHandOptions();
            }
        }

        bool allDone = true;
        int time;
        // time = 30;
        time = 300;
        for (int i = time; i > 0; i--)
        {
            allDone = true;
            if (network)
            {
                MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", previousPlayer.gameObject.name + " discarded " + mostRecentDiscard.ToString() + "\nDecision time remaining: " + i + " seconds left");
            }
            else
            {
                SendPlayersMessage(previousPlayer.gameObject.name + " discarded " + mostRecentDiscard.ToString() + "\nDecision time remaining: " + i + " seconds left");
            }
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
            {
                break;
            }
        }

        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", "All players done making a decision");
        }
        else
        {
            SendPlayersMessage("All players done making a decision");
        }

        foreach (MahjongPlayerBase player in players)
        {
            player.SetPlayerState(PlayerState.waiting);
        }

        // string message = "All players passed up the discard";
        string message = "All players done making a decision";


        // MahjongPlayerBase next = players[];
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

        // if (nextPlayer.currentDecision == decision.chow)
        //     message = nextPlayer.gameObject.name + " calls pong";

        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", message);
        }
        else
        {
            SendPlayersMessage(message);
        }

        currentPlayer = nextPlayer;

        // if (nextPlayer.currentDecision == decision.pass)
        // {
        //     mostRecentDiscard.transform.parent = DeadTileParent.transform;
        //     mostRecentDiscard = null;
        // }

        //reset everyone else's decisions
        foreach (MahjongPlayerBase player in players)
        {
            if (player != currentPlayer)
            {
                player.currentDecision = decision.none;
                if (player.GetComponent<HumanPlayer>() != null)
                    player.GetComponent<HumanPlayer>().FlipUI();
            }
        }

        yield return new WaitForSeconds(1);

        StartCoroutine(TakeTurn(currentPlayer));
    }

    public IEnumerator GameWin()
    {
        state = GameState.finished;
        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("winAnim");
        }
        else
        {
            currentPlayer.currentAvatar.PlayWinAnim();
        }

        yield return new WaitForSeconds(2f);

        foreach (MahjongPlayerBase player in players)
        {
            if (player.GetComponent<HumanPlayer>() != null)
            {
                player.GetComponent<HumanPlayer>().playerCanvas.SetActive(false);
            }
        }

        MultiplayerGameManager.Instance.EnableCanvas();
        StopAllCoroutines();
    }

    public GameState GetGameState()
    {
        return state;
    }
    public List<Tile> GetBoard()
    {
        return board;
    }

    public List<MahjongPlayerBase> GetPlayers()
    {
        return players;
    }
    public void SetRemoteBoardValues(List<Tile> board)
    {
        Debug.Log("Updating Board Information on remote");

        this.board = board;

    }
    public void SetDealer(int index)
    {
        int dieRollResult = (index - 1) % players.Count;
        dealer = players[dieRollResult];
        StartCoroutine(DistributeTiles());
    }

    public virtual void SendPlayersMessage(string message)
    {
        foreach (MahjongPlayerBase player in players)
        {

            if (player.TryGetComponent<HumanPlayer>(out HumanPlayer human))
            {
                human.debugText.text = message;
            }
        }
    }

    public void FirstNetworkedTurn()
    {
        StartCoroutine(FirstTurn(dealer));
    }

    public static int CompareTileNumbers(Tile x, Tile y)
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


}

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum GameState { setup, playing, finished };

public class MahjongManager : MonoBehaviour
{
    public bool debug;
    public static MahjongManager mahjongManager;
    // [SerializeField]
    protected List<Tile> board;
    public List<Tile> wall;

    protected List<Tile> deadTiles;

    public Tile mostRecentDiscard;
    protected static int MAXTILECOUNT = 144;
    protected GameState state;

    [SerializeField]
    protected List<MahjongPlayerBase> players;// = new List<MahjongPlayerBase>(4);

    public MahjongPlayerBase dealer, currentPlayer, nextPlayer;
    public MahjongPlayerBase previousPlayer;

    protected int round, numRounds;
    public GameObject InitialTileParent, TileSizeReference, DeadTileParent;
    public BoxCollider TileBoundaries;
    public bool network;



    // Start is called before the first frame update
    protected virtual void Awake()
    {
        if (mahjongManager != null && mahjongManager != this)
        {
            Destroy(gameObject);
        }
        else
        {
            mahjongManager = this;
        }

        if (!network)
        {
            InitializeGame();
        }
        if(GetComponent<PhotonView>() == null)
        {
            network = false;
        }
        else
        {
            network = true;
        }
        InitialTileParent = GameObject.Find("Tiles");
        DeadTileParent = GameObject.Find("Dead Tiles");
        TileBoundaries = GameObject.Find("TileBoundaries").GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitializeGame()
    {
        players = new List<MahjongPlayerBase>();
        if (!network)
            players.AddRange(FindObjectsOfType<MahjongPlayerBase>().ToList<MahjongPlayerBase>());
        else
            players = MultiplayerGameManager.Instance.players;
        //initialize some stuff
        state = GameState.setup;
        board = new List<Tile>(MAXTILECOUNT);
        wall = new List<Tile>();
        deadTiles = new List<Tile>();
        mostRecentDiscard = null;

        if (!TileBoundaries.isTrigger)
            TileBoundaries.isTrigger = true;

        //put all the tiles into board structure;
        if (!network || (network && PhotonNetwork.IsMasterClient))
            foreach (Tile tile in InitialTileParent.transform.GetComponentsInChildren<Tile>())
            {
                board.Add(tile);
            }
        if (!network)
            StartCoroutine(BoardSetup());
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // Debug.Log("Trying Master Client Board Setup");
                StartCoroutine(BoardSetup());
            }
        }
    }

    public IEnumerator BoardSetup()
    {
        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", "Shuffling Board and Creating Walls");
        }
        else
        {
            SendPlayersMessage("Shuffling Board and Creating Walls");
        }
        yield return new WaitForSeconds(2);
        System.Random rand = new System.Random();
        int n = board.Count;
        while (n > 1)
        {
            int k = rand.Next(n--);
            Tile temp = board[n];
            board[n] = board[k];
            board[k] = temp;
        }

        // wall = board;
        // Debug.Log(TileBoundaries.bounds.max);

        float distanceReference = TileSizeReference.transform.localScale.z / 2;
        float heightReference = TileSizeReference.transform.localScale.y / 2.75f;
        int multiplier = 0;

        for (int x = 0; x < board.Count; x++)
        {
            board[x].transform.localRotation = Quaternion.Euler(0, 0, -90);
            if (x % 36 == 0)
            {
                multiplier = 0;
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
            // break;
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
            // break;
            if (x < 108)
            {
                // board[x].transform.Rotate(Vector3.left  * 90);
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
            // tile.transform.Rotate(new Vector3(0, 0, -90));
        }

        yield return new WaitForSeconds(2);

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
        Debug.Log("Rolling dice for dealer");
        yield return new WaitForSeconds(2);

        System.Random rand = new System.Random();
        int dieRoll = rand.Next(2, 13);
        int dieRollResult = (dieRoll - 1) % players.Count;
        if (!debug)
            dealer = players[dieRollResult];
        else
        {
            if(network)
                dealer = players[0];
            else
                dealer = FindObjectOfType<HumanPlayer>();
        }

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
            StartCoroutine(CreateWalls());
        }


    }
    public IEnumerator CreateWalls()
    {

        yield return new WaitForSeconds(2);

        int dealerIndex = players.IndexOf(dealer);

        //create walls
        int wallIndex = (MAXTILECOUNT / 4) * dealerIndex;
        wall.AddRange(board.GetRange(wallIndex, MAXTILECOUNT - wallIndex));
        wall.AddRange(board.GetRange(0, wallIndex));

        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", "Distributing tiles to players");
        }
        else
        {
            SendPlayersMessage("Distributing tiles to players");
        }

        List<Tile> distributedTiles = wall.GetRange(0, 1 + (16 * players.Count));
        wall.RemoveRange(0, 1 + (16 * players.Count));
        dealer.GetComponent<MahjongPlayerBase>().AddTile(distributedTiles[0]);

        for (int i = 1; i < 1 + (16 * players.Count); i++)
        {

            players[(dealerIndex + ((i - 1) / 16)) % players.Count].AddTile(distributedTiles[i]);
        }

        if (!network || (network && PhotonNetwork.IsMasterClient))
            foreach (MahjongPlayerBase player in players)
            {
                // player.VisuallySortTiles();
                player.ArrangeTiles();
            }

        yield return new WaitForSeconds(2);

        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", "Replacing flowers");
        }
        else
        {
            SendPlayersMessage("Replacing flowers");
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

        Debug.Log("Flowers Finished");

        if (!network || (network && PhotonNetwork.IsMasterClient))
            foreach (MahjongPlayerBase player in players)
            {
                // player.VisuallySortTiles();
                player.ArrangeTiles();
            }

        currentPlayer = dealer;
        nextPlayer = players[(dealerIndex + 1) % players.Count];

        yield return new WaitForSeconds(2);
        state = GameState.playing;

        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("turn1");
        }
        else
        {
            StartCoroutine(FirstTurn(dealer));
        }
    }

    IEnumerator FirstTurn(MahjongPlayerBase player)
    {
        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", player.gameObject.name + " is taking their turn.");
        }
        else
        {
            SendPlayersMessage(player.gameObject.name + " is taking their turn.");
        }

        player.SetPlayerState(PlayerState.discarding);

        yield return new WaitForSeconds(2);


        //do a time based implementation so people cannot stall out the turn;
        int time;
        if (!debug)
            time = 60;
        else
            time = 300;
        for (int i = time; i > 0; i--)
        {
            if (network)
            {
                MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", "Turn time remaining: " + i + " seconds left");
            }
            else
            {
                SendPlayersMessage("Turn time remaining: " + i + " seconds left");
            }
            yield return new WaitForSeconds(1);
            if (mostRecentDiscard != null)
            {
                break;
            }
        }
        if (mostRecentDiscard == null)
        {
            // player.SetDiscardChoice(player.currentDrawnTile());
            // // player.SetNullDrawnTile();
            // player.DiscardTile();
            player.ForceDiscard();
        }

        // Debug.Log(player.gameObject.name + " discarded " + mostRecentDiscard.number + " " + mostRecentDiscard.tileType);
        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", "Player Discarded " + mostRecentDiscard.ToString());
        }
        else
        {
            SendPlayersMessage(player.gameObject.name + " discarded " + mostRecentDiscard.ToString());
        }
        player.SetPlayerState(PlayerState.waiting);

        // if (!debug)
        mostRecentDiscard.transform.position = Vector3.zero;
        mostRecentDiscard.transform.rotation = Quaternion.Euler(0, 90, 90);

        StartCoroutine(BetweenTurn());
    }

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

        yield return new WaitForSeconds(2);

        player.SetPlayerState(PlayerState.discarding);
        if (player.currentDecision != decision.none && player.currentDecision != decision.pass)
        {
            if (network)
            {
                MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", player.gameObject.name + " stole the discard");
            }
            else
            {
                SendPlayersMessage(player.gameObject.name + " stole the discard");
            }
            player.StealTile();
        }
        else
        {

            if (network)
            {
                MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", player.gameObject.name + " is drawing a tile");
            }
            else
            {
                SendPlayersMessage(player.gameObject.name + " is drawing a tile");
            }
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

        yield return new WaitForSeconds(2);


        //set most recent discard as null after the player has drawn so they can make the decision
        foreach (MahjongPlayerBase user in players)
            user.ResetMelds();
        player.currentDecision = decision.none;
        mostRecentDiscard = null;

        //do a time based implementation so people cannot stall out the turn;
        int time;
        if (!debug)
            time = 60;
        else
            time = 300;
        for (int i = time; i > 0; i--)
        {
            if (network)
            {
                MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", "Turn time remaining: " + i + " seconds left");
            }
            else
            {
                SendPlayersMessage("Turn time remaining: " + i + " seconds left");
            }
            yield return new WaitForSeconds(1);
            if (mostRecentDiscard != null)
            {
                break;
            }
        }
        if (mostRecentDiscard == null)
        {

            player.ForceDiscard();

            // mostRecentDiscard = player.currentDrawnTile();
            // player.SetNullDrawnTile();
        }

        Debug.Log(player.gameObject.name + " discarded " + mostRecentDiscard.number + " " + mostRecentDiscard.tileType);
        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", "Player Discarded " + mostRecentDiscard.ToString());
        }
        else
        {
            SendPlayersMessage(player.gameObject.name + " discarded " + mostRecentDiscard.ToString());
        }
        player.SetPlayerState(PlayerState.waiting);

        if (!debug)
            mostRecentDiscard.transform.position = Vector3.up * 0.5f;
        yield return new WaitForSeconds(2);

        StartCoroutine(BetweenTurn());
    }

    public IEnumerator BetweenTurn()
    {
        //set the player that just went to the previous player
        previousPlayer = currentPlayer;

        // Debug.Log("Decision Time");
        // if (network)
        // {
        //     MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", "Last discard: " + MahjongManager.mahjongManager.mostRecentDiscard.ToString());
        // }
        // else
        // {
        //     SendPlayersMessage("Last discard: " + MahjongManager.mahjongManager.mostRecentDiscard.ToString());
        // }

        foreach (MahjongPlayerBase player in players)
        {
            player.SetPlayerState(PlayerState.deciding);
            player.CalculateHandOptions();
        }



        bool allDone = true;
        int time;
        if (!debug)
            time = 30;
        else
            time = 300;
        for (int i = time; i > 0; i--)
        {
            allDone = true;
            if (network)
            {
                MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", "Player Discarded " + mostRecentDiscard.ToString() + "\nDecision time remaining: " + i + " seconds left");
            }
            else
            {
                SendPlayersMessage("Player Discarded " + mostRecentDiscard.ToString() + "\nDecision time remaining: " + i + " seconds left");
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
                Debug.Log("all players finished");
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

        string message = "All players passed up the discard";


        // MahjongPlayerBase next = players[];
        nextPlayer = players[(players.IndexOf(currentPlayer) + 1) % players.Count];
        Debug.Log("set next player but lets check the decision list");
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
                    message = nextPlayer.gameObject.name + " calls kang";
                    break;
                }
                //same with pong, but kang get priority so its lower
                if (player.currentDecision == decision.pong)
                {
                    nextPlayer = player;
                    message = nextPlayer.gameObject.name + " calls pong";
                    break;
                }

            }
        }

        if (nextPlayer.currentDecision == decision.chow)
            message = nextPlayer.gameObject.name + " calls pong";

        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall("message", message);
        }
        else
        {
            SendPlayersMessage(message);
        }

        currentPlayer = nextPlayer;
        //reset everyone else's decisions
        foreach (MahjongPlayerBase player in players)
        {
            if (player != currentPlayer)
            {
                player.currentDecision = decision.none;
            }
        }

        yield return new WaitForSeconds(1);

        StartCoroutine(TakeTurn(currentPlayer));
    }

    public void FinishGame()
    {
        state = GameState.finished;
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
        StartCoroutine(CreateWalls());
    }

    public void SendPlayersMessage(string message)
    {
        foreach (MahjongPlayerBase player in MahjongManager.mahjongManager.GetPlayers())
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




}

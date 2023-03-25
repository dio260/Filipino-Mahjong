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

    protected MahjongPlayerBase dealer, previousPlayer, currentPlayer, nextPlayer;

    protected int round, numRounds;
    public GameObject InitialTileParent, TileSizeReference;
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
        InitialTileParent = GameObject.Find("Tiles");
        TileBoundaries = GameObject.Find("TileBoundaries").GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitializeGame()
    {
        players = new List<MahjongPlayerBase>();
        if(!network)
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
        foreach (Tile tile in InitialTileParent.transform.GetComponentsInChildren<Tile>())
        {
            // tile.GetComponent<Rigidbody>().useGravity = false;
            // tile.GetComponent<Rigidbody>().isKinematic = true;
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
        Debug.Log("Shuffling Board");
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
            foreach(Tile tile in board)
            {
                tile.RPCTileAdd();
            }
        }

        StartCoroutine(RollDice());
    }

    public IEnumerator RollDice()
    {
        Debug.Log("Rolling dice for dealer");
        System.Random rand = new System.Random();
        int dieRollResult = (rand.Next(2, 13) - 1) % 4;
        if (!debug)
            dealer = players[dieRollResult];
        else
            dealer = players[0];

        if (network)
        {
            MultiplayerMahjongManager.multiMahjongManager.MasterRPCCall(
                "message", "Dealer is player at index " + dieRollResult
            );
        }

        Debug.Log("Creating Walls");
        yield return new WaitForSeconds(2);

        //create walls
        int wallIndex = (MAXTILECOUNT / 4) * dieRollResult;
        wall.AddRange(board.GetRange(wallIndex, MAXTILECOUNT - wallIndex));
        wall.AddRange(board.GetRange(0, wallIndex));

        // Debug.Log("Distributing Hand");

        // List<Tile> distributedTiles = wall.GetRange(0, 65);
        // wall.RemoveRange(0, 65);
        // dealer.GetComponent<MahjongPlayerBase>().AddTile(distributedTiles[0]);
        // // distributedTiles.Remove(wall[0]);
        // for (int i = 1; i < 65; i++)
        // {
        //     players[(dieRollResult + ((i - 1) / 16)) % 4].AddTile(distributedTiles[i]);
        // }

        // foreach (MahjongPlayerBase player in players)
        // {
        //     // player.VisuallySortTiles();
        //     player.ArrangeTiles();
        // }
        // // StartCoroutine(Wait());
        // yield return new WaitForSeconds(2);

        // Debug.Log("Replacing Flowers");

        // int needFlowers = -1;
        // while (needFlowers != 0)
        // {
        //     needFlowers = 4;
        //     foreach (MahjongPlayerBase player in players)
        //     {
        //         int remainingFlowers = player.replaceInitialFlowerTiles();
        //         if (remainingFlowers == 0)
        //             needFlowers -= 1;
        //     }
        // }


        // foreach (MahjongPlayerBase player in players)
        // {
        //     // player.VisuallySortTiles();
        //     player.ArrangeTiles();
        // }

        // currentPlayer = dealer;
        // nextPlayer = players[(dieRollResult + 1) % players.Count];

        // yield return new WaitForSeconds(2);
        // state = GameState.playing;
        // StartCoroutine(TakeTurn(currentPlayer));
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

        if (!debug)
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




}

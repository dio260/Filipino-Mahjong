using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { setup, playing, finished };

public class MahjongManager : MonoBehaviour
{
    public static MahjongManager mahjongManager;
    // [SerializeField]
    private List<Tile> board;
    public List<Tile> wall;

    private List<Tile> deadTiles;

    public Tile mostRecentDiscard;
    private static int MAXTILECOUNT = 144;
    private GameState state;

    [SerializeField]
    private List<MahjongPlayerBase> players;// = new List<MahjongPlayerBase>(4);

    private MahjongPlayerBase dealer, previousPlayer, currentPlayer, nextPlayer;

    private int round, numRounds;
    public GameObject InitialTileParent;
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
        board = new List<Tile>(MAXTILECOUNT);
        wall = new List<Tile>();
        deadTiles = new List<Tile>();
        // dealer = players[0];
        // players = new MahjongPlayerBase[4];

        //put all the tiles into board structure;
        foreach (Tile tile in InitialTileParent.transform.GetComponentsInChildren<Tile>())
        {
            board.Add(tile);
        }




        BoardSetup();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void BoardSetup()
    {
        Debug.Log("Shuffling Board");
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

        RollDice();
    }

    void RollDice()
    {
        System.Random rand = new System.Random();
        int dieRollResult = (rand.Next(2, 13) - 1) % 4;
        Debug.Log(dieRollResult);
        dealer = players[dieRollResult];

        //create walls
        int wallIndex = (MAXTILECOUNT / 4) * dieRollResult;
        wall.AddRange(board.GetRange(wallIndex, MAXTILECOUNT - wallIndex));
        wall.AddRange(board.GetRange(0, wallIndex));


        List<Tile> distributedTiles = wall.GetRange(0, 65);
        wall.RemoveRange(0, 65);
        dealer.GetComponent<MahjongPlayerBase>().AddTile(distributedTiles[0]);
        // distributedTiles.Remove(wall[0]);
        for (int i = 1; i < 65; i++)
        {
            players[(dieRollResult + ((i - 1) / 16)) % 4].AddTile(distributedTiles[i]);
        }

        StartCoroutine(Wait());

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
            player.VisuallySortTiles();
        }

        currentPlayer = dealer;
        nextPlayer = players[(dieRollResult + 1) % players.Count];

        // state = GameState.playing;
        // StartCoroutine(TakeTurn(currentPlayer));
    }

    IEnumerator TakeTurn(MahjongPlayerBase player)
    {
        if (player.currentDecision != decision.pass)
        {
            player.StealTile();
        }
        else
        {
            player.DrawTile();
            yield return new WaitForSeconds(1);
            while (player.currentDrawnTile().tileType == suit.flower)
            {
                player.DrawFlowerTile();
                yield return new WaitForSeconds(1);
            }
            // current implementation doesnt add the drawn tile to hand afterwards
        }


        while (player.IsWaiting())
        { }




    }

    IEnumerator BetweenTurn()
    {
        yield return new WaitForSeconds(5);

        MahjongPlayerBase next = players[0];

        foreach (MahjongPlayerBase player in players)
        {
            if (player != currentPlayer || player != nextPlayer)
            {
                //only one person can possibly do kang at any given time
                //so break the loop and give them the turn
                if (player.currentDecision == decision.kang)
                {
                    nextPlayer = player;
                    break;
                }
                //same with pong, but kang get priority so its lower
                if (player.currentDecision == decision.pong)
                {
                    nextPlayer = player;
                    break;
                }
                //both a chow and a pass result in the next person taking their turn anyway
                //so we do not check it here
            }
        }
        previousPlayer = currentPlayer;
        currentPlayer = nextPlayer;
        nextPlayer = players[(players.IndexOf(nextPlayer) + 1) % players.Count];

        StartCoroutine(TakeTurn(currentPlayer));
    }

    public void FinishGame()
    {
        state = GameState.finished;
    }


    IEnumerator Wait()
    {
        yield return new WaitForSeconds(10);

    }

}
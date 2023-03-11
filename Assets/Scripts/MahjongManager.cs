using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { setup, playing, finished };

public class MahjongManager : MonoBehaviour
{
    public static MahjongManager mahjongManager;
    private List<Tile> board;
    public List<Tile> wall;

    private List<Tile> deadTiles;

    public Tile mostRecentDiscard;
    private static int MAXTILECOUNT = 144;
    private GameState state;

    private List<MahjongPlayerBase> players = new List<MahjongPlayerBase>(4);

    private MahjongPlayerBase dealer, previousPlayer, currentPlayer, nextPlayer;

    private int round, numRounds;
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
        state = GameState.setup;
        board = new List<Tile>(MAXTILECOUNT);
        wall = new List<Tile>();
        deadTiles = new List<Tile>();
        // players = new MahjongPlayerBase[4];


        dealer = players[1];

        BoardSetup();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void BoardSetup()
    {
        System.Random rand = new System.Random();
        int n = board.Count;
        while (n > 1)
        {
            int k = rand.Next(n--);
            Tile temp = board[n];
            board[n] = board[k];
            board[k] = temp;
        }

        RollDice();
    }

    void RollDice()
    {



        state = GameState.playing;

        currentPlayer = players[0];
        nextPlayer = players[1];
        StartCoroutine(TakeTurn(currentPlayer));
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

}

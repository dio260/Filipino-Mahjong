using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { setup, playing, mahjong };

public class MahjongManager : MonoBehaviour
{
    private List<Tile> board;
    private static int MAXTILECOUNT = 144;
    private GameState state;

    private MahjongPlayerBase[] players = new MahjongPlayerBase[4];

    private MahjongPlayerBase dealer, currentPlayerTurn;

    private int round, numRounds;
    // Start is called before the first frame update
    void Awake()
    {
        state = GameState.setup;
        board = new List<Tile>(MAXTILECOUNT);
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

    }

    public void FinishGame()
    {
        state = GameState.mahjong;
    }

}

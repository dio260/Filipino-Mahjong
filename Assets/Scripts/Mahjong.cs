using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { setup, playing, mahjong };
public class Mahjong : MonoBehaviour
{
    private Tile[] board;
    private GameState state;

    private Player[] players;
    // Start is called before the first frame update
    void Awake()
    {
        board = new Tile[144];
        players = new Player[4];
        state = GameState.setup;
        BoardSetup();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void BoardSetup()
    {
        System.Random rand = new System.Random();
        int n = board.Length;
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

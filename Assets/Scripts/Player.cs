using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    int[,] hand = new int[,] {
        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
    };
    //private List<Tile> hand;
    //private Tile drawn;
    private Mahjong gameManager;
    //private List<Tile> hand;
    // Start is called before the first frame update
    private int score;
    void Awake()
    {
        //hand = new List<Tile>(16);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Finish()
    {
        //gameManager.FinishGame();
    }
}

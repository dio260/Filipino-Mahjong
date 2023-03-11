using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MahjongPlayerBase : MonoBehaviour
{
    // int[,] hand = new int[,] {
    //     {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
    //     {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
    //     {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
    //     {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
    // };
    private List<Tile> hand;
    //private Tile drawn;
    private MahjongManager gameManager;
    //private List<Tile> hand;
    // Start is called before the first frame update
    private int score;
    private static int maxHandSize = 17;

    private bool win;
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
    void CalculateHandOptions()
    {

    }
    void ActivateWin()
    {
        
    }
    void SortTilesBySuit()
    {
        //first get suits
        List<Tile> balls;
        List<Tile> sticks;
        List<Tile> chars;

        foreach(Tile tile in hand)
        {
            switch(tile.tileType)
            {
                case suit.ball:
                break;
                case suit.stick:
                break;
                case suit.character:
                break;

            }
        }
    }
}

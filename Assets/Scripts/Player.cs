using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Tile[] hand;
    private Tile drawn;
    private Mahjong gameManager;
    //private List<Tile> hand;
    // Start is called before the first frame update
    void Awake()
    {
        Tile[] hand = new Tile[16];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Finish()
    {
        gameManager.FinishGame();
    }
}

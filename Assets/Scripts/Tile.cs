using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum suit { stick, ball, character, flower };
public class Tile : MonoBehaviour
{
    public suit tileType;
    public int number;
    public bool onBoard, closed, open;

    public TMP_Text debugText;

    //to be used in hand win algo
    public bool winning;
    // Start is called before the first frame update
    void Awake()
    {
        if(tileType == suit.flower)
        {
            gameObject.name = tileType.ToString();
            debugText.text = tileType.ToString();

        }
        else
        {
            gameObject.name = number + " " + tileType.ToString();
            debugText.text = number + " " + tileType.ToString();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

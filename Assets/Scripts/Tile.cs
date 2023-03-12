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
            debugText.text = tileType.ToString();
        }
        else
        {
            debugText.text = number + '\n' + tileType.ToString();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

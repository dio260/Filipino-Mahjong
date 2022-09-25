using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum type { stick, ball, character };
public class Tile : MonoBehaviour
{
    public type tileType;
    public int number;
    public bool onBoard;
    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

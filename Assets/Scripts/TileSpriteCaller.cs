using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSpriteCaller : MonoBehaviour
{
    // Start is called before the first frame update
    public static TileSpriteCaller sprites;
    public Sprite[] ballsprites;
    public Sprite[] charsprites;
    public Sprite[] sticksprites;
    public Sprite[] winddragonsprites;
    public Sprite[] flowerset1;
    public Sprite[] flowerset2;

    int flowerIndex1, flowerIndex2;

    void Awake()
    {
        // Debug.Log("Awake called on spritecaller");
        if(sprites != null && sprites != this)
        {
            Destroy(sprites.gameObject);
        }
        else
        {
            sprites = this;
        }
        flowerIndex1 = 0;
        flowerIndex2 = 0;
    }

    // Update is called once per frame
    public Sprite GetFlower1()
    {
        // Debug.Log("flower set 1: " + flowerset1.Length + " index: " + flowerIndex1);
        Sprite flower = flowerset1[flowerIndex1];
        flowerIndex1 += 1;
        return flower;
    }
    // Update is called once per frame
    public Sprite GetFlower2()
    {
        // Debug.Log("flower set 1: " + flowerset2.Length + " index: " + flowerIndex2);
        Sprite flower = flowerset2[flowerIndex2];
        flowerIndex2 += 1;
        return flower;
    }
}

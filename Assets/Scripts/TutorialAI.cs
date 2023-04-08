using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialAI : MahjongPlayerBase
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void ForceDiscard()
    {
        discardChoice = drawnTile;
        DeclareDiscard();
    }
}

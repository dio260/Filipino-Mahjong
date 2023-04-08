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

    public override void DrawTile()
    {
        drawnTile = TutorialManager.tutorial.wall[0];
        TutorialManager.tutorial.wall.RemoveAt(0);
    }
    public override void DiscardTile()
    {
        discardChoice.transform.localPosition = new Vector3(0, 0.05f, 0.05f);
        TutorialManager.tutorial.mostRecentDiscard = discardChoice;
        closedHand.RemoveAt(closedHand.IndexOf(discardChoice));
        discardChoice.transform.parent = null;
        discardChoice.owner = null;
        drawnTile = null;
        discardChoice = null;
    }
}

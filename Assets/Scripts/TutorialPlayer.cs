using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPlayer : HumanPlayer
{
    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();
        todasButton.onClick.AddListener(TutorialManager.tutorial.ResetNextButton);
        pongButton.onClick.AddListener(TutorialManager.tutorial.ResetNextButton);
        kangButton.onClick.AddListener(TutorialManager.tutorial.ResetNextButton);
        chowButton.onClick.AddListener((TutorialManager.tutorial.ResetNextButton));
        passButton.onClick.AddListener(TutorialManager.tutorial.ResetNextButton);
        discardButton.onClick.AddListener(TutorialManager.tutorial.ResetNextButton);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

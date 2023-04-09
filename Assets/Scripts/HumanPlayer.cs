using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class HumanPlayer : MahjongPlayerBase
{
    //stuff to be moved to HumanPlayer child class
    public Camera playerCam;
    public GameObject playerCanvas;
    protected Button sortButton, passButton, chowButton, pongButton, kangButton, todasButton, discardButton;
    public TMP_Text debugText, tileText, helpUIText;
    public Image tileImage1, tileImage2;
    public Button exitButton;
    public RectTransform HelpUI;
    protected bool HelpOpen;
    protected Vector3 camRotation, camPosition;
    protected GameObject tileSwap;

    public override void Awake()
    {
        base.Awake();
        //move to Humanplayer
        playerCam = GetComponentInChildren<Camera>();
        camRotation = playerCam.transform.eulerAngles;
        camPosition = playerCam.transform.localPosition;

        playerCanvas = GetComponentInChildren<Canvas>().gameObject;
        sortButton = playerCanvas.transform.Find("Sort Tiles").GetComponent<Button>();
        passButton = playerCanvas.transform.Find("Pass").GetComponent<Button>();
        discardButton = playerCanvas.transform.Find("Discard Button").GetComponent<Button>();
        pongButton = playerCanvas.transform.Find("Pong").GetComponentInChildren<Button>();
        kangButton = playerCanvas.transform.Find("Kang").GetComponentInChildren<Button>();
        chowButton = playerCanvas.transform.Find("Chow").GetComponentInChildren<Button>();
        todasButton = playerCanvas.transform.Find("Todas").GetComponentInChildren<Button>();


        sortButton.onClick.AddListener(() => VisuallySortTiles());
        todasButton.onClick.AddListener(() => DeclareWin());
        pongButton.onClick.AddListener(() => MakeDecision(decision.pong));
        kangButton.onClick.AddListener(() => MakeDecision(decision.kang));
        chowButton.onClick.AddListener(() => MakeDecision(decision.chow));
        passButton.onClick.AddListener(() => MakeDecision(decision.pass));
        discardButton.onClick.AddListener(() => DeclareDiscard());


        // //button debugs
        // sortButton.onClick.AddListener(() => DebugButtonClick(sortButton.gameObject.name));
        // todasButton.onClick.AddListener(() => DebugButtonClick(todasButton.gameObject.name));
        // pongButton.onClick.AddListener(() => DebugButtonClick(pongButton.gameObject.name));
        // kangButton.onClick.AddListener(() => DebugButtonClick(kangButton.gameObject.name));
        // chowButton.onClick.AddListener(() => DebugButtonClick(chowButton.gameObject.name));
        // passButton.onClick.AddListener(() => DebugButtonClick(passButton.gameObject.name));
        // discardButton.onClick.AddListener(() => DebugButtonClick(discardButton.gameObject.name));

        FlipUI();
        discardButton.gameObject.SetActive(false);
        tileImage1.enabled = false;
        tileImage2.enabled = false;
        exitButton.onClick.AddListener(LocalSceneLoader.sceneLoader.LoadMenu);
        exitButton.gameObject.SetActive(false);
        playerCanvas.SetActive(false);
    }

    // public void DebugButtonClick(string button)
    // {
    //     Debug.Log(button + "pressed");
    // }

    // Update is called once per frame
    public virtual void Update()
    {
        if (MahjongManager.mahjongManager.GetGameState() == GameState.playing && (!networked || (networked && GetComponent<NetworkedPlayer>().photonView.IsMine)))
        {

            //bird's eye board view;
            if (Input.GetKey(KeyCode.Space))
            {
                playerCam.transform.rotation = Quaternion.Euler(Vector3.left * -90);
                playerCam.transform.position = new Vector3(0, 1.5f, 0);
                playerCanvas.SetActive(false);
            }
            else
            {
                playerCam.transform.rotation = Quaternion.Euler(camRotation);
                playerCam.transform.localPosition = camPosition;
                playerCanvas.SetActive(true);
            }

            //UI being set active

            //setting buttons active when conditions are fulfilled
            if (discardChoice != null)
            {
                discardButton.gameObject.SetActive(true);
            }
            else
            {
                discardButton.gameObject.SetActive(false);
            }

            if (currentState == PlayerState.deciding && currentDecision == decision.none)
            {
                passButton.gameObject.SetActive(true);
            }
            else
            {
                passButton.gameObject.SetActive(false);
            }

            switch (currentState)
            {
                case PlayerState.discarding:
                    if (drawnTile != null)
                    {
                        tileText.text = "Drawn:    Discard:";
                    }
                    else
                    {
                        tileText.text = "Selected Discard:";
                    }
                    break;
                case PlayerState.waiting:
                    tileText.text = "";
                    break;
                case PlayerState.deciding:
                    tileText.text = "Selected Meld Tiles:";
                    break;
                default:
                    tileText.text = "";
                    break;

            }


            //opening the help UI
            if (Input.GetKeyDown(KeyCode.Tab) && (HelpUI.sizeDelta.y == 60 || HelpUI.sizeDelta.y == 500))
            {
                if (!HelpOpen)
                {
                    helpUIText.text = "Exit Game ->";
                    exitButton.gameObject.SetActive(true);
                    StartCoroutine(OpenHelp());
                }
                else
                {
                    helpUIText.text = "Press Tab to open help";
                    exitButton.gameObject.SetActive(false);
                    StartCoroutine(CloseHelp());
                }
            }


            // if (!networked || (networked && GetComponent<NetworkedPlayer>().photonView.IsMine))
            // {
            // Debug.Log(new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0));
            Vector3 mouseWorldPos = playerCam.ViewportToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, playerCam.nearClipPlane));

            Ray mouseWorldRay = playerCam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(mouseWorldRay.origin, mouseWorldRay.direction, Color.blue * Vector3.Distance(transform.position, closedHandParent.position), 0f);
            if (Physics.Raycast(mouseWorldRay, out RaycastHit hit, Vector3.Distance(transform.position, closedHandParent.position))
                && hit.transform.GetComponent<Tile>())
            // if (Physics.Raycast(mouseWorldRay, out RaycastHit hit, 5f))
            {
                // Debug.Log("touched tile at " + hit.point);
                if (closedHandParent.Find(hit.transform.name) != null)
                {
                    if (Input.GetMouseButtonDown(0) && currentState != PlayerState.waiting)
                    {
                        switch (currentState)
                        {
                            case PlayerState.deciding:

                                if (this != MahjongManager.mahjongManager.currentPlayer)
                                {
                                    if (networked)
                                    {
                                        hit.transform.GetComponent<Tile>().TileRPCCall("SelectForMeld");
                                    }
                                    else
                                    {
                                        SelectMeldTile(hit.transform.GetComponent<Tile>());
                                    }
                                }
                                break;
                            case PlayerState.discarding:
                                if (networked)
                                {
                                    hit.transform.GetComponent<Tile>().TileRPCCall("PlayerDiscard");
                                }
                                else
                                {
                                    SelectDiscardTile(hit.transform.GetComponent<Tile>());
                                    // discardChoice = hit.transform.GetComponent<Tile>();
                                }
                                break;
                        }

                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        SwapTilePosition(hit.transform.gameObject);
                    }

                }
                // }
            }

        }
        else if (MahjongManager.mahjongManager.GetGameState() == GameState.finished)
        {
            if (HelpOpen)
            {
                StartCoroutine(CloseHelp());
            }
            helpUIText.text = "Exit Game ->";
            exitButton.gameObject.SetActive(true);
        }


    }

    protected void SwapTilePosition(GameObject tile)
    {
        if (tileSwap != null && tile != tileSwap)
        {
            Vector3 newPos = new Vector3(tile.transform.position.x, tileSwap.transform.position.y, tile.transform.position.z);
            tile.transform.position = new Vector3(tileSwap.transform.position.x, tile.transform.position.y, tileSwap.transform.position.z);
            tileSwap.transform.position = newPos;
            tileSwap = null;
            return;
        }

        if (tile == tileSwap)
        {
            tileSwap = null;
            return;
        }

        tileSwap = tile;
    }

    protected override void MakeDecision(decision dec)
    {
        base.MakeDecision(dec);
        FlipUI();
    }

    public override void CalculateHandOptions()
    {
        // base.CalculateHandOptions();
        Debug.Log("Calculating Hand Options");

        canPong = false;
        canKang = false;
        canChow = false;
        canWin = false;

        Tile discard = MahjongManager.mahjongManager.mostRecentDiscard;

        //most priority is a winning hand, takes precedence
        //edge case, closed hand size is 1, so waiting on the last pair
        if (closedHand.Count == 1 &&
        closedHand[0].number == discard.number &&
        closedHand[0].tileType == discard.tileType)
        {
            //GUI stuff probably needs to be moved to Human as well
            canWin = true;
            // StartCoroutine(todasButton.GetComponentInParent<ButtonFlip>().Flip());
        }
        else if (CalculateSevenPairs())
        {
            canWin = true;
            // StartCoroutine(todasButton.GetComponentInParent<ButtonFlip>().Flip());
        }
        else if (CalculateNormalWin())
        {
            canWin = true;
            // StartCoroutine(todasButton.GetComponentInParent<ButtonFlip>().Flip());
        }



        pongMeld = new List<Tile> { discard };
        chowMeldLeft = new List<Tile> { discard };
        chowMeldMiddle = new List<Tile> { discard };
        chowMeldRight = new List<Tile> { discard };
        kangMeld = new List<Tile> { discard };

        //auto calculate kang as a bandaid
        foreach (Tile tile in closedHand)
        {
            //need the last equivalency check to due to the simultaneous method call
            if (tile.tileType == discard.tileType && tile.number == discard.number && tile != discard)
            {
                kangMeld.Add(tile);
            }
        }

        foreach (Tile tile in selectedTiles)
        {
            if (tile.tileType == discard.tileType)
            {
                // Debug.Log("Passed suit check");

                if (tile.number == discard.number)
                {
                    if (pongMeld.Count < 3)
                        pongMeld.Add(tile);
                }

                //take advantage of the subarrays being sorted numerically for chow
                //consider three types of chow melds
                //outermost number
                if (tile.number == discard.number - 2 && !HasNumber(chowMeldLeft, tile.number))
                {
                    chowMeldLeft.Add(tile);
                }
                if (tile.number == discard.number + 2 && !HasNumber(chowMeldRight, tile.number))
                {
                    chowMeldRight.Add(tile);
                }

                if (tile.number == discard.number - 1)
                {
                    if (!HasNumber(chowMeldLeft, tile.number))
                        chowMeldLeft.Add(tile);
                    if (!HasNumber(chowMeldMiddle, tile.number))
                        chowMeldMiddle.Add(tile);
                }

                if (tile.number == discard.number + 1)
                {
                    if (!HasNumber(chowMeldRight, tile.number))
                        chowMeldRight.Add(tile);
                    if (!HasNumber(chowMeldMiddle, tile.number))
                        chowMeldMiddle.Add(tile);
                }
            }
        }

        if (pongMeld.Count == 3)
        {
            canPong = true;
        }
        else
        {
            canPong = false;
        }
        if (kangMeld.Count == 4)
        {
            canKang = true;
        }
        else
        {
            canKang = false;
        }
        if ((chowMeldLeft.Count == 3
        || chowMeldMiddle.Count == 3
        || chowMeldRight.Count == 3) &&
        (MahjongManager.mahjongManager.GetPlayers().IndexOf(MahjongManager.mahjongManager.previousPlayer) + 1) %
        MahjongManager.mahjongManager.GetPlayers().Count == MahjongManager.mahjongManager.GetPlayers().IndexOf(this))
        {
            Debug.Log("Prev player index: " + MahjongManager.mahjongManager.GetPlayers().IndexOf(MahjongManager.mahjongManager.previousPlayer)
            + "\nMy Index: " + MahjongManager.mahjongManager.GetPlayers().IndexOf(this));
            canChow = true;
        }
        else
        {
            canChow = false;
        }

        if (canPong && !pongButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(pongButton.GetComponentInParent<ButtonFlip>().Flip());
        }
        else if (!canPong && pongButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(pongButton.GetComponentInParent<ButtonFlip>().Flip());
        }
        if (canKang && !kangButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(kangButton.GetComponentInParent<ButtonFlip>().Flip());
        }
        else if (!canKang && kangButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(kangButton.GetComponentInParent<ButtonFlip>().Flip());
        }
        if (canChow && !chowButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(chowButton.GetComponentInParent<ButtonFlip>().Flip());
        }
        else if (!canChow && chowButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(chowButton.GetComponentInParent<ButtonFlip>().Flip());
        }

        if (canWin && !todasButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(todasButton.GetComponentInParent<ButtonFlip>().Flip());
        }
        else if (!canWin && todasButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(todasButton.GetComponentInParent<ButtonFlip>().Flip());
        }
    }

    [PunRPC]
    public override void ArrangeTiles()
    {
        // Debug.Log("Calling Human Override arrange");

        if (discardChoice != null)
        {
            discardChoice.transform.position -= Vector3.up * 0.025f;
        }

        if (selectedTiles.Count != 0)
        {
            foreach (Tile tile in selectedTiles)
            {
                discardChoice.transform.position -= Vector3.up * 0.025f;
            }
        }

        base.ArrangeTiles();

        if (discardChoice != null)
        {
            discardChoice.transform.position += Vector3.up * 0.025f;
        }

        if (selectedTiles.Count != 0)
        {
            foreach (Tile tile in selectedTiles)
            {
                discardChoice.transform.position += Vector3.up * 0.025f;
            }
        }
    }

    public override void VisuallySortTiles()
    {
        // Debug.Log("Calling Human Override Visual Tilesort");
        if (discardChoice != null)
        {
            discardChoice.transform.position -= Vector3.up * 0.025f;
        }

        if (selectedTiles.Count != 0)
        {
            foreach (Tile tile in selectedTiles)
            {
                discardChoice.transform.position -= Vector3.up * 0.025f;
            }
        }

        base.VisuallySortTiles();

        if (discardChoice != null)
        {
            discardChoice.transform.position += Vector3.up * 0.025f;
        }

        if (selectedTiles.Count != 0)
        {
            foreach (Tile tile in selectedTiles)
            {
                discardChoice.transform.position += Vector3.up * 0.025f;
            }
        }
    }



    public void FlipWinButton()
    {
        StartCoroutine(todasButton.GetComponentInParent<ButtonFlip>().Flip());
    }

    public void SelectDiscardTile(Tile clicked)
    {

        if (discardChoice != null)
        {
            discardChoice.transform.position -= Vector3.up * 0.025f;
            if (discardChoice == clicked)
            {
                Debug.Log("Deselected " + clicked.ToString() + " as discard");
                discardChoice = null;
                tileImage2.sprite = null;
                tileImage2.enabled = false;
                return;
            }
        }

        Debug.Log("Selected " + clicked.ToString() + " to discard");


        discardChoice = clicked;
        clicked.transform.position += Vector3.up * 0.025f;

        tileImage2.enabled = true;
        tileImage2.sprite = discardChoice.tileImage.sprite;
    }

    public void SelectMeldTile(Tile clicked)
    {
        Debug.Log("Selected " + clicked.ToString() + " for a Meld");

        if (selectedTiles.Contains(clicked))
        {
            selectedTiles.Remove(clicked);
            clicked.transform.position -= Vector3.up * 0.025f;
        }
        else
        {
            if (selectedTiles.Count < 2)
            {
                selectedTiles.Add(clicked);
                clicked.transform.position += Vector3.up * 0.025f;
            }
        }

        if (selectedTiles.Count > 0)
        {
            tileImage1.enabled = true;
            tileImage1.sprite = null;
            tileImage2.sprite = null;
            if (selectedTiles.Count == 1)
            {
                tileImage2.enabled = false;
                tileImage1.sprite = selectedTiles[0].tileImage.sprite;
            }
            else
            {
                tileImage2.enabled = true;
                tileImage1.sprite = selectedTiles[0].tileImage.sprite;
                tileImage2.sprite = selectedTiles[1].tileImage.sprite;
            }

            CalculateHandOptions();
        }
    }

    public override void AddDrawnTileToClosedHand()
    {
        // tileText.text = "Drawn:    Discard:";
        tileImage1.enabled = true;
        tileImage1.sprite = drawnTile.tileImage.sprite;
        base.AddDrawnTileToClosedHand();
    }

    public override void StealTile()
    {
        base.StealTile();
        // tileText.text = "Selected Discard:";
        tileImage2.enabled = true;
        tileImage1.enabled = false;
        // tileImage2.sprite = drawnTile.tileImage.sprite;
    }

    public void FlipUI()
    {
        if (chowButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(chowButton.transform.parent.GetComponent<ButtonFlip>().Flip());
        }
        if (pongButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(pongButton.transform.parent.GetComponent<ButtonFlip>().Flip());
        }
        if (kangButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(kangButton.transform.parent.GetComponent<ButtonFlip>().Flip());
        }
        if (todasButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(todasButton.transform.parent.GetComponent<ButtonFlip>().Flip());
        }
        discardButton.gameObject.SetActive(false);
    }

    public void DebugClearHand()
    {
        closedHand = new List<Tile>();
    }

    public IEnumerator OpenHelp()
    {
        while (HelpUI.rect.height < 500)
        {
            HelpUI.sizeDelta += Vector2.up;

            if (HelpUI.sizeDelta.y > 69)
            {
                if (HelpUI.GetChild(1).localScale.y < 1)
                {
                    HelpUI.GetChild(1).localScale += Vector3.up * 0.01f;
                    if (HelpUI.GetChild(1).localScale.y > 1)
                    {
                        HelpUI.GetChild(1).localScale = Vector3.one;
                    }
                }
            }
            if (HelpUI.sizeDelta.y > 160)
            {
                if (HelpUI.GetChild(2).localScale.y < 1)
                {
                    HelpUI.GetChild(2).localScale += Vector3.up * 0.0028f;
                    if (HelpUI.GetChild(2).localScale.y > 1)
                    {
                        HelpUI.GetChild(2).localScale = Vector3.one;
                    }
                }
            }
            yield return new WaitForSeconds(0.00001f);
        }
        HelpOpen = true;
        // Debug.Log(HelpUI.rect.height);
    }
    public IEnumerator CloseHelp()
    {
        while (HelpUI.rect.height > 60)
        {
            HelpUI.sizeDelta -= Vector2.up;

            if (HelpUI.sizeDelta.y < 155)
            {
                if (HelpUI.GetChild(1).localScale.y > 0)
                {
                    HelpUI.GetChild(1).localScale -= Vector3.up * 0.012f;
                    if (HelpUI.GetChild(1).localScale.y < 0)
                    {
                        HelpUI.GetChild(1).localScale = Vector3.one + Vector3.down;
                    }
                }
            }
            if (HelpUI.sizeDelta.y < 485)
            {
                if (HelpUI.GetChild(2).localScale.y > 0)
                {
                    HelpUI.GetChild(2).localScale -= Vector3.up * 0.0028f;
                    if (HelpUI.GetChild(2).localScale.y < 0)
                    {
                        HelpUI.GetChild(2).localScale = Vector3.one + Vector3.down;
                    }
                }
            }
            yield return new WaitForSeconds(0.00001f);
        }
        HelpOpen = false;
        // Debug.Log(HelpUI.rect.height);

    }
}

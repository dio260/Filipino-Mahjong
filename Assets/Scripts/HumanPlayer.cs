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
    GameObject playerCanvas;
    Button sortButton, passButton, chowButton, pongButton, kangButton, todasButton, discardButton;
    public TMP_Text debugText;
    Vector3 camRotation, camPosition;
    GameObject tileSwap;

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


        //button debugs
        sortButton.onClick.AddListener(() => DebugButtonClick(sortButton.gameObject.name));
        todasButton.onClick.AddListener(() => DebugButtonClick(todasButton.gameObject.name));
        pongButton.onClick.AddListener(() => DebugButtonClick(pongButton.gameObject.name));
        kangButton.onClick.AddListener(() => DebugButtonClick(kangButton.gameObject.name));
        chowButton.onClick.AddListener(() => DebugButtonClick(chowButton.gameObject.name));
        passButton.onClick.AddListener(() => DebugButtonClick(passButton.gameObject.name));
        discardButton.onClick.AddListener(() => DebugButtonClick(discardButton.gameObject.name));

        HideUI();
        discardButton.gameObject.SetActive(false);
    }

    public void DebugButtonClick(string button)
    {
        Debug.Log(button + "pressed");
    }

    // Update is called once per frame
    void Update()
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

            //setting buttons active when conditions are fulfilled
            if (discardChoice != null)
            {
                discardButton.gameObject.SetActive(true);
            }
            else
            {
                discardButton.gameObject.SetActive(false);
            }

            if (currentState == PlayerState.deciding)
            {
                passButton.gameObject.SetActive(true);
            }
            else
            {
                passButton.gameObject.SetActive(false);
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
                                Debug.Log("Selected " + hit.transform.GetComponent<Tile>().ToString() + " to discard");
                                if (networked)
                                {
                                    hit.transform.GetComponent<Tile>().TileRPCCall("PlayerDiscard");
                                }
                                else
                                {
                                    discardChoice = hit.transform.GetComponent<Tile>();
                                }
                                break;
                        }

                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        Debug.Log("holding tile to swap");
                        SwapTilePosition(hit.transform.gameObject);
                    }

                }
                // }
            }

        }


    }

    void SwapTilePosition(GameObject tile)
    {
        if (tileSwap != null && tile != tileSwap)
        {
            Vector3 newPos = tile.transform.position;
            tile.transform.position = tileSwap.transform.position;
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

        //auto calculate kang as a bandaid
        foreach (Tile tile in closedHand)
        {
            if (tile.tileType == discard.tileType && tile.number == discard.number)
            {
                kangMeld.Add(tile);
            }
        }

        pongMeld = new List<Tile> { discard };
        chowMeldLeft = new List<Tile> { discard };
        chowMeldMiddle = new List<Tile> { discard };
        chowMeldRight = new List<Tile> { discard };
        kangMeld = new List<Tile> { discard };

        foreach (Tile tile in selectedTiles)
        {
            if (tile.tileType == discard.tileType)
            {
                Debug.Log("Passed suit check");

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
                    Debug.Log("Tile is 2 lower");
                    chowMeldLeft.Add(tile);
                }
                if (tile.number == discard.number + 2 && !HasNumber(chowMeldRight, tile.number))
                {
                    Debug.Log("Tile is 2 higher");
                    chowMeldRight.Add(tile);
                }

                if (tile.number == discard.number - 1)
                {
                    Debug.Log("Tile is 1 lower");

                    if (!HasNumber(chowMeldLeft, tile.number))
                        chowMeldLeft.Add(tile);
                    if (!HasNumber(chowMeldMiddle, tile.number))
                        chowMeldMiddle.Add(tile);
                }

                if (tile.number == discard.number + 1)
                {
                    Debug.Log("Tile is 1 higher");

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
        if (chowMeldLeft.Count == 3
        || chowMeldMiddle.Count == 3
        || chowMeldRight.Count == 3 &&
        (MahjongManager.mahjongManager.GetPlayers().IndexOf(MahjongManager.mahjongManager.previousPlayer) + 1) %
        MahjongManager.mahjongManager.GetPlayers().Count == MahjongManager.mahjongManager.GetPlayers().IndexOf(this))
        {
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
        else if(!canKang && kangButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(kangButton.GetComponentInParent<ButtonFlip>().Flip());
        }
        if (canChow && !chowButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(chowButton.GetComponentInParent<ButtonFlip>().Flip());
        }
        else if(!canChow && chowButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(chowButton.GetComponentInParent<ButtonFlip>().Flip());
        }

        if (canWin && !todasButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(todasButton.GetComponentInParent<ButtonFlip>().Flip());
        }
        else if(!canWin && todasButton.transform.parent.GetComponent<ButtonFlip>().open)
        {
            StartCoroutine(todasButton.GetComponentInParent<ButtonFlip>().Flip());
        }
    }

    public void FlipWinButton()
    {
        StartCoroutine(todasButton.GetComponentInParent<ButtonFlip>().Flip());
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
            CalculateHandOptions();
        }
    }

    public void HideUI()
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
}

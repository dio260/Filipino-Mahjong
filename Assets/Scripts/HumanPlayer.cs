using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class HumanPlayer : MahjongPlayerBase
{
    //stuff to be moved to HumanPlayer child class
    Camera playerCam;
    public Button passButton, chowButton, pongButton, kangButton, todasButton;
    public TMP_Text debugText;

    GameObject tileSwap;

    // List<Tile> selectedTiles = new List<Tile>();
    void Start()
    {
        //move to Humanplayer
        playerCam = GetComponent<Camera>();
        todasButton.onClick.AddListener(() => DeclareWin());
        pongButton.onClick.AddListener(() => DeclarePong());
        kangButton.onClick.AddListener(() => DeclareKang());
        chowButton.onClick.AddListener(() => DeclareChow());
        passButton.onClick.AddListener(() => passTurn());

        chowButton.gameObject.SetActive(false);
        todasButton.gameObject.SetActive(false);
        pongButton.gameObject.SetActive(false);
        kangButton.gameObject.SetActive(false);
        passButton.gameObject.SetActive(false);

        debugText.text = currentState.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 hitPos;

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
                    Debug.Log("Selected Tile");
                    if (selectedTiles.Count < 4)
                    {
                        SelectTile(hit.transform.GetComponent<Tile>());
                    }

                }
                else if (Input.GetMouseButtonDown(1))
                {
                    // hitPos = hit.point
                    Debug.Log("holding tile to swap");
                    // hit.transform.position = new Vector3(mouseWorldRay.origin.x, mouseWorldRay.origin.y, hit.transform.position.z);
                    // hit.transform.position += new Vector3(Input.GetAxis("Mouse X") * Time.deltaTime * 1.75f, Input.GetAxis("Mouse Y") * Time.deltaTime * 1.75f, 0);
                    SwapTilePosition(hit.transform.gameObject);
                }

            }
        }

    }

    void SwapTilePosition(GameObject tile)
    {
        if(tileSwap != null && tile != tileSwap)
        {
            Vector3 newPos = tile.transform.position;
            tile.transform.position = tileSwap.transform.position;
            tileSwap.transform.position = newPos;
            tileSwap = null;
            return;
        }

        if(tile == tileSwap)
        {
            tileSwap = null;
            return;
        }
        
        tileSwap = tile;
    }

    protected override void CalculateHandOptions()
    {
        // base.CalculateHandOptions();

        Tile discard = MahjongManager.mahjongManager.mostRecentDiscard;

        pongMeld = new List<Tile> { discard };
        chowMeldLeft = new List<Tile> { discard };
        chowMeldMiddle = new List<Tile> { discard };
        chowMeldRight = new List<Tile> { discard };
        kangMeld = new List<Tile> { discard };
        foreach (Tile tile in selectedTiles)
        {
            if (tile.tileType == discard.tileType)
            {
                if (tile.number == discard.number)
                {
                    if (pongMeld.Count < 3)
                        pongMeld.Add(tile);
                    kangMeld.Add(tile);
                }

                //take advantage of the subarrays being sorted numerically for chow
                //consider three types of chow melds
                //outermost number
                if (tile.number == discard.number - 2 && !HasNumber(chowMeldLeft, discard.number))
                {
                    chowMeldLeft.Add(tile);
                }
                if (tile.number == discard.number + 2 && !HasNumber(chowMeldRight, discard.number))
                {
                    chowMeldRight.Add(tile);
                }

                if (tile.number == discard.number - 1)
                {
                    if (!HasNumber(chowMeldLeft, discard.number))
                        chowMeldLeft.Add(tile);
                    if (!HasNumber(chowMeldMiddle, discard.number))
                        chowMeldMiddle.Add(tile);
                }

                if (tile.number == discard.number + 1)
                {
                    if (!HasNumber(chowMeldRight, discard.number))
                        chowMeldRight.Add(tile);
                    if (!HasNumber(chowMeldMiddle, discard.number))
                        chowMeldMiddle.Add(tile);
                }
            }
        }

        if (pongMeld.Count == 3)
        {
            canPong = true;
        }
        if (kangMeld.Count == 4)
        {
            canKang = true;
        }
        if(chowMeldLeft.Count == 3
        || chowMeldMiddle.Count == 3
        || chowMeldRight.Count == 3)
        {
            canChow = true;
        }

        if (canPong)
        {
            pongButton.gameObject.SetActive(true);
        }
        if (canKang)
        {
            kangButton.gameObject.SetActive(true);
        }
        if (canChow)
        {
            chowButton.gameObject.SetActive(true);
        }
    }

    protected override void DeclarePong()
    {
        openHand.Add(MahjongManager.mahjongManager.mostRecentDiscard);
        openHand.AddRange(selectedTiles);

    }
    protected override void DeclareKang()
    {
        openHand.AddRange(kangMeld);
        canKang = false;
    }

    protected override void DeclareChow()
    {
        openHand.Add(MahjongManager.mahjongManager.mostRecentDiscard);
        openHand.AddRange(selectedTiles);
    }

    void SelectTile(Tile clicked)
    {
        if (selectedTiles.Contains(clicked))
        {
            selectedTiles.Remove(clicked);
        }
        else
        {
            selectedTiles.Add(clicked);
        }
    }
}

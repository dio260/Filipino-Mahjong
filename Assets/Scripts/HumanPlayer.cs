using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HumanPlayer : MahjongPlayerBase
{
    //stuff to be moved to HumanPlayer child class
    Camera playerCam;
    void Awake()
    {
        //move to Humanplayer
        playerCam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(Input.mousePosition);
        Vector3 mouseWorldPos = playerCam.ViewportToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, playerCam.nearClipPlane));

        Ray mouseWorldRay = playerCam.ScreenPointToRay(Input.mousePosition);
        // Debug.Log(mouseWorldRay.origin);
        if (Physics.Raycast(mouseWorldRay, out RaycastHit hit, Vector3.Distance(transform.position, closedHandParent.position)))
        {
            // Debug.Log("touched");
            if (closedHandParent.Find(hit.transform.name) != null && Input.GetMouseButton(0))
            {
                // Debug.Log("holding tile");
                hit.transform.position = new Vector3(mouseWorldRay.origin.x, mouseWorldRay.origin.y, hit.transform.position.z);
            }
        }
    }
}

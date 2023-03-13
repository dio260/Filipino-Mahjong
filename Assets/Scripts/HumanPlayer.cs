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
        Vector2 hitPos;

        // Debug.Log(new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0));
        Vector3 mouseWorldPos = playerCam.ViewportToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, playerCam.nearClipPlane));

        Ray mouseWorldRay = playerCam.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(mouseWorldRay.origin, mouseWorldRay.direction,Color.blue, 5f);
        if (Physics.Raycast(mouseWorldRay, out RaycastHit hit, Vector3.Distance(transform.position, closedHandParent.position))
            && hit.transform.GetComponent<Tile>())
        // if (Physics.Raycast(mouseWorldRay, out RaycastHit hit, 5f))
        {
            Debug.Log("touched tile at " + hit.point);
            if (closedHandParent.Find(hit.transform.name) != null && Input.GetMouseButton(0))
            {
                // hitPos = hit.point
                Debug.Log("holding tile, ");
                // hit.transform.position = new Vector3(mouseWorldRay.origin.x, mouseWorldRay.origin.y, hit.transform.position.z);
                hit.transform.position += new Vector3(Input.GetAxis("Mouse X") * Time.deltaTime * 1.75f, Input.GetAxis("Mouse Y") * Time.deltaTime * 1.75f, 0);
            }
        }
    }
}

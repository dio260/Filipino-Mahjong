using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanSpin : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        //spin
        transform.Rotate(Vector3.up * 0.25f);
    }
}

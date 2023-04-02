using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonFlip : MonoBehaviour
{
    public Transform buttonSide, backSide;
    public bool open;
    // Start is called before the first frame update
    void Awake()
    {
        open = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            StartCoroutine(Flip());
        }
    }

    public IEnumerator Flip()
    {
        if(Mathf.Abs(transform.rotation.eulerAngles.y) == 180)
        {
            // Debug.Log("rotating backside");

            while((Mathf.Abs(transform.rotation.eulerAngles.y)) >= 5)
            {
                transform.Rotate(Vector3.up * 5);
                yield return new WaitForSeconds(0.005f);
                if(Mathf.Abs(transform.rotation.eulerAngles.y) > 270)
                {
                    buttonSide.SetAsFirstSibling();
                }
            }
            transform.rotation = Quaternion.Euler(Vector3.zero);
        }
        else
        {
            // Debug.Log("rotating frontside");

            while((180 - Mathf.Abs(transform.rotation.eulerAngles.y)) >= 5)
            {
                // Debug.Log(transform.rotation.eulerAngles.y);
                transform.Rotate(Vector3.up * 5);
                yield return new WaitForSeconds(0.005f);
                if(transform.rotation.eulerAngles.y > 90)
                {
                    backSide.SetAsFirstSibling();
                }
            }
            transform.rotation = Quaternion.Euler(Vector3.up * 180);
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    // Start is called before the first frame update
    public float rollForce;
    void Start()
    {
        // StartCoroutine(DiceRoll());
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.D))
        {
            StartCoroutine(DiceRoll());
        }
    }
    IEnumerator DiceRoll()
    {
        // GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-1 ,1), 0.5f, Random.Range(-1 ,1)) * rollForce, ForceMode.Impulse);
        
        GetComponent<Rigidbody>().AddForce(Vector3.up * rollForce, ForceMode.Impulse);
        GetComponent<Rigidbody>().AddTorque((Vector3.forward + Vector3.down) * rollForce * 30, ForceMode.Impulse);
        yield return new WaitForSeconds(0.1f);
        while(GetComponent<Rigidbody>().velocity != Vector3.zero)
        {
            yield return new WaitForSeconds(0.1f);
        }
        if(Physics.Raycast(transform.position, Vector3.up, out RaycastHit hit, 1) && hit.collider.isTrigger)
            Debug.Log(int.Parse(hit.collider.gameObject.name));

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocalSceneLoader : MonoBehaviour
{
    public static LocalSceneLoader sceneLoader;
    // Start is called before the first frame update
    void Awake()
    {
        // DontDestroyOnLoad(gameObject);
        if (sceneLoader != null && sceneLoader != this)
        {
            Destroy(gameObject);
        }
        else
        {
            sceneLoader = this;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadTutorial()
    {
        Debug.Log("Tutorial Load Clicked");
        SceneManager.LoadScene("Tutorial");
    }

    public void LoadMenu()
    {
        Debug.Log("Main Menu Clicked");

        SceneManager.LoadScene("Main Menu");
    }
}

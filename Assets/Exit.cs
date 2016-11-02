using UnityEngine;
using System.Collections;

public class Exit : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer && Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

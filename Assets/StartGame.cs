using UnityEngine;
using System.Collections;

public class StartGame : SceneLoad {

    void Start()
    {
        Time.timeScale = 1.0f;
    }

	// Update is called once per frame
	void Update () {
	
        if(Input.GetButtonDown("Cir 1"))
        {
            Load("TestScene");
        }

	}
}

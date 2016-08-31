using UnityEngine;
using System.Collections;

public class FrameCounter : MonoBehaviour
{

    public int FPS = 60; 

    void Awake()
    {

        Application.targetFrameRate = FPS;

    }

    void OnGUI()
    {

        GUILayout.Label((1 / Time.deltaTime).ToString());

    }

}

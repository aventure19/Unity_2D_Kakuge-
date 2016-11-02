using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoad : MonoBehaviour
{

    public void Load(string Scene)
    {
        SceneManager.LoadScene(Scene);
        Debug.Log("Scene is changed.");
    }

}

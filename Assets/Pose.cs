using UnityEngine;
using System.Collections;

public class Pose : MonoBehaviour
{

    // ポーズ画面のUI
    public Canvas ca;

    // キャラクタのスクリプト
    ElementScript[] es = new ElementScript[2];
    public GameObject[] chara = new GameObject[2];

    void Start()
    {

        ca.enabled = false;

        es[0] = chara[0].GetComponent<ElementScript>();
        es[1] = chara[1].GetComponent<ElementScript>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ca.enabled = true;

            es[0].enabled = false;

            es[1].enabled = false;

            Time.timeScale = 0;
        }

        if (ca.enabled)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                ca.enabled = false;

                es[0].enabled = true;

                es[1].enabled = true;

                Time.timeScale = 1.0f;
            }
        }

    }
}

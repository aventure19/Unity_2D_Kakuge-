using UnityEngine;
using System.Collections;

public class ReturnToGame : MonoBehaviour {

    Pose p;
    public Camera c; 

	// Use this for initialization
	public void ReturntoGame () {

        p = c.GetComponent<Pose>();

        p.ca.enabled = false;

        p.es[0].enabled = true;

        p.es[1].enabled = true;

        Time.timeScale = 1.0f;

    }
}

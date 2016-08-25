using UnityEngine;
using System.Collections;

public class Hitbox_Base : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Attack")
        {

            Debug.Log("1");
            transform.root.GetComponent<ElementScript1>().isHited();

        }

    }

}

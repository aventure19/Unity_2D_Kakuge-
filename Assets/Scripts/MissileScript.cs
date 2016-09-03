using UnityEngine;
using System.Collections;

public class MissileScript : MonoBehaviour
{

    public float speed = 5;

    public Vector3 moveDirection = Vector3.right;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        transform.Translate(moveDirection * speed * Time.deltaTime);

    }

}

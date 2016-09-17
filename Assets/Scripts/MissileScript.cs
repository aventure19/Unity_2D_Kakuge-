using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class MissileScript : MonoBehaviour
{

    public float speed = 5;

    public Vector3 moveDirection = Vector3.right;

    private Collider col;

    // Use this for initialization
    void Start()
    {

        col = GetComponent<Collider>();

    }

    // Update is called once per frame
    void Update()
    {

        transform.Translate(moveDirection * speed * Time.deltaTime);

        if (col.enabled == false || Physics.Raycast(transform.position, moveDirection, 0.5f, 1 << LayerMask.NameToLayer("LandScape"))) Destroy(gameObject);

    }

}

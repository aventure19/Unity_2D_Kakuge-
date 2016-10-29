using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class MissileScript : MonoBehaviour
{

    public float speed = 5;

    public Vector3 moveDirection = Vector3.right;

    private Collider col;

    // 自分と敵のAttackDecisionScript
    public AttackDecisionScript myAds;
    public AttackDecisionScript enAds;

    // Use this for initialization
    void Start()
    {

        col = GetComponent<Collider>();

        myAds = GetComponent<AttackDecisionScript>();

    }

    // Update is called once per frame
    void Update()
    {

        transform.Translate(moveDirection * speed * Time.deltaTime);

        if (col.enabled == false || Physics.Raycast(transform.position, moveDirection, 0.5f, 1 << LayerMask.NameToLayer("LandScape"))) Destroy(gameObject);

        RaycastHit hit;

        if (Physics.Raycast(transform.position, moveDirection, out hit, 0.2f, 1 << LayerMask.NameToLayer("Tobidougu")))
        {
            if (hit.transform.tag == "Attack")
            {
                Destroy(gameObject);
                Destroy(hit.transform.gameObject);
            }
        }

    }

}

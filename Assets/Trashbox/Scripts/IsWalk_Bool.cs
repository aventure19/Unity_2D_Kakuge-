using UnityEngine;
using System.Collections;

public class IsWalk_Bool : MonoBehaviour
{

    public string up, right, left;

    private Animator animator;
    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(up) == true)
        {
            transform.position += transform.forward * 0.01f;
            animator.SetBool("IsWalk_Bool", true);
        }
        else {
            animator.SetBool("IsWalk_Bool", false);
        }
        if (Input.GetKey(right) == true)
        {
            transform.Rotate(0, 10, 0);
        }
        if (Input.GetKey(left))
        {
            transform.Rotate(0, -10, 0);
        }
    }
}
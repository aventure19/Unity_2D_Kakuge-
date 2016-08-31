using UnityEngine;
using System.Collections;

public class FollowingTarget : MonoBehaviour
{

    public GameObject objTarget;
    public Vector3 offset;

    void Start()
    {
        updatePostion();

    }

    void LateUpdate()
    {
        updatePostion();
    }

    void updatePostion()
    {
        Vector3 pos = objTarget.transform.localPosition;

        transform.localPosition = pos + offset;
    }
}


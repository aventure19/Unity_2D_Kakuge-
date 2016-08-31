using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour
{

    public Transform target1;
    public Transform target2;

    public Vector3 CameraOffset = new Vector3(0, 2.5f, -5f);

    float startDistance;

    void Start()
    {

        startDistance = Vector3.Distance(target1.position, target2.position);

    }

    void OnPreRender()
    {

        float t = Vector3.Distance(target1.position, target2.position) / startDistance;

        if (t < 1) t = 1;

        Camera.main.transform.position = (target1.position + target2.position) / 2 + CameraOffset * t;

    }

}

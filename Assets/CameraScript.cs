using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour
{

    public Transform target1;
    public Transform target2;
    public Vector3 CameraOffset = new Vector3(0, 2.5f, -5f);

    private float StartDistance;

    void Start()
    {

        StartDistance = Vector3.Distance(target1.position, target2.position);

    }

    void OnPreRender()
    {

        float CurrentDistance = Vector3.Distance(target1.position, target2.position);

        float t = CurrentDistance / StartDistance;

        Camera.main.transform.position = (target1.position + target2.position) / 2 + CameraOffset * t;

    }

}

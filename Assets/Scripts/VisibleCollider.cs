using UnityEngine;

[RequireComponent(typeof(Collider))]
public class VisibleCollider : MonoBehaviour
{

    // colliderと同じ形のメッシュを任意で入れる
    public Mesh mesh;

    // Gizmoの色
    public Color color = new Color(1, 0, 0, 0.2f);

    void OnDrawGizmos()
    {

        SphereCollider sc = GetComponent<SphereCollider>();
        BoxCollider bc = GetComponent<BoxCollider>();
        CapsuleCollider cc = GetComponent<CapsuleCollider>();

        Gizmos.color = color;

        if (sc)
        {

            Vector3 offset = transform.right * sc.center.x + transform.up * sc.center.y + transform.forward * sc.center.z;

            Vector3 size = Vector3.one * sc.radius * 2;

            if (sc.enabled) Gizmos.DrawMesh(mesh, transform.position + offset, transform.rotation, size);

        }

        else if (bc)
        {

            Vector3 offset = transform.right * bc.center.x + transform.up * bc.center.y + transform.forward * bc.center.z;

            Vector3 size = bc.size;

            if (bc.enabled) Gizmos.DrawMesh(mesh, transform.position + offset, transform.rotation, size);

        }

        else if (cc)
        {

            Vector3 offset = transform.right * cc.center.x + transform.up * cc.center.y + transform.forward * cc.center.z;

            Vector3 size = new Vector3(cc.radius / 0.5f, cc.height / 2, cc.radius / 0.5f);

            if (cc.enabled) Gizmos.DrawMesh(mesh, transform.position + offset, transform.rotation, size);

        }

    }

}

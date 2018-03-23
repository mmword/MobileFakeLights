using UnityEngine;
using System.Collections;

public class CameraRotateAround : MonoBehaviour
{
    public Transform Target;
    public Vector3 Axis = Vector3.up;
    public float Speed = 10f;

    void Update()
    {
        var p = Target.position;
        var rot = Quaternion.AngleAxis(Speed * Time.deltaTime,Axis);
        var v = transform.position - p;
        v = rot * v;
        transform.position = p + v;
        transform.rotation = Quaternion.LookRotation(-v.normalized);
    }
}
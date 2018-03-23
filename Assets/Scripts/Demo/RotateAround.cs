using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour {

    public Transform target;
    public float speed;
	
	// Update is called once per frame
	void Update () {
        transform.RotateAround(target.position, Vector3.up, speed * Time.deltaTime);
	}
}

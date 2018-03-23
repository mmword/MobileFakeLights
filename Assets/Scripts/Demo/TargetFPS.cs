using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFPS : MonoBehaviour {

    public int target = 60;

	// Use this for initialization
	void Start () {
        Application.targetFrameRate = target;

    }
	
}

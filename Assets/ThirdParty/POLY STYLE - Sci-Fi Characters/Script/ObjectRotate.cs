using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotate : MonoBehaviour {
	public float a;

	void Play () {
		a = 0.3F;
	}

	void Stop () {
		a = 0;
	}

	void Update () {
		transform.Rotate(0, a, 0);
	}
}

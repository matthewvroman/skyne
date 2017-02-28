using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentryManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider col) {
		if (col.gameObject.tag == "Bullet") {
			Destroy (this.gameObject);
		}
	}
}

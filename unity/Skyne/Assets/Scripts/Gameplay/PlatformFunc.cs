using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformFunc : MonoBehaviour
{

	MovingPlatformManager platMan;

	// Use this for initialization
	void Start ()
	{
		platMan = transform.parent.GetComponent<MovingPlatformManager> ();
	}


	void OnCollisionEnter (Collision col)
	{
		if (col.gameObject.tag == "Player")
		{
			col.transform.parent = gameObject.transform;
		}
	}

	void OnCollisionStay (Collision col)
	{
		if (col.gameObject.tag == "Player")
		{
			col.transform.parent = gameObject.transform;
		}
	}

	void OnCollisionExit (Collision col)
	{
		if (col.gameObject.tag == "Player")
		{
			col.transform.parent = null;
		}
	}

	void OnTriggerEnter (Collider col)
	{
		if (col.gameObject.tag == "Bullet")
		{
			Debug.Log ("Hello");
			platMan.SetPos (!platMan.GetSetPos ());
		}
	}
}

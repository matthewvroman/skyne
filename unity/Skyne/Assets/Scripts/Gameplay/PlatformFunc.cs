﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformFunc : MonoBehaviour
{

	MovingPlatformManager platMan;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		platMan = transform.parent.GetComponent<MovingPlatformManager> ();
	}

	void OnCollisionEnter (Collision col)
	{
		if (col.gameObject.tag == "Player")
		{
			col.transform.parent = gameObject.transform;
		}

		if (col.gameObject.tag == "Bullet")
		{
			//Debug.Log ("Hello");
			platMan.SetPos (!platMan.GetSetPos ());
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

	/*void OnTriggerStay (Collider col)
	{
		if (col.gameObject.tag == "Player")
		{
			col.transform.parent = gameObject.transform;
		}
	}

	void OnTriggerExit (Collider col)
	{
		if (col.gameObject.tag == "Player")
		{
			col.transform.parent = null;
		}
	} */
}

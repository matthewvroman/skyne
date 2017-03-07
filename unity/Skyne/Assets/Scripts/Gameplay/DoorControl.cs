using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorControl : MonoBehaviour
{
	GameObject door;

	Transform DoorClosedPos;
	Transform DoorOpenPos;

	public float closeSpeed;

	bool isOpen = true;

	public bool canClose;

	// Use this for initialization
	void Start ()
	{
		door = this.transform.FindChild ("Door").gameObject;
		DoorClosedPos = gameObject.transform.Find ("DoorClosePos");
		DoorOpenPos = gameObject.transform.Find ("DoorOpenPos");
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (canClose)
		{
			if (isOpen)
			{
				door.SetActive (false);
				door.transform.position = Vector3.Lerp (door.transform.position, DoorOpenPos.position, Time.deltaTime * closeSpeed);
			}
			else
			{
				door.SetActive (true);
				door.transform.position = Vector3.Lerp (door.transform.position, DoorClosedPos.position, Time.deltaTime * closeSpeed);
			}
		}
		else
		{
			door.SetActive (false);
		}
	}

	void OnTriggerEnter (Collider col)
	{
		if (col.gameObject.tag == "Player")
		{
			isOpen = false;
		}
	}

	/// <summary>
	/// State whether door is open or closed. True = open, false = closed. 
	/// </summary>
	/// <param name="state">If set to <c>true</c> state.</param>
	public void setDoorState (bool state) {
		isOpen = state;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorControl : MonoBehaviour
{
	GameObject door;

	Transform doorClosedPos;
	Transform doorOpenPos;

	public float closeSpeed;

	private bool isOpen = true;

	public bool canClose;

	public bool finalDoor;

	// Use this for initialization
	void Start ()
	{
		door = this.transform.FindChild ("Door").gameObject;
		doorClosedPos = gameObject.transform.Find ("DoorClosePos");
		doorOpenPos = gameObject.transform.Find ("DoorOpenPos");
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (canClose)
		{
			if (isOpen)
			{
				door.transform.position = Vector3.Lerp (door.transform.position, doorOpenPos.position, Time.deltaTime * closeSpeed);
			}
			else
			{
				door.SetActive (true);
				door.transform.position = Vector3.Lerp (door.transform.position, doorClosedPos.position, Time.deltaTime * closeSpeed);
			}

			if (finalDoor)
			{
				if (GameState.inst.AllKeysFound ())
				{
					//door.transform.position = Vector3.Lerp (door.transform.position, doorOpenPos.position, Time.deltaTime * closeSpeed);
					isOpen = true;
				}
				else
				{
					//door.transform.position = Vector3.Lerp (door.transform.position, doorClosedPos.position, Time.deltaTime * closeSpeed);
					isOpen = false;
				}
			}
		}
		else
		{
			door.SetActive (false);
		}

		if (door.transform.position == doorOpenPos.position)
		{
			//door.SetActive (false);
		}

		/*if (finalDoor)
		{
			if (GameState.inst.AllKeysFound())
			{
				door.transform.position = Vector3.Lerp (door.transform.position, doorOpenPos.position, Time.deltaTime * closeSpeed);
			}
			else
			{
				door.transform.position = Vector3.Lerp (door.transform.position, doorClosedPos.position, Time.deltaTime * closeSpeed);
			}
		} */
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
	public void setDoorState (bool state)
	{
		isOpen = state;
	}

	public void setCanClose (bool c)
	{
		canClose = c;
	}
}

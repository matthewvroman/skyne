using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveRoom : MonoBehaviour 
{
	[HideInInspector] public GameObject saveSpawnPoint; 

	SaveCollider saveCollider; 

	// Ensures that you can only save once per time entering the save room
	public bool readyToSave; 

	void Start()
	{
		saveCollider = transform.Find("SaveCollider").GetComponent<SaveCollider>(); 
		saveSpawnPoint = transform.Find("SaveSpawnPoint").gameObject; 
	}

	void Update()
	{
		if (saveCollider.playerInside)
		{
			SaveRoomManager.inst.inSaveRoom = true; 
			SaveRoomManager.inst.curSaveRoom = this;  
		}
		else if (SaveRoomManager.inst.curSaveRoom == this)
		{
			SaveRoomManager.inst.inSaveRoom = false; 
			SaveRoomManager.inst.justSaved = false;
		}

		if (!saveCollider.playerInside)
		{
			readyToSave = true;
		}
	}
}

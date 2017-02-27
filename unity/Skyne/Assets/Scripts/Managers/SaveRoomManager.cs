using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveRoomManager : Singleton<SaveRoomManager> 
{
	// Save-room related stuff
	public bool inSaveRoom; 
	[HideInInspector] public SaveRoom curSaveRoom; 
	public bool justSaved; 

	// Temporary saving UI
	public GameObject pressToSaveText;
	public GameObject gameSavedText; 
	
	// Update is called once per frame
	void Update () 
	{
		pressToSaveText.SetActive(false); 
		gameSavedText.SetActive(false);

		// Check save room usage
		if (inSaveRoom && curSaveRoom != null)
		{
			if (curSaveRoom.readyToSave && !justSaved)
			{
				pressToSaveText.SetActive(true); 

				// Input for confirming the save
				if (Input.GetKeyDown(KeyCode.E))
				{
					justSaved = true; 
					curSaveRoom.readyToSave = false; 

					// Call PlayerPrefsManager to create the save
					PlayerPrefsManager.inst.SavePlayerPrefs(); 
				}
			}
			else if (justSaved)
			{
				gameSavedText.SetActive(true); 
			}
		}


		// Check save room usage
		/*
		if (inSaveRoom && curSaveRoom != null)
		{
			if (!justSaved)
			{
				pressToSaveText.SetActive(true); 
				gameSavedText.SetActive(false); 

				// Input for confirming the save
				if (Input.GetKeyDown(KeyCode.E))
				{
					justSaved = true; 
					curSaveRoom.readyToSave = false; 

					// Call PlayerPrefsManager to create the save
					PlayerPrefsManager.inst.SavePlayerPrefs(); 
				}
			}
			else
			{
				pressToSaveText.SetActive(false); 
				gameSavedText.SetActive(true); 
			}
		}
		else
		{
			pressToSaveText.SetActive(false); 
			gameSavedText.SetActive(false); 
		}
		*/ 
	}

	public SaveRoom GetSaveRoom(string name)
	{
		return transform.Find(name).GetComponent<SaveRoom>(); 
	}
}

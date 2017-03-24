using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveRoomManager : Singleton<SaveRoomManager> 
{
	// Save-room related stuff
	public bool inSaveRoom; 
	[HideInInspector] public SaveRoom curSaveRoom; 
	public bool justSaved; 

	// Stop save UI from appearing when a save is loaded
	[HideInInspector] public bool saveReady; 

	// Temporary saving UI
	public GameObject pressToSaveText;
	public GameObject gameSavedText; 
	
	// Update is called once per frame
	void Update () 
	{
		pressToSaveText.SetActive(false); 
		gameSavedText.SetActive(false);

		// Check save room usage
		if (inSaveRoom && curSaveRoom != null && saveReady)
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

		if (GlobalManager.inst.GameplayIsActive() && !inSaveRoom)
		{
			saveReady = true; 
		}
	}

	public SaveRoom GetSaveRoom(string name)
	{
		return transform.Find(name).GetComponent<SaveRoom>(); 
	}
}

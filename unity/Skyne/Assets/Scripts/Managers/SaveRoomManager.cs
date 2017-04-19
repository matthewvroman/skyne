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
	public bool saveReady; 

	// Temporary saving UI
	public GameObject pressToSaveText;
	public GameObject gameSavedText; 

	PlayerManager pManager; 

	void Start()
	{
		saveReady = false; 
		pressToSaveText.SetActive(false); 
	}

	// Update is called once per frame
	void Update () 
	{
		pressToSaveText.SetActive(false); 
		gameSavedText.SetActive(false);

		if (!GlobalManager.inst.GameplayIsActive())
		{
			return; 
		}

		if (pManager == null)
		{
			pManager = LevelData.inst.player.GetComponent<PlayerManager>(); 
		}

		// Check save room usage
		if (inSaveRoom && curSaveRoom != null && saveReady)
		{
			if (curSaveRoom.readyToSave && !justSaved && pManager.GetIsAlive())
			{
				pressToSaveText.SetActive(true); 

				// Input for confirming the save
				if (Input.GetKeyDown(KeyCode.E))
				{
					justSaved = true; 
					curSaveRoom.readyToSave = false; 

					curSaveRoom.TriggerSaveParticles(); 

					// Call PlayerPrefsManager to create the save
					PlayerPrefsManager.inst.SavePlayerPrefs(); 
				}
			}
			else if (justSaved)
			{
				gameSavedText.SetActive(true); 
			}
		}
	}

	void FixedUpdate()
	{
		if (!GlobalManager.inst.GameplayIsActive())
		{
			return; 
		}

		if (curSaveRoom == null)
		{
			saveReady = true; 
		}
		else if (!inSaveRoom)
		{ 
			if (Vector3.Distance(LevelData.inst.player.transform.position, curSaveRoom.saveSpawnPoint.transform.position) > 1)
			{
				saveReady = true; 
			}
		}
	}

	public SaveRoom GetSaveRoom(string name)
	{
		return transform.Find(name).GetComponent<SaveRoom>(); 
	}
}

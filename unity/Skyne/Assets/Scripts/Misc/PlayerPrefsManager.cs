using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script contains functions for reading and writing data from PlayerPrefs
/// This script does not store game data itself; the data comes from the game state managers
/// Note: this class directly references data from GameStateManager, rather than using parameters 
/// </summary>

[ExecuteInEditMode]
public class PlayerPrefsManager : Singleton<PlayerPrefsManager> 
{
	[Tooltip("Click in inspector to reset the player save data. WARNING: All data will be lost")]
	[SerializeField] private bool resetPlayerPrefs; 

	
	// Use for [ExecuteInEditMode] functionality ONLY
	void Update () 
	{
		if (resetPlayerPrefs)
		{
			resetPlayerPrefs = false; 
			ResetPlayerPrefs(); 
			Debug.Log("The player prefs have been reset"); 
		}
			
	}


	public void ResetPlayerPrefs ()
	{
		// Back up some values from player prefs
		float musicVol = GetSavedMusicVolume(0); 
		float sfxVol = GetSavedSFXVolume(0); 
		float mSensitivity = GetSavedMouseSensitivity(GlobalManager.inst.defaultSensitivity); 

		// Clear all player prefs
		PlayerPrefs.DeleteAll(); 

		// Reassign the saved values
		SaveMusicVolume(musicVol); 
		SaveSFXVolume(sfxVol); 
		SaveMouseSensitivity(mSensitivity); 

		PlayerPrefs.Save(); 
	}

	/// <summary>
	/// Determines whether PlayerPrefs data exists for a savegame
	/// </summary>
	/// <returns><c>true</c>, if a save exists <c>false</c> otherwise.</returns>
	public bool SaveExists()
	{
		// Might need to add additional validity checks
		return GetBool("saveExists", false); 
	}

	/// <summary>
	/// Call this function from a save room to save all game data to player prefs
	/// </summary>
	public void SavePlayerPrefs()
	{
		// A bunch of stuff here

		// Save a key to indicate that PlayerPrefs exist
		SetBool("saveExists", true); 

		// Set the index (integer) representing which save room is being used

		// If the player is still at the start of the game, save as "start"
		if (SaveRoomManager.inst.curSaveRoom == null)
		{
			PlayerPrefs.SetString("saveRoom", "start"); 
		}
		else
		{
			PlayerPrefs.SetString("saveRoom", SaveRoomManager.inst.curSaveRoom.gameObject.name); 
		}

		// Save the ambush rooms
		for (int i = 0; i < GameState.inst.ambushRoomsDone.Length; i++)
		{
			SetBool("ambush" + i, GameState.inst.ambushRoomsDone[i]); 
		}

		// Save which keys have been found
		for (int i = 0; i < GameState.inst.keysFound.Length; i++)
		{
			SetBool("key" + i, GameState.inst.keysFound[i]); 
		}

		// Save the upgrades found
		for (int i = 0; i < GameState.inst.upgradesFound.Length; i++)
		{
			SetBool("upgrade" + i, GameState.inst.upgradesFound[i]); 
		}

		SaveRoomStates();

		PlayerManager player = GameObject.FindObjectOfType<PlayerManager>(); 

		// Save the player health
		//PlayerPrefs.SetInt("playerHealth", GameState.inst.playerHealth);
		PlayerPrefs.SetFloat("playerHealth", player.getHealth()); 
		PlayerPrefs.Save(); 
	}

	/// <summary>
	/// Loads data from PlayerPrefs into the properties of GameState
	/// </summary>
	public void LoadPlayerPrefs()
	{
		// Load the room states
		LoadRoomStates(); 

		string curSaveRoomName = GetString("saveRoom", "start");

		if (curSaveRoomName != "start")
		{
			SaveRoom curSaveRoom = SaveRoomManager.inst.GetSaveRoom(curSaveRoomName); 

			if (curSaveRoom != null)
			{
				SaveRoomManager.inst.curSaveRoom = curSaveRoom; 
				SaveRoomManager.inst.curSaveRoom.readyToSave = false; 

				// Set the player position based on the data stored in the SaveRoom
				// TODO- this is an awkward way to get a reference to the player. Might want a better solution
				LevelData.inst.player.transform.position = curSaveRoom.saveSpawnPoint.transform.position; 
				LevelData.inst.player.transform.rotation = curSaveRoom.saveSpawnPoint.transform.rotation; 
			}
		}

		// Load the ambush rooms done
		for (int i = 0; i < GameState.inst.ambushRoomsDone.Length; i++)
		{
			GameState.inst.ambushRoomsDone[i] = GetBool("ambush" + i, false); 
		}

		// Load the keys found
		for (int i = 0; i < GameState.inst.keysFound.Length; i++)
		{
			GameState.inst.keysFound[i] = GetBool("key" + i, false); 
		}

		// Load the upgrades found
		for (int i = 0; i < GameState.inst.upgradesFound.Length; i++)
		{
			GameState.inst.upgradesFound[i] = GetBool("upgrade" + i, false); 
		}

		// Load the player health
		//GameState.inst.playerHealth = GetInt("playerHealth", 100);

		PlayerManager player = GameObject.FindObjectOfType<PlayerManager>(); 

		// Player health is set in the PlayerManager's Start() via GetStoredPlayerHealth()
	}

	public float GetStoredPlayerHealth()
	{
		return GetFloat("playerHealth", 100);
	}

	public void SaveRoomStates()
	{
		// Save a key to indicate that PlayerPrefs exist
		SetBool("saveExists", true);

		for (int level = 0; level < LevelData.inst.numLevels; level++)
		{
			for (int column = 0; column < LevelData.inst.numColumns; column++)
			{
				for (int row = 0; row < LevelData.inst.numRows; row++)
				{
					string gridKey = "gridSpace-" + level + "-" + column + "-" + row; 
					SetBool(gridKey, GameState.inst.GetGridSpaceRevealedOnMap(level+1, column+1, row+1)); 
					//Debug.Log("gridKey: " + gridKey); 
				}
			}
		}
	}

	public void LoadRoomStates()
	{
		for (int level = 0; level < LevelData.inst.numLevels; level++)
		{
			for (int column = 0; column < LevelData.inst.numColumns; column++)
			{
				for (int row = 0; row < LevelData.inst.numRows; row++)
				{
					string gridKey = "gridSpace-" + level + "-" + column + "-" + row; 

					if (GetBool(gridKey, false))
					{
						GameState.inst.SetGridSpaceRevealedOnMap(level + 1, column + 1, row + 1); 
					}
				}
			}
		}
	}

	public void SaveSFXVolume(float value)
	{
		PlayerPrefs.SetFloat("sfxVolume", value); 
	}

	public float GetSavedSFXVolume(float defaultValue)
	{
		return GetFloat("sfxVolume", defaultValue); 
	}

	public void SaveMusicVolume(float value)
	{
		PlayerPrefs.SetFloat("musicVolume", value); 
	}

	public float GetSavedMusicVolume(float defaultValue)
	{
		return GetFloat("musicVolume", defaultValue);
	}

	public void SaveMouseSensitivity(float value)
	{
		PlayerPrefs.SetFloat("mouseSensitivity", value); 
	}

	public float GetSavedMouseSensitivity(float defaultValue)
	{
		return GetFloat("mouseSensitivity", defaultValue); 
	}

	public float GetSavedHealth()
	{
		return GetFloat("playerHealth", 100); 
	}


	/*
	 * Helper functions for PlayerPrefs
	 */

	/// <summary>
	/// Helper for PlayersPrefs.SetInt(). Takes a boolean input and converts it to a 0 or 1 integer to store in PlayerPrefs
	/// </summary>
	/// <param name="key">PlayerPrefs Key</param>
	/// <param name="value">Boolean value to be converted to 0 or 1</param>
	void SetBool(string key, bool value)
	{
		if (!value)
			PlayerPrefs.SetInt(key, 0); 
		else
			PlayerPrefs.SetInt(key, 1); 
	}

	/// <summary>
	/// Helper for PlayerPrefs.GetInt(key). If the key is not found, the returned value is the default value parameter. 
	/// </summary>
	/// <returns>The int value for the key</returns>
	/// <param name="key">PlayerPrefs string key</param>
	/// <param name="defaultValue">Default value if the key is not found in PlayerPrefs.</param>
	int GetInt(string key, int defaultValue)
	{
		if (PlayerPrefs.HasKey(key))
		{
			return PlayerPrefs.GetInt(key); 
		}

		//Debug.Log("Key " + key + " not found in PlayerPrefs. Using default value: " + defaultValue);
		return defaultValue; 
	}

	/// <summary>
	/// Helper for PlayerPrefs.GetInt(key). If the key is not found, the returned value is the default value parameter. 
	/// Same as GetInt(), but checks if result is a valid int for a boolean (0 or 1). Returns default value if invalid format
	/// </summary>
	/// <returns>The bool value for the key</returns>
	/// <param name="key">PlayerPrefs string key</param>
	/// <param name="defaultValue">Default value if the key is not found in PlayerPrefs.</param>
	bool GetBool(string key, bool defaultValue)
	{
		if (PlayerPrefs.HasKey(key))
		{
			if (PlayerPrefs.GetInt(key) == 0 || PlayerPrefs.GetInt(key) == 1)
			{
				return PlayerPrefs.GetInt(key) == 0 ? false : true; 
			}
			Debug.LogError("Key " + key + " is invalid: " + PlayerPrefs.GetInt(key) + ". Bool ints must be either 0 or 1. Using default value: " + defaultValue); 
			return defaultValue; 
		}

		//Debug.Log("Key " + key + " not found in PlayerPrefs. Using default value: " + defaultValue);
		return defaultValue; 
	}

	/// <summary>
	/// Helper for PlayerPrefs.GetFloat(key). If the key is not found, the returned value is the default value parameter. 
	/// </summary>
	/// <returns>The float value for the key</returns>
	/// <param name="key">PlayerPrefs string key</param>
	/// <param name="defaultValue">Default value if the key is not found in PlayerPrefs.</param>
	float GetFloat(string key, float defaultValue)
	{
		if (PlayerPrefs.HasKey(key))
		{
			return PlayerPrefs.GetFloat(key); 
		}

		//Debug.Log("Key " + key + " not found in PlayerPrefs. Using default value: " + defaultValue);
		return defaultValue; 
	}

	/// <summary>
	/// Helper for PlayerPrefs.GetString(key). If the key is not found, the returned value is the default value parameter. 
	/// </summary>
	/// <returns>The string value for the key</returns>
	/// <param name="key">PlayerPrefs string key</param>
	/// <param name="defaultValue">Default value if the key is not found in PlayerPrefs.</param>
	string GetString(string key, string defaultValue)
	{
		if (PlayerPrefs.HasKey(key))
		{
			return PlayerPrefs.GetString(key); 
		}

		//Debug.Log("Key " + key + " not found in PlayerPrefs. Using default value: " + defaultValue);
		return defaultValue; 
	}




}

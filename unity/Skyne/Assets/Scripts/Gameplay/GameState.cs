using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : Singleton<GameState> 
{
// How can we save room data?
	// Option 1: Save keys related to the roomName, and additional modifiers after
	// Option 2: Save a list of grid coordinates that have been entered, then match those up

	// Associates a bool with each room
	public RoomData[] roomStateData; 

	/// <summary>
	/// Holds data associated with the state of each room
	/// </summary>
	[System.Serializable]
	public struct RoomData
	{
		public string roomName; 
		public bool revealed; 

		// Add other variables as needed - for example, stuff about enemies or room-specific changes
		// Idea: if we need to save very room-specific data, such as the state of a particular object, we could map that to an array of values for each room
	}

	// Save the abilities the player has found
	// For now, just use a simple bool array to determine which have been found
	[Space(5)]
	[Header("Abilities: (0 = double_j) (1 = wall_j) (2 = dash)")]
	[Header("Weapons: (3 = charge) (4 = wide) (5 = rapid)")]
	[Tooltip("(Read only before game start) Stores which upgrades have been found. Array indexes are generated in the code.")]
	public bool[] upgradesFound; 

	// Save which keys have been collected
	[Space(5)]
	[Header("(0 = Boss 1) (1 = Boss 2) (2 = Boss 3)")]
	[Tooltip("(Read only before game start) Stores which keys have been collected.")]
	public bool[] keysFound; 

	[Space(10)]
	[Range(0,100)]
	[Tooltip("The player's current health, from 0 to 100.")]
	public int playerHealth; 

	[Tooltip("The player's current shooting mode.")]
	public PlayerShooting.PlayerShootMode pShootMode; 

	// TODO If keys are returned to the treasure room, keep track of that data; maybe change upgrades found to int array


	/// <summary>
	/// Initializes the default game data. If a save is detected, this data will be overwritten by the saved data
	/// </summary>
	void GenerateInitialGameData ()
	{
		// Initialize the array of room data
		// IMPORTANT: this reads from SceneMapping's sceneList property. 
		// The sceneList must be read in (via MainManager.SceneMapping.StartReadInListFromDirectory()) before roomStateData is populated
		roomStateData = new RoomData[SceneMapping.inst.sceneList.Length]; 

		// Populate roomData
		for (int i = 0; i < roomStateData.Length; i++)
		{
			roomStateData[i] = new RoomData (); 
			roomStateData[i].roomName = SceneMapping.inst.sceneList[i].sceneName; 
			roomStateData[i].revealed = false;
		}

		// Update the map
		if (MapDisplay.inst.gameObject.activeSelf)
		{
			MapDisplay.inst.SpawnMap(); 
		}

		// If upgradesFound and keysFound are invalid, regenerate them
		if (upgradesFound == null || upgradesFound.Length != 6)
		{
			/*
			upgradesFound = new bool[6]; 
			for (int i = 0; i < upgradesFound.Length; i++)
				upgradesFound[i] = false; 
			*/
		}
		if (keysFound == null || keysFound.Length != 3)
		{
			/*
			keysFound = new bool[3]; 
			for (int i = 0; i < keysFound.Length; i++)
				keysFound[i] = false; 
			*/
		}
	}
		

	/// <summary>
	/// Alter the "revealed" state of a room on the map
	/// </summary>
	/// <param name="roomName">Room name.</param>
	/// <param name="toRevealed">True if the room is being revealed, false if being hidden</param>
	/// <returns><c>true</c>, if the room's state is different after the change, <c>false</c> otherwise.</returns>
	public bool SetRoomRevealedOnMap(int level, int column, int row, bool toRevealed)
	{
		// TODO
		// This is weird, it should be column - 1 and row - 1. It looks like sceneMapping is saving nothing into its column/row at index 0
		string roomName = SceneMapping.inst.GetSceneAt(level, column, row); 

		//Debug.Log("Set " + roomName + " to be revealed; level: " + level + "; column: " + column + "; row: " + row); 

		int foundIndex = GetRoomStateIndexOf(roomName); 
		if (foundIndex != -1)
		{
			bool origRevealedState = roomStateData[foundIndex].revealed; 
			roomStateData[foundIndex].revealed = toRevealed; 

			if (origRevealedState != toRevealed)
				return true; 
		}
		return false; 
	}

	/// <summary>
	/// Returns whether a room has been revealed on the map
	/// </summary>
	/// <returns><c>true</c>, if room is revealed, <c>false</c> otherwise.</returns>
	/// <param name="roomName">Room name.</param>
	public bool GetRoomRevealedOnMap(string roomName)
	{
		int foundIndex = GetRoomStateIndexOf(roomName); 
		if (foundIndex != -1)
		{
			return roomStateData[foundIndex].revealed; 
		}
		return false; 
	}

	/// <summary>
	/// Room states are stored in an unordered array
	/// Searches the array and returns the index of the room, or -1 if not found
	/// Not the most efficient solution, but better for flexibility when storing data about rooms
	/// </summary>
	public int GetRoomStateIndexOf(string name)
	{
		for (int i = 0; i < roomStateData.Length; i++)
		{
			if (roomStateData[i].roomName == name)
				return i; 
		}
		return -1; 
	}

	/// <summary>
	/// Begins the data setup process for loading a game
	/// If save data exists, the default game data is overwritten with data pulled from PlayerPrefs via PlayerPrefsManager
	/// </summary>
	public void LoadGame ()
	{
		// This function must be called to initialize the default values
		// If a save file exists, these initialized data slots will be overwritten with the data from the save file
		GenerateInitialGameData(); 

		if (PlayerPrefsManager.inst.SaveExists())
		{
			//Debug.Log("Save file found. Loading from PlayerPrefs...");  
			PlayerPrefsManager.inst.LoadPlayerPrefs(); 
		}
		else
		{
			//Debug.Log("No save file found. Using initial game data..."); 
		}

		// Updates the player grid position at the start and loads the inital scenes needed
		//LevelData.inst.UpdatePlayerGridPos(); 
		LevelData.inst.RefreshLoadedScenes(); 
	}

	/// <summary>
	/// Checks whether the player has collected all the keys in the game
	/// </summary>
	/// <returns><c>true</c>, if all keys have been found, <c>false</c> otherwise.</returns>
	public bool AllKeysFound()
	{
		for (int i = 0; i < keysFound.Length; i++)
		{
			if (!keysFound[i])
			{
				return false; 
			}
		}
		return true; 
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine.SceneManagement; 

/// <summary>
/// Class for managing player grid positions and level loading
/// Keeps track of the player's current column/row position
/// Keeps a list of grid positions considered "active" that should have their associated scenes loaded
/// Note: none of the variables stored in this class need to be saved into PlayerPrefs. Data that can be saved should go in GameState
/// </summary>
public class LevelData : Singleton<LevelData> 
{
	[Tooltip("Number of columns in the game world grid")]
	public int numColumns; 

	[Tooltip("Number of rows in the game world grid")]
	public int numRows; 

	[Tooltip("Number of height levels in the game world grid")]
	public int numLevels; 

	[Tooltip("The size of the x and z grid space edges in Unity units")]
	public int gridEdgeSize; 

	[Tooltip("How many rooms adjacent to the player are loaded?")]
	public int roomLoadRadius; 

	[Tooltip("Past the roomLoadRadius, how many rooms are loaded in the 4 direction sightline?")]
	public int sightlineRoomLoad; 

	[Tooltip("(Read-only) The player's current column (starting on the left)")]
	 public int curColumn;

	[Tooltip("(Read-only) The player's current row (starting on the bottom)")]
	public int curRow;

	[Tooltip("(Read-only) The player's current height level (starting on the bottom level)")]
	public int curLevel;

	[Tooltip("Ground level 1's height (Y axis)")]
	public float level1Height; 

	[Tooltip("Ground level 2's height (Y axis)")]
	public float level2Height; 

	[Tooltip("Ground level 3's height (Y axis)")]
	public float level3Height; 

	[Tooltip("(Read-only) String representation of the player's current grid position in the format (letter, number)")]
	public string curGridPos; 

	[Tooltip("(Read-only) The list of grid positions considered \"active\" in the format (letter, number)")]
	public List<string> activeGridPositions; 

	[Tooltip("(Read-only) The list of level segment scenes currently loaded)")]
	public List<string> activeScenes; 

	[Tooltip("The player object (DRAG IN)")]
	public GameObject player; 
	 

	/// <summary>
	/// Updates the player's current level, column, and row
	/// </summary>
	/// <returns><c>true</c>, if player grid position was updated, <c>false</c> otherwise.</returns>
	public void UpdatePlayerGridPos () 
	{
		/*
		 * Update player data
		 */ 

		// Use the player's x and z coordinates to calculate the current column and row respectively
		curColumn = Mathf.FloorToInt(player.transform.position.x / gridEdgeSize) + 1; 
		curRow = Mathf.FloorToInt(player.transform.position.z / gridEdgeSize) + 1; 

		// Update player height level
		// Simple height calculation; may need to adjust
		// Also, detect if the player has fallen out of the game world
		if (player.transform.position.y < -50)
		{
			Debug.Log("Player has fallen out of world"); 

			// Temporary- load instant game over to test it
			// TODO revise this
			GlobalManager.inst.LoadGameOver(); 
		}
		else if (player.transform.position.y < level2Height)
		{
			curLevel = 1; 
		}
		else if (player.transform.position.y < level3Height)
		{
			curLevel = 2; 
		}
		else
		{
			curLevel = 3; 
		}

		// Compare the last frame's grid position with the current frame's grid position. If they don't match, the grid position has changed
		string oldGridPos = curGridPos; 

		int[] curLevelArray = { curLevel }; 
		curGridPos = LevelDataFunctions.GetGridPositionString(curLevelArray, curColumn, curRow); 

		// If the grid position has changed, update the active grid positions and scenes list, then determine which scenes to load and unload
		if (oldGridPos != curGridPos)
		{
			if (GlobalManager.inst.globalState == GlobalManager.GlobalState.Gameplay || GlobalManager.inst.globalState == GlobalManager.GlobalState.SetupGameplay)
			{
				LevelData.inst.RefreshLoadedScenes(); 
			}
		}
	}

	/// <summary>
	/// Updates the game map and which grid positions and scenes are considered active. 
	/// Determines which scenes to load and unload, then calls SceneLoading to load/unload them
	/// </summary>
	public void RefreshLoadedScenes()
	{
		// Set the player's current room on the map screen to be revealed (even if it already is)
		//bool shouldSaveMap = GameState.inst.SetRoomRevealedOnMap(curLevel, curColumn, curRow, true); 

		bool shouldSaveMap = GameState.inst.SetGridSpaceRevealedOnMap(curLevel, curColumn, curRow); 

		// Update the map (enables map tiles if they've just been discovered)
		MapDisplay.inst.UpdateMap(); 

		// Permanently save to PlayerPrefs whenever the map has changed
		// Map data is preserved even if the player dies and reloads
		// This constant map updating functionality has been depcrecated
		/*
		if (shouldSaveMap)
		{
			PlayerPrefsManager.inst.SaveRoomStates();
		}
		*/ 



		// Update the grid positions and use those to find which scenes are active and should be loaded/unloaded
		UpdateActiveGridPositions(); 
		UpdateActiveScenes(); 
		FindScenesToLoad(); 
		FindScenesToUnload(); 
	}


	/// <summary>
	/// When the player transitions to a new section of the grid, clear and repopulate the array of active grid positions
	/// Currently, active grid positions include the player's grid position and a square of adjacent spaces (max total of 9 active spaces)
	/// </summary>
	void UpdateActiveGridPositions ()
	{
		activeGridPositions.Clear(); 

		for (int i = 1; i <= numLevels; i++)
		{
			// Iterate through every grid position (including the curPosition) based on the roomLoadRadius
			for (int x = -roomLoadRadius; x <= roomLoadRadius; x++)
			{
				for (int y = -roomLoadRadius; y <= roomLoadRadius; y++)
				{
					float circleDist = Vector2.Distance(new Vector2(0, 0), new Vector2(x, y)); 

					//float circleDist = Vector3.Distance(new Vector3 (0, 0, curLevel), new Vector3 (x, y, i)); 

					//Debug.Log("circleDist: " + circleDist + "; radius: " + roomLoadRadius); 

					if (circleDist <= roomLoadRadius)
					{
						AddToActiveGridPositions(i, curColumn + x, curRow + y); 
					}
				}
			}

			//Add extra rooms based on the sightlineRoomLoad
			for (int s = 1; s <= sightlineRoomLoad; s++)
			{
				// Top
				AddToActiveGridPositions(i, curColumn, curRow + roomLoadRadius + s);
				// Bottom
				AddToActiveGridPositions(i, curColumn, curRow - roomLoadRadius - s);
				// Right
				AddToActiveGridPositions(i, curColumn + roomLoadRadius + s, curRow);
				// Left
				AddToActiveGridPositions(i, curColumn - roomLoadRadius - s, curRow);
			}

			/*
			// Player's grid position
			AddToActiveGridPositions(i, curColumn, curRow); 

			// Top, bottom, left, right by 1
			AddToActiveGridPositions(i, curColumn - 1, curRow); 
			AddToActiveGridPositions(i, curColumn + 1, curRow); 
			AddToActiveGridPositions(i, curColumn, curRow - 1); 
			AddToActiveGridPositions(i, curColumn, curRow + 1);

			// Diagonals by 1
			AddToActiveGridPositions(i, curColumn - 1, curRow - 1); 
			AddToActiveGridPositions(i, curColumn - 1, curRow + 1); 
			AddToActiveGridPositions(i, curColumn + 1, curRow - 1); 
			AddToActiveGridPositions(i, curColumn + 1, curRow + 1); 
			*/ 
		}

		// Add extra rooms based on the sightlineRoomLoad (only on player's current level)
		/*
		for (int s = 1; s <= sightlineRoomLoad; s++)
		{
			// Top
			AddToActiveGridPositions(curLevel, curColumn, curRow + roomLoadRadius + s);
			// Bottom
			AddToActiveGridPositions(curLevel, curColumn, curRow - roomLoadRadius - s);
			// Right
			AddToActiveGridPositions(curLevel, curColumn + roomLoadRadius + s, curRow);
			// Left
			AddToActiveGridPositions(curLevel, curColumn - roomLoadRadius - s, curRow);
		}
		*/ 
	}

	void UpdateActiveScenes ()
	{
		activeScenes.Clear(); 

		foreach (string cStringPos in activeGridPositions)
		{
			// Get the list of levels
			int[] cLevelArray = LevelDataFunctions.GetLevelsFrom(cStringPos); 

			// Choose the first level in the list (there should only be one)
			int cLevel = cLevelArray[0]; 

			// Convert the letter,number format in column,row
			int[] cPosition = LevelDataFunctions.GetColumnAndRowFromFull(cStringPos);

			// Get the scene name associated with that column and row, which is stored in SceneMapping and accessed with GetSceneAt()
			// BUG: should these be -1
			string cScene = SceneMapping.inst.GetSceneAt(cLevel, cPosition[0], cPosition[1]); 

			// If that scene name is not already contained in active scenes, add it
			if (cScene != "EMPTY" && !activeScenes.Contains(cScene))
			{
				activeScenes.Add(cScene); 
			}
		}
	}

	/// <summary>
	/// Iterates through each scene marked as active and loads it if the scene is not already loaded in the SceneManager
	/// </summary>
	void FindScenesToLoad()
	{
		//SceneLoading.inst.startedLoadingLevels = true; 

		// Iterate through the name of each scene that should be active. If it isn't loaded, then set it to load via SceneLoading
		foreach(string curScene in activeScenes)
		{
			
			if (!SceneManager.GetSceneByName(curScene).isLoaded)
			{
				//Debug.Log("Load scene: " + curScene); 
				SceneLoading.inst.LoadLevelScene(curScene); 
			}


		}

	}

	/// <summary>
	/// Iteratively determine which scenes to unload and call SceneLoading to unload them
	/// </summary>
	void FindScenesToUnload()
	{
		// Iterate through each grid position. If its associated scene is found in SceneManager's loaded scene list but not in activeScenes, unload it
		for (int level = 1; level <= numLevels; level++)
		{
			for (int column = 1; column <= numColumns; column++)
			{
				for (int row = 1; row <= numRows; row++)
				{ 
					string curScene = SceneMapping.inst.GetSceneAt(level, column, row); 

					if (curScene != "EMPTY" && !activeScenes.Contains(curScene) && SceneManager.GetSceneByName(curScene).isLoaded)
					{
						//Debug.Log("Unload scene: " + curScene); 
						SceneLoading.inst.UnloadLevelScene(curScene); 
					}
				}
			}
		}
	}

	/// <summary>
	/// Unloads a level scenes, except for Mainlevel
	/// </summary>
	/*
	public void UnloadAllLevelScenes()
	{
		foreach (string curScene in activeScenes)
		{
			if (SceneManager.GetSceneByName(curScene).isLoaded)
			{ 
				SceneLoading.inst.UnloadLevelScene(curScene); 
			}
		}
	}
	*/ 


	/// <summary>
	/// Adds a grid position in string format to the array of active grid positions
	/// Ensures that coordinates cannot be duplicates or be off the map (lower than row/column 1 or greater than the max number of rows/columns)
	/// </summary>
	/// <returns><c>true</c>, if the position was added, <c>false</c> if the position was already stored or invalid.</returns>
	/// <param name="column">Column (x) from 1 to numColumns</param>
	/// <param name="row">Row (y) from 1 to numRows</param>
	bool AddToActiveGridPositions (int level, int column, int row)
	{
		int[] newLevelArray = { level }; 
		string roomName = LevelDataFunctions.GetGridPositionString(newLevelArray, column, row); 
		if (!GridContains(level, column, row) || activeGridPositions.Contains(roomName))
		{
			return false; 
		}
		activeGridPositions.Add(roomName); 
		return true; 
	}


	/// <summary>
	/// Removes a grid position from the array of active grid positions
	/// </summary>
	/// <returns><c>true</c>, if the position was removed <c>false</c> if the position wasn't stored or was invalid.</returns>
	/// <param name="column">Column (x) from 1 to numColumns</param>
	/// <param name="row">Row (y) from 1 to numRows</param>
	bool RemoveFromActiveGridPositions (int level, int column, int row)
	{
		int[] newLevelArray = { level }; 
		string roomName = LevelDataFunctions.GetGridPositionString(newLevelArray, column, row);
		if (GridContains(level, column, row) && activeGridPositions.Contains(roomName))
		{
			activeGridPositions.Remove(roomName); 
			return true; 
		}
		return false; 
	}


	/// <summary>
	/// Returns if a grid position is valid on the level grid (x and y coordinates between 1 and numColumns/Rows)
	/// </summary>
	/// <returns><c>true</c>, if valid grid position, <c>false</c> if out of bounds.</returns>
	/// <param name="column">Column (x) from 1 to numColumns</param>
	/// <param name="row">Row (y) from 1 to numRows</param>
	public bool GridContains (int level, int column, int row)
	{
		if (level < 1 || level > numLevels || column < 1 || column > numColumns || row < 1 || row > numRows)
			return false;
		return true; 
	}


}


public class LevelDataFunctions
{
	/// <summary>
	/// Returns a string version of grid coordinates (level, column, row)
	/// </summary>
	/// <returns>Room string in format (letter, number)</returns>
	/// <param name="column">Column (x) from 1 to numColumns</param>
	/// <param name="row">Row (y) from 1 to numRows</param>
	public static string GetGridPositionString(int[] level, int column, int row)
	{
		string levelS = ""; 
		for (int i = 0; i < level.Length; i++)
		{
			levelS += level[i]; 
		}
		levelS += "-"; 

		// Convert the column to the corresponding capital letter using ASCII values
		return levelS + (char)(column + 64) + "" + row; 
	}

	/// <summary>
	/// Converts a full level string (with just one grid position) to individual column and row integers
	/// </summary>
	/// <returns>A array of length 2 in the form of [column, row]</returns>
	/// <param name="gridPosition">Grid position string in the form of levels - letter (capital) and number</param>
	public static int[] GetColumnAndRowFromFull(string gridPosition)
	{
		string cr = GetColumnAndRowSubstringFrom(gridPosition); 

		int[] result = new int[2]; 
		result[0] = (int)(cr[0]) - 64; 
		result[1] = int.Parse(cr.Substring(1)); 

		return result; 
	}

	/// <summary>
	/// Converts a row and column string to individual column and row integers
	/// </summary>
	/// <returns>A array of length 2 in the form of [column, row]</returns>
	/// <param name="gridPosition">Grid position string in the form of letter (capital) and number</param>
	public static int[] GetColumnAndRowFrom(string gridPosition)
	{
		int[] result = new int[2]; 
		result[0] = (int)(gridPosition[0]) - 64; 
		result[1] = int.Parse(gridPosition.Substring(1)); 

		return result; 
	}

	/// <summary>
	/// Converts a full level string into an array of each level contained in the string
	/// </summary>
	/// <returns>The levels from.</returns>
	/// <param name="gridPosition">Grid position.</param>
	public static int[] GetLevelsFrom(string gridPosition)
	{
		string levelS = GetLevelSubstringFrom(gridPosition); 
		string[] resultS = levelS.Split(','); 

		int[] result = new int[resultS.Length]; 

		for (int i = 0; i < resultS.Length; i++)
		{
			result[i] = int.Parse(resultS[i]); 
		}

		return result; 
	}

	/// <summary>
	/// Extract the string portion containing only the levels from the scene name
	/// </summary>
	/// <returns>The level substring from.</returns>
	/// <param name="gridPosition">Grid position.</param>
	public static string GetLevelSubstringFrom(string gridPosition)
	{ 
		int index = gridPosition.IndexOf('-'); 
		//Debug.Log(gridPosition); 
		//Debug.Log(gridPosition.Substring(0, index)); 
		return gridPosition.Substring(0, index); 
	}


	/// <summary>
	/// Extract the string portion containing only the row and columns from the scene name 
	/// </summary>
	/// <returns>The column and row substring from.</returns>
	/// <param name="gridPosition">Grid position.</param>
	public static string GetColumnAndRowSubstringFrom(string gridPosition)
	{
		int index = gridPosition.IndexOf('-'); 

			
		//Debug.Log("gridPosition: " + gridPosition + "; Length: " + gridPosition.Length + " IndexOf: " + index); 
		//Debug.Log(gridPosition.Substring(index + 1, gridPosition.Length - index - 1)); 
		return gridPosition.Substring(index + 1, gridPosition.Length - index - 1); 
	}
}

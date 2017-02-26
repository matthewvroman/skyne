using UnityEngine;
using System.Collections;
using System.IO;

[ExecuteInEditMode]
public class SceneMapping : Singleton<SceneMapping> 
{
	[Tooltip("Click while in editor to regenerate the list of level scenes.")]
	public bool generateSceneList; 

	[Tooltip("If true, the scene list generates on startup (only in editor mode though)")]
	public bool generateSceneListOnStartup; 

	[Tooltip("The path to the level scenes")]
	public string scenePath; 

	[Tooltip("(Ready-only) The list of each scene (level section) contained in the world and the grid positions that the scene occupies. Grid positions must be marked in the format letter (capital) and number. For example: C10")]
	public sceneMap[] sceneList; 

	// The main 2D array storing which scene names are associated with each grid position
	// If a grid position has no associated scene, the string "EMPTY" is stored instead
	//private string[,] sceneMapping; 

	// The main 3D array storing which scene names are assoiated with each grid position
	// If a grid position has no associated scene, the string "EMPTY" is stored isntead
	private string[,,] sceneMapping; 



	[System.Serializable]
	public struct sceneMap
	{
		public string sceneName;

		[HideInInspector] public int[] gridLevels; 

		// the 2D grid positions
		[HideInInspector] public string[] gridPositions; 
	}

	public void StartReadInListFromDirectory()
	{
		#if UNITY_EDITOR
		if (generateSceneListOnStartup)
		{
			//Debug.Log("Read in scene list (startup)"); 
			ReadInListFromDirectory(); 
		}
		#endif
	}



	/// <summary>
	/// Called from LevelManager
	/// Takes the sceneList manually entered in the Inspector and populates the sceneMapping array with sceneNames that correspond to each grid position
	/// If a grid position has no scene associated with it, that space is marked as "EMPTY"
	/// </summary>
	/// /// <param name="numRows">Number height levels.</param>
	/// <param name="numColumns">Number columns.</param>
	/// <param name="numRows">Number rows.</param>
	/// <param name="levelManager">Level manager.</param>
	public void GenerateSceneMapping(int numLevels, int numColumns, int numRows)
	{
		// First, for each element in scene map, fill out the gridPositions array by parsing the scene name
		// sceneName must be formatted in two parts
		// Part 1 specifies the height levels the scene takes up
		// Denote with each height level #, followed by a comma; add a '-' after the list of levels
		// Example 1: 1-
		// Example 2: 1,2,3-
		// Part 2 specifies the flat grid positions associated with the scene
		// Denote with an uppercase letter followed by a number
		// If a scene takes up multiple grid positions, separate each coordinate with a comma. The order of these positions does not matter
		// Example 1: 1-A2
		// Example 2: 1,2-C12,C13,D12,D13 (any order works for grid spaces)

		for (int i = 0; i < sceneList.Length; i++)
		{
			string gPositions = LevelDataFunctions.GetColumnAndRowSubstringFrom(sceneList[i].sceneName); 
			sceneList[i].gridPositions = gPositions.Split(','); 

			string gLevels = LevelDataFunctions.GetLevelSubstringFrom(sceneList[i].sceneName); 
			string[] splitLevels = gLevels.Split(',');
			sceneList[i].gridLevels = new int[splitLevels.Length]; 
			for (int g = 0; g < splitLevels.Length; g++)
				sceneList[i].gridLevels[g] = int.Parse(splitLevels[g]); 
		} 

		// Initialize a 3D array of grid positions that will correspond with scene names specified in the sceneList
		sceneMapping = new string[numLevels, numColumns, numRows]; 

		// Fill each index with an empty room string; these will be overwritten in rooms that have an associated scene
		// "EMPTY" denotes an empty room
		for (int level = 0; level < numLevels; level++)
		{
			for (int column = 0; column < numColumns; column++)
			{
				for (int row = 0; row < numRows; row++)
				{
					sceneMapping[level, column, row] = "EMPTY"; 
				}
			}
		}

		//Debug.Log("test: " + sceneMapping[2, 0, 0]); 

		// Iterate through each entry in the scene list
		for (int i = 0; i < sceneList.Length; i++)
		{
			// For each entry, iterate through gridPositions
			// Replace the "EMPTY" string in sceneMapping for a grid position with the name of the scene
			// Throw an error if a duplicate scene mapping is attempted for a grid position
			for (int j = 0; j < sceneList[i].gridPositions.Length; j++)
			{
				for (int l = 0; l < sceneList[i].gridLevels.Length; l++)
				{
					// Extract the column and row data from the string (ex: B14 -> Column 2, Row 14)
					int[] curPosition = LevelDataFunctions.GetColumnAndRowFromFull(sceneList[i].gridPositions[j]); 

					//Debug.Log("l:" + sceneList[i].gridLevels[l] + ", i:" + i + ", j: " + j); 
					//Debug.Log(sceneMapping[2, 0, 0]);  

					// Test for error cases
					if (!LevelData.inst.GridContains(sceneList[i].gridLevels[l], curPosition[0], curPosition[1]))
					{
						Debug.LogError("Invalid grid position on scene " + i + ". gridPosition " + sceneList[i].gridPositions[j] + " is outside the level grid"); 
					}
					else if (sceneMapping[sceneList[i].gridLevels[l] - 1, curPosition[0] - 1, curPosition[1] - 1] != "EMPTY")
					{
						Debug.LogError("Invalid grid position on scene " + i + " at gridPosition " + sceneList[i].gridPositions[j] + ". That grid slot already contains scene " + sceneMapping[sceneList[i].gridLevels[l] - 1, curPosition[0] - 1, curPosition[1] - 1]); 
					}
					// If no errors, replace "EMPTY" with the corresponding scene name
					else
					{
						// TODO: this might be wrong; why not curPosition - 1?
						sceneMapping[sceneList[i].gridLevels[l] - 1, curPosition[0] - 1, curPosition[1] - 1] = sceneList[i].sceneName; 
					}
				}
			}
		}
			
		//Debug.Log("Test: " + sceneMapping[0, 0, 0]); 
		//PrintSceneMapping(); 
	}

	/// <summary>
	/// Debug function for displaying the content of scene mapping
	/// Shows what scene names are associated with each grid position based on level, column, and row (0 based)
	/// </summary>
	public void PrintSceneMapping()
	{
		for (int i = 0; i < sceneMapping.GetLength(0); i++)
		{
			for (int j = 0; j < sceneMapping.GetLength(1); j++)
			{
				for (int k = 0; k < sceneMapping.GetLength(2); k++)
				{
					if (sceneMapping[i, j, k] != "EMPTY")
						Debug.Log("Scene at L:" + i + "; C:" + j + "; R:" + k + " -- " + sceneMapping[i, j, k]); 
				}
			}
		}
	}


	/// <summary>
	/// Gets the scene at these indexes (1 based)
	/// </summary>
	public string GetSceneAt(int level, int column, int row)
	{
		if (sceneMapping == null)
			Debug.LogError("Tried to get a scene when sceneMapping is null"); 

		if (!LevelData.inst.GridContains(level, column, row))
		{
			return "EMPTY"; 
		}

		//Debug.Log("Get scene at " + level + "-" + column + row); 
		int[] test = {level}; 
		//Debug.Log(LevelManager.inst.GetGridPositionString(test, column, row)); 
		return sceneMapping[level - 1, column - 1, row - 1]; 
	}


	// Use for [ExecuteInEditMode] functionality ONLY
	void Update()
	{
		if (generateSceneList)
		{
			generateSceneList = false; 
			ReadInListFromDirectory(); 

		}
	}

	/// <summary>
	/// Reads in a list of scene names from a directory specified by the scenePath variable
	/// This function updates the sceneList array 
	/// Can be run manually from inspector at any time by clicking generateSceneList
	/// Checking generateSceneListOnStartup ensures that this function is run every time the game is run
	/// Note that this function can only run in the editor
	/// </summary>
	void ReadInListFromDirectory()
	{
		#if UNITY_EDITOR

		DirectoryInfo levelDirectoryPath = new DirectoryInfo(scenePath);
		FileInfo[] fileInfo = levelDirectoryPath.GetFiles("*.unity", SearchOption.AllDirectories);

		sceneList = new sceneMap[fileInfo.Length];

		int i = 0; 
		foreach (FileInfo file in fileInfo) 
		{
			sceneList[i] = new sceneMap(); 
			sceneList[i].sceneName = file.Name.Substring(0, file.Name.Length - file.Extension.Length);


			i++; 
		}

		#endif
	}
		
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class MapDisplay : Singleton<MapDisplay> 
{
	// Probably change this to an array eventually
	[Tooltip("Prefab for map tile.")]
	public GameObject mapTile; 
	[Tooltip("Array of parent objects for storing mapTiles for each level of the map")]
	public GameObject[] mapTileParents;

	public GameObject[,,] mapTiles; 

	public float mapTileSize; 

	// 1, 2, or 3
	public int displayLevel; 

	// Temporary slider for adjusting floor/level view
	public Slider floorSlider; 

	public GameObject statusPanel; 

	/*
	[System.Serializable]
	public struct MapPieces
	{
		public Sprite none; 
		public Sprite all;
		public Sprite threeSides; 
		public Sprite twoSides; 
		public Sprite corner; 
		public Sprite oneSide; 
	}
	*/ 

	//public MapPieces[] mapPieces
	 


	/// <summary>
	/// Updated version of spawn map, which spawns map tiles based on a map image broken into pieces with Processing
	/// </summary>
	public void SpawnMap()
	{
		// Clear mapTiles
		mapTiles = new GameObject[LevelData.inst.numLevels, LevelData.inst.numColumns, LevelData.inst.numRows];

		for (int level = 0; level < LevelData.inst.numLevels; level++)
		{
			for (int column = 0; column < LevelData.inst.numColumns; column++)
			{
				for (int row = 0; row < LevelData.inst.numRows; row++)
				{
					// Instantiate a new mapTile under the parent corresponding to its level
					GameObject newMapTile = GameObject.Instantiate(mapTile); 
					newMapTile.transform.SetParent(mapTileParents[level].transform);

					// Choose the correct position for the new tile and rename it
					float xPos = column * mapTileSize; 
					float yPos = row * mapTileSize; 
					newMapTile.GetComponent<RectTransform>().localPosition = new Vector3 (xPos, yPos, 0); 
					newMapTile.name = "L:" + (level + 1) + ", C:" + (column + 1) + ", R:" + (row + 1); 

					// Add each created tile to the array of stored map tiles
					mapTiles[level, column, row] = newMapTile; 

					// Pick the correct image for the map tile
					//Sprite test = newMapTile.GetComponent<Image>().sprite; 
					//LoadMapPieces test = GetComponent<LoadMapPieces>(); 
					LoadMapPieces loadMapPieces = GetComponent<LoadMapPieces>(); 

					newMapTile.GetComponent<Image>().sprite = loadMapPieces.GetLevelMapPieces(level + 1)[loadMapPieces.GetMapIndex(column,row)]; 

					// Update the gameObject's active property based on whether that map position has been discovered
					// Temporarily disabled for first playable
					if (!GameState.inst.gridSpacesEntered[level, column, row])
					{
						newMapTile.SetActive(false); 
					}
				}
			}
		}
	}



	/*
	/// <summary>
	/// Outdated version of spawn map, which spawns the map based on the loaded room data
	/// </summary>
	public void SpawnMap () 
	{
		//Debug.Log("Spawn map: " + LevelData.inst.numLevels + " x " + LevelData.inst.numColumns + " x " + LevelData.inst.numRows); 
		Debug.Log("GameState.inst.roomStateData.Length = " + GameState.inst.roomStateData.Length); 

		// Clear mapTiles
		mapTiles = new GameObject[LevelData.inst.numLevels, LevelData.inst.numColumns, LevelData.inst.numRows]; 

		// Spawn a bunch of map tiles on level 1
		// Doing this in Start() is not a good long-term solution
		for (int i = 0; i < GameState.inst.roomStateData.Length; i++)
		{
			string roomName = GameState.inst.roomStateData[i].roomName; 
			string columnsRows = LevelDataFunctions.GetColumnAndRowSubstringFrom(roomName); 
			string[] gridPositions = columnsRows.Split(','); 
			int[] roomLevels = LevelDataFunctions.GetLevelsFrom(roomName); 

			// Add grid positions for each level
			for (int l = 0; l < roomLevels.Length; l++)
			{
				// Create a new tile for each grid position that the room occupies
				for (int j = 0; j < gridPositions.Length; j++)
				{
					// Instantiate a new mapTile under the parent corresponding to its level
					GameObject newMapTile = GameObject.Instantiate(mapTile); 
					newMapTile.transform.SetParent(mapTileParents[roomLevels[l] - 1].transform); 

					// Choose the correct position for the new tile and rename it
					int[] columnRow = LevelDataFunctions.GetColumnAndRowFromFull(gridPositions[j]); 
					newMapTile.GetComponent<RectTransform>().localPosition = new Vector3 (columnRow[0] * mapTileSize, columnRow[1] * mapTileSize, 0); 
					newMapTile.name = "L:" + roomLevels[l] + ", C:" + columnRow[0] + ", R:" + columnRow[1]; 

					// Add each created tile to the array of stored map tiles
					mapTiles[roomLevels[l] - 1, columnRow[0] - 1, columnRow[1] - 1] = newMapTile; 

					// Choose the correct sprite for each tile
					ChooseTileSprite(roomLevels[l] - 1, columnRow, gridPositions, newMapTile.GetComponent<Image>(), newMapTile.GetComponent<RectTransform>()); 

					// Update the gameObject's active property based on whether that map position has been discovered
					// Temporarily disabled for first playable
					if (!GameState.inst.GetRoomRevealedOnMap(roomName))
					{
						//newMapTile.SetActive(false); 
					}
				}
			}
		}

		//GameObject newMapTile = GameObject.Instantiate(mapTile); 

	}
	*/ 

	public void UpdateMap()
	{
		if (mapTiles == null)
			return; 

		for (int i = 0; i < mapTiles.GetLength(0); i++)
		{
			for (int j = 0; j < mapTiles.GetLength(1); j++)
			{
				for (int k = 0; k < mapTiles.GetLength(2); k++)
				{
					GameObject curMapTile = mapTiles[i, j, k]; 
					if (curMapTile != null && GameState.inst.GetGridSpaceRevealedOnMap(i + 1, j + 1, k + 1))
					{
						curMapTile.SetActive(true); 
					}
				}
			}
		}
	}

	/// <summary>
	/// Outdated version of UpdateMap
	/// When the player changes their grid position, check to see if the map should be updated
	/// </summary>
	/*
	public void UpdateMap()
	{
		if (mapTiles == null)
			return; 

		for (int i = 0; i < mapTiles.GetLength(0); i++)
		{
			for (int j = 0; j < mapTiles.GetLength(1); j++)
			{
				for (int k = 0; k < mapTiles.GetLength(2); k++)
				{
					GameObject curMapTile = mapTiles[i, j, k]; 
					if (curMapTile != null)
					{
						string roomName = SceneMapping.inst.GetSceneAt(i + 1, j + 1, k + 1); 

						if (roomName == "EMPTY")
						{
							Debug.LogError("UpdateMap: Attempted to check non-null map tile at an EMPTY scene position"); 
						}
						else if (GameState.inst.GetRoomRevealedOnMap(roomName))
						{
							curMapTile.SetActive(true); 
						}
					}
				}
			}
		}
	}
	*/ 

	/// <summary>
	/// Outdated function
	/// Used for choosing which map pieces to load to procedurally fill in a map
	/*
	void ChooseTileSprite(int level, int[] columnRow, string[] gridPositions, Image img, RectTransform rect)
	{
		// Choose which mapPieces struct of sprites should be used
		MapPieces curMapPieces = mapPieces[level];

		// List of booleans for adjacent grid pieces (are they within the same room?)
		bool t = false; // above
		bool b = false; // below
		bool l = false; // left
		bool r = false; // right

		// Iterate through all gridPositions and fill in bools above
		for (int i = 0; i < gridPositions.Length; i++)
		{
			int[] cr = LevelDataFunctions.GetColumnAndRowFromFull(gridPositions[i]); 
			int curCol = cr[0];
			int curRow = cr[1]; 

			if (curCol == columnRow[0] && curRow == columnRow[1])
				continue; 

			if (curCol == columnRow[0] && curRow == columnRow[1] + 1) 
				t = true; 
			else if (curCol == columnRow[0] && curRow == columnRow[1] - 1)
				b = true;
			else if (curCol == columnRow[0] - 1 && curRow == columnRow[1])
				l = true;
			else if (curCol == columnRow[0] + 1 && curRow == columnRow[1])
				r = true; 
		}

		//Debug.Log("Tile at L: " + (level+1) + ", (" + columnRow[0] + "," + columnRow[1] + ") -- t: " + t + ", b:" + b + ", l: " + l + ", r: " + r); 

		// With the bools filled in, choose the correct map tile

		// No adjacent rooms
		if (!t && !b && !l && !r)
		{
			img.sprite = curMapPieces.all;
		}
		// Top only
		else if (t && !b && !l && !r)
		{
			img.sprite = curMapPieces.threeSides; 
		}
		// Left only
		else if (!t && !b && l && !r)
		{
			img.sprite = curMapPieces.threeSides; 
			rect.rotation = Quaternion.Euler(0, 0, 90); 
		}
		// Bottom only
		else if (!t && b && !l && !r)
		{
			img.sprite = curMapPieces.threeSides; 
			rect.rotation = Quaternion.Euler(0, 0, 180); 
		}
		// Right only
		else if (!t && !b && !l && r)
		{
			img.sprite = curMapPieces.threeSides; 
			rect.rotation = Quaternion.Euler(0, 0, -90); 
		}
		// Top and bottom
		else if (t && b && !l && !r)
		{
			img.sprite = curMapPieces.twoSides; 
			rect.rotation = Quaternion.Euler(0, 0, 90); 
		}
		// Left and right
		else if (!t && !b && l && r)
		{
			img.sprite = curMapPieces.twoSides; 
		}
		// Top and left and right
		else if (t && !b && l && r)
		{
			img.sprite = curMapPieces.oneSide; 
			rect.rotation = Quaternion.Euler(0, 0, 180); 
		}
		// Bottom and left and right
		else if (!t && b && l && r)
		{
			img.sprite = curMapPieces.oneSide; 
		}
		// Top and left and bottom
		else if (t && b && l && !r)
		{
			img.sprite = curMapPieces.oneSide; 
			rect.rotation = Quaternion.Euler(0, 0, -90); 
		}
		// Top and right and bottom
		else if (t && b && !l && r)
		{
			img.sprite = curMapPieces.oneSide; 
			rect.rotation = Quaternion.Euler(0, 0, 90); 
		}
		// Top and left
		else if (t && !b && l && !r)
		{
			img.sprite = curMapPieces.corner; 
			rect.rotation = Quaternion.Euler(0, 0, 90); 
		}
		// Top and right
		else if (t && !b && !l && r)
		{
			img.sprite = curMapPieces.corner; 
		}
		// Bottom and left
		else if (!t && b && l && !r)
		{
			img.sprite = curMapPieces.corner; 
			rect.rotation = Quaternion.Euler(0, 0, 180); 
		}
		// Bottom and right
		else if (!t && b && !l && r)
		{
			img.sprite = curMapPieces.corner; 
			rect.rotation = Quaternion.Euler(0, 0, -90); 
		}
		// All
		else
		{
			img.sprite = curMapPieces.none;

		}
	}
	*/ 
	
	// Update is called once per frame
	// Maybe should call this from mainManager?
	void Update () 
	{
		// Temporary input
		// Use 'Q' key to reveal/hide status panel and map
		if (Input.GetKeyDown(KeyCode.Q))
		{
			statusPanel.SetActive(!statusPanel.activeSelf); 
		}
			
		if (floorSlider != null)
		{
			displayLevel = (int)(floorSlider.value); 
		}

		// Change current display level based on which level the player is on
		// Removed- the player should be able to change this at any time
		//displayLevel = LevelData.inst.curLevel; 

		// Update display level
		for (int i = 0; i < mapTileParents.Length; i++)
		{
			mapTileParents[i].SetActive(false); 
		}

		if (displayLevel > 0 && displayLevel <= mapTileParents.Length && mapTileParents[displayLevel - 1] != null)
		{
			mapTileParents[displayLevel - 1].SetActive(true); 
		}
	}
}

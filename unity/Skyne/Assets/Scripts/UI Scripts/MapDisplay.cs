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

	public bool generateNewMapTiles; 

	// 1, 2, or 3
	public int displayLevel; 

	public GameObject statusPanel; 
	 
	public GameObject blinkingTile; 
	public bool useBlinkingTile;
	public float blinkAlpha; 
	public float blinkSpeed;

	public GameObject dirArrow; 

	// See http://answers.unity3d.com/questions/976184/ui-recttransform-position-screen-resolution.html
	public CanvasScaler canvasScaler;
	private Vector2 screenScale
	{
		get
		{
			if (canvasScaler == null)
			{
				canvasScaler = GetComponentInParent<CanvasScaler>();
			}

			if (canvasScaler)
			{
				return new Vector2(canvasScaler.referenceResolution.x / Screen.width, canvasScaler.referenceResolution.y / Screen.height);
			}
			else
			{
				return Vector2.one;
			}
		}
	}



	/// <summary>
	/// Updated version of spawn map, which spawns map tiles based on a map image broken into pieces with Processing
	/// </summary>
	public void SpawnMap()
	{
		if (generateNewMapTiles)
		{
			// Clear mapTiles
			mapTiles = new GameObject[LevelData.inst.numLevels, LevelData.inst.numColumns, LevelData.inst.numRows];

			// Clear children of mapTileParents
			for (int i = 0; i < mapTileParents.Length; i++)
			{
				foreach (Transform child in mapTileParents[i].transform) 
					GameObject.Destroy(child.gameObject);
			}

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
						float xPos = mapTileParents[0].transform.position.x + column * mapTileSize; 
						float yPos = mapTileParents[0].transform.position.y + row * mapTileSize; 
						newMapTile.GetComponent<RectTransform>().position = new Vector3 (xPos, yPos, 0); 
						newMapTile.name = "L:" + (level + 1) + ", C:" + (column + 1) + ", R:" + (row + 1); 

						// Add each created tile to the array of stored map tiles
						mapTiles[level, column, row] = newMapTile; 

						// Pick the correct image for the map tile
						//Sprite test = newMapTile.GetComponent<Image>().sprite; 
						//LoadMapPieces test = GetComponent<LoadMapPieces>(); 
						LoadMapPieces loadMapPieces = GetComponent<LoadMapPieces>(); 

						newMapTile.GetComponent<Image>().sprite = loadMapPieces.GetLevelMapPieces(level + 1)[loadMapPieces.GetMapIndex(column, row)]; 

						// Update the gameObject's active property based on whether that map position has been discovered
						if (!GameState.inst.gridSpacesEntered[level, column, row])
						{
							newMapTile.SetActive(false); 
						}
					}
				}
			}
		}
		else
		{
			// Clear mapTiles
			mapTiles = new GameObject[LevelData.inst.numLevels, LevelData.inst.numColumns, LevelData.inst.numRows];

			for (int level = 0; level < LevelData.inst.numLevels; level++)
			{
				for (int column = 0; column < LevelData.inst.numColumns; column++)
				{
					for (int row = 0; row < LevelData.inst.numRows; row++)
					{
						string curName = "L:" + (level + 1) + ", C:" + (column + 1) + ", R:" + (row + 1); 
						GameObject curMapTile = mapTileParents[level].transform.Find(curName).gameObject; 

						// Add each found tile to the array of stored map tiles
						mapTiles[level, column, row] = curMapTile; 

						// Update the gameObject's active property based on whether that map position has been discovered
						if (!GameState.inst.gridSpacesEntered[level, column, row])
						{
							curMapTile.SetActive(false); 
						}
					}
				}
			}
		}
	}

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

	void Start()
	{
		if (useBlinkingTile)
			StartCoroutine("BlinkTile");
		else
			blinkingTile.SetActive(false); 
	}
	
	// Update is called once per frame
	// Maybe should call this from mainManager?
	void Update () 
	{			
		// Change current display level based on which level the player is on
		// Removed- the player should be able to change this at any time
		//displayLevel = LevelData.inst.curLevel; 

		if (GlobalManager.inst.globalState != GlobalManager.GlobalState.Gameplay)
		{
			return;
		}

		// Update display level
		for (int i = 0; i < mapTileParents.Length; i++)
		{
			mapTileParents[i].SetActive(false); 
		}

		if (displayLevel > 0 && displayLevel <= mapTileParents.Length && mapTileParents[displayLevel - 1] != null)
		{
			mapTileParents[displayLevel - 1].SetActive(true); 
		}

		// Update blinking tile
		// Currently, the blinking tile just uses the positions of tiles on the first index of the MapTile parents
		if (useBlinkingTile)
		{
			string curName = "L:" + (1) + ", C:" + (LevelData.inst.curColumn) + ", R:" + (LevelData.inst.curRow);
			//Debug.Log("Map curName: " + curName); 
			float xPos = mapTileParents[0].transform.Find(curName).position.x; 
			float yPos = mapTileParents[0].transform.Find(curName).position.y; 

			blinkingTile.GetComponent<RectTransform>().position = new Vector3 (xPos, yPos, 0); 

			if (LevelData.inst.curLevel != displayLevel)
			{
				Color curColor = blinkingTile.GetComponent<Image>().color; 
				blinkingTile.GetComponent<Image>().color = new Color (curColor.r, curColor.g, curColor.b, 0);
			}
		}

		// Update directional arrow
		// Update the arrow's direction based on which direction the player is facing
		dirArrow.transform.eulerAngles = new Vector3(0, 0, -LevelData.inst.player.transform.rotation.eulerAngles.y + 90); 

		// Then update the arrow's position
		if (mapTileSize != 0)
		{
			//dirArrow.GetComponent<RectTransform>().localPosition = new Vector3 (LevelData.inst.player.transform.position.x / mapTileSize, LevelData.inst.player.transform.position.z / mapTileSize, 0);
			//dirArrow.GetComponent<RectTransform>().localPosition = new Vector3 (LevelData.inst.player.transform.position.x, LevelData.inst.player.transform.position.z, 0);

			//Debug.Log("ArrowX: " + LevelData.inst.player.transform.position.x / mapTileSize); 

			//float posX = (LevelData.inst.player.transform.position.x / LevelData.inst.gridEdgeSize) * mapTileSize * screenScale.x; 
			//float posY = (LevelData.inst.player.transform.position.z / LevelData.inst.gridEdgeSize) * mapTileSize * screenScale.y;
			//float posX = (LevelData.inst.player.transform.position.x / (LevelData.inst.gridEdgeSize)) * mapTileSize * screenScale.x;  
			//float posY = (LevelData.inst.player.transform.position.z / (LevelData.inst.gridEdgeSize)) * mapTileSize * screenScale.y; 

			// First, calculate, based on the player's position in the world, where they should fall based on the map tile layout
			// For example, if the player is at an x position of 48 in Unity units, that translates to 1.5 in terms of map tiles
			float tileX = LevelData.inst.player.transform.position.x / LevelData.inst.gridEdgeSize; 
			float tileY = LevelData.inst.player.transform.position.z / LevelData.inst.gridEdgeSize; 

			// With the map tile position, figure out how to multiply the arrow to the right position 
			// TODO: Figure out how to get the right position, especially with canvas scaling messing it up
			float posX = tileX * 58.5833f;
			float posY = tileY * 58.5833f; 

			dirArrow.GetComponent<RectTransform>().anchoredPosition = new Vector3 (posX, posY, 0);
		}

	}

	IEnumerator BlinkTile()
	{
		while (true)
		{
			if (LevelData.inst.curLevel == displayLevel)
			{
				Color curColor = blinkingTile.GetComponent<Image>().color; 

				if (curColor.a != 0)
					blinkingTile.GetComponent<Image>().color = new Color (curColor.r, curColor.g, curColor.b, 0);
				else
					blinkingTile.GetComponent<Image>().color = new Color (curColor.r, curColor.g, curColor.b, blinkAlpha);
			}

			yield return new WaitForSecondsRealtime(blinkSpeed);
		}
	}

	public void LevelToggleClicked(int toggledLevel)
	{
		//displayLevel = toggledLevel; 
	}

	public void IncrementDisplayFloor()
	{
		if (displayLevel < 3)
			displayLevel++; 
	}

	public void DecrementDisplayFloor()
	{
		if (displayLevel > 1)
			displayLevel--; 
	}
}

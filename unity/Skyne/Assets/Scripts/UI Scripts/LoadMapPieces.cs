using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityEditor;

[ExecuteInEditMode]
public class LoadMapPieces : MonoBehaviour 
{
	public bool loadMapPieces; 
	public string mapLoadPath; 

	public int numColumns; 
	public int numRows; 

	// These must be 1D arrays because Unity can't serialize 2D arrays
	// Hence, 2D arrays can't have their data loaded into the inspector and preserved when the game starts (Bad Unity!)
	public Sprite[] level1MapPieces; 
	public Sprite[] level2MapPieces; 
	public Sprite[] level3MapPieces; 
	
	// Update is called once per frame
	void Update () 
	{
		#if UNITY_EDITOR

		if (loadMapPieces)
		{
			loadMapPieces = false; 
			LoadAllMapLevels();
		}

		#endif
	}

	void LoadAllMapLevels()
	{
		LoadMapLevel(1); 
		LoadMapLevel(2); 
		LoadMapLevel(3); 

		if (level1MapPieces == null || level2MapPieces == null || level3MapPieces == null)
		{
			EditorUtility.DisplayDialog("Load Error", "There was a problem loading the map pieces", "Ok"); 
		}
		else
		{
			EditorUtility.DisplayDialog("Loaded map pieces", "All map pieces have been loaded from assets", "Ok"); 
		}
	}

	void LoadMapLevel(int loadLevel)
	{
		Sprite[] curMapPieces = new Sprite[numColumns * numRows];

		//Sprite[,] curMapPieces = GetLevelMapPieces(loadLevel); 

		for (int x = 0; x < numColumns; x++)
		{
			for (int y = 0; y < numRows; y++)
			{
				string loadPath = mapLoadPath + "/level" + loadLevel + "/" + x + "-" + y;
				//Debug.Log("loadPath: " + loadPath); 
				//Debug.Log("load into: " + GetMapIndex(x, y)); 

				curMapPieces[GetMapIndex(x,y)] = (Sprite)AssetDatabase.LoadAssetAtPath(loadPath + ".png", typeof(Sprite)); 
			}
		}
		SetLevelMapPieces(loadLevel, curMapPieces); 
	}

	public int GetMapIndex(int x, int y)
	{
		return x + y * numColumns; 

	}

	// Somewhat hard-coded subtitute for creating an array of 2D Sprite arrays (painful)
	// Instead just returns the arbitrary level#MapPieces array based on an index
	public Sprite[] GetLevelMapPieces(int getLevel)
	{
		if (level1MapPieces == null)
			Debug.LogError("level1MapPieces null");
		if (level2MapPieces == null)
			Debug.LogError("level2MapPieces null"); 
		if (level3MapPieces == null)
			Debug.LogError("level3MapPieces null"); 

		if (getLevel == 1)
			return level1MapPieces;
		else if (getLevel == 2)
			return level2MapPieces;
		else if (getLevel == 3)
			return level3MapPieces;
		else
		{
			Debug.LogError("Tried to return a map pieces array that isn't labeled between 1 - 3"); 
			return level1MapPieces; 
		}
	}

	public void SetLevelMapPieces (int setLevel, Sprite[] newSprites)
	{
		if (setLevel == 1)
		{
			level1MapPieces = newSprites;  
		}
		else if (setLevel == 2)
			level2MapPieces = newSprites; 
		else
			level3MapPieces = newSprites; 
	}
}

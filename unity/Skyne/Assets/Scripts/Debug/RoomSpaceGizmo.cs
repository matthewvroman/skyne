using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent (typeof (GridAlign))]
public class RoomSpaceGizmo : MonoBehaviour 
{
	public bool vizOnSelect; 
	public bool alwaysVisualize; 
	public Color32 roomColor;

	GridAlign gridAlign; 

	string sceneName; 
	int[] levels; 

	public bool drawSceneOrigin; 


	void Update()
	{
		if (gridAlign == null)
			gridAlign = GetComponent<GridAlign>(); 

		sceneName = gameObject.scene.name; 
		levels = LevelDataFunctions.GetLevelsFrom(sceneName); 
		//LevelDataFunctions.Get
	}

	void OnDrawGizmos()
	{
		if (alwaysVisualize)
		{
			DrawCubes(false); 
		}

	}

	void OnDrawGizmosSelected() 
	{
		if (vizOnSelect)
		{
			DrawCubes(true); 
		}

		if (drawSceneOrigin)
		{
			Gizmos.color = Color.red; 
			Gizmos.DrawSphere(transform.position, 2);
		}
			
	}

	void DrawCubes(bool selected)
	{
		// Iterate through each level
		for (int l = 0; l < levels.Length; l++)
		{
			int curLevel = levels[l];

			string gPositions = LevelDataFunctions.GetColumnAndRowSubstringFrom(sceneName); 
			string[] gridPositions = gPositions.Split(','); 

			// Iterate through each row and column position
			foreach (string curGridPos in gridPositions)
			{
				int[] columnRow = LevelDataFunctions.GetColumnAndRowFrom(curGridPos); 

				// Draw a gizmo box at the level, column, and row
				// NOTE: this assumes height is determined by the grid divisor
				float posX = (columnRow[0] - 1) * gridAlign.gridDivisor.x + gridAlign.gridDivisor.x/2;
				float posY = (curLevel - 1) * gridAlign.gridDivisor.y + gridAlign.gridDivisor.y/2; 
				float posZ = (columnRow[1] - 1) * gridAlign.gridDivisor.z + gridAlign.gridDivisor.z/2; 
				Vector3 pos = new Vector3 (posX, posY, posZ); 
				Vector3 size = new Vector3 (gridAlign.gridDivisor.x, gridAlign.gridDivisor.y, gridAlign.gridDivisor.z); 

				Gizmos.color = (Color)(roomColor);
				Gizmos.DrawCube(pos, size); 

				if (!selected)
					Gizmos.color = Color.black;
				else
					Gizmos.color = Color.white; 
				Gizmos.DrawWireCube(pos, size); 

			}
		}
	}
}

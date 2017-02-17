using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGizmos : MonoBehaviour
{
	public int numColumns;
	public int numRows; 
	public float gridEdgeSize; 
	public bool drawCorners; 

	public bool drawGridLabels; 

	void OnDrawGizmos() 
	{
		Gizmos.color = Color.yellow;

		//Gizmos.DrawWireSphere(new Vector3(0, 0, 0), 5);

		for (int column = 0; column < numColumns + 1; column++)
		{
			for (int row = 0; row < numRows + 1; row++)
			{
				if (drawCorners)
				{
					Gizmos.DrawSphere(new Vector3 (column * gridEdgeSize, 0, row * gridEdgeSize), 0.8f);
					Gizmos.DrawSphere(new Vector3 (column * gridEdgeSize, 32, row * gridEdgeSize), 0.8f);
					Gizmos.DrawSphere(new Vector3 (column * gridEdgeSize, 64, row * gridEdgeSize), 0.8f);
					Gizmos.DrawSphere(new Vector3 (column * gridEdgeSize, 96, row * gridEdgeSize), 0.8f);
				}
			}
		}

		if (drawGridLabels)
		{
			for (int column = 1; column <= numColumns; column++)
			{
				Gizmos.DrawIcon(new Vector3((column-1) * gridEdgeSize + (gridEdgeSize/2), 0, -12), "label-" + ((char)(column + 64)) + ".png", true);
			}

			for (int row = 1; row <= numRows; row++)
			{
				
				Gizmos.DrawIcon(new Vector3(-12, 0, (row-1) * gridEdgeSize + (gridEdgeSize/2)), "label-" + row + ".png", true);
			}

			// Levels
			Gizmos.DrawIcon(new Vector3(-12, 16, -12), "label-" + 1 + ".png", true);
			Gizmos.DrawIcon(new Vector3(-12, 48, -12), "label-" + 2 + ".png", true);
			Gizmos.DrawIcon(new Vector3(-12, 80, -12), "label-" + 3 + ".png", true);

		}

			
	}
}

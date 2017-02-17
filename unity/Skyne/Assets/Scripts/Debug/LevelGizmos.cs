using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGizmos : MonoBehaviour
{
	public int numColumns;
	public int numRows; 
	public float gridEdgeSize; 
	public bool drawCorners; 

	void OnDrawGizmos() 
	{
		Gizmos.color = Color.yellow;

		//Gizmos.DrawWireSphere(new Vector3(0, 0, 0), 5);

		for (int column = 0; column < numColumns; column++)
		{
			for (int row = 0; row < numRows; row++)
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
			
	}
}

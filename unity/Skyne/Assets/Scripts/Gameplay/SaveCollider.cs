using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveCollider : MonoBehaviour 
{
	public bool playerInside; 

	void OnTriggerEnter (Collider col)
	{
		if (!GlobalManager.inst.GameplayIsActive())
		{
			return; 
		}

		if (col.tag == "Player")
		{
			playerInside = true;  
		}
	}

	void OnTriggerExit (Collider col)
	{
		if (col.tag == "Player")
		{
			playerInside = false; 
		}
	}


}

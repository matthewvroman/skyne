using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasurePickup : MonoBehaviour 
{
	void Start()
	{
		/*
		if (GameState.inst.treasureFound)
			Destroy(this.gameObject);
			*/ 
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Player")
		{
			//GameState.inst.SetTreasureFound(true); 
			GlobalManager.inst.LoadOutro(); 

			Destroy(this.gameObject);
		}
	}
}

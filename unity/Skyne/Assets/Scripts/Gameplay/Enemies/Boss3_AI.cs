using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss3_AI : Enemy 
{

	void SetupEnemy ()
	{
		started = true;
		alive = true; 
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (GlobalManager.inst.GameplayIsActive())
		{
			// TODO- this is a temporary fix to solve issues with level loading
			if (!started && GameObject.FindGameObjectWithTag("Player") != null)
			{
				SetupEnemy(); 
			}
		}
	}

	protected override void EnemyDestroy()
	{
		GameState.inst.keysFound [2] = true;
		//KeyPickupManager.inst.SpawnKeyPickup(transform.position, 2); 
	}
}

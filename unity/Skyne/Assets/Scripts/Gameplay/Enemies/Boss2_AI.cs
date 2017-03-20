using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss2_AI : Enemy 
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
		//Debug.Log("Enemy destroyed called"); 
		GameState.inst.keysFound [1] = true;
		//KeyPickupManager.inst.SpawnKeyPickup(transform.position, 1); 
	}
}

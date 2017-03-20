using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss3_AI : Enemy 
{

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	protected override void EnemyDestroy()
	{
		GameState.inst.keysFound [2] = true;
		KeyPickupManager.inst.SpawnKeyPickup(transform.position, 2); 
	}
}

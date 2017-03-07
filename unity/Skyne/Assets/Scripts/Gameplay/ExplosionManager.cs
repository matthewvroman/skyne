﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : Singleton<ExplosionManager> 
{
	public GameObject enemyExplosionPrefab; 

	public void SpawnEnemyExplosion(Vector3 spawnPos)
	{
		GameObject newExplosion = GameObject.Instantiate(enemyExplosionPrefab, spawnPos, Quaternion.identity, transform); 
	}


}

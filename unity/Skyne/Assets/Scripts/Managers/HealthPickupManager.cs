using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickupManager : Singleton<HealthPickupManager> 
{
	public GameObject healthPickupPrefab; 

	/// <summary>
	/// Tries the spawn health pickup. Returns true if a pickup was spawned
	/// </summary>
	/// <param name="spawner">Spawner.</param>
	/// <param name="percChance">Perc chance (0 - 100).</param>
	public bool TrySpawnHealthPickup(Vector3 spawnPos, float percChance)
	{
		float chance = Random.Range(0, 100); 

		if (chance <= percChance)
		{
			GameObject newHealthPickup = GameObject.Instantiate(healthPickupPrefab, spawnPos, Quaternion.identity, transform); 
			return true;
		}
		return false; 
	}
}

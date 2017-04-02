using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickupManager : Singleton<HealthPickupManager> 
{
	public GameObject healthPickupPrefab; 

	// Spawn velocity variables
	public float upwardSpawnForce;
	public float minOutMultiplier; 
	public float maxOutMultiplier; 

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

	/// <summary>
	/// Spawns health pickups based on a min and max value
	/// </summary>
	public void SpawnHealthPickups(Vector3 spawnPos, float min, float max)
	{
		int numSpawn = Mathf.RoundToInt(Random.Range(min, max)); 

		if (numSpawn <= 0)
		{
			return; 
		}

		float angleStep = 360 / numSpawn; 

		for (int i = 0; i < numSpawn; i++)
		{
			GameObject newHealthPickup = GameObject.Instantiate(healthPickupPrefab, spawnPos, Quaternion.identity, transform); 

			// Calculate rotation angle
			float rotAngle = i * angleStep; 

			newHealthPickup.transform.Rotate(new Vector3 (0, rotAngle, 0)); 

			Rigidbody rb = newHealthPickup.GetComponent<Rigidbody>(); 

			float outMultiplier = Random.Range(minOutMultiplier, maxOutMultiplier); 

			//rb.AddRelativeForce(new Vector3(0, 4, 0) + (transform.forward * 1.5f), ForceMode.Impulse); 
			rb.AddRelativeForce(new Vector3(0, upwardSpawnForce, 0) + (transform.forward * outMultiplier), ForceMode.Impulse); 
			 
		}
	}

	public void SpawnHealthPickups(Vector3 spawnPos, int[] choices)
	{
		if (choices.Length == 0)
		{
			return; 
		}

		int choosenIndex = Mathf.RoundToInt(Random.Range(0, choices.Length - 1)); 
		int numSpawn = choices[choosenIndex]; 

		if (numSpawn <= 0)
		{
			return; 
		}

		float angleStep = 360 / numSpawn; 

		for (int i = 0; i < numSpawn; i++)
		{
			GameObject newHealthPickup = GameObject.Instantiate(healthPickupPrefab, spawnPos, Quaternion.identity, transform); 

			// Calculate rotation angle
			float rotAngle = i * angleStep; 

			newHealthPickup.transform.Rotate(new Vector3 (0, rotAngle, 0)); 

			Rigidbody rb = newHealthPickup.GetComponent<Rigidbody>(); 

			float outMultiplier = Random.Range(minOutMultiplier, maxOutMultiplier); 

			//rb.AddRelativeForce(new Vector3(0, 4, 0) + (transform.forward * 1.5f), ForceMode.Impulse); 
			rb.AddRelativeForce(new Vector3(0, upwardSpawnForce, 0) + (transform.forward * outMultiplier), ForceMode.Impulse); 

		}
	}
}

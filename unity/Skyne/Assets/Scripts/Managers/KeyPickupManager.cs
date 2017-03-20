using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPickupManager : Singleton<KeyPickupManager> 
{
	public GameObject keyPickupPrefab; 

	public void SpawnKeyPickup(Vector3 spawnPos, int keyIndex)
	{
		GameObject newKeyPickup = GameObject.Instantiate(keyPickupPrefab, spawnPos, Quaternion.identity, transform); 
	}
}

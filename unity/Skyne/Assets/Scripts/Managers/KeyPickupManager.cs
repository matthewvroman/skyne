using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPickupManager : Singleton<KeyPickupManager> 
{
	public GameObject keyPickupPrefab; 

	public void SpawnKeyPickup(Vector3 spawnPos, int keyIndex)
	{
		Debug.Log("SpawnKeyPickup()"); 
		GameObject newKeyPickup = GameObject.Instantiate(keyPickupPrefab, spawnPos, Quaternion.identity, transform); 
		newKeyPickup.GetComponent<KeyPickup>().keyIndex = keyIndex; 
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPickup : MonoBehaviour 
{
	[Header("Keys: (0 = Boss1) (1 = Boss2) (3 = Boss3)")]
	public int keyIndex; 

	// Use this for initialization
	void Start () 
	{
		// Destroy the upgrade pickup if the player already has it
		//Debug.Log("upgradesFound length: " + GameState.inst.upgradesFound.Length); 

		if (GameState.inst.keysFound[keyIndex])
		{
			Destroy(this.gameObject); 
		}
	}

	// Update is called once per frame
	void Update () {

	}

	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Player")
		{
			if (GameState.inst.keysFound[keyIndex] != null)
			{
				GameState.inst.keysFound[keyIndex] = true; 
			}

			Destroy(this.gameObject); 
		}
	}
}

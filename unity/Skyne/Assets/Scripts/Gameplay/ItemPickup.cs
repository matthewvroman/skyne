using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour 
{
	[Header("Abilities: (0 = double_j) (1 = wall_j) (2 = dash)")]
	[Header("Weapons: (3 = charge) (4 = wide) (5 = rapid)")]
	[Header("Keys: (6 = key1) (7 = key2) (8 = key3)")]
	[Tooltip("What type of item is this?")]
	public int itemTypeIndex; 

	// Use this for initialization
	void Start () 
	{
		// Destroy the upgrade pickup if the player already has it
		Debug.Log("upgradesFound length: " + GameState.inst.upgradesFound.Length); 

		if (GameState.inst.upgradesFound[itemTypeIndex])
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
			if (GameState.inst.upgradesFound[itemTypeIndex] != null)
			{
				GameState.inst.upgradesFound[itemTypeIndex] = true; 

				// If charge found, set current beam to charge
				if (itemTypeIndex == 3)
				{
					PlayerShooting.inst.ChangeWeaponType(PlayerShooting.PlayerShootMode.Charge); 
				}
				else if (itemTypeIndex == 4)
				{
					PlayerShooting.inst.ChangeWeaponType(PlayerShooting.PlayerShootMode.Wide); 
				}
				else if (itemTypeIndex == 5)
				{
					PlayerShooting.inst.ChangeWeaponType(PlayerShooting.PlayerShootMode.Rapid); 
				}
			}

			Destroy(this.gameObject); 
		}
	}
}

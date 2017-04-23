using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour 
{
	[Header("Abilities: (0 = double_j) (1 = wall_j) (2 = dash)")]
	[Header("Weapons: (3 = charge) (4 = wide) (5 = rapid)")]
	public int itemTypeIndex; 

	AudioSource audio1;
	public AudioClip equipSound;

	// Use this for initialization
	void Start () 
	{
		// Destroy the upgrade pickup if the player already has it
		//Debug.Log("upgradesFound length: " + GameState.inst.upgradesFound.Length); 

		audio1 = GetComponent<AudioSource>(); 
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (!GlobalManager.inst.GameplayIsActive())
		{
			return; 
		}

		if (GameState.inst.upgradesFound[itemTypeIndex])
		{
			Destroy(this.gameObject); 
		}
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Player")
		{
			//audio1.PlayOneShot (equipSound);

			if (GameState.inst.upgradesFound[itemTypeIndex] != null)
			{
				GameState.inst.upgradesFound[itemTypeIndex] = true; 

				MessageTrigger messageTrigger = GetComponent<MessageTrigger>(); 
				if (messageTrigger != null)
				{
					messageTrigger.TriggerMessage(); 
				}

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

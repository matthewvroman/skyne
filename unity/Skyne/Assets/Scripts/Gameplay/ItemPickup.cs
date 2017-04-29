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

	bool waitForDestroy; 

	public ParticleSystem itemParticles1; 
	public ParticleSystem itemParticles2; 
	public Light itemPointLight; 
	public float lightFadeSpeed; 
	public GameObject model; 

	// Use this for initialization
	void Start () 
	{
		// Destroy the upgrade pickup if the player already has it
		//Debug.Log("upgradesFound length: " + GameState.inst.upgradesFound.Length); 
		waitForDestroy = false; 

		audio1 = GetComponent<AudioSource>(); 
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (!GlobalManager.inst.GameplayIsActive())
		{
			return; 
		}

		if (!waitForDestroy && GameState.inst.upgradesFound[itemTypeIndex])
		{
			Destroy(this.gameObject); 
		}

		if (waitForDestroy)
		{
			// Fade out the light
			if (itemPointLight != null && itemPointLight.intensity > 0)
			{
				itemPointLight.intensity -= lightFadeSpeed * Time.deltaTime; 

				if (itemPointLight.intensity < 0)
					itemPointLight.intensity = 0; 
			}

			if (CheckDestroy())
			{
				Destroy(this.gameObject); 
			}
		}
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Player" && !waitForDestroy)
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

			//Destroy(this.gameObject); 
			DestroyItem(); 
		}
	}

	void DestroyItem()
	{
		Collider col = GetComponent<Collider>(); 
		if (col != null)
		{
			col.enabled = false; 
		}

		if (model != null)
		{
			model.SetActive(false); 
		}

		waitForDestroy = true; 

		if (itemParticles1 != null)
		{
			itemParticles1.enableEmission = false;
		}
		if (itemParticles1 != null)
		{
			itemParticles2.enableEmission = false; 
		}

		if (audio1 != null)
		{
			audio1.mute = true; 
		}
	}


	bool CheckDestroy()
	{
		if (itemParticles1 != null)
		{
			if (itemParticles1.isPlaying)
			{
				return false; 
			}
		}

		if (itemParticles2 != null)
		{
			if (itemParticles2.isPlaying)
			{
				return false; 
			}
		}

		if (itemPointLight != null)
		{
			if (itemPointLight.intensity > 0)
			{
				return false; 
			}
		}

		return true; 
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaPickup : MonoBehaviour 
{
	public int staminaPickupIndex; 

	AudioSource audio1; 
	public AudioClip collectAudio; 

	// Use this for initialization
	void Start () 
	{
		audio1 = GetComponent<AudioSource> ();
	}

	void Update()
	{
		if (!GlobalManager.inst.GameplayIsActive())
		{
			return; 
		}

		if (GameState.inst.staminaPickupsFound[staminaPickupIndex])
		{
			Destroy(this.gameObject); 
		}
	}
	
	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Player")
		{
			if (GameState.inst.staminaPickupsFound[staminaPickupIndex] != null)
			{
				GameState.inst.staminaPickupsFound[staminaPickupIndex] = true; 
			}
				
			MessageTrigger messageTrigger = GetComponent<MessageTrigger>(); 
			if (messageTrigger != null)
			{
				messageTrigger.TriggerMessage(); 
			}

			Destroy(this.gameObject); 
		}
	}
}

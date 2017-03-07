using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour 
{
	public float healthValue; 

	public float duration; 

	public bool noDurationCountdown; 

	float durationTimer; 

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (!noDurationCountdown && durationTimer > 0)
		{
			durationTimer -= Time.deltaTime; 

			if (durationTimer < 0)
			{
				Destroy(this.gameObject); 
			}
		}
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Player")
		{
			PlayerManager.HealCalculator(healthValue); 
			Destroy(this.gameObject);
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasurePickup : MonoBehaviour 
{
	Rigidbody rb;
	public float moveSpeed; 
	public bool moving; 

	public Light pointLight;
	public float maxIntensity; 
	public float increaseSpeed; 

	void Start()
	{
		/*
		if (GameState.inst.treasureFound)
			Destroy(this.gameObject);
			*/
		rb = GetComponent<Rigidbody>(); 
		pointLight.intensity = 0; 
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Player")
		{
			//GameState.inst.SetTreasureFound(true); 
			GlobalManager.inst.LoadOutro(); 

			Destroy(this.gameObject);
		}
	}

	void OnCollisionStay(Collision col)
	{
		if (col.collider.tag != "Enemy")
		{
			moving = false; 
		}
	}

	void FixedUpdate()
	{
		if (moving)
		{
			//rb.AddRelativeForce(transform.forward * moveSpeed); 
			rb.AddForce(transform.forward * moveSpeed); 
		}
		else
		{
			rb.velocity = Vector3.zero; 
		}
	}

	void Update()
	{
		if (pointLight.intensity < maxIntensity)
		{
			pointLight.intensity += increaseSpeed * Time.deltaTime;

			if (pointLight.intensity > maxIntensity)
				pointLight.intensity = maxIntensity; 
		}
	}
}

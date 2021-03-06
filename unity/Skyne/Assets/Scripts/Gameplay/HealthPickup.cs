﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour 
{
	public float healthValue; 

	public float duration; 

	public bool noDurationCountdown; 

	float durationTimer; 

	Rigidbody rb; 

	AudioSource audio1;

	SphereCollider myCollider;

	public AudioClip heathPickup;
	public AudioClip heathPickup2;
	public AudioClip heathPickup3;

	public bool hitGround; 
	public bool startedHoming; 

	public bool canHome; 
	public GameObject targetObj; 
	public float homingRange; 
	public float turnSpeed; 
	public float speed; 

	public float gravityMultiplier; 

	[Tooltip("Affects time scaling during slowdown. See notes in Timescaler.CalculateDeltaTime.")]
	public float deltaTimePerc; 

	[Tooltip("Assign the non-trigger collider here so it can be called to ignore collision with the player.")]
	public SphereCollider physicsCollider; 
	public SphereCollider triggerCollider; 

	public ParticleSystem[] particleSystems; 
	public Light light; 
	public bool waitForDestroy; 

	// Use this for initialization
	void Start () 
	{
		rb = GetComponent<Rigidbody>(); 
		triggerCollider.enabled = false; 

		durationTimer = duration; 

		myCollider = GetComponent<SphereCollider> ();

		audio1 = this.GetComponent<AudioSource> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (GlobalManager.inst.GameplayIsActive() && targetObj == null)
		{
			if (GameObject.FindObjectOfType<PlayerManager>().gameObject != null)
			{
				targetObj = GameObject.FindObjectOfType<PlayerManager>().gameObject; 
			}
		}

		if (!noDurationCountdown && durationTimer > 0)
		{
			durationTimer -= Time.deltaTime; 

			if (durationTimer <= 0)
			{

				triggerCollider.enabled = false; 
				//Destroy(this.gameObject); 
				waitForDestroy = true; 

				for (int i = 0; i < particleSystems.Length; i++)
				{
					particleSystems[i].enableEmission = false; 
				}
				if (light != null)
				{
					light.intensity = 0; 
				}
			}
		}

		if (waitForDestroy)
		{
			bool shouldDestroy = true; 
			rb.velocity = Vector3.zero; 

			for (int i = 0; i < particleSystems.Length; i++)
			{
				if (particleSystems[i].isPlaying)
				{
					shouldDestroy = false; 
				}
			}

			if (shouldDestroy)
			{
				Destroy(this.gameObject); 
			}
		}
	}

	void FixedUpdate()
	{
		if (!startedHoming && !waitForDestroy)
		{
			rb.AddForce(-Vector3.up * gravityMultiplier); 
		}

		if (targetObj != null && canHome && hitGround && !waitForDestroy)
		{
			Vector3 targetDir = targetObj.transform.position - transform.position;
			float step = turnSpeed * Timescaler.inst.CalculateDeltaTime(deltaTimePerc);
			Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
			transform.rotation = Quaternion.LookRotation(newDir);

			if (!startedHoming && Vector3.Distance(transform.position, targetObj.transform.position) <= homingRange)
			{
				startedHoming = true; 
				rb.useGravity = false; 
				physicsCollider.enabled = false; 
				triggerCollider.enabled = true; 
			}

			if (startedHoming)
			{
				if (Time.timeScale != 0)
				{
					rb.velocity = transform.forward * Timescaler.inst.GetFixedUpdateSpeed(speed, deltaTimePerc); 
				}
			}
		}
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Player" && !waitForDestroy)
		{
			PlayerManager.HealCalculator(healthValue); 
			int randNum;

			randNum = Random.Range (1, 4);

			switch (randNum)
			{
			case 1:
				//audio1.PlayOneShot(heathPickup);
				GlobalAudio.inst.PlaySound(heathPickup); 
				break;

			case 2: 
				//audio1.PlayOneShot (heathPickup2);
				GlobalAudio.inst.PlaySound(heathPickup2); 
				break;

			case 3:
				//audio1.PlayOneShot (heathPickup3);
				GlobalAudio.inst.PlaySound(heathPickup3);
				break;
			}
			ExplosionManager.inst.SpawnHealthPickupExplosion(transform.position); 

			myCollider.enabled = false;
			//Destroy(this.gameObject, 0.8f);
			Destroy(this.gameObject);
		}
	}

	void OnCollisionEnter(Collision col)
	{
		hitGround = true; 
		//rb.useGravity = false; 
		//physicsCollider.enabled = false; 
		//triggerCollider.enabled = true; 
	}
}

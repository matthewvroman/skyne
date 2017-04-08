using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour 
{
	public float healthValue; 

	public float duration; 

	public bool noDurationCountdown; 

	float durationTimer; 

	Rigidbody rb; 

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

	// Use this for initialization
	void Start () 
	{
		rb = GetComponent<Rigidbody>(); 
		triggerCollider.enabled = false; 
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

			if (durationTimer < 0)
			{
				Destroy(this.gameObject); 
			}
		}
	}

	void FixedUpdate()
	{
		if (!startedHoming)
		{
			rb.AddForce(-Vector3.up * gravityMultiplier); 
		}

		if (targetObj != null && canHome && hitGround)
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
		if (col.tag == "Player")
		{
			PlayerManager.HealCalculator(healthValue); 
			ExplosionManager.inst.SpawnHealthPickupExplosion(transform.position); 
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

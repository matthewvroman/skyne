using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour 
{
	[Tooltip("True if this bullet originates from a player and is friendly; false otherwise")]
	public bool playerBullet; 

	[Tooltip("The bullet's speed multiplier")]
	public float speed; 

	[Tooltip("Damage this individual bullet inflicts")]
	public float damage;

	[Tooltip("How long does this bullet move (in seconds) before it is destroyed")]
	public float lifetime; 

	public bool shouldDestroy; 

	float lifetimeTimer; 

	// when time slows, what percent is used in the deltaTime calculation
	[Tooltip("Affects time scaling during slowdown. See notes in Timescaler.CalculateDeltaTime.")]
	public float deltaTimePerc; 

	public bool hasTarget; 
	public Vector3 target; 

	public GameObject targetObj; 

	public bool homingBullet; 
	public float turnSpeed; 

	Rigidbody rb; 

	[Tooltip("Indicates that the player can shoot this bullet (must be marked as !playerBullet)")]
	public bool playerShootable; 

	[Tooltip("The health of a playerShootable bullet")]
	public float health; 

	// Use this for initialization
	void Start () 
	{
		lifetimeTimer = lifetime;

		if (hasTarget)
		{
			transform.LookAt(target);
		}

		rb = GetComponent<Rigidbody>(); 
	}

	// Update is called once per frame
	void Update ()
	{
		// If homing, make the bullet turn towards the target (if the target exists)
		if (targetObj != null && homingBullet)
		{
			
			Vector3 targetDir = targetObj.transform.position - transform.position;
			float step = turnSpeed * Timescaler.inst.CalculateDeltaTime(deltaTimePerc);
			Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
			transform.rotation = Quaternion.LookRotation(newDir);

		}

		// Don't use a rigidbody for movement; use manual movement with custom deltaTime instead
		//transform.position += transform.forward * Timescaler.inst.CalculateDeltaTime(deltaTimePerc) * speed;

		// Try this with a rigidbody, but not in fixedUpdate ()
		//Vector3 newPos = transform.position + transform.forward * Timescaler.inst.CalculateDeltaTime(deltaTimePerc) * speed;
		//rb.MovePosition(newPos); 

		// Reduce the lifetime (based on the calculated deltaTime)
		lifetimeTimer -= Timescaler.inst.CalculateDeltaTime(deltaTimePerc); 

		// Destroy the bullet when the lifetimeTimer runs out
		if (lifetimeTimer <= 0)
			shouldDestroy = true; 

		if (playerShootable && health <= 0)
			shouldDestroy = true; 

		if (shouldDestroy)
			DestroyBullet(); 
	}

	void FixedUpdate ()
	{
		//Vector3 newPos = transform.position + transform.forward * Timescaler.inst.CalculateDeltaTime(deltaTimePerc) * speed;

		// Attempt at using a rigidbody
		//rb.MovePosition(newPos); 

		// Attempt at calculating new velocity based on time scale differences
		// This would be used for normal rigidbody speed
		// However, this math is probably wrong
		//float timescaleDiff = Timescaler.inst.CalculateDeltaTime(deltaTimePerc) - Time.timeScale; 
		//float newSpeed = speed + (speed * timescaleDiff); 

		//rb.velocity = transform.forward * newSpeed; 

		//rb.velocity = transform.forward * speed * Timescaler.inst.CalculateTimescale(deltaTimePerc); 

		//rb.velocity = transform.forward * (speed + speed * (Timescaler.inst.CalculateTimescale(deltaTimePerc) - Time.timeScale));

		// Problem, this is doubling the speed, but it should be increasing tenfold with the current numbers
		//rb.velocity = transform.forward * (speed + speed * (Timescaler.inst.CalculateTimescale(deltaTimePerc) - Time.timeScale)); 

		if (Time.timeScale != 0)
		{
			//rb.velocity = transform.forward * (speed / Time.timeScale) * deltaTimePerc;
			//rb.velocity = transform.forward * (speed / Time.timeScale) * Timescaler.inst.CalculateDeltaTimePerc(deltaTimePerc);
			rb.velocity = transform.forward * Timescaler.inst.GetFixedUpdateSpeed(speed, deltaTimePerc); 
		}
	}

	void OnTriggerEnter(Collider col)
	{
		// Collision with another bullet of the same type
		if (col.tag == "Bullet" && col.GetComponent<Bullet>().playerBullet == playerBullet)
		{
			return; 
		}
			
		if (playerBullet)
		{
			if (col.tag != "Player" && col.tag != "Enemy")
			{
				//Debug.Log("Set shouldDestroy to true"); 
				shouldDestroy = true; 
			}
		}
		else
		{
			if (playerShootable && col.tag == "Bullet" && col.GetComponent<Bullet>().playerBullet)
			{
				health -= col.GetComponent<Bullet>().damage; 
			}
			else if (col.tag != "Enemy")
			{
				shouldDestroy = true; 
			}
		}
	}

	/// <summary>
	/// Private function for destroying the bullet
	/// To activate, set shouldDestroy = true, which will call this function on the next Update()
	/// </summary>
	void DestroyBullet()
	{
		Destroy(this.gameObject); 
	}
}

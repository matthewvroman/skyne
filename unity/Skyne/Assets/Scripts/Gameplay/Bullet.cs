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

	// Use this for initialization
	void Start () 
	{
		lifetimeTimer = lifetime;

		if (hasTarget)
		{
			transform.LookAt(target);
		}
	}

	// Update is called once per frame
	void Update ()
	{
		// Don't use a rigidbody for movement; use manual movement with custom deltaTime instead
		transform.position += transform.forward * Timescaler.inst.CalculateDeltaTime(deltaTimePerc) * speed;

		// Reduce the lifetime (based on the calculated deltaTime)
		lifetimeTimer -= Timescaler.inst.CalculateDeltaTime(deltaTimePerc); 

		// Destroy the bullet when the lifetimeTimer runs out
		if (lifetimeTimer <= 0)
			shouldDestroy = true; 

		if (shouldDestroy)
			DestroyBullet(); 
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

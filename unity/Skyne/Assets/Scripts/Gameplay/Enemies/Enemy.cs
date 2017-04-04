using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	[Tooltip ("The current health of the enemy")]
	public float health;
	public float maxHealth;

	[Tooltip ("The percent chance from 0 - 100 that the enemy will drop a health pickup.")]
	public float hpDropPercChance;
	public int minHPDrop;
	public int maxHPDrop;

	[Tooltip ("The array of possible health pickup spawn counts. One index is randomly choosen to spawn x number of pickups")]
	public int[] hpDropChoices;

	[Tooltip ("Set to true once the enemy state machine has been started (cannot happen before level load finished).")]
	public bool started;

	public bool hasDeathAnimation;

	//Determines whether the enemy is alive or not. Is not currently ever changed.
	public bool alive;

	public AudioClip damageSound;

	public Animator anim;

	Color defaultColor;
	Color damageColor = Color.red;

	// Smoke Particles
	[System.Serializable]
	public struct SmokeParticles
	{
		public ParticleSystem system;
		public int maxParticles;
		public float startSmokingHealth;

		public float nearDeathHealth;
		public float nearDeathMultiplier;
	}

	public SmokeParticles[] smokeParticles;

	/// <summary>
	/// Weak points determine colliders that are affected by shots and the defense
	/// </summary>
	public struct WeakPoint
	{
		[Tooltip ("The collider mapped to this weak point")]
		public Collider weakPointCollider;

		[Tooltip ("The bullet damage is modified by this value to calculate the final damage value. 0.5 means half damage.")]
		public float defenseModifier;
	}

	protected virtual void Update ()
	{
		
	}

	void OnEnable ()
	{
		GlobalManager.OnGamePausedUpdated += HandleGamePausedUpdated; 
	}

	void OnDisable ()
	{
		GlobalManager.OnGamePausedUpdated -= HandleGamePausedUpdated;
	}

	/// <summary>
	/// Called when the pause state of the game has changed
	/// The Enemy class should respond in this function and make sure its AI and physics pause
	/// This may need to be handled on a class-by-class basic rather than in the superclass
	/// </summary>
	/// <param name="newState">If set to <c>true</c> new state.</param>
	void HandleGamePausedUpdated (bool newState)
	{

	}

		
	public void OnShot (Collision collision, Bullet bullet, float defenseModifier, bool isWeakPoint)
	{
		Collider col = collision.collider; 

		Debug.Log ("On shot; bullet.playerBullet = " + bullet.playerBullet + "; bullet.shouldDestroy = " + bullet.shouldDestroy); 

		// Check that the bullet was shot by the player and hasn't already been set to destroy itself
		if (bullet.playerBullet && !bullet.shouldDestroy)
		{
			// Update the enemy health
			float actualDamage = bullet.damage * defenseModifier; 
			health -= actualDamage; 
			Debug.Log ("bullet.damage: " + bullet.damage * defenseModifier); 

			// Update the smoke particles
			UpdateSmokeParticleEmission (); 

			// TODO: Animate the enemy taking damage

			// Spawn some hit particles
			// Determine how many to spawn
			int numParticles = (int)(actualDamage * 6); 

			// If the bullet has hit an enemy weak point
			if (isWeakPoint)
			{
				numParticles *= 2; 

				// Do an extra particle effect to indicate that a weak point has been hit
				// Do a weak damage flash
			}
			// If the bullet has hit a part of the enemy that isn't a weak point but still does damage
			else
			{
				// Do a strong damage flash
			}

			//this.GetComponentInChildren<SkinnedMeshRenderer> ().material.color = Color.Lerp (Color.white, Color.red, Mathf.PingPong (Time.time * 4, 0.7f));
			StopCoroutine("DamageFlash");
			StartCoroutine ("DamageFlash");

			ExplosionManager.inst.SpawnEnemyHitParticles (transform.position, numParticles);

			// Check for the enemy's death
			if (health <= 0 && alive)
			{
				if (hasDeathAnimation)
				{
					// Call death animation
					anim.SetBool ("isDead", true); 
					alive = false; 
				}
				else
				{
					alive = false; 
					DestroyEnemy ();
				}
			}
		}
	}

	void UpdateSmokeParticleEmission ()
	{
		for (int i = 0; i < smokeParticles.Length; i++)
		{
			ParticleSystem.EmissionModule emis = smokeParticles [i].system.emission; 

			if (health <= smokeParticles [i].nearDeathHealth)
			{
				emis.rateOverTime = ((smokeParticles [i].maxParticles * (maxHealth - health)) / maxHealth) * smokeParticles [i].nearDeathMultiplier;  
			}
			else if (health <= smokeParticles [i].startSmokingHealth)
			{
				emis.rateOverTime = (smokeParticles [i].maxParticles * (maxHealth - health)) / maxHealth;
			}
		}
	}

	IEnumerator DamageFlash ()
	{
		SkinnedMeshRenderer renderer = this.GetComponentInChildren<SkinnedMeshRenderer>();
		Color color = renderer.material.color;
		color = Color.red;
		renderer.material.color = color;

		yield return new WaitForSeconds(0.2f);

		float time = 0.0f;

		while(color != Color.white)
		{
			color = Color.Lerp (Color.red, Color.white, time * 2);
			time += Time.deltaTime;
			renderer.material.color = color;
			yield return new WaitForEndOfFrame();
		}
	}

	void DisableSmokeParticles ()
	{
		for (int i = 0; i < smokeParticles.Length; i++)
		{
			smokeParticles [i].system.enableEmission = false; 
		}
	}

	public void DestroyEnemy ()
	{
		EnemyDestroy (); 

		DisableSmokeParticles (); 

		// Call HealthPickupManager to potentially spawn a healthpickup
		// TODO: replace transform.position with a more specific spawn point where the health pickup should be instantiated
		//HealthPickupManager.inst.TrySpawnHealthPickup(transform.position, hpDropPercChance); 

		//HealthPickupManager.inst.SpawnHealthPickups(transform.position, minHPDrop, maxHPDrop); 
		HealthPickupManager.inst.SpawnHealthPickups (transform.position, hpDropChoices); 

		// Call ExplosionManager to create an explosion
		ExplosionManager.inst.SpawnEnemyExplosion (transform.position); 

		// Destroy the gameobject
		Destroy (this.gameObject);
	}

	protected virtual void EnemyDestroy () {

	}
}

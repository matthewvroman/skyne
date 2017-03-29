using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour 
{
	[Tooltip("The current health of the enemy")]
	public float health;
	public float maxHealth;

	[Tooltip("The percent chance from 0 - 100 that the enemy will drop a health pickup.")]
	public float hpDropPercChance; 
	public int minHPDrop;
	public int maxHPDrop; 

	[Tooltip("Set to true once the enemy state machine has been started (cannot happen before level load finished).")]
	public bool started; 

	public bool hasDeathAnimation; 

	//Determines whether the enemy is alive or not. Is not currently ever changed.
	public bool alive;

	public AudioClip damageSound;

	public Animator anim;

	/// <summary>
	/// Weak points determine colliders that are affected by shots and the defense
	/// </summary>
	public struct WeakPoint
	{
		[Tooltip("The collider mapped to this weak point")]
		public Collider weakPointCollider; 

		[Tooltip("The bullet damage is modified by this value to calculate the final damage value. 0.5 means half damage.")]
		public float defenseModifier; 
	}

	void OnEnable()
	{
		GlobalManager.OnGamePausedUpdated += HandleGamePausedUpdated; 
	}

	void OnDisable()
	{
		GlobalManager.OnGamePausedUpdated -= HandleGamePausedUpdated;
	}

	/// <summary>
	/// Called when the pause state of the game has changed
	/// The Enemy class should respond in this function and make sure its AI and physics pause
	/// This may need to be handled on a class-by-class basic rather than in the superclass
	/// </summary>
	/// <param name="newState">If set to <c>true</c> new state.</param>
	void HandleGamePausedUpdated(bool newState)
	{

	}
		
		
	public void OnShot(Collider col, float defenseModifier)
	{
		Bullet bullet = col.GetComponent<Bullet>();

		Debug.Log("On shot; bullet.playerBullet = " + bullet.playerBullet + "; bullet.shouldDestroy = " + bullet.shouldDestroy); 

		// Check that the bullet was shot by the player and hasn't already been set to destroy itself
		if (bullet.playerBullet && !bullet.shouldDestroy)
		{
			// Update the enemy health
			health -= bullet.damage * defenseModifier; 
			Debug.Log("bullet.damage: " + bullet.damage * defenseModifier); 

			// Set the bullet to be destroyed
			bullet.shouldDestroy = true;  

			// TODO: Animate the enemy taking damage

			// Spawn some hit particles
			ExplosionManager.inst.SpawnEnemyHitParticles(transform.position); 

			// Check for the enemy's death
			if (health <= 0 && alive)
			{
				if (hasDeathAnimation)
				{
					// Call death animation
					anim.SetBool("isDead", true); 
					alive = false; 
				}
				else
				{
					alive = false; 
					DestroyEnemy();
				}
			}
		}
	}

	public void DestroyEnemy()
	{
		EnemyDestroy(); 

		// Call HealthPickupManager to potentially spawn a healthpickup
		// TODO: replace transform.position with a more specific spawn point where the health pickup should be instantiated
		//HealthPickupManager.inst.TrySpawnHealthPickup(transform.position, hpDropPercChance); 

		HealthPickupManager.inst.SpawnHealthPickups(transform.position, minHPDrop, maxHPDrop); 

		// Call ExplosionManager to create an explosion
		ExplosionManager.inst.SpawnEnemyExplosion(transform.position); 

		// Destroy the gameobject
		Destroy (this.gameObject);
	}

	protected abstract void EnemyDestroy(); 
}

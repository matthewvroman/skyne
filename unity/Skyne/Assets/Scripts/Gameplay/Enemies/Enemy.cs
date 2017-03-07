using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour 
{
	[Tooltip("The current health of the enemy")]
	public float health; 

	[Tooltip("The percent chance from 0 - 100 that the enemy will drop a health pickup.")]
	public float hpDropPercChance; 

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

	// Initialization called from subclass
	protected void EnemyParentStart () 
	{
		
	}
	
	// Update called from subclass
	protected bool EnemyParentUpdate () 
	{
		if (MainGameplayManager.inst != null && GlobalManager.inst.GameplayIsActive())
		{
			return true; 
		}
		return false; 
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

			// Check for the enemy's death
			if (health <= 0)
			{
				DestroyEnemy();
			}
		}
	}

	public void DestroyEnemy()
	{
		// Call HealthPickupManager to potentially spawn a healthpickup
		// TODO: replace transform.position with a more specific spawn point where the health pickup should be instantiated
		HealthPickupManager.inst.TrySpawnHealthPickup(transform.position, hpDropPercChance); 

		// Call ExplosionManager to create an explosion
		ExplosionManager.inst.SpawnEnemyExplosion(transform.position); 

		// Destroy the gameobject
		Destroy (this.gameObject);
	}
}

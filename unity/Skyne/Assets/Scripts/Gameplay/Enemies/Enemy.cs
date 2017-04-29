using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	protected Vector3 spawnPos; 

	[Space(5)]
	[Header("Parent: State")]
	[Tooltip ("The current health of the enemy")]
	public float health;
	[HideInInspector] public float maxHealth;

	//Determines whether the enemy is alive or not. Is not currently ever changed.
	public bool alive;

	[Tooltip ("Set to true once the enemy state machine has been started (cannot happen before level load finished).")]
	[HideInInspector] public bool started;


	[Tooltip ("The array of possible health pickup spawn counts. One index is randomly choosen to spawn x number of pickups. The highest drop should be in the last index.")]
	public int[] hpDropChoices;

	// If set to true in OnShot, the last index in hpDropChoices will be picked, which should be the highest number of drops
	bool useMaxHPDrop = false; 

	public bool isIdling = true;

	[HideInInspector] public GameObject target; 

	[Space(5)]
	[Header("Parent: Sounds")]
	public AudioClip idleSound;
	public AudioClip damageSound;
	public AudioClip criticalDamageSound;
	public AudioClip sparkSound;
	public AudioClip detectSound;
	public AudioClip attackSound;
	public AudioClip deathSound;

	[Space(5)]
	[Header ("Parent: Music")]
	public AudioClip fightMusic;
	public AudioClip normMusic;

	[Space(5)]
	[Header("Parent: Animation")]
	public Animator anim;

	public bool hasDeathAnimation;

	[Space(5)]
	[Header("Parent: Hit Colors")]
	public Color defaultColor;
	public Color criticalHitColor; 
	public Color normalHitColor;
	public Color lowHitColor; 

	[Space(5)]
	[Header("Parent: Damage Modifiers")]
	public float criticalHitDamageModifier; 
	public float normalHitDamageModifier; 
	public float lowHitDamageModifier; 

	protected List<GameObject> enemyColliders; 

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

	[Space(5)]
	[Header("Parent: Particles")]

	public SmokeParticles[] smokeParticles;

	public ParticleSystem shotFireParticles; 

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

	protected virtual void Start ()
	{
		spawnPos = transform.position; 
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

	protected void CheckBossDead()
	{
		if (alive && GameState.inst.bossDefeated)
		{
			if (hasDeathAnimation)
			{
				// Call death animation
				anim.SetBool ("isDead", true);
				//this.GetComponent<AudioSource> ().PlayOneShot (deathSound);
				alive = false; 
				PreEnemyDestroy(); 
			}
			else
			{
				alive = false; 
				DestroyEnemy ();
			}
		}
	}

	protected void ParentSetupEnemy()
	{
		target = GameObject.FindGameObjectWithTag ("Player");

		if (target == null)
		{
			Debug.LogError("Failed to find target for " + name); 
		}

		alive = true;
		maxHealth = health; 
		started = true;

		SetupColliderArray(); 
	}

	void SetupColliderArray()
	{
		enemyColliders = new List<GameObject> (); 

		EnemyWeakPoint[] wp = GetComponentsInChildren<EnemyWeakPoint>(); 

		foreach (EnemyWeakPoint cur in wp)
		{
			// If the collider is on the Enemy layer, add it to the list
			if (cur.gameObject.layer == 12)
			{
				enemyColliders.Add(cur.gameObject); 
			}
		}
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

	/// <summary>
	/// Does a linecast between the bolt and its target. Returns false if there are any obstacles in the way.
	/// Note that the linecast can't intersect the colliders of the bolt or the target, or this will always return false
	/// </summary>
	protected bool CanHitTarget()
	{
		if (target == null)
		{
			return false; 
		}

		//float startDistMultiplier = 0.5f; 
		//Vector3 start = transform.position + dir * startDistMultiplier; 

		Vector3 dir = (target.transform.position - transform.position).normalized; 
		Vector3 start = transform.position; 
		Vector3 end = target.transform.position - dir * 1; 

		// First, check to make sure the start point is not overlapping the current collider
		RaycastHit startDistCheck; 
		bool validStart = false; 
		int loopSafetyCheck = 0; 

		// Temporarily Set the layers of each enemyCollider to IgnoreRaycast
		foreach (GameObject cur in enemyColliders)
		{
			cur.layer = 2; 
		}

		if (Physics.Linecast(start, end))
		{
			Debug.DrawLine(start, end, Color.yellow); 
			//Debug.LogError("Obstacle Found"); 
			return false; 
		}
		else
		{
			Debug.DrawLine(start, end, Color.white); 
			return true; 
		}

		// Reset the enemy collider layers
		// This uses a hard coded enemy layer integer
		foreach (GameObject cur in enemyColliders)
		{
			cur.layer = 12; 
		}
	}

		
	public void OnShot (Collision collision, Bullet bullet, EnemyWeakPoint.WeakPointType weakPointType)
	{
		// Enemy can't be hit anymore if it is already dead
		if (!alive)
		{
			return; 
		}

		EnemyShot(); 

		Collider col = collision.collider; 

		Debug.Log ("On shot; bullet.playerBullet = " + bullet.playerBullet + "; bullet.shouldDestroy = " + bullet.shouldDestroy); 

		//this.GetComponent<AudioSource> ().PlayOneShot (damageSound);

		// Check that the bullet was shot by the player and hasn't already been set to destroy itself
		if (bullet.playerBullet && !bullet.shouldDestroy)
		{
			// Choose which defense modifier to use
			float defenseModifier = 1; 

			if (weakPointType == EnemyWeakPoint.WeakPointType.Critical)
			{
				defenseModifier = criticalHitDamageModifier;
				this.GetComponent<AudioSource> ().PlayOneShot (criticalDamageSound);
			}
			else if (weakPointType == EnemyWeakPoint.WeakPointType.Normal)
			{
				defenseModifier = normalHitDamageModifier;
				this.GetComponent<AudioSource> ().PlayOneShot (damageSound);
			}
			else if (weakPointType == EnemyWeakPoint.WeakPointType.Low)
			{
				defenseModifier = lowHitDamageModifier; 
				this.GetComponent<AudioSource> ().PlayOneShot (damageSound);
			}

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

			StopCoroutine("DamageFlash");

			// If the bullet has hit an enemy weak point
			if (weakPointType == EnemyWeakPoint.WeakPointType.Critical)
			{
				//numParticles *= 3;
				numParticles = 0; 

				// Do an extra particle effect to indicate that a weak point has been hit
				Quaternion contactRot = Quaternion.FromToRotation(Vector3.up, collision.contacts[0].normal);
				ExplosionManager.inst.SpawnCriticalHitExplosion(collision.contacts[0].point, contactRot); 

				// Do a critical damage flash
				StartCoroutine("DamageFlash", criticalHitColor);
			}
			// If the bullet has hit a part of the enemy that isn't a weak point but still does normal damage
			else if (weakPointType == EnemyWeakPoint.WeakPointType.Normal)
			{
				// Do a normal damage flash
				StartCoroutine("DamageFlash", normalHitColor); 
			}
			// If the bullet has hit a section of the enemy with strong armor
			else if (weakPointType == EnemyWeakPoint.WeakPointType.Low)
			{
				// Do a weak damage flash
				numParticles = 0; 
				StartCoroutine("DamageFlash", lowHitColor); 
			}

			//this.GetComponentInChildren<SkinnedMeshRenderer> ().material.color = Color.Lerp (Color.white, Color.red, Mathf.PingPong (Time.time * 4, 0.7f));


			ExplosionManager.inst.SpawnEnemyHitParticles (transform.position, numParticles);

			// Check for the enemy's death
			if (health <= 0 && alive)
			{
				// If the last collider hit to kill the enemy is critical, ensure the max amount of health is dropped
				if (weakPointType == EnemyWeakPoint.WeakPointType.Critical)
				{
					useMaxHPDrop = true; 
				}

				if (hasDeathAnimation)
				{
					// Call death animation
					anim.SetBool ("isDead", true);
					//this.GetComponent<AudioSource> ().PlayOneShot (deathSound);
					alive = false; 
					PreEnemyDestroy(); 
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

	IEnumerator DamageFlash (Color flashColor)
	{
		SkinnedMeshRenderer[] renderer = this.GetComponentsInChildren<SkinnedMeshRenderer>();
		Color color = renderer[0].material.color;
		color = flashColor;
		foreach (SkinnedMeshRenderer rend in renderer)
		{
			rend.material.color = color;
		}

		yield return new WaitForSeconds(0.2f);

		float time = 0.0f;

		while(color != defaultColor)
		{
			color = Color.Lerp (flashColor, defaultColor, time * 2);
			time += Time.deltaTime;
			foreach (SkinnedMeshRenderer rend in renderer)
			{
				rend.material.color = color;
			}
			//renderer.material.color = color;
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

		if (this is Boss1_AI)
		{

		}
		else
		{
			if (useMaxHPDrop)
			{
				HealthPickupManager.inst.SpawnHealthPickups(transform.position, hpDropChoices[hpDropChoices.Length - 1]); 
			}
			else
			{
				HealthPickupManager.inst.SpawnHealthPickups(transform.position, hpDropChoices);
			}

			// Call ExplosionManager to create an explosion
			if (!(this is FortManager))
			{
				ExplosionManager.inst.SpawnEnemyExplosion(transform.position); 
			}
			else
			{
				Debug.Log("Spawn fort explosion"); 
				ExplosionManager.inst.SpawnFortExplosion(transform.position); 
			}

			this.GetComponent<AudioSource> ().PlayOneShot (deathSound);

			// Destroy the gameobject
			Destroy(this.gameObject);
		}
	}

	protected virtual void EnemyShot () {

	}

	protected virtual void EnemyDestroy () {

	}

	protected virtual void PreEnemyDestroy () {

	}

	public bool GetIsIdling() {
		return this.isIdling;
	}
}

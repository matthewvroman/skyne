using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; 

public class FortManager : Enemy 
{
	//Lists the different possible enemy states
	public enum State
	{
		IDLE,
		POSITION,
		TURN_TOWARDS_TARGET,
		MELEE
	}

	[Space(5)]
	[Header("Fort: State Machine")]
	public State state = FortManager.State.IDLE;

	[Tooltip ("Drag in the bullet prefab for the fort")]
	public GameObject bulletPrefab; 

	//Var holding the distance from the enemy to the player
	float tarDistance;

	[Space(5)]
	[Header("Fort: Behavior variables")]
	[Tooltip ("The distance at which the enemy will start attacking")]
	public float attackDistance; 
	[Tooltip ("The closest distance the fort will approach the player")]
	public float closeDistance; 
	[Tooltip ("Distance at which enemy starts moving towards player")]
	public float aggroDistance;
	[Tooltip ("Enemy's general move speed")]
	public float moveSpeed;
	[Tooltip ("Turning speed (only when the player is within attack range)")]
	public float attackTurnSpeed;

	// Melee
	[Tooltip ("When the player is in front of the enemy, how close must they be for the enemy to melee attack")]
	public float meleeDistance;
	[Tooltip ("How close to the front-facing direction of the enemy does the player need to be for the fort to melee? 1 = exact front, < 1 = close range")]
	public float meleeFrontZone; 

	public float meleeDelay; 
	[SerializeField] float curMeleeDelay; 

	public GameObject meleeCollider; 

	public ParticleSystem meleeParticles; 
	public float meleeParticlesEmission; 

	[Tooltip ("How long the enemy must wait after firing a shot")]
	public float cooldownLength; 
	float cooldownTimer; 

	//[Tooltip ("The player gameobject")]
	//Transform target;
	[Tooltip ("This enemy's rigidbody")]
	Rigidbody rBody;

	NavMeshAgent agent; 

	AudioSource[] audios;

	AudioSource fortAudio;
	AudioSource detectAudio;


	//public AudioClip moveSound;
	//public AudioClip chargeSound;
	//public AudioClip shootSound;
	//public AudioClip idleSound;

	[Space(5)]
	[Header("Fort: Extra game object functionality")]
	public GameObject bulletSpawner; 
	public GameObject frontFacingObj; 

	[Space(5)]
	[Header("Fort: Shooting behavior")]
	// Shooting
	public float shootDelay; 
	float curShootDelay;  

	float onShotTimer; 
	public float onShotTimerLength; 


	/// <summary>
	/// Custom Start() method invoked in Update() only once the game is fully loaded
	/// Only called once based on the started variable, located in the Enemy parent class
	/// </summary>
	void SetupEnemy ()
	{
		ParentSetupEnemy();

		//target = GameObject.FindGameObjectWithTag ("Player").GetComponent<Transform> ();
		rBody = GetComponent<Rigidbody> ();
		agent = GetComponent<NavMeshAgent>();
		//alive = true;
		state = FortManager.State.IDLE;
		//maxHealth = health; 

		audios = GetComponents<AudioSource> ();

		fortAudio = audios [0];
		detectAudio = audios [1];

		meleeCollider.SetActive(false); 

		//START State Machine
		StartCoroutine ("BSM");

		//started = true; 
	}

	void Update () 
	{
		if (!GlobalManager.inst.GameplayIsActive())
		{
			return; 
		}

		// If the enemy hasn't been set up yet, call it's setup
		// This isn't called until the game has been fully loaded to avoid any incomplete load null references
		if (!started)
		{
			SetupEnemy (); 
		}

		// Don't update if the game is paused or still loading
		if (alive && target != null)
		{
			if (onShotTimer > 0)
			{
				onShotTimer -= Time.deltaTime; 
				if (onShotTimer <= 0)
					onShotTimer = 0; 
			}

			if (state != FortManager.State.IDLE)
			{
				isIdling = false;
			}
			else
			{
				isIdling = true;
			}

			// State machine changes

			//Determines the distance from the enemy to the player
			tarDistance = Vector3.Distance(target.transform.position, transform.position);


			//Switches between states based on the distance from the player to the enemy
			if (state == FortManager.State.MELEE)
			{
				// Nothing here
			}
			// Try attacking the player, as long as the enemy has moved out of idle 
			else if (state != FortManager.State.IDLE && CanHitTarget())
			{
				// Update the shooting delay
				if (curShootDelay >= 0 && cooldownTimer == 0)
				{
					curShootDelay -= Time.deltaTime;
					if (curShootDelay < 0)
						curShootDelay = 0; 
				}

				if (cooldownTimer > 0)
				{
					cooldownTimer -= Time.deltaTime; 
					if (cooldownTimer <= 0)
					{
						cooldownTimer = 0; 
						anim.SetBool("isDefending", false); 
					}
				}

				// Update the melee delay
				if (curMeleeDelay > 0)
				{
					curMeleeDelay -= Time.deltaTime; 
					if (curMeleeDelay < 0)
						curMeleeDelay = 0; 
				}

				// Melee is first priority for tests
				if (tarDistance < meleeDistance)
				{
					TryMelee(); 
				}
				// Make the fort stop a distance from the player if it still needs to shoot
				// If in melee, the fort will continue moving up to the player
				else if (tarDistance <= closeDistance && !anim.GetBool("isDefending"))
				{
					state = state = FortManager.State.TURN_TOWARDS_TARGET;
					TryAttackClose(); 
				}
				else if (tarDistance < attackDistance)
				{
					state = state = FortManager.State.POSITION;
					TryAttackWalk(); 
				}
			}
			/*
			else if (tarDistance < aggroDistance)
			{
				state = FortManager.State.POSITION;
			}
			else if (state != FortManager.State.MELEE)
			{
				state = FortManager.State.IDLE;
			}
			*/ 
			else if (state != FortManager.State.IDLE && tarDistance < aggroDistance)
			{
				state = FortManager.State.POSITION;
			}
			else
			{
				if (tarDistance < aggroDistance && CanHitTarget() && tarDistance <= aggroDistance)
				{
					state = FortManager.State.POSITION; 

					detectAudio.PlayOneShot (detectSound);
				}
				else if (onShotTimer == 0)
				{
					state = FortManager.State.IDLE;
				}
			}


		}

		if (!alive)
		{
			//fortAudio.clip = null;
			//fortAudio.PlayOneShot (deathSound);

			if (anim.GetCurrentAnimatorStateInfo(2).IsName("DeathDone"))
			{
				DestroyEnemy(); 
			}
		}

	}

	// Fort State Machine
	IEnumerator BSM ()
	{
		while (alive)
		{
			// If the game is paused or still loading, don't continue with the coroutine and reset the while loop
			if (!GlobalManager.inst.GameplayIsActive())
			{
				yield return null; 
				continue; 
			}

			switch (state)
			{
			case State.IDLE:
				Idle ();
				break;

			case State.POSITION:
				Position ();
				break;
			case State.TURN_TOWARDS_TARGET:
				TurnTowardsTarget(); 
				break; 
			case State.MELEE:
				Melee(); 
				break; 
			}
			yield return null;
		}
	}


	/// <summary>
	/// Called when the fort sees if it can attack. If the fort has a clear path to hit the target, it will turns towards it then attack. 
	/// If there isn't a clear path, it will return to its positioning state
	/// </summary>
	void TryAttackClose()
	{
		// Don't let it attack until facing the player
		float dot = GetDot(); 

		// Also test dot for vertical level
		Vector3 targetVertical = new Vector3(0, 0, 0);
		Vector3 thisVertical = new Vector3(0, target.transform.position.y, 0); 
		float verticalDot = Vector3.Dot(frontFacingObj.transform.forward, (targetVertical - thisVertical).normalized);

		if (dot > 0.999f)
		{
			Attack(); 
		}
	}

	void TryAttackWalk()
	{
		// Don't let it attack until facing the player
		float dot = GetDot();

		if (dot > 0.95f)
		{
			Debug.Log("Fort walking attack"); 
			Attack(); 
		}
	}

	void TryMelee()
	{
		if (curMeleeDelay == 0)
		{
			Vector3 targetFlatPosition = new Vector3 (target.transform.position.x, transform.position.y, target.transform.position.z); 
			Vector3 thisFlatPosition = new Vector3 (transform.position.x, transform.position.y, transform.position.z); 

			float dot = Vector3.Dot(transform.forward, (targetFlatPosition - thisFlatPosition).normalized);

			if (dot > meleeFrontZone)
			{
				state = FortManager.State.MELEE;
				anim.SetTrigger("Melee"); 
				curMeleeDelay = meleeDelay; 
			}
		}
	}


	/// <summary>
	/// Does a linecast between the fort and its target. Returns false if there are any obstacles in the way.
	/// Note that the linecast can't intersect the colliders of the fort or the target, or this will always return false
	/// </summary>
	/*
	bool CanHitTarget()
	{
		Vector3 dir = (target.position - transform.position).normalized; 
		Vector3 start = transform.position + dir * 0.5f; 
		Vector3 end = target.position - dir * 1; 

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
	}
	*/ 


	//The Idling state, what the enemy does when the player is not close.
	void Idle ()
	{
		anim.SetBool("isWalking", false); 
		agent.speed = 0; 

		fortAudio.clip = idleSound;
		fortAudio.loop = true;

		if (!fortAudio.isPlaying)
		{
			fortAudio.Play ();
		}

		if (CanHitTarget ())
		{
			fortAudio.PlayOneShot (detectSound);
		}
	}


	//The Positioning state, when the enemy first notices the player, it will get closer so that it can start attacking.
	void Position ()
	{
		anim.SetBool("isWalking", true); 

		Vector3 targetPosition = new Vector3 (target.transform.position.x, this.transform.position.y, target.transform.position.z);

		agent.destination = target.transform.position; 
		agent.speed = moveSpeed; 

		fortAudio.clip = idleSound;
		fortAudio.loop = true;

		if (!fortAudio.isPlaying)
		{
			fortAudio.Play ();
		}

		//curShootDelay = shootDelay; 
	}


	void TurnTowardsTarget ()
	{
		float dot = GetDot(); 

		if (dot < 0.95f)
		{
			anim.SetBool("isWalking", true); 
		}
		else
		{
			anim.SetBool("isWalking", false); 
		}

		anim.SetBool("isWalking", true); 
		agent.speed = 0;

		fortAudio.clip = idleSound;
		fortAudio.loop = true;

		if (!fortAudio.isPlaying)
		{
			fortAudio.Play ();
		}

		Quaternion q = Quaternion.LookRotation(target.transform.position - frontFacingObj.transform.position);
		transform.rotation = Quaternion.RotateTowards(frontFacingObj.transform.rotation, q, attackTurnSpeed * Time.deltaTime);
	}

	void Melee()
	{
		anim.SetBool("isWalking", false); 
		agent.speed = 0;
	}

	void Attack()
	{
		Vector3 targetPosition = new Vector3 (target.transform.position.x, this.transform.position.y, target.transform.position.z);

		if (curShootDelay == 0)
		{
			//Debug.Log("Shoot bullet"); 

			curShootDelay = shootDelay;
			cooldownTimer = cooldownLength; 

			//anim.SetBool("isShooting", true); 
			anim.SetTrigger("Shoot"); 
		}
	}

	protected override void EnemyShot()
	{
		onShotTimer = onShotTimerLength; 

		if (state == FortManager.State.IDLE)
		{
			state = FortManager.State.POSITION; 
		}
	}

	// Called if the enemy has a destroy animation, right as the destroy animation starts
	protected override void PreEnemyDestroy()
	{
		agent.speed = 0; 
		agent.enabled = false; 
		anim.SetBool("isDefending", false); 
		anim.SetBool("isWalking", false); 
	}

	protected override void EnemyDestroy()
	{ 
		agent.speed = 0; 
		agent.enabled = false; 
	}

	float GetDot()
	{
		// Uses flattened height by giving this position and the target the same y value
		Vector3 targetFlatPosition = new Vector3(target.transform.position.x, frontFacingObj.transform.position.y, target.transform.position.z); 
		Vector3 thisFlatPosition = new Vector3(frontFacingObj.transform.position.x, frontFacingObj.transform.position.y, frontFacingObj.transform.position.z); 

		return Vector3.Dot(frontFacingObj.transform.forward, (targetFlatPosition - thisFlatPosition).normalized);
	}

	void MeleeOver()
	{
		//Debug.Log("Fort MeleeOver");

		meleeCollider.SetActive(false); 
		state = FortManager.State.POSITION;

		meleeParticles.emissionRate = 0; 
	}

	void ShotOver()
	{
		anim.SetBool("isDefending", true); 
		anim.SetTrigger("Defend"); 
	}

	void OnAnimMelee()
	{
		meleeCollider.SetActive(true); 
		meleeParticles.emissionRate = meleeParticlesEmission; 
	}

	void OnAnimShoot()
	{
		// Pass ProjectileManager this bolt's bullet spawner and shoot a new bullet
		//ProjectileManager.inst.Shoot_E_Normal(bulletSpawner, true); 
		ProjectileManager.inst.EnemyShoot(bulletSpawner, bulletPrefab, true); 

		fortAudio.volume = Random.Range (0.8f, 1);
		fortAudio.pitch = Random.Range (0.8f, 1);
		fortAudio.PlayOneShot (attackSound);

		if (shotFireParticles != null)
			shotFireParticles.Play(); 
	}

	void DefenseDone()
	{
		//Empty function
	}

	void DeathSFX ()
	{
		fortAudio.PlayOneShot (deathSound);
	}
}
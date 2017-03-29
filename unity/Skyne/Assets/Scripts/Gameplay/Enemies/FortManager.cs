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
		ATTACK,
		DEFENSE,
		MELEE
	}

	public State state;

	[Tooltip ("Drag in the bullet prefab for the fort")]
	public GameObject bulletPrefab; 

	//Var holding the distance from the enemy to the player
	float tarDistance;

	[Tooltip ("The distance at which the enemy will start attacking")]
	public float attackDistance; 
	[Tooltip ("Distance at which enemy starts moving towards player")]
	public float aggroDistance;
	[Tooltip ("When the player is in front of the enemy, how close must they be for the enemy to melee attack")]
	public float meleeDistance;

	[Tooltip ("Enemy's general move speed")]
	public float moveSpeed;
	[Tooltip ("Turning speed (only when the player is within attack range)")]
	public float attackTurnSpeed;

	[Tooltip ("How close to the front-facing direction of the enemy does the player need to be for the fort to melee? 1 = exact front, < 1 = close range")]
	public float meleeFrontZone; 

	[Tooltip ("How long the enemy must wait after firing a shot")]
	public float cooldownLength; 

	float cooldownTimer; 

	[Tooltip ("The player gameobject")]
	Transform target;
	[Tooltip ("This enemy's rigidbody")]
	Rigidbody rBody;

	NavMeshAgent agent; 

	public GameObject bulletSpawner; 
	public GameObject frontFacingObj; 

	public GameObject tempShield; 

	// Shooting
	public float shootDelay; 
	public float curShootDelay;  

	public AudioSource fortAudio;

	public AudioClip moveSound;
	public AudioClip chargeSound;
	public AudioClip shootSound;
	public AudioClip idleSound;

	/// <summary>
	/// Custom Start() method invoked in Update() only once the game is fully loaded
	/// Only called once based on the started variable, located in the Enemy parent class
	/// </summary>
	void SetupEnemy ()
	{
		target = GameObject.FindGameObjectWithTag ("Player").GetComponent<Transform> ();
		rBody = GetComponent<Rigidbody> ();
		//bulletSpawner = transform.Find("BulletSpawner").gameObject; 
		agent = GetComponent<NavMeshAgent>();
		alive = true;
		state = FortManager.State.IDLE;

		fortAudio = GetComponent<AudioSource> ();

		//START State Machine
		StartCoroutine ("FSM");

		started = true; 

		// Temporary shield
		//tempShield.SetActive(false); 
	}

	void Update () 
	{
		// Don't update if the game is paused or still loading
		if (GlobalManager.inst.GameplayIsActive())
		{
			// If the enemy hasn't been set up yet, call it's setup
			// This isn't called until the game has been fully loaded to avoid any incomplete load null references
			if (!started)
			{
				SetupEnemy(); 
			}

			// Update the shooting delay
			if (curShootDelay >= 0 && state != State.DEFENSE && state != State.MELEE)
			{
				curShootDelay -= Time.deltaTime;
				if (curShootDelay < 0)
					curShootDelay = 0; 
			}

			// Update the shooting delay
			if (cooldownTimer >= 0)
			{
				cooldownTimer -= Time.deltaTime;
				if (cooldownTimer < 0)
				{
					cooldownTimer = 0; 

					//curShootDelay = shootDelay; 

					// Temporary
					//tempShield.SetActive(false); 
				}
			}

			if (anim.GetBool("isShooting") == true && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
			{
				anim.SetBool("isShooting", false); 
			}

			if (!alive)
			{
				agent.speed = 0; 

				if (anim.GetCurrentAnimatorStateInfo(1).IsName("DeathDone"))
				{
					DestroyEnemy(); 
				}
			}
		}
	}

	// Bolt State Machine
	IEnumerator FSM ()
	{
		while (alive)
		{
			// If the game is paused or still loading, don't continue with the coroutine
			if (!GlobalManager.inst.GameplayIsActive())
			{
				yield return null; 
			}

			switch (state)
			{
			case State.IDLE:
				Idle ();
				break;

			case State.POSITION:
				Position ();
				break;
		
			case State.ATTACK:
				Attack ();
				break;

			case State.TURN_TOWARDS_TARGET:
				TurnTowardsTarget(); 
				break; 

			case State.DEFENSE:
				Defense(); 
				break; 

			case State.MELEE:
				Melee(); 
				break; 
			}

			yield return null;
		}
	}

	void FixedUpdate ()
	{
		if (GlobalManager.inst.GameplayIsActive() && target != null)
		{
			//Determines the distance from the enemy to the player
			tarDistance = Vector3.Distance(target.position, transform.position);

			if (state != State.MELEE)
			{
				if (tarDistance < attackDistance)
				{
					TryAttack(); 
				}
				else if (tarDistance < aggroDistance)
				{
					state = FortManager.State.POSITION;
				}
				else
				{
					state = FortManager.State.IDLE;
				}

				if (cooldownTimer != 0)
				{
					state = FortManager.State.DEFENSE; 
				}

				if (tarDistance < meleeDistance)
				{
					TryMelee(); 
				}
			}
			else
			{
				if (!anim.GetCurrentAnimatorStateInfo(2).IsName("Swing"))
				{
					anim.SetBool("isSwinging", false); 
					state = FortManager.State.IDLE; 
				}
			}

			/*
			//Switches between states based on the distance from the player to the enemy
			if (state != State.MELEE)
			{
				if (cooldownTimer == 0)
				{
					if (tarDistance < attackDistance)
					{
						TryAttack(); 
					}
					else if (tarDistance < aggroDistance)
					{
						state = FortManager.State.POSITION;
					}
					else
					{
						state = FortManager.State.IDLE;
					}
				}

				// Check for melee attack
				if (tarDistance < meleeDistance)
				{
					TryMelee(); 
				}
			}
			else
			{
				if (!anim.GetCurrentAnimatorStateInfo(2).IsName("Swing"))
				{
					anim.SetBool("isSwinging", false); 
					state = FortManager.State.IDLE; 
				}
			}
			*/ 
		}
	}

	/// <summary>
	/// Called when the bolt sees if it can attack. If the bolt has a clear path to hit the target, it will turns towards it then attack. 
	/// If there isn't a clear path, it will return to its positioning state
	/// </summary>
	void TryAttack()
	{
		// Test if the enemy has a sightline to reach the player
		// If yes, 
		if (CanHitTarget())
		{
			// Don't let it attack until facing the player

			// Uses flattened height by giving this position and the target the same y value
			Vector3 targetFlatPosition = new Vector3(target.position.x, frontFacingObj.transform.position.y, target.position.z); 
			Vector3 thisFlatPosition = new Vector3(frontFacingObj.transform.position.x, frontFacingObj.transform.position.y, frontFacingObj.transform.position.z); 

			//float dot = Vector3.Dot(transform.forward, (target.position - transform.position).normalized);
			float dot = Vector3.Dot(frontFacingObj.transform.forward, (targetFlatPosition - thisFlatPosition).normalized);

			//if (dot > 0.9999f)
			if (dot > 0.95f)
			{
				state = FortManager.State.ATTACK;
			}
			else
			{
				state = state = FortManager.State.TURN_TOWARDS_TARGET;
			}
		}
		else
		{
			state = FortManager.State.POSITION;
		}
	}

	void TryMelee()
	{
		Vector3 targetFlatPosition = new Vector3(target.position.x, transform.position.y, target.position.z); 
		Vector3 thisFlatPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z); 

		float dot = Vector3.Dot(transform.forward, (targetFlatPosition - thisFlatPosition).normalized);

		if (dot > meleeFrontZone)
		{
			state = FortManager.State.MELEE;
			anim.SetBool("isSwinging", true); 
		}
	}


	/// <summary>
	/// Does a linecast between the bolt and its target. Returns false if there are any obstacles in the way.
	/// Note that the linecast can't intersect the colliders target, or this will always return false
	/// </summary>
	bool CanHitTarget()
	{
		Vector3 dir = (target.position - transform.position).normalized; 
		Vector3 start1 = transform.position + dir; 
		Vector3 start2 = frontFacingObj.transform.position; 
		Vector3 end = target.position - dir * 1; 

		RaycastHit hit1 = new RaycastHit();
		RaycastHit hit2 = new RaycastHit(); 

		if ((Physics.Linecast(start1, end, out hit1) && hit1.collider.gameObject != this.gameObject) || (Physics.Linecast(start2, end, out hit2) && hit2.collider.gameObject != this.gameObject))
		{
			Debug.DrawLine(start1, end, Color.yellow); 
			Debug.DrawLine(start2, end, Color.yellow); 
			//Debug.LogError("Obstacle Found"); 
			return false; 
		}
		else
		{
			Debug.DrawLine(start1, end, Color.white); 
			Debug.DrawLine(start2, end, Color.white); 
			return true; 
		}
	}


	//The Idling state, what the enemy does when the player is not close.
	void Idle ()
	{
		anim.SetBool("isWalking", false);
		anim.SetBool("isDefending", false);
		agent.speed = 0; 

		fortAudio.clip = idleSound;

		if (!fortAudio.isPlaying)
		{
			fortAudio.Play ();
		}
	}


	//The Positioning state, when the enemy first notices the player, it will get closer so that it can start attacking.
	void Position ()
	{
		anim.SetBool("isWalking", true);
		anim.SetBool("isDefending", false);

		Vector3 targetPosition = new Vector3 (target.position.x, this.transform.position.y, target.position.z);

		agent.destination = target.position; 
		agent.speed = moveSpeed; 

		fortAudio.clip = moveSound;

		if (!fortAudio.isPlaying)
		{
			fortAudio.Play ();
		}

		curShootDelay = shootDelay; 
	}


	void TurnTowardsTarget ()
	{
		anim.SetBool("isWalking", true);
		anim.SetBool("isDefending", false);
		agent.speed = 0;
		Quaternion q = Quaternion.LookRotation(target.position - frontFacingObj.transform.position);
		transform.rotation = Quaternion.RotateTowards(frontFacingObj.transform.rotation, q, attackTurnSpeed * Time.deltaTime);

		fortAudio.clip = moveSound;

		if (!fortAudio.isPlaying)
		{
			fortAudio.Play ();
		}
	}


	//The Attcking state, once close enough, the enemy will charge at the player.  
	void Attack ()
	{
		anim.SetBool("isWalking", false);
		anim.SetBool("isDefending", false);
		Vector3 targetPosition = new Vector3 (target.position.x, this.transform.position.y, target.position.z);

		agent.speed = 0;

		//fortAudio.clip = idleSound;

		if (!fortAudio.isPlaying)
		{
			//fortAudio.Play ();
		}

		if (curShootDelay == 0 && cooldownTimer == 0)
		{
			curShootDelay = shootDelay;

			fortAudio.volume = Random.Range (0.8f, 1);
			fortAudio.pitch = Random.Range (0.8f, 1);
			fortAudio.PlayOneShot (shootSound);

			// Pass ProjectileManager this bolt's bullet spawner and shoot a new bullet
			//ProjectileManager.inst.Shoot_Fort(bulletSpawner, true); 
			ProjectileManager.inst.EnemyShoot(bulletSpawner, bulletPrefab, true); 

			cooldownTimer = cooldownLength; 
			//tempShield.SetActive(true); 

			anim.SetBool("isShooting", true); 
		}
	}

	void Defense ()
	{
		anim.SetBool("isWalking", false);
		anim.SetBool("isDefending", true); 
		agent.speed = 0;

		//fortAudio.clip = idleSound;

		if (!fortAudio.isPlaying)
		{
			fortAudio.Play ();
		}

		Quaternion q = Quaternion.LookRotation(target.position - frontFacingObj.transform.position);
		transform.rotation = Quaternion.RotateTowards(frontFacingObj.transform.rotation, q, attackTurnSpeed * Time.deltaTime);
	}

	void Melee ()
	{
		anim.SetBool("isWalking", false);
		anim.SetBool("isDefending", false);
		agent.speed = 0;

		fortAudio.clip = idleSound;

		if (!fortAudio.isPlaying)
		{
			fortAudio.Play ();
		}
	}

	public void EndMelee ()
	{

	}

	/*void OnTriggerEnter(Collider col) {
		if (col.gameObject.GetComponent<Bullet> ().playerBullet)
		{
			fortAudio.volume = 1;
			fortAudio.pitch = 1;
			fortAudio.clip = null;
			fortAudio.PlayOneShot (damageSound);
		}
	} */

	protected override void EnemyDestroy()
	{

	}
}
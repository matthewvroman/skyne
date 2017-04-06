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

	public State state;

	[Tooltip ("Drag in the bullet prefab for the fort")]
	public GameObject bulletPrefab; 

	//Var holding the distance from the enemy to the player
	float tarDistance;

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

	[Tooltip ("When the player is in front of the enemy, how close must they be for the enemy to melee attack")]
	public float meleeDistance;
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

	AudioSource fortAudio;

	public AudioClip moveSound;
	public AudioClip chargeSound;
	public AudioClip shootSound;
	public AudioClip idleSound;

	public GameObject bulletSpawner; 
	public GameObject frontFacingObj; 

	// Shooting
	public float shootDelay; 
	float curShootDelay;  

	/// <summary>
	/// Custom Start() method invoked in Update() only once the game is fully loaded
	/// Only called once based on the started variable, located in the Enemy parent class
	/// </summary>
	void SetupEnemy ()
	{
		target = GameObject.FindGameObjectWithTag ("Player").GetComponent<Transform> ();
		rBody = GetComponent<Rigidbody> ();
		agent = GetComponent<NavMeshAgent>();
		alive = true;
		state = FortManager.State.IDLE;
		maxHealth = health; 

		fortAudio = GetComponent<AudioSource> ();

		//START State Machine
		StartCoroutine ("BSM");

		started = true; 
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

			// TODO Redo this condition to use the new animation layer for shooting
			// Potentially change shooting to a trigger
			/*
			if (anim.GetBool("isShooting") == true && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
			{
				anim.SetBool("isShooting", false); 
			}
			*/ 


			// State machine changes

			//Determines the distance from the enemy to the player
			tarDistance = Vector3.Distance(target.position, transform.position);

			//Switches between states based on the distance from the player to the enemy

			// Try attacking the player, as long as the enemy has moved out of idle 
			if (state != FortManager.State.IDLE && CanHitTarget())
			{
				// Update the shooting delay
				if (curShootDelay >= 0)
				{
					curShootDelay -= Time.deltaTime;
					if (curShootDelay < 0)
						curShootDelay = 0; 
				}

				if (tarDistance <= closeDistance)
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
			else if (tarDistance < aggroDistance)
			{
				state = FortManager.State.POSITION;
			}
			else
			{
				state = FortManager.State.IDLE;
			}

		}

		if (!alive)
		{
			if (anim.GetCurrentAnimatorStateInfo(1).IsName("DeathDone"))
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
			case State.TURN_TOWARDS_TARGET:
				TurnTowardsTarget(); 
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

		// Uses flattened height by giving this position and the target the same y value
		Vector3 targetFlatPosition = new Vector3(target.position.x, frontFacingObj.transform.position.y, target.position.z); 
		Vector3 thisFlatPosition = new Vector3(frontFacingObj.transform.position.x, frontFacingObj.transform.position.y, frontFacingObj.transform.position.z); 

		//float dot = Vector3.Dot(transform.forward, (target.position - transform.position).normalized);
		float dot = Vector3.Dot(frontFacingObj.transform.forward, (targetFlatPosition - thisFlatPosition).normalized);

		// Also test dot for vertical level
		Vector3 targetVertical = new Vector3(0, 0, 0);
		Vector3 thisVertical = new Vector3(0, target.position.y, 0); 
		float verticalDot = Vector3.Dot(frontFacingObj.transform.forward, (targetVertical - thisVertical).normalized);

		if (dot > 0.999f)
		{
			Attack(); 
		}
	}

	void TryAttackWalk()
	{
		Debug.Log("Fort walking attack"); 
		Attack(); 
	}


	/// <summary>
	/// Does a linecast between the fort and its target. Returns false if there are any obstacles in the way.
	/// Note that the linecast can't intersect the colliders of the fort or the target, or this will always return false
	/// </summary>
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
	}


	//The Positioning state, when the enemy first notices the player, it will get closer so that it can start attacking.
	void Position ()
	{
		anim.SetBool("isWalking", true); 

		Vector3 targetPosition = new Vector3 (target.position.x, this.transform.position.y, target.position.z);

		agent.destination = target.position; 
		agent.speed = moveSpeed; 

		fortAudio.clip = moveSound;
		fortAudio.loop = true;

		if (!fortAudio.isPlaying)
		{
			fortAudio.Play ();
		}

		//curShootDelay = shootDelay; 
	}


	void TurnTowardsTarget ()
	{
		anim.SetBool("isWalking", true); 
		agent.speed = 0;

		fortAudio.clip = moveSound;
		fortAudio.loop = true;

		if (!fortAudio.isPlaying)
		{
			fortAudio.Play ();
		}

		Quaternion q = Quaternion.LookRotation(target.position - frontFacingObj.transform.position);
		transform.rotation = Quaternion.RotateTowards(frontFacingObj.transform.rotation, q, attackTurnSpeed * Time.deltaTime);
	}

	void Attack()
	{
		Vector3 targetPosition = new Vector3 (target.position.x, this.transform.position.y, target.position.z);

		if (curShootDelay == 0)
		{
			Debug.Log("Shoot bullet"); 

			curShootDelay = shootDelay;

			// Pass ProjectileManager this bolt's bullet spawner and shoot a new bullet
			//ProjectileManager.inst.Shoot_E_Normal(bulletSpawner, true); 
			ProjectileManager.inst.EnemyShoot(bulletSpawner, bulletPrefab, true); 

			fortAudio.volume = Random.Range (0.8f, 1);
			fortAudio.pitch = Random.Range (0.8f, 1);
			fortAudio.PlayOneShot (shootSound);

			anim.SetBool("isShooting", true); 
		}
	}

	// Called if the enemy has a destroy animation, right as the destroy animation starts
	protected override void PreEnemyDestroy()
	{
		agent.speed = 0; 
		agent.enabled = false; 
	}

	protected override void EnemyDestroy()
	{ 
		agent.speed = 0; 
		agent.enabled = false; 
	}

	void MeleeOver()
	{

	}

	void ShotOver()
	{

	}
}
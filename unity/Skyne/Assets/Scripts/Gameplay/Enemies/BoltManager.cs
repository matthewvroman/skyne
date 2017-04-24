using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; 

public class BoltManager : Enemy 
{
	//Lists the different possible enemy states
	public enum State
	{
		IDLE,
		POSITION,
		TURN_TOWARDS_TARGET
	}

	[Space(5)]
	[Header("Bolt: State Machine")]
	public State state = BoltManager.State.IDLE;

	//Var holding the distance from the enemy to the player
	float tarDistance;

	[Space(5)]
	[Header("Bolt: Behavior variables")]
	[Tooltip ("The distance at which the enemy will start attacking")]
	public float attackDistance; 
	[Tooltip ("The closest distance the bolt will approach the player")]
	public float closeDistance; 
	[Tooltip ("Distance at which enemy starts moving towards player")]
	public float aggroDistance;
	[Tooltip ("Enemy's general move speed")]
	public float moveSpeed;
	[Tooltip ("Turning speed (only when the player is within attack range)")]
	public float attackTurnSpeed;


	//[Tooltip ("The player gameobject")]
	//Transform target;
	[Tooltip ("This enemy's rigidbody")]
	Rigidbody rBody;

	NavMeshAgent agent; 

	AudioSource[] audios;

	AudioSource boltAudio;
	AudioSource detectAudio;
	AudioSource musicCon;

	//public AudioClip shootSound;
	//public AudioClip idleSound;

	[Space(5)]
	[Header("Bolt: Extra game object functionality")]
	public GameObject bulletSpawner; 
	public GameObject frontFacingObj; 

	[Space(5)]
	[Header("Bolt: Shooting behavior")]
	// Shooting
	[Tooltip ("Drag in the bullet prefab for the bolt")]
	public GameObject bulletPrefab; 
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
		//target = GameObject.FindGameObjectWithTag ("Player").GetComponent<Transform>();

		rBody = GetComponent<Rigidbody> ();
		agent = GetComponent<NavMeshAgent>();
		//alive = true;
		state = BoltManager.State.IDLE;
		//maxHealth = health; 

		musicCon = GameObject.Find ("MusicController").GetComponent<AudioSource> ();;
		audios = GetComponents<AudioSource> ();

		boltAudio = audios [0];
		detectAudio = audios [1];

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

			if (state != BoltManager.State.IDLE)
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

			// Try attacking the player, as long as the enemy has moved out of idle 
			if (state != BoltManager.State.IDLE && CanHitTarget())
			{
				// Update the shooting delay
				if (curShootDelay >= 0 && !anim.GetCurrentAnimatorStateInfo(1).IsName("Shoot"))
				{
					curShootDelay -= Time.deltaTime;
					if (curShootDelay < 0)
						curShootDelay = 0; 
				}

				if (tarDistance <= closeDistance)
				{
					state = state = BoltManager.State.TURN_TOWARDS_TARGET;
					TryAttackClose(); 
				}
				else if (tarDistance < attackDistance)
				{
					state = state = BoltManager.State.POSITION;
					TryAttackWalk(); 
				}
			}
			else if (state != BoltManager.State.IDLE && tarDistance < aggroDistance)
			{
				state = BoltManager.State.POSITION;
			}
			else
			{
				if (tarDistance < aggroDistance && CanHitTarget())
				{
					state = BoltManager.State.POSITION; 
					detectAudio.PlayOneShot (detectSound);
				}
				else if (onShotTimer == 0)
				{
					state = BoltManager.State.IDLE;
				}
			}

		}

		if (!alive)
		{
			//boltAudio.clip = null;
			//boltAudio.PlayOneShot (deathSound);

			if (anim.GetCurrentAnimatorStateInfo(2).IsName("DeathDone"))
			{
				DestroyEnemy(); 
			}
		}

	}

	// Bolt State Machine
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
			}
			yield return null;
		}
	}


	/// <summary>
	/// Called when the bolt sees if it can attack. If the bolt has a clear path to hit the target, it will turns towards it then attack. 
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

		//Debug.Log("Bolt walk dot: " + dot); 

		if (dot > 0.95f)
		{
			//Debug.Log("Bolt walking attack"); 
			Attack(); 
		}
	}


	/// <summary>
	/// Does a linecast between the bolt and its target. Returns false if there are any obstacles in the way.
	/// Note that the linecast can't intersect the colliders of the bolt or the target, or this will always return false
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

		boltAudio.clip = idleSound;
		boltAudio.loop = true;

		if (!boltAudio.isPlaying)
		{
			boltAudio.Play ();
		}

		/*if (CanHitTarget ())
		{
			boltAudio.PlayOneShot (detectSound);
		} */
	}


	//The Positioning state, when the enemy first notices the player, it will get closer so that it can start attacking.
	void Position ()
	{
		anim.SetBool("isWalking", true); 

		Vector3 targetPosition = new Vector3 (target.transform.position.x, this.transform.position.y, target.transform.position.z);

		agent.destination = target.transform.position; 
		agent.speed = moveSpeed; 

		boltAudio.clip = idleSound;
		boltAudio.loop = true;

		if (!boltAudio.isPlaying)
		{
			boltAudio.Play ();
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

		agent.speed = 0;

		boltAudio.clip = idleSound;
		boltAudio.loop = true;

		if (!boltAudio.isPlaying)
		{
			boltAudio.Play ();
		}

		Quaternion q = Quaternion.LookRotation(target.transform.position - frontFacingObj.transform.position);
		transform.rotation = Quaternion.RotateTowards(frontFacingObj.transform.rotation, q, attackTurnSpeed * Time.deltaTime);
	}

	void Attack()
	{
		Vector3 targetPosition = new Vector3 (target.transform.position.x, this.transform.position.y, target.transform.position.z);

		if (curShootDelay == 0)
		{
			Debug.Log("Shoot bullet"); 

			curShootDelay = shootDelay;

			//anim.Play("Shoot"); 
			anim.SetTrigger("Shoot"); 
			//anim.SetBool("isShooting", true); 
		}
	}
		
	void OnAnimShoot()
	{
		// Pass ProjectileManager this bolt's bullet spawner and shoot a new bullet
		//ProjectileManager.inst.Shoot_E_Normal(bulletSpawner, true); 
		ProjectileManager.inst.EnemyShoot(bulletSpawner, bulletPrefab, true); 

		boltAudio.volume = Random.Range (0.8f, 1);
		boltAudio.pitch = Random.Range (0.8f, 1);
		boltAudio.PlayOneShot (attackSound);

		if (shotFireParticles != null)
			shotFireParticles.Play(); 
	}

	float GetDot()
	{
		// Uses flattened height by giving this position and the target the same y value
		Vector3 targetFlatPosition = new Vector3(target.transform.position.x, frontFacingObj.transform.position.y, target.transform.position.z); 
		Vector3 thisFlatPosition = new Vector3(frontFacingObj.transform.position.x, frontFacingObj.transform.position.y, frontFacingObj.transform.position.z); 

		return Vector3.Dot(frontFacingObj.transform.forward, (targetFlatPosition - thisFlatPosition).normalized);
	}

	protected override void EnemyShot()
	{
		onShotTimer = onShotTimerLength; 

		if (state == BoltManager.State.IDLE)
		{
			state = BoltManager.State.POSITION; 
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

		detectAudio.PlayOneShot (deathSound);
	}
}
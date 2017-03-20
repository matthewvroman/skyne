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
		TURN_TOWARDS_TARGET,
		ATTACK
	}

	public State state;

	//Var holding the distance from the enemy to the player
	float tarDistance;

	[Tooltip ("The distance at which the enemy will start attacking")]
	public float attackDistance; 
	[Tooltip ("Distance at which enemy starts moving towards player")]
	public float aggroDistance;
	[Tooltip ("Enemy's general move speed")]
	public float moveSpeed;
	[Tooltip ("Turning speed (only when the player is within attack range)")]
	public float attackTurnSpeed;

	[Tooltip ("The player gameobject")]
	Transform target;
	[Tooltip ("This enemy's rigidbody")]
	Rigidbody rBody;

	NavMeshAgent agent; 

	AudioSource boltAudio;

	public AudioClip shootSound;
	public AudioClip idleSound;
	public AudioClip moveSound;
	public AudioClip detectSound;

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
		state = BoltManager.State.IDLE;

		boltAudio = GetComponent<AudioSource> ();

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

			// Update the shooting delay
			if (curShootDelay >= 0 && !anim.GetCurrentAnimatorStateInfo(0).IsName("Shoot"))
			{
				curShootDelay -= Time.deltaTime;
				if (curShootDelay < 0)
					curShootDelay = 0; 
			}

			if (anim.GetBool("isShooting") == true && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
			{
				anim.SetBool("isShooting", false); 
			}
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

	// Bolt State Machine
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

			case State.ATTACK:
				Attack ();
				break;
			case State.TURN_TOWARDS_TARGET:
				TurnTowardsTarget(); 
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

			//Switches between states based on the distance from the player to the enemy
			if (tarDistance < attackDistance)
			{
				TryAttack(); 
			}
			else if (tarDistance < aggroDistance)
			{
				state = BoltManager.State.POSITION;
			}
			else
			{
				state = BoltManager.State.IDLE;
			}
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

			// Also test dot for vertical level
			Vector3 targetVertical = new Vector3(0, 0, 0);
			Vector3 thisVertical = new Vector3(0, target.position.y, 0); 
			float verticalDot = Vector3.Dot(frontFacingObj.transform.forward, (targetVertical - thisVertical).normalized);

			if (dot > 0.999f)
			{
				state = BoltManager.State.ATTACK;
			}
			else
			{
				state = state = BoltManager.State.TURN_TOWARDS_TARGET;
			}
		}
		else
		{
			state = BoltManager.State.POSITION;
		}
	}


	/// <summary>
	/// Does a linecast between the bolt and its target. Returns false if there are any obstacles in the way.
	/// Note that the linecast can't intersect the colliders of the bolt or the target, or this will always return false
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

		boltAudio.clip = idleSound;
		boltAudio.loop = true;

		if (!boltAudio.isPlaying)
		{
			boltAudio.Play ();
		}
	}


	//The Positioning state, when the enemy first notices the player, it will get closer so that it can start attacking.
	void Position ()
	{
		anim.SetBool("isWalking", true); 

		Vector3 targetPosition = new Vector3 (target.position.x, this.transform.position.y, target.position.z);

		agent.destination = target.position; 
		agent.speed = moveSpeed; 

		boltAudio.clip = moveSound;
		boltAudio.loop = true;

		if (!boltAudio.isPlaying)
		{
			boltAudio.Play ();
		}

		curShootDelay = shootDelay; 
	}


	void TurnTowardsTarget ()
	{
		anim.SetBool("isWalking", true); 
		agent.speed = 0;

		boltAudio.clip = moveSound;
		boltAudio.loop = true;

		if (!boltAudio.isPlaying)
		{
			boltAudio.Play ();
		}

		Quaternion q = Quaternion.LookRotation(target.position - frontFacingObj.transform.position);
		transform.rotation = Quaternion.RotateTowards(frontFacingObj.transform.rotation, q, attackTurnSpeed * Time.deltaTime);
	}


	//The Attcking state, once close enough, the enemy will charge at the player.  
	void Attack ()
	{
		anim.SetBool("isWalking", false); 
		Vector3 targetPosition = new Vector3 (target.position.x, this.transform.position.y, target.position.z);

		agent.speed = 0;

		if (curShootDelay == 0)
		{
			curShootDelay = shootDelay;

			// Pass ProjectileManager this bolt's bullet spawner and shoot a new bullet
			ProjectileManager.inst.Shoot_E_Normal(bulletSpawner, true); 

			boltAudio.volume = Random.Range (0.8f, 1);
			boltAudio.pitch = Random.Range (0.8f, 1);
			boltAudio.PlayOneShot (shootSound);

			anim.SetBool("isShooting", true); 
		}
	}

	/*void OnTriggerEnter(Collider col) {
		if (col.gameObject.GetComponent<Bullet> ().playerBullet)
		{
			boltAudio.volume = 1;
			boltAudio.pitch = 1;
			boltAudio.clip = null;
			boltAudio.PlayOneShot (damageSound);
		}
	} */
}
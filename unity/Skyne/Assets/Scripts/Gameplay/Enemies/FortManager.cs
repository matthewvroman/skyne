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
		MELEE
	}

	public State state;

	//Determines whether the enemy is alive or not. Is not currently ever changed.
	private bool alive;

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

	GameObject bulletSpawner; 

	public GameObject tempShield; 

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
		bulletSpawner = transform.Find("BulletSpawner").gameObject; 
		agent = GetComponent<NavMeshAgent>();
		alive = true;
		state = FortManager.State.IDLE;

		//START State Machine
		StartCoroutine ("FSM");

		started = true; 

		// Temporary shield
		tempShield.SetActive(false); 
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
			if (curShootDelay >= 0)
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

					// Temporary
					tempShield.SetActive(false); 
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

			//Switches between states based on the distance from the player to the enemy
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

				// Check for melee attack
				if (tarDistance < meleeDistance)
				{
					TryMelee(); 
				}
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
			Vector3 targetFlatPosition = new Vector3(target.position.x, bulletSpawner.transform.position.y, target.position.z); 
			Vector3 thisFlatPosition = new Vector3(bulletSpawner.transform.position.x, bulletSpawner.transform.position.y, bulletSpawner.transform.position.z); 

			//float dot = Vector3.Dot(transform.forward, (target.position - transform.position).normalized);
			float dot = Vector3.Dot(bulletSpawner.transform.forward, (targetFlatPosition - thisFlatPosition).normalized);

			if (dot > 0.9999f)
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
		Vector3 start2 = bulletSpawner.transform.position; 
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
		agent.speed = 0; 
	}


	//The Positioning state, when the enemy first notices the player, it will get closer so that it can start attacking.
	void Position ()
	{
		Vector3 targetPosition = new Vector3 (target.position.x, this.transform.position.y, target.position.z);

		agent.destination = target.position; 
		agent.speed = moveSpeed; 

		curShootDelay = shootDelay; 
	}


	void TurnTowardsTarget ()
	{
		agent.speed = 0;
		Quaternion q = Quaternion.LookRotation(target.position - bulletSpawner.transform.position);
		transform.rotation = Quaternion.RotateTowards(bulletSpawner.transform.rotation, q, attackTurnSpeed * Time.deltaTime);
	}


	//The Attcking state, once close enough, the enemy will charge at the player.  
	void Attack ()
	{
		Vector3 targetPosition = new Vector3 (target.position.x, this.transform.position.y, target.position.z);

		agent.speed = 0;

		if (curShootDelay == 0 && cooldownTimer == 0)
		{
			curShootDelay = shootDelay;

			// Pass ProjectileManager this bolt's bullet spawner and shoot a new bullet
			//ProjectileManager.inst.Shoot_Fort(bulletSpawner, false); 

			// Test
			ProjectileManager.inst.Shoot_BossBigOrb(bulletSpawner); 

			cooldownTimer = cooldownLength; 
			tempShield.SetActive(true); 
		}
	}

	void Melee ()
	{
		agent.speed = 0; 
	}

	public void EndMelee ()
	{

	}
}
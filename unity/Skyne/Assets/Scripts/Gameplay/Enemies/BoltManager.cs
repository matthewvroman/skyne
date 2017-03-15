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

	//Determines whether the enemy is alive or not. Is not currently ever changed.
	private bool alive;

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

	GameObject bulletSpawner; 

	// Shooting
	public float shootDelay; 
	float curShootDelay;  


	void SetupEnemy ()
	{
		target = GameObject.FindGameObjectWithTag ("Player").GetComponent<Transform> ();
		rBody = GetComponent<Rigidbody> ();
		bulletSpawner = transform.Find("BulletSpawner").gameObject; 
		agent = GetComponent<NavMeshAgent>();
		alive = true;
		state = BoltManager.State.IDLE;

		//START State Machine
		StartCoroutine ("BSM");

		started = true; 
	}

	void Update () 
	{
		if (GlobalManager.inst.GameplayIsActive())
		{
			if (!started)
			{
				SetupEnemy(); 
			}

			if (curShootDelay >= 0)
			{
				curShootDelay -= Time.deltaTime;
				if (curShootDelay < 0)
					curShootDelay = 0; 
			}

			//agent.stoppingDistance = attackDistance; 
		}
	}

	// Bolt State Machine
	IEnumerator BSM ()
	{
		while (alive)
		{
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

	void TryAttack()
	{
		// Test if the enemy has a sightline to reach the player
		// If yes, 
		if (CanHitTarget())
		{
			// Don't let it attack until facing the player

			// Uses flattened height by giving this position and the target the same y value
			Vector3 targetFlatPosition = new Vector3(target.position.x, transform.position.y, target.position.z); 
			Vector3 thisFlatPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z); 

			//float dot = Vector3.Dot(transform.forward, (target.position - transform.position).normalized);
			float dot = Vector3.Dot(transform.forward, (targetFlatPosition - thisFlatPosition).normalized);

			if (dot > 0.98f)
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

	bool CanHitTarget()
	{
		/*
		Ray ray = new Ray (transform.position, transform.forward);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 100) && hit.collider.gameObject == target)
		{
			return true; 
		}
		return false; 
		*/
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
		//transform.rotation = Quaternion.Euler (0, 0, 0);
		//Debug.Log ("Idling");
		agent.speed = 0; 
	}

	//The Positioning state, when the enemy first notices the player, it will get closer so that it can start attacking.
	void Position ()
	{
		Vector3 targetPosition = new Vector3 (target.position.x, this.transform.position.y, target.position.z);
		//this.transform.LookAt (target.transform);

		agent.destination = target.position; 
		agent.speed = moveSpeed; 

		curShootDelay = shootDelay; 

		//Debug.Log ("Positioning");
	}

	void TurnTowardsTarget ()
	{
		agent.speed = 0;
		Quaternion q = Quaternion.LookRotation(target.position - transform.position);
		transform.rotation = Quaternion.RotateTowards(transform.rotation, q, attackTurnSpeed * Time.deltaTime);
	}

	//The Attcking state, once close enough, the enemy will charge at the player.  
	void Attack ()
	{
		Vector3 targetPosition = new Vector3 (target.position.x, this.transform.position.y, target.position.z);

		agent.speed = 0;

		//this.transform.LookAt (targetPosition);

		if (curShootDelay == 0)
		{
			curShootDelay = shootDelay; 
			ProjectileManager.inst.Shoot_E_Normal(bulletSpawner); 
		}
	}

	void OnDrawGizmos()
	{

		// Forward position
		//Gizmos.color = new Color32 (255, 0, 0, 250); 
		//Gizmos.DrawSphere(transform.position + transform.forward, 0.5f);

		// Direction of player
		//Vector3 dirTowardsPlayer = (target.transform.position - transform.position).normalized * 3; 
		//Vector3 frontPosition = transform.position + dirTowardsPlayer; 
		//Gizmos.color = new Color32 (255, 0, 0, 250); 
		//Gizmos.DrawSphere(frontPosition, 0.5f);
	}
}














/*
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

	//Determines whether the enemy is alive or not. Is not currently ever changed.
	private bool alive;

	//Var holding the distance from the enemy to the player
	float tarDistance;

	[Tooltip ("The distance at which the enemy will start attacking within its moveCircle")]
	public float closeAttackDistance;
	[Tooltip ("The distance at which the enemy will start attacking when at the edge of its moveCircle")]
	public float farAttackDistance;
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

	GameObject bulletSpawner; 

	// Shooting
	public float shootDelay; 
	float curShootDelay;  

	public bool moveCircleIsAtOrigin; 
	public Vector3 moveCircleOrigin; 
	public float moveCircleRadius; 
	public bool drawMoveCircleGizmo; 

	void Start () 
	{
		if (moveCircleIsAtOrigin)
			moveCircleOrigin = transform.position; 
	}

	void SetupEnemy ()
	{
		target = GameObject.FindGameObjectWithTag ("Player").GetComponent<Transform> ();
		rBody = GetComponent<Rigidbody> ();
		bulletSpawner = transform.Find("BulletSpawner").gameObject; 
		agent = GetComponent<NavMeshAgent>();
		alive = true;
		state = BoltManager.State.IDLE;

		//START State Machine
		StartCoroutine ("BSM");

		started = true; 
	}

	void Update () 
	{
		if (GlobalManager.inst.GameplayIsActive())
		{
			if (!started)
			{
				SetupEnemy(); 
			}

			if (curShootDelay >= 0)
			{
				curShootDelay -= Time.deltaTime;
				if (curShootDelay < 0)
					curShootDelay = 0; 
			}

			agent.stoppingDistance = closeAttackDistance; 
		}
	}

	// Bolt State Machine
	IEnumerator BSM ()
	{
		while (alive)
		{
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
			if (tarDistance < closeAttackDistance)
			{
				TryAttack(); 
			}
			else if (tarDistance < aggroDistance)
			{
				// Test if the bolt is at the edge of its circle
				Vector3 dirTowardsPlayer = (target.transform.position - transform.position).normalized * 3; 
				Vector3 frontPosition = transform.position + dirTowardsPlayer; 

				// Stop movement if the direction towards the player is not in the moveCircle
				if (Vector3.Distance(frontPosition, moveCircleOrigin) >= moveCircleRadius)
				{
					agent.speed = 0;

					// Switch to attack mode if within long distance shot range
					if (tarDistance < farAttackDistance)
					{
						TryAttack(); 
					}
				}
				else
				{
					state = BoltManager.State.POSITION;
				}
			}
			else
			{
				state = BoltManager.State.IDLE;
			}
		}
	}

	void TryAttack()
	{
		// Don't let it attack until facing the player
		float dot = Vector3.Dot(transform.forward, (target.position - transform.position).normalized);
		if (dot > 0.98f)
		{
			state = BoltManager.State.ATTACK;
		}
		else
		{
			state = state = BoltManager.State.TURN_TOWARDS_TARGET;
		}
	}

	//The Idling state, what the enemy does when the player is not close.
	void Idle ()
	{
		//transform.rotation = Quaternion.Euler (0, 0, 0);
		//Debug.Log ("Idling");
		agent.speed = 0; 
	}

	//The Positioning state, when the enemy first notices the player, it will get closer so that it can start attacking.
	void Position ()
	{
		Vector3 targetPosition = new Vector3 (target.position.x, this.transform.position.y, target.position.z);
		//this.transform.LookAt (target.transform);

		agent.destination = target.position; 
		agent.speed = moveSpeed; 

		curShootDelay = shootDelay; 

		//Debug.Log ("Positioning");
	}

	void TurnTowardsTarget ()
	{
		agent.speed = 0;
		Quaternion q = Quaternion.LookRotation(target.position - transform.position);
		transform.rotation = Quaternion.RotateTowards(transform.rotation, q, attackTurnSpeed * Time.deltaTime);
	}

	//The Attcking state, once close enough, the enemy will charge at the player.  
	void Attack ()
	{
		Vector3 targetPosition = new Vector3 (target.position.x, this.transform.position.y, target.position.z);

		agent.speed = 0;

		//this.transform.LookAt (targetPosition);

		if (curShootDelay == 0)
		{
			curShootDelay = shootDelay; 
			ProjectileManager.inst.Shoot_E_Normal(bulletSpawner); 
		}
	}

	void OnDrawGizmos()
	{
		if (drawMoveCircleGizmo)
		{
			if (moveCircleIsAtOrigin && !Application.isPlaying)
			{
				Gizmos.color = new Color32 (255, 255, 0, 50); 
				Gizmos.DrawSphere(new Vector3 (transform.position.x, transform.position.y, transform.position.z), moveCircleRadius); 
			}
			else
			{
				Gizmos.color = new Color32 (255, 255, 0, 50); 
				Gizmos.DrawSphere(new Vector3 (moveCircleOrigin.x, moveCircleOrigin.y, moveCircleOrigin.z), moveCircleRadius); 
			}
		}

		// Forward position
		//Gizmos.color = new Color32 (255, 0, 0, 250); 
		//Gizmos.DrawSphere(transform.position + transform.forward, 0.5f);

		// Direction of player
		//Vector3 dirTowardsPlayer = (target.transform.position - transform.position).normalized * 3; 
		//Vector3 frontPosition = transform.position + dirTowardsPlayer; 
		//Gizmos.color = new Color32 (255, 0, 0, 250); 
		//Gizmos.DrawSphere(frontPosition, 0.5f);
	}
}
*/ 
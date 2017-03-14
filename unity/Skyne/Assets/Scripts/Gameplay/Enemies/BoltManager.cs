using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltManager : Enemy 
{
	//Lists the different possible enemy states
	public enum State
	{
		IDLE,
		POSITION,
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
	[Tooltip ("Charger's charging speed")]
	public float chargeSpeed;

	[Tooltip ("The player gameobject")]
	public Transform target;
	[Tooltip ("This enemy's rigidbody")]
	public Rigidbody rBody;

	public GameObject bulletSpawner; 

	// Shooting
	public float shootDelay; 
	float curShootDelay; 

	void Start () 
	{
		
	}

	void SetupEnemy ()
	{
		target = GameObject.FindGameObjectWithTag ("Player").GetComponent<Transform> ();
		rBody = GetComponent<Rigidbody> ();
		bulletSpawner = transform.Find("BulletSpawner").gameObject; 
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
				state = BoltManager.State.ATTACK;
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

	//The Idling state, what the enemy does when the player is not close.
	void Idle ()
	{
		transform.rotation = Quaternion.Euler (0, 0, 0);
		Debug.Log ("Idling");
	}

	//The Positioning state, when the enemy first notices the player, it will get closer so that it can start attacking.
	void Position ()
	{
		Vector3 targetPosition = new Vector3 (target.position.x, this.transform.position.y, target.position.z);
		this.transform.LookAt (target.transform);

		//rBody.AddForce (transform.forward * moveSpeed);

		Debug.Log ("Positioning");
	}

	//The Attcking state, once close enough, the enemy will charge at the player.  
	void Attack ()
	{
		Vector3 targetPosition = new Vector3 (target.position.x, this.transform.position.y, target.position.z);
		//this.transform.LookAt (target.transform);
		this.transform.LookAt (targetPosition);

		//rBody.AddForce (transform.forward * chargeSpeed);
		//Debug.Log ("Attacking");

		if (curShootDelay == 0)
		{
			curShootDelay = shootDelay; 
			ProjectileManager.inst.Shoot_E_Normal(bulletSpawner); 
		}
	}
}

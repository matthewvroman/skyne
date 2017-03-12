using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChargerManager : Enemy
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

	void Start ()
	{
		// Parent class Start()
		EnemyParentStart (); 

		target = GameObject.FindGameObjectWithTag ("Player").GetComponent<Transform> ();
		rBody = GetComponent<Rigidbody> ();

		state = ChargerManager.State.IDLE;

		alive = true;

		//START State Machine
		StartCoroutine ("CSM");
	}

	// Charger State Machine
	IEnumerator CSM ()
	{
		while (alive)
		{
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

	void Update ()
	{
		// Parent class Update()
		if (EnemyParentUpdate ())
		{
			EnemyChildUpdate (); 
		}
	}

	void FixedUpdate ()
	{
		//Determines the distance from the enemy to the player
		tarDistance = Vector3.Distance (target.position, transform.position);

		//Switches between states based on the distance from the player to the enemy
		if (tarDistance < attackDistance)
		{
			state = ChargerManager.State.ATTACK;
		}
		else if (tarDistance < aggroDistance)
		{
			state = ChargerManager.State.POSITION;
		}
		else
		{
			state = ChargerManager.State.IDLE;
		}
	}

	void EnemyChildUpdate ()
	{

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
		this.transform.LookAt (targetPosition);

		rBody.AddForce (transform.forward * moveSpeed);

		Debug.Log ("Positioning");
	}

	//The Attcking state, once close enough, the enemy will charge at the player.  
	void Attack ()
	{
		Vector3 targetPosition = new Vector3 (target.position.x, this.transform.position.y, target.position.z);
		this.transform.LookAt (targetPosition);

		rBody.AddForce (transform.forward * chargeSpeed);
		Debug.Log ("Attacking");
	}
}


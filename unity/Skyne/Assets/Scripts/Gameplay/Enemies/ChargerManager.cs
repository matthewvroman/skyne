using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChargerManager : Enemy
{
	public enum State
	{
		IDLE,
		POSITION,
		ATTACK
	}

	public State state;
	private bool alive;

	float tarDistance;

	public float attackDistance;
	public float aggroDistance;

	public float moveSpeed;
	public float chargeSpeed;

	public float damping;

	public Transform target;
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

		tarDistance = Vector3.Distance (target.position, transform.position);

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

	void Idle ()
	{
		transform.rotation = Quaternion.Euler (0, 0, 0);
		Debug.Log ("Idling");
	}

	void Position ()
	{
		Vector3 targetPosition = new Vector3 (target.position.x, this.transform.position.y, target.position.z);
		this.transform.LookAt (targetPosition);

		rBody.AddForce (transform.forward * moveSpeed);

		Debug.Log ("Positioning");
	}

	void Attack ()
	{
		Vector3 targetPosition = new Vector3 (target.position.x, this.transform.position.y, target.position.z);
		this.transform.LookAt (targetPosition);

		rBody.AddForce (transform.forward * chargeSpeed);
		Debug.Log ("Attacking");
	}
}


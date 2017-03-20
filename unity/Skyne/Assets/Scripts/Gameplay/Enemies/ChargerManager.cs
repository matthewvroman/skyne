using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

	//Var holding the distance from the enemy to the player
	float tarDistance;

	public NavMeshAgent agent;

	[Tooltip ("The distance at which the enemy will start attacking")]
	public float attackDistance;
	[Tooltip ("Distance at which enemy starts moving towards player")]
	public float aggroDistance;
	[Tooltip ("Enemy's general move speed")]
	public float moveSpeed;
	[Tooltip ("Charger's charging speed")]
	public float chargeSpeed;

	//Transform target;
	//GameObject player;
	GameObject target;

	//public Animator anim;

	//Rigidbody rBody;

	void Start ()
	{
		/*player = GameObject.FindGameObjectWithTag ("Player");//.GetComponent<Transform> ();
		target = GameObject.FindGameObjectWithTag("Target");

		rBody = GetComponent<Rigidbody> ();
		state = ChargerManager.State.IDLE;
		alive = true;

		agent = gameObject.GetComponent<NavMeshAgent> ();

		//START State Machine
		StartCoroutine ("CSM");

		started = true; */
	}

	void SetupEnemy()
	{
		//player = GameObject.FindGameObjectWithTag ("Player");//.GetComponent<Transform> ();
		target = GameObject.FindGameObjectWithTag("Player");

		//rBody = GetComponent<Rigidbody> ();
		state = ChargerManager.State.IDLE;
		alive = true;

		agent = gameObject.GetComponent<NavMeshAgent> ();

		//START State Machine
		StartCoroutine ("CSM");

		started = true; 
	}

	// Charger State Machine
	IEnumerator CSM ()
	{
		while (alive)
		{
			/*if (!GlobalManager.inst.GameplayIsActive())
			{
				yield return null; 
			} */

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
		// TODO- fix global level loading bug here
		if (!started && GlobalManager.inst.GameplayIsActive() && GameObject.FindGameObjectWithTag ("Player") != null)
		{
			SetupEnemy(); 
		} 

		//Debug.Log (agent.autoBraking);

		if (!alive)
		{
			agent.speed = 0; 

			if (anim.GetCurrentAnimatorStateInfo(1).IsName("DeathDone"))
			{
				DestroyEnemy(); 
			}
		}
	}

	void FixedUpdate ()
	{
		if (GlobalManager.inst.GameplayIsActive () && target != null)
		{
			//Determines the distance from the enemy to the player
			tarDistance = Vector3.Distance (target.transform.position, transform.position);

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
	}

	//The Idling state, what the enemy does when the player is not close.
	void Idle ()
	{
		transform.rotation = Quaternion.Euler (0, 0, 0);

		agent.destination = transform.position;

		//Debug.Log ("Idling");
	}

	//The Positioning state, when the enemy first notices the player, it will get closer so that it can start attacking.
	void Position ()
	{
		//Vector3 targetPosition = new Vector3 (target.position.x, this.transform.position.y, target.position.z);
		//this.transform.LookAt (targetPosition);

		//rBody.AddForce (transform.forward * moveSpeed);

		agent.speed = moveSpeed;
		agent.acceleration = moveSpeed;

		agent.destination = target.transform.position;
		agent.autoBraking = true;

		anim.SetFloat ("Velocity", agent.velocity.x + agent.velocity.z);
		//Debug.Log ("Positioning");
	}

	//The Attcking state, once close enough, the enemy will charge at the player.  
	void Attack ()
	{
		/*Vector3 targetPosition = new Vector3 (target.transform.position.x, this.transform.position.y, target.transform.position.z);
		this.transform.LookAt (targetPosition);

		rBody.AddForce (transform.forward * chargeSpeed); */

		agent.speed = chargeSpeed;
		agent.acceleration = chargeSpeed;

		agent.destination = target.transform.position; 
		agent.autoBraking = false;

		//Debug.Log ("Attacking");
	}
}


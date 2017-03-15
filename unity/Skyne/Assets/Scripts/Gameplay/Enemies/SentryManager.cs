using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SentryManager : Enemy
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

	public NavMeshAgent agent;

	public float attackDist;

	public float aggroDist;

	public float moveSpeed;

	public float shotDelay;
	float curShotDelay;

	GameObject target;

	GameObject bulletSpawner; 

	void Start ()
	{
		/*target = GameObject.FindGameObjectWithTag ("Player");

		state = SentryManager.State.IDLE;
		alive = true;

		agent = gameObject.GetComponent<NavMeshAgent> ();

		bulletSpawner = transform.Find("BulletSpawner").gameObject; 

		//START State Machine
		StartCoroutine ("CSM"); */
	}

	void SetupEnemy() {
		target = GameObject.FindGameObjectWithTag ("Player");

		state = SentryManager.State.IDLE;
		alive = true;

		agent = gameObject.GetComponent<NavMeshAgent> ();

		bulletSpawner = transform.Find("BulletSpawner").gameObject; 

		//START State Machine
		StartCoroutine ("CSM");
	}

	IEnumerator CSM ()
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

	void Update ()
	{
		if (!started && GlobalManager.inst.GameplayIsActive())
		{
			SetupEnemy(); 
		}

		tarDistance = Vector3.Distance (target.transform.position, transform.position);

		//Switches between states based on the distance from the player to the enemy
		if (tarDistance < attackDist)
		{
			state = SentryManager.State.ATTACK;
		}
		else if (tarDistance < aggroDist)
		{
			state = SentryManager.State.POSITION;
		}
		else
		{
			state = SentryManager.State.IDLE;
		} 

		if (curShotDelay >= 0)
		{
			curShotDelay -= Time.deltaTime;
			if (curShotDelay < 0)
				curShotDelay = 0; 
		}

		//Debug.Log (agent.velocity);
	}

	void Idle ()
	{
		transform.rotation = Quaternion.Euler (0, 0, 0);
		agent.destination = transform.position;

		Debug.Log ("Idling");
	}

	void Position ()
	{
		agent.destination = target.transform.position;
		agent.speed = moveSpeed;
		agent.acceleration = moveSpeed;

		agent.stoppingDistance = 30;

		Debug.Log ("Positioning");
	}

	IEnumerator SlowDown ()
	{
		agent.Stop ();

		yield return new WaitForSeconds (1);

		agent.stoppingDistance = 0;
		agent.speed = 0;
		agent.acceleration = 0;

		agent.Resume ();
	} 

	void Attack ()
	{
		StartCoroutine (SlowDown());

		Vector3 targetPosition = new Vector3 (target.transform.position.x, this.transform.position.y, target.transform.position.z);
		this.transform.LookAt (target.transform);

		agent.destination = target.transform.position;

		if (curShotDelay == 0)
		{
			curShotDelay = shotDelay; 
			ProjectileManager.inst.Shoot_E_Normal(bulletSpawner); 
		}

		Debug.Log ("Attacking");
	}
}

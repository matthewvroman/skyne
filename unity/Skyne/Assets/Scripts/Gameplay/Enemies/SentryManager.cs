﻿using System.Collections;
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

	//bool canSeeTarget;

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

	void SetupEnemy ()
	{
		target = GameObject.FindGameObjectWithTag ("Player");

		state = SentryManager.State.IDLE;
		alive = true;

		agent = gameObject.GetComponent<NavMeshAgent> ();

		bulletSpawner = transform.Find ("BulletSpawner").gameObject; 

		//START State Machine
		StartCoroutine ("CSM");
	}

	IEnumerator CSM ()
	{
		while (alive)
		{
			if (!GlobalManager.inst.GameplayIsActive ())
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
		if (GlobalManager.inst.GameplayIsActive ())
		{
			if (!started)
			{
				SetupEnemy (); 
			}

			if (curShotDelay >= 0)
			{
				curShotDelay -= Time.deltaTime;
				if (curShotDelay < 0)
					curShotDelay = 0; 
			}

			//Debug.Log (canSeeTarget());
		}

		if (GlobalManager.inst.GameplayIsActive () && target != null)
		{
			tarDistance = Vector3.Distance (target.transform.position, transform.position);

			//Switches between states based on the distance from the player to the enemy
			if (tarDistance < attackDist && canSeeTarget ())
			{
				state = SentryManager.State.ATTACK;
			}
			else if (tarDistance < aggroDist && tarDistance >= agent.stoppingDistance && canSeeTarget ()) //&& !canSeeTarget)
			{
				state = SentryManager.State.POSITION;
			}
			else
			{
				state = SentryManager.State.IDLE;
			} 
		}
	}

	void Idle ()
	{
		//transform.rotation = Quaternion.Euler (0, 0, 0);
		agent.destination = transform.position;
		//agent.speed = 0;

		//Debug.Log ("Idling");
	}

	bool canSeeTarget ()
	{
		Vector3 dir = (target.transform.position - transform.position).normalized; 
		Vector3 start = transform.position + dir * 0.5f; 
		Vector3 end = target.transform.position - dir * 1; 

		if (Physics.Linecast (start, end))
		{
			Debug.DrawLine (start, end, Color.yellow); 
			//Debug.LogError("Obstacle Found"); 
			return false; 
		}
		else
		{
			Debug.DrawLine (start, end, Color.white); 
			return true; 
		}
	}

	void Position ()
	{
		Vector3 targetPosition = new Vector3 (target.transform.position.x, this.transform.position.y, target.transform.position.z);

		agent.destination = target.transform.position;
		agent.speed = moveSpeed;
		agent.acceleration = moveSpeed;

		agent.stoppingDistance = 40;

		curShotDelay = shotDelay; 

		//Debug.Log ("Positioning");
	}

	IEnumerator SlowDown ()
	{
		agent.Stop ();

		yield return new WaitForSeconds (0.5f);

		agent.stoppingDistance = 0;
		agent.speed = 0.1f;
		agent.acceleration = 1;

		//Debug.Log ("hello");
		agent.Resume ();
	}

	void Attack ()
	{
		if (agent.speed > 1)
		{
			StartCoroutine ("SlowDown");
		}

		//Vector3 targetPosition = new Vector3 (target.transform.position.x, this.transform.position.y, target.transform.position.z);

		agent.destination = target.transform.position;

		if (curShotDelay == 0)
		{
			curShotDelay = shotDelay; 
			ProjectileManager.inst.Shoot_E_Normal (bulletSpawner, false); 
		}

		//Debug.Log ("Attacking");
	}
}

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

	[Space(5)]
	[Header("Charger: State Machine")]
	public State state = ChargerManager.State.IDLE;

	//Var holding the distance from the enemy to the player
	float tarDistance;

	public NavMeshAgent agent;

	Rigidbody rBody;

	[Space(5)]
	[Header("Charger: Behavior variables")]
	[Tooltip ("The distance at which the enemy will start attacking")]
	public float attackDistance;
	[Tooltip ("Distance at which enemy starts moving towards player")]
	public float aggroDistance;
	[Tooltip ("Enemy's general move speed")]
	public float moveSpeed;
	[Tooltip ("Charger's charging speed")]
	public float chargeSpeed;

	public float waitingSpeed;
	public float chargeCooldown;
	public float chargeCounter = 0;

	float timer = 0.4f;
	float curTimer;

	AudioSource[] audios;

	AudioSource chargerAudio;
	AudioSource detectAudio;

	//Transform target;
	//GameObject player;
	//GameObject target;

	//public Animator anim;

	//Rigidbody rBody;

	void SetupEnemy()
	{
		ParentSetupEnemy();

		//player = GameObject.FindGameObjectWithTag ("Player");//.GetComponent<Transform> ();

		//target = GameObject.FindGameObjectWithTag("Player");

		//rBody = GetComponent<Rigidbody> ();
		state = ChargerManager.State.IDLE;
		//alive = true;

		curTimer = timer;

		agent = gameObject.GetComponent<NavMeshAgent> ();

		//maxHealth = health; 

		rBody = GetComponent<Rigidbody> ();

		audios = GetComponents<AudioSource> ();

		chargerAudio = audios [0];
		detectAudio = audios [1];

		//START State Machine
		StartCoroutine ("CSM");

		//started = true; 
	}

	// Charger State Machine
	IEnumerator CSM ()
	{
		while (alive)
		{
			// If the game is paused or still loading, don't continue with the coroutine and reset the while loop
			if (!GlobalManager.inst.GameplayIsActive())
			{
				yield return null; 
				continue; 
			}

			if (target == null)
			{
				Debug.LogError("Target is null"); 
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

	protected override void Update ()
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
			base.Update();

			// Destroy this enemy if the boss is dead
			CheckBossDead();

			tarDistance = Vector3.Distance (target.transform.position, transform.position);

			/*if (health <= 0)
		{
			chargerAudio.loop = false;
			chargerAudio.clip = deathSound;
			if (!chargerAudio.isPlaying)
			{
				chargerAudio.Play ();
			}
		} */

			if (state == ChargerManager.State.IDLE && CanHitTarget () && tarDistance <= aggroDistance)
			{
				detectAudio.PlayOneShot (detectSound);
			}

			//Debug.Log (agent.autoBraking);
		}

		if (!alive)
		{
			agent.speed = 0; 

			//chargerAudio.clip = null;
			//chargerAudio.PlayOneShot (deathSound);

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
			//tarDistance = Vector3.Distance (target.transform.position, transform.position);

			//Switches between states based on the distance from the player to the enemy
			/*if (tarDistance < attackDistance)
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
			} */
		}
	}

	/*
	bool CanSeeTarget ()
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
	*/ 

	//The Idling state, what the enemy does when the player is not close.
	void Idle ()
	{
		agent.Resume ();
		isIdling = true;

		if (tarDistance < aggroDistance && CanHitTarget())
		{
			state = ChargerManager.State.POSITION;
		}

		transform.rotation = Quaternion.Euler (0, 0, 0);

		chargeCounter = 0;

		agent.destination = transform.position;

		if (!chargerAudio.isPlaying)
		{
			chargerAudio.clip = idleSound;
			chargerAudio.Play ();
		}
			
		//Debug.Log ("Idling");
	}

	//The Positioning state, when the enemy first notices the player, it will get closer so that it can start attacking.
	void Position ()
	{
		//Vector3 targetPosition = new Vector3 (target.position.x, this.transform.position.y, target.position.z);
		//this.transform.LookAt (targetPosition);

		//rBody.AddForce (transform.forward * moveSpeed);

		isIdling = false;

		agent.Resume ();

		if (tarDistance > aggroDistance)
		{
			state = ChargerManager.State.IDLE;
		} 
		else if (tarDistance < attackDistance)
		{
			state = ChargerManager.State.ATTACK;
		}

		if (chargeCounter <= 0)
		{
			chargeCounter = 0;
		}
		else
		{
			chargeCounter -= Time.deltaTime;
		}

		agent.speed = moveSpeed;
		agent.acceleration = moveSpeed;

		agent.destination = target.transform.position;
		agent.autoBraking = true;

		chargerAudio.clip = attackSound;
		chargerAudio.volume = Mathf.Lerp (chargerAudio.volume, 0.8f, 10 * Time.deltaTime);
		chargerAudio.pitch = Mathf.Lerp (chargerAudio.pitch, 0.8f, 10 * Time.deltaTime);

		if (!chargerAudio.isPlaying)
		{
			//chargerAudio.volume = 0.8f;
			//chargerAudio.pitch = 0.8f;

			chargerAudio.Play ();
		}

		anim.SetFloat ("Velocity", agent.velocity.x + agent.velocity.z);
		//Debug.Log ("Positioning");
	}

	IEnumerator Charge() {
		agent.speed = chargeSpeed;
		agent.acceleration = chargeSpeed;

		yield return new WaitForSeconds (1);

		agent.Stop ();

		chargeCounter = chargeCooldown;
	}

	//The Attcking state, once close enough, the enemy will charge at the player.  
	void Attack ()
	{
		/*Vector3 targetPosition = new Vector3 (target.transform.position.x, this.transform.position.y, target.transform.position.z);
		this.transform.LookAt (targetPosition);

		rBody.AddForce (transform.forward * chargeSpeed); */

		isIdling = false;

		if (tarDistance > aggroDistance)
		{
			state = ChargerManager.State.IDLE;
		} 
		else if (tarDistance > attackDistance)
		{
			state = ChargerManager.State.POSITION;
		}

		if (chargeCounter > 0)
		{
			agent.speed = waitingSpeed;
			agent.acceleration = waitingSpeed;
			agent.Resume ();
			chargeCounter -= Time.deltaTime;
		} 
		else if (chargeCounter <= 0)
		{
			StartCoroutine ("Charge");
		}

		chargerAudio.clip = attackSound;
		//chargerAudio.volume = 1f;
		//chargerAudio.pitch = 1.1f;

		chargerAudio.volume = Mathf.Lerp (chargerAudio.volume, 1, 10 * Time.deltaTime);
		chargerAudio.pitch = Mathf.Lerp (chargerAudio.pitch, 1.1f, 10 * Time.deltaTime);

		if (!chargerAudio.isPlaying)
		{
			chargerAudio.Play ();
		}

		agent.destination = target.transform.position; 
		agent.autoBraking = false;

		//Debug.Log ("Attacking");
	}

	/*void OnTriggerEnter(Collider col) {
		if (col.gameObject.GetComponent<Bullet> ().playerBullet)
		{
			chargerAudio.volume = 1;
			chargerAudio.pitch = 1;
			chargerAudio.clip = null;
			chargerAudio.PlayOneShot (damageSound);
		}
	} */

	protected override void EnemyShot()
	{
		if (state == ChargerManager.State.IDLE)
		{
			state = ChargerManager.State.POSITION; 
		}
	}

	protected override void EnemyDestroy()
	{

	}

	void OnCollisionEnter(Collision col) {
		if (col.gameObject.tag == "Player")
		{
			StopCoroutine ("Charge");
			agent.velocity = -agent.velocity;

			//agent.speed = 0;
			//agent.acceleration = 0;

			// Calculate Angle Between the collision point and the player
			//Vector3 dir = -(col.contacts [0].point - transform.position).normalized;
			// We then get the opposite (-Vector3) and normalize it
			//dir = -dir.normalized;
			//dir.y = 1;

			// And finally we add force in the direction of dir and multiply it by force. 
			// This will push back the player
			//rBody.AddForce (dir * , ForceMode.VelocityChange);
			//chargeCounter = chargeCooldown;
		}
	}

	void DeathSFX ()
	{
		chargerAudio.PlayOneShot (deathSound);
	}
}


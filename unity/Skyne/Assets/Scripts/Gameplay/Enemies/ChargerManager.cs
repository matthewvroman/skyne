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

	[Space (5)]
	[Header ("Charger: State Machine")]
	public State state = ChargerManager.State.IDLE;

	//Var holding the distance from the enemy to the player
	float tarDistance;

	public NavMeshAgent agent;

	Rigidbody rBody;

	[Space (5)]
	[Header ("Charger: Behavior variables")]
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

	void SetupEnemy ()
	{
		ParentSetupEnemy ();

		state = ChargerManager.State.IDLE;

		curTimer = timer;

		agent = gameObject.GetComponent<NavMeshAgent> ();

		rBody = GetComponent<Rigidbody> ();

		audios = GetComponents<AudioSource> ();

		chargerAudio = audios [0];
		detectAudio = audios [1];

		//START State Machine
		StartCoroutine ("CSM");
	}

	// Charger State Machine
	IEnumerator CSM ()
	{
		while (alive)
		{
			// If the game is paused or still loading, don't continue with the coroutine and reset the while loop
			if (!GlobalManager.inst.GameplayIsActive ())
			{
				yield return null; 
				continue; 
			}

			if (target == null)
			{
				Debug.LogWarning ("Target is null"); 
				yield return null; 
				continue; 
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
		if (!GlobalManager.inst.GameplayIsActive ())
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
			base.Update ();

			// Destroy this enemy if the boss is dead
			CheckBossDead ();

			tarDistance = Vector3.Distance (target.transform.position, transform.position);

			if (state == ChargerManager.State.IDLE && CanHitTarget () && tarDistance <= aggroDistance)
			{
				detectAudio.PlayOneShot (detectSound);
			}
		}

		if (!alive)
		{
			agent.speed = 0; 

			if (anim.GetCurrentAnimatorStateInfo (1).IsName ("DeathDone"))
			{
				DestroyEnemy (); 
			}
		}
	}

	//The Idling state, what the enemy does when the player is not close.
	void Idle ()
	{
		agent.Resume ();
		isIdling = true;

		if (tarDistance < aggroDistance && CanHitTarget ())
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
	}

	//The Positioning state, when the enemy first notices the player, it will get closer so that it can start attacking.
	void Position ()
	{
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
			chargerAudio.Play ();
		}

		anim.SetFloat ("Velocity", agent.velocity.x + agent.velocity.z);
	}

	IEnumerator Charge ()
	{
		agent.speed = chargeSpeed;
		agent.acceleration = chargeSpeed;

		yield return new WaitForSeconds (1);

		agent.Stop ();

		chargeCounter = chargeCooldown;
	}

	//The Attcking state, once close enough, the enemy will charge at the player.
	void Attack ()
	{
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

		chargerAudio.volume = Mathf.Lerp (chargerAudio.volume, 1, 10 * Time.deltaTime);
		chargerAudio.pitch = Mathf.Lerp (chargerAudio.pitch, 1.1f, 10 * Time.deltaTime);

		if (!chargerAudio.isPlaying)
		{
			chargerAudio.Play ();
		}

		agent.destination = target.transform.position; 
		agent.autoBraking = false;
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

	protected override void EnemyShot ()
	{
		if (state == ChargerManager.State.IDLE)
		{
			state = ChargerManager.State.POSITION; 
		}
	}

	protected override void EnemyDestroy ()
	{

	}

	void OnCollisionEnter (Collision col)
	{
		if (col.gameObject.tag == "Player")
		{
			StopCoroutine ("Charge");
			agent.velocity = -agent.velocity;
		}
	}

	void DeathSFX ()
	{
		chargerAudio.PlayOneShot (deathSound);
	}
}


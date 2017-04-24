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

	[Space(5)]
	[Header("Sentry: State Machine")]
	public State state = SentryManager.State.IDLE;

	[Tooltip ("Drag in the bullet prefab for the sentry")]
	public GameObject bulletPrefab; 

	//Var holding the distance from the enemy to the player
	public float tarDistance;

	[Space(5)]
	[Header("Sentry: Behavior variables")]
	public NavMeshAgent agent;

	public float attackDist;

	public float aggroDist;

	public float moveSpeed;

	public float turnSpeed;

	public float shotDelay;
	float curShotDelay;

	//public GameObject target;

	//GameObject frontObject;

	AudioSource[] audios;

	AudioSource sentryAudio;
	AudioSource detectAudio;

	public float atkTimer;
	float curAtkTimer;

	public LayerMask seePlayer;

	//public AudioClip idleSound;
	//public AudioClip shootSound;

	//public Animator anim;

	//bool CanSeeTarget;

	[Space(5)]
	[Header("Sentry: Extra game object functionality")]
	public GameObject bulletSpawner;

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
		ParentSetupEnemy();

		//Debug.Log("Sentry SetupEnemy()"); 
		//target = GameObject.FindGameObjectWithTag ("Player");

		state = SentryManager.State.IDLE;
		//alive = true;

		agent = gameObject.GetComponent<NavMeshAgent> ();

		audios = GetComponents<AudioSource> ();

		sentryAudio = audios [0];
		detectAudio = audios [1];

		//bulletSpawner = transform.Find ("BulletSpawner").gameObject; 
		//frontObject = bulletSpawner;

		curAtkTimer = atkTimer;

		//maxHealth = health; 

		//START State Machine
		StartCoroutine ("SSM");

		//started = true;
	}

	IEnumerator SSM ()
	{
		while (alive)
		{
			// If the game is paused or still loading, don't continue with the coroutine and reset the while loop
			if (!GlobalManager.inst.GameplayIsActive())
			{
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

	void Update ()
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

		if (alive && target != null)
		{
		/*	if (health <= 0)
			{
				sentryAudio.loop = false;
				sentryAudio.clip = deathSound;
				if (!sentryAudio.isPlaying)
				{
					sentryAudio.Play ();
				}
			} */

			if (curShotDelay >= 0)
			{
				curShotDelay -= Time.deltaTime;
				if (curShotDelay < 0)
					curShotDelay = 0; 
			}

			//Debug.Log (CanSeeTarget());
		}

		if (target != null)
		{
			tarDistance = Vector3.Distance (target.transform.position, transform.position);

			if (state != SentryManager.State.IDLE)
			{
				isIdling = false;
			}
			else
			{
				isIdling = true;
			}

			if (state == SentryManager.State.IDLE && CanHitTarget() && tarDistance <= aggroDist)
			{
				detectAudio.PlayOneShot (detectSound);
			}

			//Switches between states based on the distance from the player to the enemy
			if (tarDistance < attackDist && CanHitTarget ())
			{
				state = SentryManager.State.ATTACK;
			}
			else if (tarDistance < aggroDist && tarDistance >= agent.stoppingDistance && CanHitTarget ()) //&& !CanSeeTarget)
			{
				state = SentryManager.State.POSITION;
			}
			else
			{
				state = SentryManager.State.IDLE;
			} 
		}

		if (!alive)
		{
			//agent.speed = 0; 

			//sentryAudio.clip = null;
			//sentryAudio.PlayOneShot (deathSound);

			if (anim.GetCurrentAnimatorStateInfo(1).IsName("DeathDone"))
			{
				DestroyEnemy(); 
			}
		}
	}

	void Idle ()
	{
		//transform.rotation = Quaternion.Euler (0, 0, 0);
		agent.destination = transform.position;
		//agent.speed = 0;

		sentryAudio.clip = idleSound;

		curAtkTimer = atkTimer;

		if (!sentryAudio.isPlaying)
		{
			sentryAudio.Play ();
		}

		/*if (CanHitTarget ())
		{
			sentryAudio.PlayOneShot (detectSound);
		} */

		anim.SetBool ("isShooting", false);
		//Debug.Log ("Idling");
	}

	/*
	bool CanSeeTarget ()
	{
		Vector3 dir = (target.transform.position - transform.position).normalized; 
		Vector3 start = transform.position + dir * 0.5f; 
		Vector3 end = target.transform.position - dir * 1; 

		if (Physics.Linecast (start, end, seePlayer))
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

	void Position ()
	{
		//Vector3 targetPosition = new Vector3 (target.transform.position.x, this.transform.position.y, target.transform.position.z);

		agent.destination = target.transform.position;
		agent.speed = moveSpeed;
		agent.acceleration = moveSpeed;

		agent.stoppingDistance = 40;

		curShotDelay = shotDelay; 

		anim.SetFloat ("Velocity", agent.velocity.x + agent.velocity.z);
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
		/*if (agent.speed > 1)
		{
			StartCoroutine ("SlowDown");
		} */

		//Vector3 targetPosition = new Vector3 (target.transform.position.x, this.transform.position.y, target.transform.position.z);

		//agent.destination = target.transform.position;

		agent.speed = 0;
		Quaternion q = Quaternion.LookRotation(target.transform.position - bulletSpawner.transform.position);
		transform.rotation = Quaternion.RotateTowards(bulletSpawner.transform.rotation, q, turnSpeed * Time.deltaTime);

		if (curAtkTimer > 0)
		{
			curAtkTimer -= Time.deltaTime;
		}
		else
		{
			curAtkTimer = 0;
		}

		if (agent.velocity.magnitude > 0)
		{
			sentryAudio.clip = idleSound;
			sentryAudio.loop = true;
		}
		else
		{
			sentryAudio.clip = idleSound;
		}

		if (!sentryAudio.isPlaying)
		{
			sentryAudio.Play ();
		}

		if (curAtkTimer == 0)
		{
			if (curShotDelay == 0)
			{
				curShotDelay = shotDelay; 
				//ProjectileManager.inst.Shoot_E_Normal (bulletSpawner, false);
				ProjectileManager.inst.EnemyShoot (bulletSpawner, bulletPrefab, false); 

				sentryAudio.volume = Random.Range (0.8f, 1);
				sentryAudio.pitch = Random.Range (0.8f, 1);
				sentryAudio.PlayOneShot (attackSound);

				if (shotFireParticles != null)
					shotFireParticles.Play (); 
			}
		}

		anim.SetBool ("isShooting", true);
		//Debug.Log ("Attacking");
	}

	/*void OnTriggerEnter(Collider col) {
		if (col.gameObject.GetComponent<Bullet> ().playerBullet)
		{
			sentryAudio.volume = 1;
			sentryAudio.pitch = 1;
			sentryAudio.clip = null;
			sentryAudio.PlayOneShot (damageSound);
		}
	} */

	protected override void EnemyShot()
	{
		if (state == SentryManager.State.IDLE)
		{
			state = SentryManager.State.POSITION; 
		}
	}

	protected override void EnemyDestroy()
	{

	}

	void DeathSFX ()
	{
		sentryAudio.PlayOneShot (deathSound);
	}
}

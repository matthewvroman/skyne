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

	[Tooltip ("Drag in the bullet prefab for the sentry")]
	public GameObject bulletPrefab; 

	//Var holding the distance from the enemy to the player
	float tarDistance;

	public NavMeshAgent agent;

	public float attackDist;

	public float aggroDist;

	public float moveSpeed;

	public float turnSpeed;

	public float shotDelay;
	float curShotDelay;

	public GameObject target;

	public GameObject bulletSpawner;
	//GameObject frontObject;

	AudioSource sentryAudio;

	//public AudioClip idleSound;
	//public AudioClip shootSound;

	//public Animator anim;

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
		Debug.Log("Sentry SetupEnemy()"); 
		target = GameObject.FindGameObjectWithTag ("Player");

		state = SentryManager.State.IDLE;
		alive = true;

		agent = gameObject.GetComponent<NavMeshAgent> ();

		sentryAudio = GetComponent<AudioSource> ();

		//bulletSpawner = transform.Find ("BulletSpawner").gameObject; 
		//frontObject = bulletSpawner;

		maxHealth = health; 

		//START State Machine
		StartCoroutine ("SSM");

		started = true;
	}

	IEnumerator SSM ()
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
			// TODO- this is a temporary fix to solve issues with level loading
			if (!started && GameObject.FindGameObjectWithTag ("Player") != null)
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

		if (!alive)
		{
			//agent.speed = 0; 

			if (anim.GetCurrentAnimatorStateInfo(1).IsName("DeathDone"))
			{
				DestroyEnemy(); 
			}
		}}

	void Idle ()
	{
		//transform.rotation = Quaternion.Euler (0, 0, 0);
		agent.destination = transform.position;
		//agent.speed = 0;

		sentryAudio.clip = idleSound;

		if (!sentryAudio.isPlaying)
		{
			sentryAudio.Play ();
		}

		anim.SetBool ("isShooting", false);
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

		if (curShotDelay == 0)
		{
			curShotDelay = shotDelay; 
			//ProjectileManager.inst.Shoot_E_Normal (bulletSpawner, false);
			ProjectileManager.inst.EnemyShoot(bulletSpawner, bulletPrefab, false); 

			sentryAudio.volume = Random.Range (0.8f, 1);
			sentryAudio.pitch = Random.Range (0.8f, 1);
			sentryAudio.PlayOneShot (attackSound);

			if (shotFireParticles != null)
				shotFireParticles.Play(); 
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

	protected override void EnemyDestroy()
	{

	}
}

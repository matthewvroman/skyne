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

	Rigidbody rBody;

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

	AudioSource chargerAudio;

	public AudioClip sawSound;
	public AudioClip idleSound;


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

		rBody = GetComponent<Rigidbody> ();

		chargerAudio = GetComponent<AudioSource> ();

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

	//The Idling state, what the enemy does when the player is not close.
	void Idle ()
	{
		agent.Resume ();

		if (tarDistance < aggroDistance && canSeeTarget())
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

		if (!chargerAudio.isPlaying)
		{
			chargerAudio.clip = sawSound;
			chargerAudio.volume = Mathf.Lerp (chargerAudio.volume, 0.8f, 10 * Time.deltaTime);
			chargerAudio.pitch = Mathf.Lerp (chargerAudio.pitch, 0.8f, 10 * Time.deltaTime);

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

		if (!chargerAudio.isPlaying)
		{
			chargerAudio.clip = sawSound;
			//chargerAudio.volume = 1f;
			//chargerAudio.pitch = 1.1f;

			chargerAudio.volume = Mathf.Lerp (chargerAudio.volume, 1, 10 * Time.deltaTime);
			chargerAudio.pitch = Mathf.Lerp (chargerAudio.pitch, 1.1f, 10 * Time.deltaTime);

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
}


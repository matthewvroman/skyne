using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss1_AI : Enemy
{

	public enum State
	{
		IDLE,
		HOMING_BULLET,
		BUSTER_SHOT,
		SPINNING,
		LASER,
		STOMP,
		//CHOOSE_ATTACK,
		DO_NOTHING
	}

	[Space(5)]
	[Header("Boss: State Machine")]
	public State state;

	float turnSpeed;

	[Space(5)]
	[Header("Boss: Behavior variables")]
	public float normTurnSpeed;
	public float laserTurnSpeed;

	public bool attacking;
	public bool choosing;

	public float timer;

	Vector3 oldEulerAngles;

	public float tarDistance;
	public float stompAggroDist;

	float tarHeight;
	public float upperLevelHeight;

	public float homingDelay;
	float curHomingDelay;
	public float homingLength;

	public float busterDelay;
	float curBusterDelay;
	public float busterLength;

	public float laserDelay;
	float curLaserDelay;
	public float laserLength;

	public float stompDelay;
	float curStompDelay;
	public float stompLength;

	bool isStomping = false;

	public float spinningDelay;
	float curSpinningDelay;
	public float spinningLength;

	public float nothingLength;

	public int chooseAttack;
	int lastAttack;

	float healthPer;

	//public Animator anim;

	[Space(5)]
	[Header("Boss: Bullet prefabs")]
	public GameObject smallHoming;
	public GameObject bigHoming;

	[Space(5)]
	[Header("Boss: Game objects")]
	public GameObject bulletSpawner1;
	public GameObject frontFacingObj; 

	GameObject stompCollider;
	GameObject stompColliderExpand;

	public GameObject laserObj;
	NavMeshAgent laserNav;

	public GameObject arm1;
	public GameObject arm2;

	bool isSpinning = false;

	AudioSource boss1Audio;

	[Space(5)]
	[Header("Boss: Audio")]
	public AudioClip spinSound;
	public AudioClip fireSound;
	public AudioClip moveSound;


	void SetupEnemy ()
	{
		ParentSetupEnemy();


		state = Boss1_AI.State.IDLE;

		attacking = false;
		choosing = true;

		turnSpeed = normTurnSpeed;

		boss1Audio = GetComponent<AudioSource> ();

		oldEulerAngles = transform.rotation.eulerAngles;

		stompCollider = transform.Find ("StompCollision").gameObject;
		stompColliderExpand = transform.Find ("StompCollisionExpand").gameObject;

		//START State Machine
		StartCoroutine ("B1SM");
	}
		

	IEnumerator B1SM ()
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
			
			case State.HOMING_BULLET:
				HomingShoot ();
				break;

			case State.BUSTER_SHOT:
				BusterShoot ();
				break;

			case State.LASER:
				Laser ();
				break;

			case State.STOMP:
				Stomp ();
				break;

			case State.SPINNING:
				Spinning ();
				break;

			case State.DO_NOTHING:
				DoNothing ();
				break;
			}
			yield return null;
		}
	}

	float GetDot()
	{
		// Uses flattened height by giving this position and the target the same y value
		Vector3 targetFlatPosition = new Vector3(target.transform.position.x, frontFacingObj.transform.position.y, target.transform.position.z); 
		Vector3 thisFlatPosition = new Vector3(frontFacingObj.transform.position.x, frontFacingObj.transform.position.y, frontFacingObj.transform.position.z); 

		return Vector3.Dot(frontFacingObj.transform.forward, (targetFlatPosition - thisFlatPosition).normalized);
	}

	// Update is called once per frame
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
			// Calculate the health percentage
			healthPer = ((health / maxHealth) * 100);

			// Update the stored distance from the player and the player height
			tarDistance = Vector3.Distance (target.transform.position, transform.position);
			tarHeight = target.transform.position.y;

			// Rotate the boss to face the player
			Quaternion q = Quaternion.LookRotation (target.transform.position - frontFacingObj.transform.position);
			q.x = 0;
			q.z = 0;
			transform.rotation = Quaternion.RotateTowards (frontFacingObj.transform.rotation, q, turnSpeed * Time.deltaTime);

			// Update sound and animation 
			float dot = GetDot(); 

			if (dot < 0.95f)
			{
				
			}
			else
			{
				
			}


			// Update the sound and animation based on the boss rotation
			if (Mathf.Abs(oldEulerAngles.y - transform.rotation.eulerAngles.y) <= 0.1f)
			{
				boss1Audio.clip = idleSound;
				if (!boss1Audio.isPlaying)
				{
					boss1Audio.Play ();
				}
				Debug.Log ("NO ROTATION");
				anim.SetFloat ("xDir", 0);
			} 
			else
			{
				//DO WHATEVER YOU WANT
				if (oldEulerAngles.y - transform.rotation.eulerAngles.y < 0)
				{
					boss1Audio.clip = moveSound;
					if (!boss1Audio.isPlaying)
					{
						boss1Audio.Play ();
					}
					Debug.Log ("Negative");
					anim.SetFloat ("xDir", -1);
				}
				else if (oldEulerAngles.y - transform.rotation.eulerAngles.y > 0.2)
				{
					boss1Audio.clip = moveSound;
					if (!boss1Audio.isPlaying)
					{
						boss1Audio.Play ();
					}
					Debug.Log ("Positive");
					anim.SetFloat ("xDir", 1);
				}
				oldEulerAngles = transform.rotation.eulerAngles;
			}

			// Update timers

			if (curHomingDelay >= 0)
			{
				curHomingDelay -= Time.deltaTime;
				if (curHomingDelay < 0)
					curHomingDelay = 0; 
			}

			if (curBusterDelay >= 0)
			{
				curBusterDelay -= Time.deltaTime;
				if (curBusterDelay < 0)
					curBusterDelay = 0; 
			}

			if (curLaserDelay >= 0)
			{
				curLaserDelay -= Time.deltaTime;
				if (curLaserDelay < 0)
					curLaserDelay = 0; 
			}

			if (curStompDelay >= 0)
			{
				curStompDelay -= Time.deltaTime;
				if (curStompDelay < 0)
					curStompDelay = 0; 
			}

			if (curSpinningDelay >= 0)
			{
				curSpinningDelay -= Time.deltaTime;
				if (curSpinningDelay < 0)
					curSpinningDelay = 0; 
			}
		} 

		if (target != null)
		{
			switch (chooseAttack)
			{
			case 1:
				if (timer <= 0)
				{
					timer = stompLength;
				}
				state = Boss1_AI.State.STOMP;
				break;

			case 2:
				if (timer <= 0)
				{
					timer = homingLength;
				}
				state = Boss1_AI.State.HOMING_BULLET;
				break;

			case 3:
				if (timer <= 0)
				{
					timer = busterLength;
				}
				state = Boss1_AI.State.BUSTER_SHOT;
				break;

			case 4:
				if (timer <= 0)
				{
					timer = nothingLength;
				}
				state = Boss1_AI.State.DO_NOTHING;
				break;

			case 5:
				if (timer <= 0)
				{
					timer = spinningLength;
				}
				state = Boss1_AI.State.SPINNING;
				break;

			case 6:
				if (timer <= 0)
				{
					timer = laserLength;
				}
				state = Boss1_AI.State.LASER;
				break;
			}
		}

		if (!alive)
		{
			if (anim.GetCurrentAnimatorStateInfo(3).IsName("DeathDone"))
			{
				DestroyEnemy(); 
			}
		}
	}

	void Idle ()
	{
		if (attacking == false && CanHitTarget ())
		{
			ChooseAttack ();
			attacking = true;
		} 
	}

	void HomingShoot ()
	{
		Debug.Log ("Homing...");

		if (timer > 0.1)
		{
			timer -= Time.deltaTime;

			if (curHomingDelay == 0)
			{
				curHomingDelay = homingDelay; 
				boss1Audio.PlayOneShot(fireSound);
				ProjectileManager.inst.EnemyShoot (bulletSpawner1, smallHoming, true);
			}

		}
		else
		{
			anim.SetTrigger ("HomingDone");
			choosing = true;
			ChooseAttack ();
		}
	}

	void BusterShoot ()
	{
		Debug.Log ("Buster...");

		if (timer > 0.1)
		{
			timer -= Time.deltaTime;

			if (curBusterDelay == 0)
			{
				curBusterDelay = busterDelay; 
				boss1Audio.PlayOneShot(fireSound);
				ProjectileManager.inst.EnemyShoot (bulletSpawner1, bigHoming, true);
			}
		}
		else
		{
			choosing = true;
			ChooseAttack ();
		}
	}

	void Laser ()
	{
		Debug.Log ("Laser..");
		turnSpeed = laserTurnSpeed;

		anim.SetTrigger ("Laser");

		if (timer < (laserLength - laserDelay))
		{
			laserObj.SetActive (true);

		}

		if (timer > 0.1)
		{
			timer -= Time.deltaTime;

			if (curLaserDelay == 0)
			{
				curLaserDelay = laserDelay; 
			}

		}
		else
		{
			anim.SetTrigger ("LaserDone");
			laserObj.SetActive (false);
			turnSpeed = normTurnSpeed;
			choosing = true;
			ChooseAttack ();
		}
	}

	void Spinning ()
	{
		Debug.Log ("Spinning..");

		if (timer > 0.1)
		{
			timer -= Time.deltaTime;

			arm1.SetActive (true);
			arm2.SetActive (true);

			if (timer < 0.5f)
			{
				//anim.SetTrigger ("Spin");
				boss1Audio.PlayOneShot (spinSound);
			}

			if (curSpinningDelay == 0)
			{
				
				anim.SetTrigger ("Spin");
				//boss1Audio.PlayOneShot (spinSound);
				curSpinningDelay = spinningDelay; 
			}

		}
		else
		{
			arm1.SetActive (false);
			arm2.SetActive (false);
			choosing = true;
			ChooseAttack ();
		}
	}

	void Stomp ()
	{
		Debug.Log ("Stomping");

		if (isStomping == true)
		{
			//stompCollider.transform.localScale = Vector3.Lerp (stompCollider.transform.localScale, stompColliderExpand.transform.localScale, Time.deltaTime * 0.999f);
		}

		if (timer > 0.1)
		{
			timer -= Time.deltaTime;

			anim.SetTrigger ("Stomp");

			if (timer < (stompLength - stompDelay))
			{
				stompCollider.transform.localScale = Vector3.Lerp (stompCollider.transform.localScale, stompColliderExpand.transform.localScale, Time.deltaTime * 0.999f);
			}

			if (curStompDelay == 0)
			{
				curStompDelay = stompDelay; 
				//stompCollider.SetActive (true);
				//isStomping = true;
			}

		}
		else
		{
			//stompCollider.SetActive (false);
			//isStomping = false;
			stompCollider.transform.localScale = new Vector3 (0, stompCollider.transform.localScale.y, 0);
			choosing = true;
			ChooseAttack ();
		}
	}

	void DoNothing ()
	{
		Debug.Log ("Waiting...");

		if (timer > 0.1)
		{
			timer -= Time.deltaTime;
		}
		else
		{
			choosing = true;
			ChooseAttack ();
		}
	}

	public int ChooseAttack ()
	{
		if (choosing)
		{
			if (tarDistance > stompAggroDist && tarHeight < upperLevelHeight)
			{
				chooseAttack = Random.Range (2, 5);
			}
			else if (tarDistance < stompAggroDist && tarHeight < upperLevelHeight)
			{
				chooseAttack = Random.Range (1, 5);
			}
			else if (tarHeight > upperLevelHeight)
			{
				chooseAttack = Random.Range (2, 7);
			}
			choosing = false;
		}

		timer = 0;
		curHomingDelay = homingDelay;
		curBusterDelay = busterDelay;
		curLaserDelay = laserDelay;
		curStompDelay = stompDelay;
		curSpinningDelay = spinningDelay;

		anim.SetTrigger ("LaserDone");

		return chooseAttack;
	}
		

	protected override void EnemyDestroy ()
	{
		Debug.Log("Boss death"); 

		GlobalManager.inst.LoadOutro(); 

		/*
		GameState.inst.keysFound [0] = true;
		//KeyPickupManager.inst.SpawnKeyPickup(transform.position, 0);


		KeyPickupManager.inst.SpawnTreasurePickup (transform.position);
		*/ 
	}
}

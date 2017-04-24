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
		//BUSTER_SHOT,
		SPINNING,
		LASER,
		STOMP,
		//CHOOSE_ATTACK,
		DO_NOTHING
	}

	[Space (5)]
	[Header ("Boss: State Machine")]
	public State state;

	float turnSpeed;

	[Space (5)]
	[Header ("Boss: Behavior variables")]
	public float normTurnSpeed;
	public float laserTurnSpeed;

	[Space (5)]
	[Header ("Boss: Spawn Enemies")]
	public GameObject Spawner1;
	public GameObject Spawner2;
	public GameObject Spawner3;
	public GameObject Spawner4;

	public GameObject[] enemyArray1;
	public GameObject[] enemyArray2;
	public GameObject[] enemyArray3;
	public GameObject[] enemyArray4;

	float spawnTimer1;
	float curSpawnTimer1;

	float spawnTimer2;
	float curSpawnTimer2;

	float spawnTimer3;
	float curSpawnTimer3;

	float spawnTimer4;
	float curSpawnTimer4;

	int chooseEnemy;

	public GameObject charger;
	public GameObject bolt;
	public GameObject sentry;


	[Space (5)]
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
	bool fireLaser;

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

	bool stompedGround = false;

	bool laserPoseReady = false;

	bool shootHomingBullets = false;

	public int phases;
	float healthPercent;

	//public Animator anim;

	[Space (5)]
	[Header ("Boss: Bullet prefabs")]
	public GameObject smallHoming;
	public GameObject bigHoming;

	[Space (5)]
	[Header ("Boss: Game objects")]
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

	[Space (5)]
	[Header ("Boss: Audio")]
	public AudioClip spinSound;
	public AudioClip fireSound;
	public AudioClip moveSound;
	public AudioClip laserSound;
	public AudioClip stompSound;

	public ParticleSystem stompRingParticles;
	public ParticleSystem stompDustParticles;
	public ParticleSystem laserChargeParticles;
	public ParticleSystem spinRingParticles;

	public bool isExploding; 
	public Explosion[] deathExplosions;

	public GameObject treasurePickupPrefab; 

	protected override void Start()
	{
		spawnPos = transform.position; 
		laserChargeParticles.enableEmission = false;
		spinRingParticles.enableEmission = false;  
	}


	void SetupEnemy ()
	{
		ParentSetupEnemy ();

		state = Boss1_AI.State.IDLE;

		attacking = false;
		choosing = true;

		turnSpeed = normTurnSpeed;

		boss1Audio = GetComponent<AudioSource> ();

		enemyArray1 = new GameObject[1];
		enemyArray2 = new GameObject[1];
		enemyArray3 = new GameObject[1];
		enemyArray4 = new GameObject[1];

		deathExplosions = GetComponentsInChildren<Explosion>();

		oldEulerAngles = transform.rotation.eulerAngles;

		isIdling = true;

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
			if (!GlobalManager.inst.GameplayIsActive () || !GameState.inst.inBossRoom)
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

			/*case State.BUSTER_SHOT:
				BusterShoot ();
				break; */

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

	float GetDot ()
	{
		// Uses flattened height by giving this position and the target the same y value
		Vector3 targetFlatPosition = new Vector3 (target.transform.position.x, frontFacingObj.transform.position.y, target.transform.position.z); 
		Vector3 thisFlatPosition = new Vector3 (frontFacingObj.transform.position.x, frontFacingObj.transform.position.y, frontFacingObj.transform.position.z); 

		return Vector3.Dot (frontFacingObj.transform.forward, (targetFlatPosition - thisFlatPosition).normalized);
	}

	// Update is called once per frame
	void Update ()
	{
		if (!GlobalManager.inst.GameplayIsActive () || !GameState.inst.inBossRoom)
		{
			return; 
		}

		isIdling = true;

		// If the enemy hasn't been set up yet, call it's setup
		// This isn't called until the game has been fully loaded to avoid any incomplete load null references
		if (!started)
		{
			SetupEnemy (); 
		}

		if (alive && target != null)
		{
			// Calculate the health percentage
			healthPercent = ((health / maxHealth) * 100);

			isIdling = true;

			Phases ();
			if (GameState.inst.inBossRoom == true)
			{
				SpawnEnemies ();
			}

			//Debug.Log (isIdling);

			// Update the stored distance from the player and the player height
			tarDistance = Vector3.Distance (target.transform.position, transform.position);
			tarHeight = target.transform.position.y;

			// Rotate the boss to face the player
			Quaternion q = Quaternion.LookRotation (target.transform.position - frontFacingObj.transform.position);
			q.x = 0;
			q.z = 0;
			transform.rotation = Quaternion.RotateTowards (frontFacingObj.transform.rotation, q, turnSpeed * Time.deltaTime);

			// Update sound and animation 
			float dot = GetDot (); 

			if (dot < 0.95f)
			{

			}
			else
			{

			}


			// Update the sound and animation based on the boss rotation
			if ((oldEulerAngles.y - transform.rotation.eulerAngles.y) < 1 && (oldEulerAngles.y - transform.rotation.eulerAngles.y) > -1)
			{
				//				boss1Audio.clip = idleSound;
				//				if (!boss1Audio.isPlaying)
				//				{
				//					boss1Audio.Play ();
				//				}
				Debug.Log ("NO ROTATION");
				//				anim.SetFloat ("xDir", 0);
			}
			else
			{
				//DO WHATEVER YOU WANT
				isIdling = true;

				if (oldEulerAngles.y - transform.rotation.eulerAngles.y < -1)
				{
					//					boss1Audio.clip = moveSound;
					//					if (!boss1Audio.isPlaying)
					//					{
					//						boss1Audio.Play ();
					//					}
					Debug.Log ("Negative");
					//					anim.SetFloat ("xDir", -1);
				}
				else if (oldEulerAngles.y - transform.rotation.eulerAngles.y > 1)
				{
					//					boss1Audio.clip = moveSound;
					//					if (!boss1Audio.isPlaying)
					//					{
					//						boss1Audio.Play ();
					//					}
					Debug.Log ("Positive");
					//					anim.SetFloat ("xDir", 1);
				}

				anim.SetFloat ("xDir", oldEulerAngles.y - transform.rotation.eulerAngles.y);
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
			case -1:
				if (timer <= 0)
				{
					timer = stompLength;
				}
				state = Boss1_AI.State.STOMP;
				break;

			case 0:
				if (timer <= 0)
				{
					timer = stompLength;
				}
				state = Boss1_AI.State.STOMP;
				break;

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
					timer = nothingLength;
				}
				state = Boss1_AI.State.DO_NOTHING;
				break;

			case 4:
				if (timer <= 0)
				{
					timer = spinningLength;
				}
				state = Boss1_AI.State.SPINNING;
				break;

			case 5:
				if (timer <= 0)
				{
					timer = laserLength;
				}
				state = Boss1_AI.State.LASER;
				break;

			case 6:
				if (timer <= 0)
				{
					timer = homingLength;
				}
				state = Boss1_AI.State.HOMING_BULLET;
				break;

			case 7:
				if (timer <= 0)
				{
					timer = homingLength;
				}
				state = Boss1_AI.State.HOMING_BULLET;
				break;

			case 8:
				if (timer <= 0)
				{
					timer = spinningLength;
				}
				state = Boss1_AI.State.SPINNING;
				break;
			}
		}

		if (!alive)
		{
			if (anim.GetCurrentAnimatorStateInfo (4).IsName ("DeathDone"))
			{
				//DestroyEnemy (); 
			}
		}
	}

	void Phases ()
	{
		if (healthPercent <= 30)
		{
			phases = 3;
		}
		else if (healthPercent <= 60)
		{
			phases = 2;
		}
		else
		{
			phases = 1;
		}

		switch (phases)
		{
		case 1:
			spawnTimer1 = 30;
			spawnTimer2 = 30;
			spawnTimer3 = 30;
			spawnTimer4 = 30;
			break;

		case 2:
			homingDelay = 0.5f;
			laserDelay = 4;
			laserLength = 12;

			spawnTimer1 = 20;
			spawnTimer2 = 20;
			spawnTimer3 = 20;
			spawnTimer4 = 20;
			break;

		case 3:
			homingDelay = 0.3f;
			homingLength = 10;
			laserDelay = 3;
			laserLength = 13;

			spawnTimer1 = 10;
			spawnTimer2 = 10;
			spawnTimer3 = 10;
			spawnTimer4 = 10;
			break;
		}
	}

	void SpawnEnemies ()
	{
		if (curSpawnTimer1 > 0 && Spawner1.transform.childCount == 0)
		{
			curSpawnTimer1 -= Time.deltaTime;
		}
		else if (curSpawnTimer1 <= 0 && Spawner1.transform.childCount == 0)
		{
			curSpawnTimer1 = 0;
		}
		else if (Spawner1.transform.childCount >= 1)
		{
			curSpawnTimer1 = spawnTimer1;
		}

		if (curSpawnTimer1 == 0)
		{
			chooseEnemy = Random.Range (1, 4);

			switch (chooseEnemy)
			{
			case 1:
				GameObject bossEnemy1 = GameObject.Instantiate (charger, Spawner1.transform.position, Quaternion.Euler (0, 0, 0), Spawner1.transform);
				enemyArray1 [0] = bossEnemy1;
				break;

			case 2:
				GameObject bossEnemy2 = GameObject.Instantiate (bolt, Spawner1.transform.position, Quaternion.Euler (0, 0, 0), Spawner1.transform);
				enemyArray1 [0] = bossEnemy2;
				break;

			case 3:
				GameObject bossEnemy3 = GameObject.Instantiate (sentry, Spawner1.transform.position, Quaternion.Euler (0, 0, 0), Spawner1.transform);
				enemyArray1 [0] = bossEnemy3;
				break;
			}
		}

		if (curSpawnTimer2 > 0 && Spawner2.transform.childCount == 0)
		{
			curSpawnTimer2 -= Time.deltaTime;
		}
		else if (curSpawnTimer2 <= 0 && Spawner2.transform.childCount == 0)
		{
			curSpawnTimer2 = 0;
		}
		else if (Spawner2.transform.childCount >= 1)
		{
			curSpawnTimer2 = spawnTimer2;
		}

		if (curSpawnTimer2 == 0)
		{
			chooseEnemy = Random.Range (1, 4);

			switch (chooseEnemy)
			{
			case 1:
				GameObject bossEnemy1 = GameObject.Instantiate (charger, Spawner2.transform.position, Quaternion.Euler (0, 0, 0), Spawner2.transform);
				enemyArray2 [0] = bossEnemy1;
				break;

			case 2:
				GameObject bossEnemy2 = GameObject.Instantiate (bolt, Spawner2.transform.position, Quaternion.Euler (0, 0, 0), Spawner2.transform);
				enemyArray2 [0] = bossEnemy2;
				break;

			case 3:
				GameObject bossEnemy3 = GameObject.Instantiate (sentry, Spawner2.transform.position, Quaternion.Euler (0, 0, 0), Spawner2.transform);
				enemyArray2 [0] = bossEnemy3;
				break;
			}
		}

		if (curSpawnTimer3 > 0 && Spawner3.transform.childCount == 0)
		{
			curSpawnTimer3 -= Time.deltaTime;
		}
		else if (curSpawnTimer3 <= 0 && Spawner3.transform.childCount == 0)
		{
			curSpawnTimer3 = 0;
		}
		else if (Spawner3.transform.childCount >= 1)
		{
			curSpawnTimer3 = spawnTimer3;
		}

		if (curSpawnTimer3 == 0)
		{
			chooseEnemy = Random.Range (1, 4);

			switch (chooseEnemy)
			{
			case 1:
				GameObject bossEnemy1 = GameObject.Instantiate (charger, Spawner3.transform.position, Quaternion.Euler (0, 0, 0), Spawner3.transform);
				enemyArray3 [0] = bossEnemy1;
				break;

			case 2:
				GameObject bossEnemy2 = GameObject.Instantiate (bolt, Spawner3.transform.position, Quaternion.Euler (0, 0, 0), Spawner3.transform);
				enemyArray3 [0] = bossEnemy2;
				break;

			case 3:
				GameObject bossEnemy3 = GameObject.Instantiate (sentry, Spawner3.transform.position, Quaternion.Euler (0, 0, 0), Spawner3.transform);
				enemyArray3 [0] = bossEnemy3;
				break;
			}
		}

		if (curSpawnTimer4 > 0 && Spawner4.transform.childCount == 0)
		{
			curSpawnTimer4 -= Time.deltaTime;
		}
		else if (curSpawnTimer4 <= 0 && Spawner4.transform.childCount == 0)
		{
			curSpawnTimer4 = 0;
		}
		else if (Spawner4.transform.childCount >= 1)
		{
			curSpawnTimer4 = spawnTimer1;
		}

		if (curSpawnTimer4 == 0)
		{
			chooseEnemy = Random.Range (1, 4);

			switch (chooseEnemy)
			{
			case 1:
				GameObject bossEnemy1 = GameObject.Instantiate (charger, Spawner4.transform.position, Quaternion.Euler (0, 0, 0), Spawner4.transform);
				enemyArray4 [0] = bossEnemy1;
				break;

			case 2:
				GameObject bossEnemy2 = GameObject.Instantiate (bolt, Spawner4.transform.position, Quaternion.Euler (0, 0, 0), Spawner4.transform);
				enemyArray4 [0] = bossEnemy2;
				break;

			case 3:
				GameObject bossEnemy3 = GameObject.Instantiate (sentry, Spawner4.transform.position, Quaternion.Euler (0, 0, 0), Spawner4.transform);
				enemyArray4 [0] = bossEnemy3;
				break;
			}

		}
	}

	void Idle ()
	{
		if (GameState.inst.inBossRoom == true)
		{
			ChooseAttack ();
			attacking = true;
		}
	}

	void HomingShoot ()
	{
		anim.SetBool ("Homing", true);

		if (shootHomingBullets == true)
		{
			//			ProjectileManager.inst.EnemyShoot (bulletSpawner1, smallHoming, true);
			if (timer > 0.1)
			{
				timer -= Time.deltaTime;

				if (curHomingDelay == 0)
				{
					boss1Audio.PlayOneShot (fireSound);
					curHomingDelay = homingDelay; 
					shotFireParticles.Play ();
					ProjectileManager.inst.EnemyShoot (bulletSpawner1, smallHoming, true);
				}

			}
			else
			{
				anim.SetBool ("HomingDone", true);
			}
		}




		//		Debug.Log ("Homing...");
		//
		//		if (timer > 0.1)
		//		{
		//			timer -= Time.deltaTime;
		//
		//			if (curHomingDelay == 0)
		//			{
		//				curHomingDelay = homingDelay; 
		//				boss1Audio.PlayOneShot (fireSound);
		//				ProjectileManager.inst.EnemyShoot (bulletSpawner1, smallHoming, true);
		//			}
		//
		//		}
		//		else
		//		{
		//			anim.SetTrigger ("HomingDone");
		//			choosing = true;
		//			ChooseAttack ();
		//		}

	}

	/*void BusterShoot ()
	{
		Debug.Log ("Buster...");

		if (timer > 0.1)
		{
			timer -= Time.deltaTime;

			if (curBusterDelay == 0)
			{
				curBusterDelay = busterDelay; 
				boss1Audio.PlayOneShot (fireSound);
				ProjectileManager.inst.EnemyShoot (bulletSpawner1, bigHoming, true);
			}
		}
		else
		{
			choosing = true;
			ChooseAttack ();
		}
	} */

	void Laser ()
	{
		Debug.Log ("Laser..");
		turnSpeed = laserTurnSpeed;

		if (laserPoseReady)
		{
			laserChargeParticles.enableEmission = false; 
		}
		else
		{
			laserChargeParticles.enableEmission = true; 
		}

		anim.SetBool ("Laser", true);


		if (timer < (laserLength - laserDelay))
		{
			laserObj.SetActive (true);

			boss1Audio.clip = laserSound;
			if (!boss1Audio.isPlaying)
			{
				boss1Audio.Play ();
			}
		}

		if (laserPoseReady)
		{
			if (timer > 0.1)
			{
				timer -= Time.deltaTime;
			}
			else
			{
				//				boss1Audio.Stop ();
				boss1Audio.clip = null;
				if (!boss1Audio.isPlaying)
				{
					boss1Audio.Play ();
				}
				laserObj.SetActive (false);
				anim.SetBool ("LaserDone", true);
				turnSpeed = normTurnSpeed;
			}
		}

		//		if (timer > 0.1)
		//		{
		//			timer -= Time.deltaTime;
		//
		//			if (curLaserDelay == 0)
		//			{
		//				curLaserDelay = laserDelay; 
		//			}
		//
		//		}
		//		else
		//		{
		//			anim.SetTrigger ("StopLaser");
		//			//laserObj.SetActive (false);
		//			fireLaser = false;
		//			turnSpeed = normTurnSpeed;
		//			//choosing = true;
		//			//ChooseAttack ();
		//		}
	}

	void Spinning ()
	{
		Debug.Log ("Spinning..");

		anim.SetBool ("Spin", true);

		// Rotate
		spinRingParticles.transform.Rotate (0, 1.5f * Time.deltaTime, 0, Space.World);

		//		if (timer > 0.1)
		//		{
		//			timer -= Time.deltaTime;
		//
		//			arm1.SetActive (true);
		//			arm2.SetActive (true);
		//
		//			if (timer < 0.5f)
		//			{
		//				//anim.SetTrigger ("Spin");
		//				
		//			}
		//
		//			if (curSpinningDelay == 0)
		//			{
		//				
		//				anim.SetTrigger ("Spin");
		//				//boss1Audio.PlayOneShot (spinSound);
		//				curSpinningDelay = spinningDelay; 
		//			}
		//
		//		}
		//		else
		//		{
		//			arm1.SetActive (false);
		//			arm2.SetActive (false);
		//			choosing = true;
		//			ChooseAttack ();
		//		}
	}

	void Stomp ()
	{
		Debug.Log ("Stomping");

		/*
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
		}*/

		anim.SetBool ("Stomp", true);

		if (stompedGround == true)
		{
			stompCollider.transform.localScale = Vector3.Lerp (stompCollider.transform.localScale, stompColliderExpand.transform.localScale, Time.deltaTime * 0.999f);
		}



		//		else
		//		{
		//			//stompCollider.SetActive (false);
		//			//isStomping = false;
		//			stompCollider.transform.localScale = new Vector3 (0, stompCollider.transform.localScale.y, 0);
		//			choosing = true;
		//			ChooseAttack ();
		//		}
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
			AttackDone ();
			//			choosing = true;
			//			ChooseAttack ();
		}
	}

	public int ChooseAttack ()
	{
		if (choosing)
		{
			if (phases == 1)
			{
				if (tarDistance > stompAggroDist && tarHeight < upperLevelHeight)
				{
					chooseAttack = Random.Range (2, 4);
				}
				else if (tarDistance < stompAggroDist && tarHeight < upperLevelHeight)
				{
					chooseAttack = Random.Range (1, 4);
				}
				else if (tarHeight > upperLevelHeight)
				{
					chooseAttack = Random.Range (2, 7);
				}
			}
			else if (phases == 2)
			{
				if (tarDistance > stompAggroDist && tarHeight < upperLevelHeight)
				{
					chooseAttack = Random.Range (2, 4);
				}
				else if (tarDistance < stompAggroDist && tarHeight < upperLevelHeight)
				{
					chooseAttack = Random.Range (0, 4);
				}
				else if (tarHeight > upperLevelHeight)
				{
					chooseAttack = Random.Range (2, 7);
				}
			}
			else if (phases == 3)
			{
				if (tarDistance > stompAggroDist && tarHeight < upperLevelHeight)
				{
					chooseAttack = Random.Range (2, 4);
				}
				else if (tarDistance < stompAggroDist && tarHeight < upperLevelHeight)
				{
					chooseAttack = Random.Range (-1, 4);
				}
				else if (tarHeight > upperLevelHeight)
				{
					chooseAttack = Random.Range (2, 9);
				}
			}	

			choosing = false;
		}

		timer = 0;
		curHomingDelay = homingDelay;
		curBusterDelay = busterDelay;
		curLaserDelay = laserDelay;
		curStompDelay = stompDelay;
		curSpinningDelay = spinningDelay;

		return chooseAttack;
	}


	protected override void EnemyDestroy ()
	{
		Debug.Log ("Boss death"); 
		anim.SetFloat ("xDir", 0);

		GlobalManager.inst.LoadOutro (); 

		/*
		GameState.inst.keysFound [0] = true;
		//KeyPickupManager.inst.SpawnKeyPickup(transform.position, 0);


		KeyPickupManager.inst.SpawnTreasurePickup (transform.position);
		*/ 
	}

	void FireLaser ()
	{
		laserPoseReady = true;
	}

	void LaserFinish ()
	{
		boss1Audio.Stop ();
		laserObj.SetActive (false);
	}

	void LaunchHoming ()
	{ 
		shootHomingBullets = true;
	}

	void HomingDone ()
	{
		shootHomingBullets = false;
		AttackDone ();
	}

	void EnableSpinDamage ()
	{
		arm1.SetActive (true);
		arm2.SetActive (true);
		spinRingParticles.enableEmission = true; 
	}

	void SpinDone ()
	{
		arm1.SetActive (false);
		arm2.SetActive (false);
		spinRingParticles.enableEmission = false; 
	}

	void StompDone ()
	{
		stompedGround = false;
		stompCollider.transform.localScale = new Vector3 (0, stompCollider.transform.localScale.y, 0);
		AttackDone ();
	}

	void StompedGround ()
	{
		stompRingParticles.Play (); 
		stompDustParticles.Play (); 
		stompedGround = true;
	}

	void AttackDone ()
	{
		laserPoseReady = false;
		anim.SetBool ("LaserDone", false);
		anim.SetBool ("HomingDone", false);
		anim.SetBool ("Homing", false);
		anim.SetBool ("Laser", false);
		anim.SetBool ("Spin", false);
		anim.SetBool ("Stomp", false);
		choosing = true;
		ChooseAttack ();
	}

	void MoveSFX ()
	{
		boss1Audio.PlayOneShot (moveSound);
	}

	void SpinSFX ()
	{
		boss1Audio.PlayOneShot (spinSound);
	}

	void laserSFX ()
	{
		boss1Audio.clip = laserSound;
		if (!boss1Audio.isPlaying)
		{
			boss1Audio.Play ();
		}
	}

	void StompSFX ()
	{
		boss1Audio.PlayOneShot (stompSound);
	}

	void StartSpawningExplosion ()
	{
		isExploding = true; 
		StartCoroutine("TriggerExplosions"); 
	}

	void StopSpawningExplosion ()
	{
		isExploding = false; 
		smokeParticles[1].system.enableEmission = false; 
		smokeParticles[2].system.enableEmission = false; 
	}

	void SpawnEndGameLight ()
	{
		GameObject treasure = GameObject.Instantiate(treasurePickupPrefab, bulletSpawner1.transform.position, transform.rotation, transform); 
		treasure.transform.Rotate(40, 0, 0); 
	}

	//public void TriggerExplosions()
	public IEnumerator TriggerExplosions()
	{
		int safetyCheck; 
		bool valid;
		int choosenIndex; 

		if (deathExplosions.Length > 0)
		{
			// Coroutine loop until isExploding is set false in StopSpawningExplosion ()
			while (isExploding)
			{
				safetyCheck = 0; 
				valid = false; 
				choosenIndex = 0; 

				while (!valid && safetyCheck < 50)
				{
					safetyCheck++; 
					choosenIndex = Random.Range(0, deathExplosions.Length); 

					if (deathExplosions[choosenIndex].CheckExplosionDone())
					{
						valid = true; 
					}
				}

				if (valid)
				{
					deathExplosions[choosenIndex].PlayParticles(); 
				}

				yield return new WaitForSeconds (0.2f); 
			}
		}
	}
}
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

	public State state;

	int phase;

	float turnSpeed;

	public float normTurnSpeed;
	public float laserTurnSpeed;

	public bool attacking;
	public bool choosing;

	//public Transform pos1;
	//public Transform pos2;
	//public Transform pos3;

	//public float moveSpeed;

	public float timer;

	public float tarDistance;
	public float stompAggroDist;

	float tarHeight;
	public float upperLevelHeight;

	public GameObject smallHoming;
	public GameObject bigHoming;

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

	GameObject target;

	GameObject boss;

	public Animator anim;

	public GameObject bulletSpawner1;

	GameObject stompCollider;
	GameObject stompColliderExpand;

	GameObject laserCol;
	NavMeshAgent laserNav;

	bool isSpinning = false;

	//AudioSource boss1Audio;

	//public AudioClip moveSound;
	//public AudioClip busterSound;
	//public AudioClip homingSound;

	// Use this for initialization
	void Start ()
	{
		/*target = GameObject.FindGameObjectWithTag ("Player");

		state = Boss1_AI.State.IDLE;
		alive = true;

		bulletSpawner1 = transform.Find ("BulletSpawner1").gameObject; 
		bulletSpawner1 = transform.Find ("BulletSpawner2").gameObject; 
		bulletSpawner1 = transform.Find ("BulletSpawner3").gameObject; 

		//START State Machine
		StartCoroutine ("B1SM"); */
	}

	void SetupBoss ()
	{
		target = GameObject.FindGameObjectWithTag ("Player");

		state = Boss1_AI.State.IDLE;
		alive = true;

		phase = 1;

		attacking = false;
		choosing = true;

		turnSpeed = normTurnSpeed;

		//boss1Audio = GetComponent<AudioSource> ();

		//boss = transform.Find ("Boss").gameObject;
		boss = GameObject.Find ("Boss");

		anim = transform.Find ("Boss2TallBoi_Model").GetComponent<Animator> ();

		//boss1Audio.Play ();

		//bulletSpawner1 = boss.transform.Find ("BulletSpawner1").gameObject; 

		//upperLevelHeight 

		stompCollider = transform.Find ("StompCollision").gameObject;
		stompColliderExpand = transform.Find ("StompCollisionExpand").gameObject;

		laserCol = transform.Find ("LaserCol").gameObject;
		laserNav = laserCol.GetComponent<NavMeshAgent> ();

		//START State Machine
		StartCoroutine ("B1SM");

		started = true;
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

	IEnumerator B1SM ()
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

	// Update is called once per frame
	void Update ()
	{
		if (GlobalManager.inst.GameplayIsActive ())
		{
			if (!started)
			{
				SetupBoss (); 
			}

			Phases ();

			healthPer = ((health / maxHealth) * 100);

			tarDistance = Vector3.Distance (target.transform.position, transform.position);
			tarHeight = target.transform.position.y;

			Quaternion q = Quaternion.LookRotation (target.transform.position - bulletSpawner1.transform.position);
			q.x = 0;
			q.z = 0;
			transform.rotation = Quaternion.RotateTowards (bulletSpawner1.transform.rotation, q, turnSpeed * Time.deltaTime);
			//transform.rotation = Quaternion.RotateTowards(


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

		if (GlobalManager.inst.GameplayIsActive () && target != null)
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
	}

	void Idle ()
	{
		if (attacking == false && canSeeTarget ())
		{
			ChooseAttack ();
			attacking = true;

			/*boss1Audio.clip = moveSound;

			if (!boss1Audio.isPlaying)
			{
				boss1Audio.Play ();
			} */
		} 
	}

	void Phases ()
	{
		if (healthPer <= 25)
		{
			phase = 3;
			//boss.transform.position = Vector3.Lerp (boss.transform.position, pos3.position, moveSpeed * Time.deltaTime);

			/*if (boss.transform.position != pos3.position)
			{
				boss1Audio.clip = moveSound;

				if (!boss1Audio.isPlaying)
				{
					boss1Audio.Play ();
				}
			} */
		}
		else if (healthPer <= 50)
		{
			phase = 2;
			//boss.transform.position = Vector3.Lerp (boss.transform.position, pos2.position, moveSpeed * Time.deltaTime);

			/*if (boss.transform.position != pos2.position)
			{
				boss1Audio.clip = moveSound;

				if (!boss1Audio.isPlaying)
				{
					boss1Audio.Play ();
				}
			} */
		}
		else
		{
			phase = 1;
			//boss.transform.position = pos1.position;

			//boss1Audio.clip = null;
		}
	}

	void HomingShoot ()
	{
		Debug.Log ("Homing...");

		/*boss1Audio.clip = moveSound;

		if (!boss1Audio.isPlaying)
		{
			boss1Audio.Play ();
		} */

		if (timer > 0.1)
		{
			timer -= Time.deltaTime;

			if (curHomingDelay == 0)
			{
				curHomingDelay = homingDelay; 
				//ProjectileManager.inst.Shoot_BossHomingOrb (bulletSpawner1); //.Shoot_E_Normal (bulletSpawner1, true);  
				ProjectileManager.inst.EnemyShoot (bulletSpawner1, smallHoming, true);

				//boss1Audio.volume = Random.Range (0.8f, 1);
				//boss1Audio.pitch = Random.Range (0.8f, 1);
				//boss1Audio.PlayOneShot (homingSound);
				//boss1Audio.PlayOneShot (homingSound);
			}

		}
		else
		{
			choosing = true;
			ChooseAttack ();
		}
	}

	void BusterShoot ()
	{
		Debug.Log ("Buster...");

		/*boss1Audio.clip = moveSound;

		if (!boss1Audio.isPlaying)
		{
			boss1Audio.Play ();
		} */

		if (timer > 0.1)
		{
			timer -= Time.deltaTime;

			if (curBusterDelay == 0)
			{
				curBusterDelay = busterDelay; 
				//ProjectileManager.inst.Shoot_BossBigOrb (bulletSpawner1); //Shoot_E_Normal (bulletSpawner3, true);
				ProjectileManager.inst.EnemyShoot (bulletSpawner1, bigHoming, true);

				//boss1Audio.volume = Random.Range (0.8f, 1);
				//boss1Audio.pitch = Random.Range (0.8f, 1);
				//boss1Audio.PlayOneShot (busterSound);
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

		if (timer < (laserLength - laserDelay))
		{
			laserCol.SetActive (true);

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
			laserCol.SetActive (false);
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

			if (timer < (spinningLength - spinningDelay))
			{
				//anim.SetTrigger ("Spin");
			}

			if (curSpinningDelay == 0)
			{
				anim.SetTrigger ("Spin");
				curSpinningDelay = spinningDelay; 
			}

		}
		else
		{
			choosing = true;
			ChooseAttack ();
		}
	}

	void Stomp ()
	{
		Debug.Log ("Stomping");

		if (isStomping = true)
		{
			//stompCollider.transform.localScale = Vector3.Lerp (stompCollider.transform.localScale, stompColliderExpand.transform.localScale, Time.deltaTime * 0.999f);
		}

		if (timer > 0.1)
		{
			timer -= Time.deltaTime;

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

		/*boss1Audio.clip = moveSound;

		if (!boss1Audio.isPlaying)
		{
			boss1Audio.Play ();
		} */

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

		return chooseAttack;
	}

	/*void OnTriggerEnter (Collider col)
	{
		if (col.gameObject.GetComponent<Bullet> ().playerBullet)
		{
			boss1Audio.volume = 1;
			boss1Audio.pitch = 1;
			boss1Audio.clip = null;
			boss1Audio.PlayOneShot (damageSound);
		}
	} */

	protected override void EnemyDestroy ()
	{
		GameState.inst.keysFound [0] = true;
		//KeyPickupManager.inst.SpawnKeyPickup(transform.position, 0);

		KeyPickupManager.inst.SpawnTreasurePickup (transform.position); 
	}
}

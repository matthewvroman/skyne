using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1_AI : Enemy
{

	public enum State
	{
		IDLE,
		HOMING_BULLET,
		BUSTER_SHOT,
		//CHOOSE_ATTACK,
		DO_NOTHING
	}

	public State state;

	int phase;

	public bool attacking;
	public bool choosing;

	public Transform pos1;
	public Transform pos2;
	public Transform pos3;

	public float moveSpeed;

	public float timer;

	public float homingDelay;
	float curHomingDelay;
	public float homingLength;

	public float busterDelay;
	float curBusterDelay;
	public float busterLength;

	public float nothingLength;

	public int chooseAttack;
	int lastAttack;

	float healthPer;

	public GameObject target;

	public GameObject boss;

	public GameObject bulletSpawner1;
	public GameObject bulletSpawner2;
	public GameObject bulletSpawner3;

	AudioSource boss1Audio;

	public AudioClip moveSound;
	public AudioClip busterSound;
	public AudioClip homingSound;

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

		boss1Audio = GetComponent<AudioSource> ();

		boss = transform.Find ("Boss").gameObject;

		boss1Audio.Play ();

		bulletSpawner1 = boss.transform.Find ("BulletSpawner1").gameObject; 
		bulletSpawner2 = boss.transform.Find ("BulletSpawner2").gameObject; 
		bulletSpawner3 = boss.transform.Find ("BulletSpawner3").gameObject; 

		pos1 = transform.Find ("Pos1").gameObject.transform;
		pos2 = transform.Find ("Pos2").gameObject.transform;
		pos3 = transform.Find ("Pos3").gameObject.transform;

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
		} 

		if (GlobalManager.inst.GameplayIsActive () && target != null)
		{
			switch (chooseAttack)
			{
			case 1:
				if (timer <= 0)
				{
					timer = busterLength;
				}
				state = Boss1_AI.State.BUSTER_SHOT;
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
			}
		}
	}

	void Idle ()
	{
		if (attacking == false && canSeeTarget ())
		{
			ChooseAttack ();
			attacking = true;

			boss1Audio.clip = moveSound;

			if (!boss1Audio.isPlaying)
			{
				boss1Audio.Play ();
			}
		} 
	}

	void Phases ()
	{
		if (healthPer <= 25)
		{
			phase = 3;
			boss.transform.position = Vector3.Lerp (boss.transform.position, pos3.position, moveSpeed * Time.deltaTime);

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
			boss.transform.position = Vector3.Lerp (boss.transform.position, pos2.position, moveSpeed * Time.deltaTime);

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
			boss.transform.position = pos1.position;

			//boss1Audio.clip = null;
		}
	}

	void HomingShoot ()
	{
		Debug.Log ("Homing...");

		boss1Audio.clip = moveSound;

		if (!boss1Audio.isPlaying)
		{
			boss1Audio.Play ();
		}

		if (timer > 0.1)
		{
			timer -= Time.deltaTime;

			if (curHomingDelay == 0)
			{
				curHomingDelay = homingDelay; 
				ProjectileManager.inst.Shoot_BossHomingOrb (bulletSpawner1); //.Shoot_E_Normal (bulletSpawner1, true); 
				ProjectileManager.inst.Shoot_BossHomingOrb (bulletSpawner2); //.Shoot_E_Normal (bulletSpawner2, true); 

				boss1Audio.volume = Random.Range (0.8f, 1);
				boss1Audio.pitch = Random.Range (0.8f, 1);
				boss1Audio.PlayOneShot (homingSound);
				boss1Audio.PlayOneShot (homingSound);
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

		boss1Audio.clip = moveSound;

		if (!boss1Audio.isPlaying)
		{
			boss1Audio.Play ();
		}

		if (timer > 0.1)
		{
			timer -= Time.deltaTime;

			if (curBusterDelay == 0)
			{
				curBusterDelay = busterDelay; 
				ProjectileManager.inst.Shoot_BossBigOrb (bulletSpawner3); //Shoot_E_Normal (bulletSpawner3, true);

				boss1Audio.volume = Random.Range (0.8f, 1);
				boss1Audio.pitch = Random.Range (0.8f, 1);
				boss1Audio.PlayOneShot (busterSound);
			}
		}
		else
		{
			choosing = true;
			ChooseAttack ();
		}
	}

	void DoNothing ()
	{
		Debug.Log ("Waiting...");

		boss1Audio.clip = moveSound;

		if (!boss1Audio.isPlaying)
		{
			boss1Audio.Play ();
		}

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
			chooseAttack = Random.Range (1, 4);
			choosing = false;
		}

		timer = 0;
		curHomingDelay = homingDelay;
		curBusterDelay = busterDelay;

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
}

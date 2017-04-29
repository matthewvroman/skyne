using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimSounds : MonoBehaviour
{

	AudioSource animAudio;

	public AudioClip footstep1;
	public AudioClip footstep2;

	public AudioClip jump;
	public AudioClip airDash;
	public AudioClip death;

	public AudioClip hit1;
	public AudioClip hit2;

	public AudioClip landing;
	public AudioClip doubleJump;

	public AudioClip wallCling;

	public PlayerManager playerManager;

	int count;
	float timer = 1;

	// Use this for initialization
	void Start ()
	{
		animAudio = GetComponent<AudioSource> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (count >= 1 && timer > 0)
		{
			timer -= Time.deltaTime;
		}
		else if (timer <= 0)
		{
			count = 0;
			timer = 1;
		}
	}

	public void FootstepSFX ()
	{
		if (playerManager.getIsWalking () == true)
		{
			int num = Random.Range (1, 3);

			switch (num)
			{
			case 1:
				animAudio.PlayOneShot (footstep1);
				break;

			case 2:
				animAudio.PlayOneShot (footstep2);
				break;
			}
		}
	}

	public void JumpSFX ()
	{
		animAudio.PlayOneShot (jump);
	}

	public void DashSFX ()
	{
		animAudio.PlayOneShot (airDash);
	}

	public void DeathSFX ()
	{
		animAudio.PlayOneShot (death);
	}

	public void DamageSFX ()
	{
		if (playerManager.GetIsAlive () == true)
		{
			if (count < 1)
			{
				int num = Random.Range (1, 3);

				switch (num)
				{
				case 1:
					animAudio.PlayOneShot (hit1);
					count += 1;
					break;

				case 2:
					animAudio.PlayOneShot (hit2);
					count += 1;
					break;
				}
			}
		}
	}

	public void LandingSFX ()
	{
		animAudio.PlayOneShot (landing);

		if (playerManager != null)
		{
			playerManager.OnLanding (); 
			//Debug.Log("Play landing particles"); 
		}
		else
		{
			Debug.Log ("Tried to play landing particles, but playerMangager ref is null"); 
		}
	}

	public void DoubleJumpSFX ()
	{
		animAudio.PlayOneShot (doubleJump);
	}

	public void WallClingSFX ()
	{
		animAudio.PlayOneShot (wallCling);
	}
}

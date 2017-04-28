using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimSounds : MonoBehaviour {

	AudioSource animAudio;

	public AudioClip footstep1;

	public AudioClip jump;
	public AudioClip airDash;
	public AudioClip death;
	public AudioClip hit;
	public AudioClip landing;
	public AudioClip doubleJump;

	public PlayerManager playerManager; 

	// Use this for initialization
	void Start () {
		animAudio = GetComponent<AudioSource> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void FootstepSFX() {
		animAudio.PlayOneShot (footstep1);
	}

	public void JumpSFX() {
		animAudio.PlayOneShot (jump);
	}

	public void DashSFX() {
		animAudio.PlayOneShot (airDash);
	}

	public void DeathSFX() {
		animAudio.PlayOneShot (death);
	}

	public void DamageSFX() {
		animAudio.PlayOneShot (hit);
	}

	public void LandingSFX() 
	{
		animAudio.PlayOneShot (landing);

		if (playerManager != null)
		{
			playerManager.OnLanding(); 
			//Debug.Log("Play landing particles"); 
		}
		else
		{
			Debug.Log("Tried to play landing particles, but playerMangager ref is null"); 
		}
	}

	public void DoubleJumpSFX() {
		animAudio.PlayOneShot (doubleJump);
	}

	public void WallClingSFX ()
	{

	}
}

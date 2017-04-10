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

	// Use this for initialization
	void Start () {
		animAudio = GetComponent<AudioSource> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void playFootstep() {
		animAudio.PlayOneShot (footstep1);
	}

	public void playJump() {
		animAudio.PlayOneShot (jump);
	}

	public void playDash() {
		animAudio.PlayOneShot (airDash);
	}

	public void playDeath() {
		animAudio.PlayOneShot (death);
	}

	public void playHit() {
		animAudio.PlayOneShot (hit);
	}

	public void playLanding() {
		animAudio.PlayOneShot (landing);
	}

	public void playDoubleJump() {
		animAudio.PlayOneShot (doubleJump);
	}
}

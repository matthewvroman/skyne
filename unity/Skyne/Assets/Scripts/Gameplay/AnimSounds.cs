using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimSounds : MonoBehaviour {

	AudioSource animAudio;
	public AudioClip footstep1;

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
}

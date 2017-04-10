using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientController : MonoBehaviour {

	AudioSource audio1;

	GameObject player;

	public AudioClip overworldSound;
	public AudioClip underworldSound;

	// Use this for initialization
	void Start () {
		audio1 = GetComponent<AudioSource> ();
		audio1.Play ();
	}

	// Update is called once per frame
	void Update () {
		player = GameObject.FindGameObjectWithTag ("Player");

		if (player.transform.position.y < 50)
		{
			audio1.clip = underworldSound;
			if (!audio1.isPlaying)
			{
				audio1.Play ();
			}
		}
		else
		{
			audio1.clip = overworldSound;
			if (!audio1.isPlaying)
			{
				audio1.Play ();
			}
		}
	}
}

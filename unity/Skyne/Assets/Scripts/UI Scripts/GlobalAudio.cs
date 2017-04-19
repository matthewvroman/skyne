using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalAudio : Singleton<GlobalAudio> 
{
	public AudioClip uiClick; 

	AudioSource audio1; 

	// Use this for initialization
	void Start () 
	{
		audio1 = GetComponent<AudioSource>(); 
	}

	public void PlayUIClick()
	{
		audio1.PlayOneShot(uiClick); 
	}
}

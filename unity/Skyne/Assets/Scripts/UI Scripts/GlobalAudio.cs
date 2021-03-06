﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalAudio : Singleton<GlobalAudio> 
{
	public AudioClip uiClick; 
	public AudioClip uiMenuUp; 

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

	public void PlayMenuUp()
	{
		audio1.PlayOneShot(uiMenuUp); 
	}

	public void PlaySound(AudioClip clip)
	{
		audio1.PlayOneShot(clip); 
	}
}

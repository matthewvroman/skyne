﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour 
{
	//ParticleSystem particles; 

	public ParticleSystem[] particles; 
	public AudioSource explosionAudio; 

	public bool dontDestroyOnFinish; 

	// Use this for initialization
	void Start () 
	{
		//particles = GetComponent<ParticleSystem>(); 
		if (explosionAudio != null && explosionAudio.clip != null)
		{
			explosionAudio.Play(); 
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (explosionAudio != null)
		{
			if (CheckExplosionDone() && !explosionAudio.isPlaying && !dontDestroyOnFinish)
			{
				Destroy(this.gameObject, 0.5f); 
			}
		}
		else
		{
			if (CheckExplosionDone() && !dontDestroyOnFinish)
			{
				Destroy(this.gameObject, 0.5f); 
			}
		}
	}

	public bool CheckExplosionDone()
	{
		for (int i = 0; i < particles.Length; i++)
		{
			if (!particles[i].isStopped)
			{
				return false; 
			}
		}
		return true; 
	}

	// Can be called externally to set all particle effects in the system to play. Not useful if dontDestroyOnFinish == false
	public void PlayParticles()
	{
		for (int i = 0; i < particles.Length; i++)
		{
			particles[i].Play(); 
		}
	}
}

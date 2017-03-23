using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour 
{
	//ParticleSystem particles; 

	public ParticleSystem[] particles; 

	// Use this for initialization
	void Start () 
	{
		//particles = GetComponent<ParticleSystem>(); 
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (CheckExplosionDestroy())
		{
			Destroy(this.gameObject); 
		}
	}

	bool CheckExplosionDestroy()
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
}

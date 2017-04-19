using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveRoom : MonoBehaviour 
{
	[HideInInspector] public GameObject saveSpawnPoint; 

	SaveCollider saveCollider; 

	public ParticleSystem saveParticles1;
	public ParticleSystem saveParticles2;

	AudioSource audio1;
	public AudioClip saveSound;

	// Ensures that you can only save once per time entering the save room
	public bool readyToSave; 

	void Start()
	{
		saveCollider = transform.Find("SaveCollider").GetComponent<SaveCollider>(); 
		saveSpawnPoint = transform.Find("SaveSpawnPoint").gameObject; 

		audio1 = GetComponent<AudioSource> ();
	}

	void Update()
	{
		if (saveCollider.playerInside)
		{
			SaveRoomManager.inst.inSaveRoom = true; 
			SaveRoomManager.inst.curSaveRoom = this;
			if (Input.GetKeyDown (KeyCode.E))
			{
				audio1.PlayOneShot (saveSound);
			}
		}
		else if (SaveRoomManager.inst.curSaveRoom == this)
		{
			SaveRoomManager.inst.inSaveRoom = false; 
			SaveRoomManager.inst.justSaved = false;
		}

		if (!saveCollider.playerInside)
		{
			readyToSave = true;
		}
	}

	public void TriggerSaveParticles()
	{
		saveParticles1.Stop(); 
		saveParticles1.Play();
		saveParticles2.Stop(); 
		saveParticles2.Play();
	}
}

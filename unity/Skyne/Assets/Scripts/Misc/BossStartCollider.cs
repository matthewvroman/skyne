using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStartCollider : MonoBehaviour 
{

	public AudioClip bossBattleMusic;

	public DoorControl door;

	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Player")
		{
			GameState.inst.inBossRoom = true;
			LevelData.inst.RefreshLoadedScenes(); 

			GameObject.Find ("MusicController").GetComponent<AudioSource> ().clip = bossBattleMusic;

			if (!GameObject.Find ("MusicController").GetComponent<AudioSource> ().isPlaying)
			{
				GameObject.Find ("MusicController").GetComponent<AudioSource> ().Play ();
			}

			door.setDoorState (false);

			// Might want to put boss start code here

			Destroy(this.gameObject); 
		}
	}
}

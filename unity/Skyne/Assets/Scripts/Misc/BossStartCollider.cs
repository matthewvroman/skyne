using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStartCollider : MonoBehaviour 
{

	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Player")
		{
			GameState.inst.inBossRoom = true;
			LevelData.inst.RefreshLoadedScenes(); 

			// Might want to put boss start code here

			Destroy(this.gameObject); 
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameCollider : MonoBehaviour 
{
	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Player")
		{
			if (GameState.inst.escapeSequenceActive)
			{
				GlobalManager.inst.LoadTitle(); 
			}
		}
	}
}

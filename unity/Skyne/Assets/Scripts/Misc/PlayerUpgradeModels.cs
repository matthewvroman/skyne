using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class would probably be better using an event listener, but this is the easier implementation 
public class PlayerUpgradeModels : MonoBehaviour
{
	public GameObject doubleJumpModel; 
	public GameObject wallJumpModel; 
	public GameObject dashModel; 

	public void Update()
	{
		if (GlobalManager.inst.GameplayIsActive())
		{
			for (int i = 0; i < 3; i++)
			{
				if (GameState.inst.upgradesFound[i])
				{
					AddUpgrade(i); 
				}
			}
		}
	}
		
	public void AddUpgrade(int upgradeIndex)
	{
		if (upgradeIndex == 0 && doubleJumpModel != null)
		{
			doubleJumpModel.SetActive(true); 
		}
		else if (upgradeIndex == 1 && wallJumpModel != null)
		{
			wallJumpModel.SetActive(true); 
		}
		else if (upgradeIndex == 2 && dashModel != null)
		{
			dashModel.SetActive(true); 
		}
	}
}

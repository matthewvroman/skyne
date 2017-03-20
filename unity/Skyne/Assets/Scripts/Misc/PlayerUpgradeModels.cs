using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class would probably be better using an event listener, but this is the easier implementation 
public class PlayerUpgradeModels : MonoBehaviour
{
	public GameObject L_doubleJumpModel; 
	public GameObject L_wallJumpModel; 
	public GameObject L_dashModel; 

	public GameObject R_doubleJumpModel; 
	public GameObject R_wallJumpModel; 
	public GameObject R_dashModel;

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
		if (upgradeIndex == 0 && L_doubleJumpModel != null)
		{
			L_doubleJumpModel.SetActive(true);
			R_doubleJumpModel.SetActive(true);
		}
		else if (upgradeIndex == 1 && L_wallJumpModel != null)
		{
			L_wallJumpModel.SetActive(true);
			R_wallJumpModel.SetActive(true); 
		}
		else if (upgradeIndex == 2 && L_dashModel != null)
		{
			L_dashModel.SetActive(true); 
			R_dashModel.SetActive(true); 
		}
	}
}

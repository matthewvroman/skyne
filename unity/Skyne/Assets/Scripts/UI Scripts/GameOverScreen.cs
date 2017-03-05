using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; 

public class GameOverScreen : MonoBehaviour 
{
	[SerializeField] private Button retryButton; 
	[SerializeField] private Button quitToTitleButton; 

	public EventSystem gameOverEventSystem; 

	void Update()
	{
		if (EventSystem.current == null)
			EventSystem.current = gameOverEventSystem; 
	}

	public void OnRetryButton()
	{
		GlobalManager.inst.LoadGameplayScreen();
	}

	public void OnQuitToTitleButton()
	{
		GlobalManager.inst.LoadTitle(); 
	}
}

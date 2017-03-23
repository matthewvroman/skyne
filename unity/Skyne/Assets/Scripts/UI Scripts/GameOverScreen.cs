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

	public float transitionFadeOutSpeed; 

	void Update()
	{
		if (EventSystem.current == null)
			EventSystem.current = gameOverEventSystem; 
	}

	public void OnRetryButton()
	{
		if (!ScreenTransition.inst.transitionActive)
		{
			ScreenTransition.inst.SetFadeOut(transitionFadeOutSpeed); 
			StartCoroutine("StartGameFadeOut"); 
		}
	}

	public void OnQuitToTitleButton()
	{
		if (!ScreenTransition.inst.transitionActive)
		{ 
			ScreenTransition.inst.SetFadeOut(transitionFadeOutSpeed); 
			StartCoroutine("LoadTitleFadeOut"); 
		}
	}

	public IEnumerator StartGameFadeOut()
	{
		while (ScreenTransition.inst.curState != ScreenTransition.TransitionState.blackScreenRest)
		{
			yield return null; 
		}

		GlobalManager.inst.GameOverToLoadScreen(); 
	}

	public IEnumerator LoadTitleFadeOut()
	{
		while (ScreenTransition.inst.curState != ScreenTransition.TransitionState.blackScreenRest)
		{
			yield return null; 
		}
		GlobalManager.inst.LoadTitle(); 
	}
}

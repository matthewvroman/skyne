using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutroScreen : MonoBehaviour 
{
	public ImageSequence imageSequence;

	public float transitionFadeOutSpeed; 
	public float endWaitDelay; 

	// Use this for initialization
	void Start () 
	{
		imageSequence.StartSequence();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (imageSequence.imageSequencesFinished && ScreenTransition.inst.curState == ScreenTransition.TransitionState.transparentScreenRest)
		{
			Debug.Log("Outro: Image sequence finished"); 

			ScreenTransition.inst.SetFadeOut(transitionFadeOutSpeed);

			// Go back to title screen
			StartCoroutine("LoadTitleFadeOut"); 
		}
	}

	public IEnumerator LoadTitleFadeOut()
	{
		while (ScreenTransition.inst.curState != ScreenTransition.TransitionState.blackScreenRest)
		{
			yield return null; 
		}

		yield return new WaitForSecondsRealtime (endWaitDelay); 

		GlobalManager.inst.OutroToTitleScreen(); 
	}
}

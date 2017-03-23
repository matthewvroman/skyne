using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class LoadScreenViz : MonoBehaviour 
{
	public Text loadText; 

	public string[] textStates; 

	public int curTextState; 

	public float stateChangeDelay; 
	float stateTimer; 

	// Use this for initialization
	void Start () 
	{
		stateTimer = stateChangeDelay;
		loadText.text = textStates[curTextState]; 
	}
	
	// Update is called once per frame
	void Update () 
	{
		stateTimer -= Time.unscaledDeltaTime; 

		if (stateTimer <= 0)
		{
			// Factors in the negative when the timer is below zero and reset
			stateTimer = stateChangeDelay + stateTimer; 

			curTextState++; 
			if (curTextState >= textStates.Length)
			{
				curTextState = 0; 
			}
			loadText.text = textStates[curTextState]; 
		}
	}
}

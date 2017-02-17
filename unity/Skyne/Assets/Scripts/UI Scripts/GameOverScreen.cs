using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class GameOverScreen : MonoBehaviour 
{
	[SerializeField] private Button retryButton; 
	[SerializeField] private Button quitToTitleButton; 

	void Awake ()
	{
		retryButton.onClick.AddListener (() => { onTryAgainClicked(); });
		quitToTitleButton.onClick.AddListener (() => { onQuitToTitleClicked(); });
	}

	void onTryAgainClicked()
	{
		GlobalManager.inst.LoadGameplayScreen(); 
	}

	void onQuitToTitleClicked()
	{
		GlobalManager.inst.LoadTitle(); 
	}


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

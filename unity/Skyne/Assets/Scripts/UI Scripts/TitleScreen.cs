using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class TitleScreen : MonoBehaviour 
{
	[SerializeField] private Button newGameButton; 
	[SerializeField] private Button continueButton; 

	void Awake ()
	{
		newGameButton.onClick.AddListener (() => { OnNewGameButtonClicked(); });
		continueButton.onClick.AddListener (() => { ContinueButtonClicked(); });
	}

	void OnNewGameButtonClicked()
	{
		Debug.Log("New game button clicked"); 

		// For now, clear PlayerPrefs when a new gameplay screen is loaded
		PlayerPrefs.DeleteAll(); 

		GlobalManager.inst.LoadGameplayScreen(); 
	}

	void ContinueButtonClicked()
	{
		Debug.Log("Continue button clicked"); 
		GlobalManager.inst.LoadGameplayScreen(); 
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

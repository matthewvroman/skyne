using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class TitleScreen : MonoBehaviour 
{
	[SerializeField] private Button newGameButton; 
	[SerializeField] private Button continueButton; 

	public GameObject mainMenu;
	public GameObject settingsMenu;

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
	void Update () 
	{
		if (PlayerPrefsManager.inst.SaveExists())
		{
			continueButton.interactable = true; 
		}
		else
		{
			continueButton.interactable = false; 
		}
	}

	public void TerminateGame()
	{
		Application.Quit();
	}

	public void SettingsMenu ()
	{
		mainMenu.SetActive (false);
		settingsMenu.SetActive (true);
	}

	public void Back()
	{
		settingsMenu.SetActive (false);
		mainMenu.SetActive (true);
	}
}

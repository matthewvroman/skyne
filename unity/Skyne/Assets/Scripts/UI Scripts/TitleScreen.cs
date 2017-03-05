using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.EventSystems; 

public class TitleScreen : MonoBehaviour 
{
	//[SerializeField] private Button newGameButton; 
	[SerializeField] private Button continueButton; 

	[SerializeField] private Button settingsButton; 
	[SerializeField] private Button backButton; 

	public GameObject mainMenu;
	public GameObject settingsMenu;

	public EventSystem titleEventSystem; 
		
	
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

		if (EventSystem.current == null)
			EventSystem.current = titleEventSystem; 
	}


	// Button click functions
	public void OnNewGameButton()
	{
		Debug.Log("New game button clicked"); 

		// For now, clear PlayerPrefs when a new gameplay screen is loaded
		PlayerPrefs.DeleteAll(); 

		GlobalManager.inst.LoadGameplayScreen(); 
	}

	// LoadGameButton
	public void OnContinueButton()
	{
		Debug.Log("Continue button clicked"); 
		GlobalManager.inst.LoadGameplayScreen(); 
	}

	// SettingsButton
	public void OnSettingsButton ()
	{
		mainMenu.SetActive (false);
		settingsMenu.SetActive (true);

		EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(backButton.gameObject); 
	}

	// QuitGameButton
	public void OnQuitButton()
	{
		Application.Quit();
	}

	// BackButton
	public void OnBackButton()
	{
		settingsMenu.SetActive (false);
		mainMenu.SetActive (true);

		EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(settingsButton.gameObject);
	}
}

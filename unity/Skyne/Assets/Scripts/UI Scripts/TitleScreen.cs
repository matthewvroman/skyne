using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.EventSystems; 
using UnityEngine.SceneManagement; 

public class TitleScreen : MonoBehaviour 
{
	//[SerializeField] private Button newGameButton; 
	[SerializeField] private Button continueButton; 

	[SerializeField] private Button settingsButton; 
	[SerializeField] private Button backButton; 

	public GameObject mainMenu;
	public GameObject settingsMenu;

	public EventSystem titleEventSystem; 

	public float transitionFadeOutSpeed; 
		
	
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
		{
			EventSystem.current = titleEventSystem; 
		}

		Cursor.lockState = CursorLockMode.Locked;
	}


	// Button click functions
	public void OnNewGameButton()
	{
		if (!ScreenTransition.inst.transitionActive)
		{
			// For now, clear PlayerPrefs when a new gameplay screen is loaded
			PlayerPrefs.DeleteAll(); 

			ScreenTransition.inst.SetFadeOut(transitionFadeOutSpeed); 
			StartCoroutine("StartGameFadeOut"); 
		}
	}

	// LoadGameButton
	public void OnContinueButton()
	{
		if (!ScreenTransition.inst.transitionActive)
		{
			ScreenTransition.inst.SetFadeOut(transitionFadeOutSpeed); 
			StartCoroutine("StartGameFadeOut"); 
		}
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

	public IEnumerator StartGameFadeOut()
	{
		while (ScreenTransition.inst.curState != ScreenTransition.TransitionState.blackScreenRest)
		{
			yield return null; 
		}

		GlobalManager.inst.TitleToLoadScreen(); 
	}
}

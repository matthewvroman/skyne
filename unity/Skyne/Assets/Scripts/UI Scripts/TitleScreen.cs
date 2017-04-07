using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.EventSystems; 
using UnityEngine.SceneManagement; 

public class TitleScreen : MonoBehaviour 
{
	[SerializeField] private Button newGameButton; 
	[SerializeField] private Button continueButton; 
	[SerializeField] private Button settingsButton; 
	[SerializeField] private Button backButton; 
	[SerializeField] private Button quitButton; 

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
		else if (GlobalManager.inst.buttonUIIsActive)
		{
			continueButton.interactable = false; 
		}

		if (EventSystem.current == null)
		{
			EventSystem.current = titleEventSystem; 
		}

		//Cursor.lockState = CursorLockMode.Locked;

		//if (titleEventSystem.
	}


	// Button click functions
	public void OnNewGameButton()
	{
		if (!ScreenTransition.inst.transitionActive)
		{
			// For now, clear PlayerPrefs when a new gameplay screen is loaded
			PlayerPrefs.DeleteAll();

			GlobalManager.inst.buttonUIIsActive = false; 

			ScreenTransition.inst.SetFadeOut(transitionFadeOutSpeed); 
			StartCoroutine("StartGameFadeOut"); 
		}
	}

	// LoadGameButton
	public void OnContinueButton()
	{
		if (!ScreenTransition.inst.transitionActive)
		{
			GlobalManager.inst.buttonUIIsActive = false; 

			ScreenTransition.inst.SetFadeOut(transitionFadeOutSpeed); 
			StartCoroutine("StartGameFadeOut"); 
		}
	}

	// SettingsButton
	public void OnSettingsButton ()
	{
		mainMenu.SetActive (false);
		settingsMenu.SetActive (true);

		//EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(backButton.gameObject); 
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

		//EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(settingsButton.gameObject);
	}

	public IEnumerator StartGameFadeOut()
	{
		while (ScreenTransition.inst.curState != ScreenTransition.TransitionState.blackScreenRest)
		{
			yield return null; 
		}

		GlobalManager.inst.TitleToLoadScreen(); 
	}

	/// <summary>
	/// Deprecated. Use for key navigation in conjunction with mouse navigation
	/// To use, add an EventTrigger component to the button listening for a PointerEnter event
	/// Then, call this function and pass the buttonName string
	/// This keeps the last button the mouse hovered over selected 
	/// </summary>
	public void SetToSelectedButton(string buttonName)
	{
		if (buttonName == "NewGameButton")
		{
			titleEventSystem.SetSelectedGameObject(newGameButton.gameObject); 
		}
		else if (buttonName == "LoadGameButton")
		{
			titleEventSystem.SetSelectedGameObject(continueButton.gameObject); 
		}
		else if (buttonName == "SettingsButton")
		{
			titleEventSystem.SetSelectedGameObject(settingsButton.gameObject); 
		}
		else if (buttonName == "QuitGameButton")
		{
			titleEventSystem.SetSelectedGameObject(quitButton.gameObject); 
		}
	}
}

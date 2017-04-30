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

	public ImageSequence imageSequence; 

	bool revealUI = false; 
	[SerializeField] private Image[] uiImages; 
	public float uiFadeInSpeed; 
	public float fastUIFadeInSpeed; 

	public GameObject mainMenu;
	public GameObject settingsMenu;
	public GameObject creditsMenu; 
	public GameObject confirmNewGamePanel; 

	public EventSystem titleEventSystem; 

	public float transitionFadeOutSpeed; 




	void Start()
	{
		newGameButton.interactable = false; 
		continueButton.interactable = false; 
		settingsButton.interactable = false; 
		backButton.interactable = false; 
		quitButton.interactable = false; 

		HideUIAtStart(); 

		if (GlobalManager.inst.skipTitleIntro)
		{
			imageSequence.SkipToEnd(); 
			uiFadeInSpeed = fastUIFadeInSpeed; 
			//imageSequence.imageSequences[imageSequence.imageSequences.Length - 1].imageObj.transform.localPosition = new Vector3 (-111.7442f, 0, 0); 
		}
		else
		{
			imageSequence.StartSequence(); 
		}
	}
		
	
	// Update is called once per frame
	void Update () 
	{
		if (PlayerPrefsManager.inst.SaveExists())
		{
			continueButton.interactable = true; 
			continueButton.gameObject.SetActive(true);
		}
		else if (GlobalManager.inst.buttonUIIsActive)
		{
			continueButton.interactable = false; 
			continueButton.gameObject.SetActive(false); 
		}

		if (EventSystem.current == null)
		{
			EventSystem.current = titleEventSystem; 
		}

		if (!imageSequence.imageSequencesFinished)
		{
			if (Input.anyKeyDown || (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
			{
				imageSequence.SkipToEnd(); 
				uiFadeInSpeed = fastUIFadeInSpeed; 
				//imageSequence.imageSequences[imageSequence.imageSequences.Length - 1].imageObj.transform.localPosition = new Vector3 (-111.7442f, 0, 0); 
			}
		}

		if (imageSequence.imageSequencesFinished && !revealUI)
		{
			GlobalManager.inst.skipTitleIntro = true; 
			revealUI = true; 
			StartCoroutine("RevealUI"); 
		}
	}


	// Button click functions
	public void OnNewGameButton()
	{
		if (!ScreenTransition.inst.transitionActive)
		{
			// Reset player prefs, but keep settings-related data
			PlayerPrefsManager.inst.ResetPlayerPrefs(); 

			GlobalAudio.inst.PlayUIClick(); 

			GlobalManager.inst.buttonUIIsActive = false; 

			ScreenTransition.inst.SetFadeOut(transitionFadeOutSpeed); 
			StartCoroutine("StartGameFadeOut"); 
		}
	}

	public void OnTryNewGameButton()
	{
		if (PlayerPrefsManager.inst.SaveExists())
		{
			//mainMenu.SetActive(false); 
			confirmNewGamePanel.SetActive(true); 
			GlobalAudio.inst.PlayUIClick(); 
		}
		else
		{
			OnNewGameButton(); 
		}
	}

	// LoadGameButton
	public void OnContinueButton()
	{
		if (!ScreenTransition.inst.transitionActive)
		{
			GlobalManager.inst.buttonUIIsActive = false; 

			GlobalAudio.inst.PlayUIClick(); 

			ScreenTransition.inst.SetFadeOut(transitionFadeOutSpeed); 
			StartCoroutine("StartGameFadeOut"); 
		}
	}

	// SettingsButton
	public void OnSettingsButton ()
	{
		mainMenu.SetActive (false);
		settingsMenu.SetActive (true);
		GlobalAudio.inst.PlayUIClick(); 

		//EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(backButton.gameObject); 
	}

	// QuitGameButton
	public void OnQuitButton()
	{
		GlobalAudio.inst.PlayUIClick(); 
		Application.Quit();
	}

	// BackButton
	public void OnBackButton()
	{
		settingsMenu.SetActive (false);
		creditsMenu.SetActive(false); 
		mainMenu.SetActive (true);
		confirmNewGamePanel.SetActive(false); 

		GlobalAudio.inst.PlayUIClick(); 

		//EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(settingsButton.gameObject);
	}

	// Credits button
	public void OnCreditsButton()
	{
		mainMenu.SetActive(false); 
		creditsMenu.SetActive(true);
		GlobalAudio.inst.PlayUIClick(); 
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
	/*
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
	*/ 

	public void HideUIAtStart()
	{
		for (int i = 0; i < uiImages.Length; i++)
		{
			uiImages[i].color = new Color (1, 1, 1, 0); 
		}
	}

	public IEnumerator RevealUI()
	{
		Color curColor = new Color (1, 1, 1, 0); 

		while (curColor.a != 1)
		{
			curColor.a += uiFadeInSpeed * Time.unscaledDeltaTime; 
			if (curColor.a > 1)
				curColor.a = 1; 

			for (int i = 0; i < uiImages.Length; i++)
			{
				uiImages[i].color = curColor; 
			}
			yield return null; 
		}

		// Once the UI buttons have finished fading in, make the buttons interactable
		newGameButton.interactable = true; 
		continueButton.interactable = true; 
		settingsButton.interactable = true; 
		backButton.interactable = true; 
		quitButton.interactable = true;
	}
}

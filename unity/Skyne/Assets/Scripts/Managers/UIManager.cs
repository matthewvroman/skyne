using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.EventSystems; 

public class UIManager : Singleton<UIManager> 
{
	public enum GameMenuState {Options, Map}; 
	public GameMenuState gameMenuState; 

	public bool gameMenuActive; 

	public GameObject optionsPanel; 
	public GameObject mapPanel; 
	public GameObject confirmQuitPanel; 

	//public bool isPaused = false; //determines whether the game is paused or not.


	[SerializeField] private Button continueButton; 
	[SerializeField] private Button quitButton; 

	//[SerializeField] private Toggle level1Toggle; 
	//[SerializeField] private Toggle level2Toggle; 
	//[SerializeField] private Toggle level3Toggle; 

	[SerializeField] private GameObject topLevelToggle; 
	[SerializeField] private GameObject middleLevelToggle; 
	[SerializeField] private GameObject bottomLevelToggle; 

	[SerializeField] private GameObject sfxSlider; 
	[SerializeField] private GameObject musicSlider; 

	public EventSystem levelEventSystem; 

	[HideInInspector] public Text endTimerUI; 

	public Image radialGradient; 
	public float maxRadialAlpha; 
	public float radialChangeRate; 
	public ParticleSystem focusParticles; 

	void Start()
	{
		endTimerUI.text = ""; 
	}

	void Update()
	{
		//CheckPause ();
		//CheckStatusPanelReveal();

		if (EventSystem.current == null)
			EventSystem.current = levelEventSystem; 

		if (GlobalManager.inst.globalState != GlobalManager.GlobalState.Gameplay)
		{
			return; 
		}

		// Escape sequence UI
		if (GameState.inst.escapeSequenceActive)
		{
			int seconds = Mathf.RoundToInt(GameState.inst.escapeTimer); 
			endTimerUI.text = (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
		}


		if (Input.GetKeyDown(KeyCode.Escape))
		{
			// If not active, set game menu to active and set it up
			if (!gameMenuActive)
			{
				Debug.Log("Reveal game menu"); 

				GlobalManager.inst.SetGamePaused(true); 
				GlobalManager.inst.buttonUIIsActive = true; 

				if (gameMenuState == GameMenuState.Options)
				{
					EnableOptionsPanel(); 
				}
				else if (gameMenuState == GameMenuState.Map)
				{
					EnableMapPanel(); 
				}

				gameMenuActive = true; 
			}
			// If active, close the game menu
			else
			{
				if (confirmQuitPanel.activeSelf)
				{
					confirmQuitPanel.SetActive(false); 
				}
				else
				{
					GlobalManager.inst.SetGamePaused(false); 
					GlobalManager.inst.buttonUIIsActive = false;
					DisablePanels(); 
					gameMenuActive = false; 
				}

			}
		}


		// Update the map
		if (gameMenuActive && gameMenuState == GameMenuState.Map)
		{
			if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
			{
				MapDisplay.inst.IncrementDisplayFloor(); 
				UpdateMapPanelToggles(); 
			}
			else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
			{
				MapDisplay.inst.DecrementDisplayFloor(); 
				UpdateMapPanelToggles(); 
			}
		}

		// Update the radial gradient
		if (!GlobalManager.inst.gamePaused)
		{
			//float curAlpha = Mathf.Lerp(radialGradient.color.a, Timescaler.inst.percentSlowed * maxRadialAlpha, 0.1f); 

			radialGradient.color = new Color (radialGradient.color.r, radialGradient.color.g, radialGradient.color.b, Mathf.Lerp(radialGradient.color.a, Timescaler.inst.percentSlowed * maxRadialAlpha, radialChangeRate));
			//radialGradient.color = new Color (radialGradient.color.r, radialGradient.color.g, radialGradient.color.b, Timescaler.inst.percentSlowed * maxRadialAlpha);
		}

		// Update the focus particles
		if (!GlobalManager.inst.gamePaused && Time.timeScale < 1)
		{
			focusParticles.enableEmission = true; 
		}
		else
		{
			focusParticles.enableEmission = false; 
		}


		 
	}

	void EnableOptionsPanel()
	{
		mapPanel.SetActive(false);
		optionsPanel.SetActive(true); 
		//EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(continueButton.gameObject);
	}

	void EnableMapPanel()
	{
		mapPanel.SetActive(true);
		optionsPanel.SetActive(false); 

		MapDisplay.inst.displayLevel = LevelData.inst.curLevel; 
		UpdateMapPanelToggles(); 
	}

	void UpdateMapPanelToggles()
	{
		if (MapDisplay.inst.displayLevel == 1)
		{
			bottomLevelToggle.GetComponent<Toggle>().isOn = true; 
			middleLevelToggle.GetComponent<Toggle>().isOn = false; 
			topLevelToggle.GetComponent<Toggle>().isOn = false; 
		}
		else if (MapDisplay.inst.displayLevel == 2)
		{
			bottomLevelToggle.GetComponent<Toggle>().isOn = false; 
			middleLevelToggle.GetComponent<Toggle>().isOn = true; 
			topLevelToggle.GetComponent<Toggle>().isOn = false; 
		}
		else if (MapDisplay.inst.displayLevel == 3)
		{
			bottomLevelToggle.GetComponent<Toggle>().isOn = false; 
			middleLevelToggle.GetComponent<Toggle>().isOn = false; 
			topLevelToggle.GetComponent<Toggle>().isOn = true;  
		}
	}

	void DisablePanels()
	{ 
		mapPanel.SetActive(false);
		optionsPanel.SetActive(false);
		confirmQuitPanel.SetActive(false); 
	}
		
	public void OnContinueButton()
	{
		GlobalManager.inst.SetGamePaused(false); 
		GlobalManager.inst.buttonUIIsActive = false;
		DisablePanels(); 
		gameMenuActive = false; 
		//EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(null);

		GlobalAudio.inst.PlayUIClick(); 
	}

	public void OnQuitButton()
	{
		GlobalManager.inst.buttonUIIsActive = false;
		GlobalManager.inst.GameplayToTitleScreen(); 
		GlobalAudio.inst.PlayUIClick(); 
	}

	public void OnTryQuitButton()
	{
		confirmQuitPanel.SetActive(true); 
		GlobalAudio.inst.PlayUIClick(); 
	}

	public void OnCancelQuitButton()
	{
		confirmQuitPanel.SetActive(false);
		GlobalAudio.inst.PlayUIClick(); 
	}

	public void OnSettingsButton()
	{
		gameMenuState = GameMenuState.Options; 
		EnableOptionsPanel(); 
		GlobalAudio.inst.PlayUIClick(); 
	}

	public void OnBackToMapButton()
	{
		gameMenuState = GameMenuState.Map; 
		EnableMapPanel(); 
		GlobalAudio.inst.PlayUIClick(); 
	}

	// Deprecated
	public void OnTabButton()
	{
		if (gameMenuState == GameMenuState.Map)
		{
			gameMenuState = GameMenuState.Options; 
			EnableOptionsPanel();
		}
		else if (gameMenuState == GameMenuState.Options)
		{
			gameMenuState = GameMenuState.Map; 
			EnableMapPanel(); 
		}
	}

	public void OnLevelButton(string levelName)
	{
		if (levelName == "Bottom")
		{
			MapDisplay.inst.displayLevel = 1; 
		}
		else if (levelName == "Middle")
		{
			MapDisplay.inst.displayLevel = 2; 
		}
		else if (levelName == "Top")
		{
			MapDisplay.inst.displayLevel = 3; 
		}
		GlobalAudio.inst.PlayUIClick(); 
	}
}

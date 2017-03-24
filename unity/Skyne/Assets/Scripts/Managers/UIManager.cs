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

	//public bool isPaused = false; //determines whether the game is paused or not.


	[SerializeField] private Button reloadButton; 
	[SerializeField] private Button continueButton; 

	//[SerializeField] private Toggle level1Toggle; 
	//[SerializeField] private Toggle level2Toggle; 
	//[SerializeField] private Toggle level3Toggle; 

	[SerializeField] private GameObject topLevelButton; 
	[SerializeField] private GameObject middleLevelButton; 
	[SerializeField] private GameObject bottomLevelButton; 

	[SerializeField] private GameObject sfxSlider; 
	[SerializeField] private GameObject musicSlider; 

	public EventSystem levelEventSystem; 

	public Text endTimerUI; 

	void Awake()
	{
		//reloadButton.onClick.AddListener (() => { LoadGameClicked(); });
	}

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
				GlobalManager.inst.SetGamePaused(true); 

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
				GlobalManager.inst.SetGamePaused(false); 
				DisablePanels(); 
				gameMenuActive = false; 
				EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(null);
			}
		}
		else if (gameMenuActive)
		{
			if (EventSystem.current.GetComponent<EventSystem>().currentSelectedGameObject != sfxSlider && EventSystem.current.GetComponent<EventSystem>().currentSelectedGameObject != musicSlider)
			{
				// Shift menu left
				if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
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
				// Shift menu right
				else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
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
	}


	void EnableOptionsPanel()
	{
		mapPanel.SetActive(false);
		optionsPanel.SetActive(true); 
		EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(continueButton.gameObject);
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
			bottomLevelButton.GetComponent<Image>().color = new Color (1, 1, 1); 
			middleLevelButton.GetComponent<Image>().color = new Color (0.8f, 0.8f, 0.8f); 
			topLevelButton.GetComponent<Image>().color = new Color (0.8f, 0.8f, 0.8f); 
		}
		else if (MapDisplay.inst.displayLevel == 2)
		{
			bottomLevelButton.GetComponent<Image>().color = new Color (0.8f, 0.8f, 0.8f); 
			middleLevelButton.GetComponent<Image>().color = new Color (1, 1, 1); 
			topLevelButton.GetComponent<Image>().color = new Color (0.8f, 0.8f, 0.8f); 
		}
		else if (MapDisplay.inst.displayLevel == 3)
		{
			bottomLevelButton.GetComponent<Image>().color = new Color (0.8f, 0.8f, 0.8f); 
			middleLevelButton.GetComponent<Image>().color = new Color (0.8f, 0.8f, 0.8f); 
			topLevelButton.GetComponent<Image>().color = new Color (1, 1, 1); 
		}
	}

	void DisablePanels()
	{ 
		mapPanel.SetActive(false);
		optionsPanel.SetActive(false);
	}
		
	public void OnContinueButton()
	{
		GlobalManager.inst.SetGamePaused(false); 
		DisablePanels(); 
		gameMenuActive = false; 
		EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(null);
	}

	public void OnQuitButton()
	{
		/*
		Time.timeScale = 1;
		GlobalManager.inst.LoadTitle (); 
		*/ 
		GlobalManager.inst.GameplayToTitleScreen(); 
	}



	/*
	/// <summary>
	/// Pauses the game.
	/// </summary>
	void CheckPause ()
	{
		if (Input.GetKeyDown (KeyCode.Escape) && !MapDisplay.inst.statusPanel.activeSelf) 
		{
			if (!GlobalManager.inst.gamePaused)
			{
				GlobalManager.inst.SetGamePaused(true); 
				menu.SetActive (true);
				EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(continueButton.gameObject);
			}
			else
			{
				GlobalManager.inst.SetGamePaused(false); 
				menu.SetActive (false);
				EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(null);
			}
		}
	}

	public void OnContinueButton()
	{
		if (GlobalManager.inst.gamePaused)
		{
			GlobalManager.inst.SetGamePaused(false); 
			menu.SetActive(false);
			EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(null);
		}
	} 

	void CheckStatusPanelReveal ()
	{
		// Use 'Q' key to reveal/hide status panel and map
		if (Input.GetKeyDown(KeyCode.Q))
		{
			if (!GlobalManager.inst.gamePaused)
			{
				GlobalManager.inst.SetGamePaused(true); 

				MapDisplay.inst.statusPanel.SetActive(!MapDisplay.inst.statusPanel.activeSelf); 

				// Set the toggle to the current floor
				level1Toggle.isOn = false;
				level2Toggle.isOn = false; 
				level3Toggle.isOn = false; 

				if (LevelData.inst.curLevel == 1)
				{
					level1Toggle.isOn = true; 
					EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(level1Toggle.gameObject);
				}
				else if (LevelData.inst.curLevel == 2)
				{
					level2Toggle.isOn = true; 
					EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(level2Toggle.gameObject);
				}
				else if (LevelData.inst.curLevel == 3)
				{
					level3Toggle.isOn = true; 
					EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(level3Toggle.gameObject);
				}
			}
			else
			{
				GlobalManager.inst.SetGamePaused(false); 

				MapDisplay.inst.statusPanel.SetActive(!MapDisplay.inst.statusPanel.activeSelf); 
				EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(null);
			}
		}
	}



	/// <summary>
	/// Reveals GameOver UI.
	/// </summary>
	public void GameOver ()
	{
		Time.timeScale = 0;
		gameOverUI.SetActive (true);
	}

	/// <summary>
	/// Exits Game
	/// </summary>
	public void ExitGameClicked ()
	{
		//Time.timeScale = 1;
		GlobalManager.inst.LoadTitle (); 
	}

	/// <summary>
	/// Triggers the end game timer when the player picks up the artifact.
	/// </summary>
	public void EndGameSequence()
	{
		endTimerUI.SetActive (true); //Activates the end sequence timer.
	}
	*/ 
}

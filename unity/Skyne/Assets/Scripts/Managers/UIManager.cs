using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.EventSystems; 

public class UIManager : Singleton<UIManager> 
{

	public GameObject menu;
	public bool isPaused = false; //determines whether the game is paused or not.

	public GameObject gameOverUI;

	[SerializeField] private Button reloadButton; 
	[SerializeField] private Button continueButton; 

	[SerializeField] private Toggle level1Toggle; 
	[SerializeField] private Toggle level2Toggle; 
	[SerializeField] private Toggle level3Toggle; 

	public EventSystem levelEventSystem; 

	public GameObject endTimerUI; 

	void Awake()
	{
		reloadButton.onClick.AddListener (() => { LoadGameClicked(); });
	}

	void Update()
	{
		CheckPause ();
		CheckStatusPanelReveal();

		if (GameState.inst.AllKeysFound ()) 
		{
			EndGameSequence ();
		}

		if (EventSystem.current == null)
			EventSystem.current = levelEventSystem; 
	}

	/// <summary>
	/// Pauses the game.
	/// </summary>
	void CheckPause ()
	{
		if (Input.GetKeyDown (KeyCode.Escape)) 
		{
			if (TogglePauseState() == true)
			{
				menu.SetActive (true);
				EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(continueButton.gameObject);
			}
			else
			{
				menu.SetActive (false);
				EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(null);
			}
		}
	}

	// Return true if isPaused has been set to true, false if ifPaused has been set to false
	bool TogglePauseState()
	{
		if (!isPaused) 
		{
			isPaused = true;
			Cursor.lockState = CursorLockMode.Confined;
			Time.timeScale = 0;

			return true; 
		} 
		else
		{
			isPaused = false; 
			Cursor.lockState = CursorLockMode.Locked;
			Time.timeScale = 1;

			return false; 
		}
	}

	void CheckStatusPanelReveal ()
	{
		// Use 'Q' key to reveal/hide status panel and map
		if (Input.GetKeyDown(KeyCode.Q))
		{
			if (TogglePauseState() == true)
			{
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
				MapDisplay.inst.statusPanel.SetActive(!MapDisplay.inst.statusPanel.activeSelf); 
				EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(null);
			}



			//TogglePauseState(); 
		}
	}

	public void Unpause ()
	{
		isPaused = !isPaused;

		Cursor.lockState = CursorLockMode.Locked;
		Time.timeScale = 1;
		menu.SetActive (false);
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
	/// Reloads game in the last save point the player visited.
	/// </summary>
	void LoadGameClicked ()
	{
		GlobalManager.inst.LoadGameplayScreen();
	}

	/// <summary>
	/// Exits Game
	/// </summary>
	public void ExitGameClicked ()
	{
		Time.timeScale = 1;
		GlobalManager.inst.LoadTitle (); 
	}

	/// <summary>
	/// Triggers the end game timer when the player picks up the artifact.
	/// </summary>
	public void EndGameSequence()
	{
		endTimerUI.SetActive (true); //Activates the end sequence timer.
	}

	public bool getIsPaused() 
	{
		return isPaused;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class UIManager : Singleton<UIManager> {

	public GameObject menu;
	public bool isPaused = false; //determines whether the game is paused or not.

	public GameObject gameOverUI;

	[SerializeField] private Button reloadButton; 

	public GameObject endTimerUI; 

	void Awake()
	{
		reloadButton.onClick.AddListener (() => { LoadGameClicked(); });
	}

	void Update()
	{
		Pause ();

		if (GameState.inst.AllKeysFound ()) {
			EndGameSequence ();
		}
	}

	/// <summary>
	/// Pauses the game.
	/// </summary>
	void Pause ()
	{
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (!isPaused) {
				isPaused = !isPaused;

				Cursor.lockState = CursorLockMode.Confined;
				Time.timeScale = 0;
				menu.SetActive (true);
			} else if (isPaused) {
				isPaused = !isPaused;

				Cursor.lockState = CursorLockMode.Locked;
				Time.timeScale = 1;
				menu.SetActive (false);
			}
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

	public bool getIsPaused() {
		return isPaused;
	}
}

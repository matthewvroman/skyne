using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class UIManager : Singleton<UIManager> {

	public GameObject menu;
	bool isPaused = false; //determines whether the game is paused or not.

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
			isPaused = !isPaused;

			if (isPaused) {
				Time.timeScale = 0;
				menu.SetActive (true);
			} else {
				Time.timeScale = 1;
				menu.SetActive (false);
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
		GlobalManager.inst.LoadTitle (); 
	}

	/// <summary>
	/// Triggers the end game timer when the player picks up the artifact.
	/// </summary>
	public void EndGameSequence()
	{
		endTimerUI.SetActive (true); //Activates the end sequence timer.
	}
}

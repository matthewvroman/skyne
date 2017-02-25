using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

	public GameObject menu;
	bool isPaused = false; //determines whether the game is paused or not.

	public GameObject gameOver;

	void Update()
	{
		Pause ();
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
	void GameOver ()
	{
		Time.timeScale = 0;
		gameOver.SetActive (true);
	}
}

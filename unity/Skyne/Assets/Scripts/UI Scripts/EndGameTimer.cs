using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EndGameTimer : MonoBehaviour {
	public Text timerText;
	public float minutes = 3;
	private float seconds = 0;
	private bool stop = false;

	void Start ()
	{
		timerText.GetComponent<Text> ();
		seconds = 0;
	}

	void Update ()
	{
		// Stops the timers functionality.
		if (stop) {
			return;
		}

		// Countdown.
		seconds -= Time.deltaTime;

		if (seconds <= 0 && minutes != 0) {
			minutes--; // Subtract one minute when seconds reach 0.
			seconds = 60; // Reset seconds to 60.
		}

		// Display the remaining time.
		timerText.text = minutes + ":" + seconds.ToString ("f0");

		// Gameover if they player runs out of time.
		if (seconds <= 0 && minutes <= 0) 
		{
			GlobalManager.inst.LoadGameOver ();
		}
	}

	public void StopTimer ()
	{
		timerText.color = Color.yellow;
		stop = true;
	}
}
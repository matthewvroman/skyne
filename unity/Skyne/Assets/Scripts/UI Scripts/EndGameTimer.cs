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
		if (stop) {
			return;
		}

		seconds -= Time.deltaTime;

		if (seconds <= 0 && minutes != 0) {
			minutes--;
			seconds = 60;
		}

		timerText.text = minutes + ":" + seconds.ToString ("f0");
	}

	public void StopTimer ()
	{
		timerText.color = Color.yellow;
		stop = true;
	}
}